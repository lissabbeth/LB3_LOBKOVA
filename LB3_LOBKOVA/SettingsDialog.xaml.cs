using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LB3_LOBKOVA
{
    public partial class SettingsDialog : Window
    {
        public double SelectedLineWidth { get; private set; }
        public Brush SelectedStrokeColor { get; private set; }
        public Brush SelectedFillColor { get; private set; }

        public delegate void SettingsChangedHandler(double lineWidth, Brush strokeColor, Brush fillColor);
        public event SettingsChangedHandler OnSettingsChanged;

        public SettingsDialog()
        {
            InitializeComponent();
            LineWidthComboBox.SelectedIndex = 1;
            StrokeColorComboBox.SelectedIndex = 0;
            FillColorComboBox.SelectedIndex = 0;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedValues();

            // Вызываем событие изменения настроек
            OnSettingsChanged?.Invoke(SelectedLineWidth, SelectedStrokeColor, SelectedFillColor);
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedValues();
            DialogResult = true;
            Close();
        }

        private void UpdateSelectedValues()
        {
            SelectedLineWidth = double.Parse(((ComboBoxItem)LineWidthComboBox.SelectedItem).Content.ToString());
            SelectedStrokeColor = (Brush)new BrushConverter().ConvertFromString(((ComboBoxItem)StrokeColorComboBox.SelectedItem).Tag.ToString());
            SelectedFillColor = (Brush)new BrushConverter().ConvertFromString(((ComboBoxItem)FillColorComboBox.SelectedItem).Tag.ToString());
        }

        public void SetLineWidth(double lineWidth)
        {
            LineWidthComboBox.SelectedItem = LineWidthComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == lineWidth.ToString());
        }

        public void SetStrokeColor(Brush strokeColor)
        {
            StrokeColorComboBox.SelectedItem = StrokeColorComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag.ToString() == strokeColor.ToString());
        }

        public void SetFillColor(Brush fillColor)
        {
            FillColorComboBox.SelectedItem = FillColorComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag.ToString() == fillColor.ToString());
        }
    }
}
