using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace xLibrary
{
    /// <summary>
    /// Логика взаимодействия для xDataField.xaml
    /// </summary>
    public partial class xDataField : UserControl
    {
        public enum FieldType { TextBox, ComboBox }; // Тип поля - текстбокс или комбобокс
        private FieldType _type = FieldType.TextBox; // по умолчанию - текстбокс
        public FieldType Type
        {
            // Получаем тип поля (не пригодится, но пусть будет)
            get { return _type; }
            set
            {
                // Запоминаем тип поля
                _type = value;
                // И в соответсвтии с ним...
                if (_type == FieldType.TextBox)
                {
                    //...либо предъявляем текстбокс и прячем комбобокс
                    xTextBox.Visibility = Visibility.Visible;
                    xComboBox.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //...либо предъявляем комбобокс и прячем текстбокс
                    xTextBox.Visibility = Visibility.Collapsed;
                    xComboBox.Visibility = Visibility.Visible;
                }
            }
        }
        // Возможность редактирования содержимого:
        // для текстбокса включен или нет, для комбобокса редактируем или нет
        public bool IsEditable
        {
            get
            {
                if (_type == FieldType.TextBox) return xTextBox.IsEnabled;
                else return xComboBox.IsEditable;
            }
            set
            {
                if (_type == FieldType.TextBox) xTextBox.IsEnabled = value;
                else xComboBox.IsEditable = value;
            }
        }
        // Заголовок поля
        public string Header
        {
            get { return xHeader.Text; }
            set { xHeader.Text = value; }
        }
        public Brush HeaderForeground
        {
            get { return xHeader.Foreground; }
            set { xHeader.Foreground = value; }
        }
        // Текущее значение
        public string Value
        {
            get
            {
                if (_type == FieldType.TextBox) return xTextBox.Text;
                else return xComboBox.Text;
            }
            set
            {
                if (_type == FieldType.TextBox) xTextBox.Text = value;
                else xComboBox.Text = value; 
            }
        }
        // Возможные значения комбобокса
        public IEnumerable ItemsSource
        {
            get { return xComboBox.ItemsSource; }
            set { xComboBox.ItemsSource = value; }
        }
        public ComboBox ComboBox
        { get { return xComboBox; } }
        public TextBox TextBox
        { get { return xTextBox; } }
        public TextAlignment HeaderTextAlignment
        {
            get { return xHeader.TextAlignment; }
            set { xHeader.TextAlignment = value; }
        }
        public TextAlignment FieldTextAligment
        {
            get { return xTextBox.TextAlignment; }
            set { xTextBox.TextAlignment = value; }
        }
        // Ширина заголовка
        public double HeaderWidth
        {
            set { colHeader.Width = new GridLength(value); }
        }
        // Ширина поля
        public double FieldWidth
        {
            set { colField.Width = new GridLength(value); }
        }
        // Внешнее событие изменения выбора элемента ComboBox`а
        public event RoutedEventHandler ComboBox_SelectionChanged;
        public event RoutedEventHandler TextBox_TextChanged;
        
        
        /// <summary>
        /// Инициализация компонентов
        /// </summary>
        public xDataField()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Внутрений обработчик события изменения выбора элемента ComboBox`а
        /// </summary>
        private void xComboBox_SelectionChanged(object sender, EventArgs e)
        {
            // Транслируем событие во вне
            //if (ComboBox_SelectionChanged != null)
            //    ComboBox_SelectionChanged(this, new RoutedEventArgs());
        }
        private void xComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // Транслируем событие во вне
            if (ComboBox_SelectionChanged != null)
                ComboBox_SelectionChanged(this, new RoutedEventArgs());
        }
        private void xTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBox_TextChanged != null)
                TextBox_TextChanged(this, new RoutedEventArgs());
        }
    }

    
}
