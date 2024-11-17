using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace LB3_LOBKOVA
{
    public partial class MainWindow : Window
    {
        private double lineWidth = 2;
        private Brush strokeColor = Brushes.Black;
        private Brush fillColor = Brushes.LightBlue;
        private string currentFileName = "Безымянный";

        public MainWindow()
        {
            InitializeComponent();
            CommandBinding saveBinding = new CommandBinding(ApplicationCommands.Save);
            saveBinding.Executed += Save_Executed;
            saveBinding.CanExecute += Save_CanExecute;
            this.CommandBindings.Add(saveBinding);

            CommandBinding openBinding = new CommandBinding(ApplicationCommands.Open);
            openBinding.Executed += Open_Executed;
            this.CommandBindings.Add(openBinding);

            UpdateWindowTitle();
            LineWidthToolBarComboBox.SelectedIndex = 1;
            StrokeColorToolBarComboBox.SelectedIndex = 0;
            FillColorToolBarComboBox.SelectedIndex = 0;
        }

        private void MainGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPosition = e.GetPosition(MainGrid);
            Polygon hexagon = new Polygon
            {
                Stroke = strokeColor,
                StrokeThickness = lineWidth,
                Fill = fillColor,
                Points = new PointCollection
                {
                    new Point(clickPosition.X + 20, clickPosition.Y),
                    new Point(clickPosition.X + 10, clickPosition.Y + 17),
                    new Point(clickPosition.X - 10, clickPosition.Y + 17),
                    new Point(clickPosition.X - 20, clickPosition.Y),
                    new Point(clickPosition.X - 10, clickPosition.Y - 17),
                    new Point(clickPosition.X + 10, clickPosition.Y - 17)
                }
            };
            MainGrid.Children.Add(hexagon);
        }

        private void MainGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(MainGrid);
            StatusText.Text = $"Координаты: X={position.X}, Y={position.Y}";
        }

        private void OpenSettingsDialog_Click(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new SettingsDialog();
            settingsDialog.SetLineWidth(lineWidth);
            settingsDialog.SetStrokeColor(strokeColor);
            settingsDialog.SetFillColor(fillColor);

            settingsDialog.OnSettingsChanged += (newLineWidth, newStrokeColor, newFillColor) =>
            {
                lineWidth = newLineWidth;
                strokeColor = newStrokeColor;
                fillColor = newFillColor;
                SetToolBarValues();
            };

            if (settingsDialog.ShowDialog() == true)
            {
                lineWidth = settingsDialog.SelectedLineWidth;
                strokeColor = settingsDialog.SelectedStrokeColor;
                fillColor = settingsDialog.SelectedFillColor;
                SetToolBarValues();
            }
        }

        private void SetToolBarValues()
        {
            LineWidthToolBarComboBox.SelectedItem = LineWidthToolBarComboBox.Items
                .Cast<System.Windows.Controls.ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == lineWidth.ToString(CultureInfo.InvariantCulture));

            StrokeColorToolBarComboBox.SelectedItem = StrokeColorToolBarComboBox.Items
                .Cast<System.Windows.Controls.ComboBoxItem>()
                .FirstOrDefault(item => item.Tag?.ToString() == strokeColor.ToString());

            FillColorToolBarComboBox.SelectedItem = FillColorToolBarComboBox.Items
                .Cast<System.Windows.Controls.ComboBoxItem>()
                .FirstOrDefault(item => item.Tag?.ToString() == fillColor.ToString());
        }

        private void LineWidthToolBarComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LineWidthToolBarComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                lineWidth = double.Parse(selectedItem.Content?.ToString() ?? "2", CultureInfo.InvariantCulture); // Значение по умолчанию
            }
        }

        private void StrokeColorToolBarComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (StrokeColorToolBarComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                strokeColor = (Brush)new BrushConverter().ConvertFromString(selectedItem.Tag?.ToString() ?? "Black");
            }
        }

        private void FillColorToolBarComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FillColorToolBarComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                fillColor = (Brush)new BrushConverter().ConvertFromString(selectedItem.Tag?.ToString() ?? "LightBlue");
            }
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainGrid.Children.Count > 0;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Текстовые файлы (txt)|*.txt|Все файлы|*.*"
            };

            if (sfd.ShowDialog() == true)
            {
                currentFileName = sfd.FileName;
                using (StreamWriter writer = new StreamWriter(currentFileName))
                {
                    foreach (Polygon hexagon in MainGrid.Children)
                    {
                        var points = string.Join(";", hexagon.Points.Select(p => $"{p.X.ToString(CultureInfo.InvariantCulture)},{p.Y.ToString(CultureInfo.InvariantCulture)}"));
                        string strokeColor = hexagon.Stroke.ToString();
                        string fillColor = hexagon.Fill.ToString();
                        double thickness = hexagon.StrokeThickness;

                        writer.WriteLine($"{points}|{strokeColor}|{thickness.ToString(CultureInfo.InvariantCulture)}|{fillColor}");
                    }
                }
                UpdateWindowTitle();
            }
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Текстовые файлы (txt)|*.txt|Все файлы|*.*"
            };

            if (ofd.ShowDialog() == true)
            {
                MainGrid.Children.Clear();
                currentFileName = ofd.FileName;
                using (StreamReader reader = new StreamReader(currentFileName))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Разделяем строку по символу '|', чтобы извлечь координаты, цвет линии, толщину линии и цвет заливки
                        string[] parts = line.Split('|');
                        if (parts.Length != 4)
                        {
                            MessageBox.Show("Некорректный формат строки: " + line);
                            continue; // Пропускаем некорректную строку
                        }

                        // Парсим координаты точек
                        string[] pointStrings = parts[0].Split(';');
                        PointCollection points = new PointCollection();
                        bool pointsParsed = true;

                        foreach (string pointStr in pointStrings)
                        {
                            string[] coords = pointStr.Split(',');
                            if (coords.Length == 2 &&
                                double.TryParse(coords[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
                                double.TryParse(coords[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
                            {
                                points.Add(new Point(x, y));
                            }
                            else
                            {
                                MessageBox.Show("Некорректный формат координат: " + pointStr);
                                pointsParsed = false;
                                break;
                            }
                        }

                        if (!pointsParsed)
                            continue; // Пропускаем эту строку, если координаты не были разобраны

                        // Парсим цвет линии
                        Brush stroke;
                        try
                        {
                            stroke = (Brush)new BrushConverter().ConvertFromString(parts[1]);
                        }
                        catch
                        {
                            MessageBox.Show("Некорректный формат цвета линии: " + parts[1]);
                            continue;
                        }

                        // Парсим толщину линии
                        if (!double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double thickness))
                        {
                            MessageBox.Show("Некорректный формат толщины линии: " + parts[2]);
                            thickness = 2; // Используем значение по умолчанию
                        }

                        // Парсим цвет заливки
                        Brush fill;
                        try
                        {
                            fill = (Brush)new BrushConverter().ConvertFromString(parts[3]);
                        }
                        catch
                        {
                            MessageBox.Show("Некорректный формат цвета заливки: " + parts[3]);
                            fill = Brushes.Transparent; // Используем прозрачную заливку по умолчанию
                        }

                        // Создаем и добавляем шестиугольник в MainGrid
                        Polygon hexagon = new Polygon
                        {
                            Stroke = stroke,
                            StrokeThickness = thickness,
                            Fill = fill,
                            Points = points
                        };
                        MainGrid.Children.Add(hexagon);
                    }
                }
                UpdateWindowTitle();
            }
        }

        private void UpdateWindowTitle()
        {
            Title = $"Графический Редактор - {System.IO.Path.GetFileName(currentFileName)}";
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Графический редактор версии 1.0", "О программе");
        }
    }
}
