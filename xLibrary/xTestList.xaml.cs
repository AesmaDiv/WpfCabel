using System;
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
using System.Reflection;
using System.ComponentModel;

namespace xLibrary
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class xTestList : UserControl
    {
        public string Name
        {
            get { return this.Name; }
            set { this.Name = value; }
        }
        public int Count
        {
            get { return xListView.Items.Count; }
        }
        public bool NewTestButton
        {
            set { Button.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
            get { return Button.Visibility == Visibility.Visible; }
        }
        
        public event RoutedEventHandler CurrentItem_Changed;    // Событие на изменение выбора текущего элемента списка
        public event RoutedEventHandler NewTest_Clicked;        // Событие на нажатие кнопки "Новый тест"
        //public event MouseButtonEventHandler Column_Clicked;
		public object CurrentItem = new Object();               // Текущий элемент списка
        private TextBox[] filterTextBox;                        // Массив текстовых полей столбцов таблицы (для фильтрации)

        #region
        /// <summary>
		/// Инициализация элемента
		/// </summary>
        public xTestList()
        {
            InitializeComponent();
        }
		/// <summary>
		/// Создание таблицы
		/// </summary>
		/// <param name="Headers">Заголовки</param>
		/// <param name="Bindings">Привязки</param>
        public void CreateTable(string[] Headers, string[] Bindings, double[] Widths)
		{
            // Инициализируем массив текстовых полей по размерности массива Привязок
            filterTextBox = new TextBox[Bindings.Length];
            for(int i=0; i<Headers.Length; i++)
			{
				// Создаем столбец таблицы
                GridViewColumn column = new GridViewColumn();
                column.Width = Widths[i];
                // Задаем привязку значений из соответствующего элемента массива Привязок
				column.DisplayMemberBinding = new Binding(Bindings[i]);

				// Создаем заголовок столбца
                TextBlock header = new TextBlock();
				header.Text = Headers[i];
                header.HorizontalAlignment = HorizontalAlignment.Center;
                // Создаем текстовое поле для фильтрации
                TextBox filter = new TextBox();
				filter.Name = Bindings[i];
                // Помещаем ссылку на текстовое поле в массив
                filterTextBox[i] = filter;
                // Устанавливаем отступы
                filter.Margin = new Thickness(0,3,0,3);
                // Назначаем обработчик события на изменение текста
				filter.TextChanged+=Filter_TextChanged;

                // Создаем панельку и помещаем в неё заголовок и текстовое поле
                StackPanel panel = new StackPanel();
                panel.Name = Bindings[i];
				panel.Orientation = Orientation.Vertical;
				panel.Children.Add(header);
				panel.Children.Add(filter);
                panel.MouseDown+=Column_Clicked;

                // Помещаем панельку в столбец
				column.Header = panel;
                // А столбец добавлям в таблицу
				xGridView.Columns.Add(column);
                // Вуа-ля
			}
            // Устанавливаем сортировку по умолчанию (id на убывание - новые тесты вверху)
            xListView.Items.IsLiveSorting = true;
            xListView.Items.SortDescriptions.Add(new SortDescription(Bindings[0], _curDir));
            
		}

        /// <summary>
        /// Добавление строки в таблицу
        /// </summary>
        /// <param name="row">элемент строки</param>
		public void AddRow(object row)
		{	
			// Собственно ну-вы-понели
            xListView.Items.Add(row);
		}
        public void SetSource(object[] rows)
        {
            xListView.ItemsSource = rows;
        }
        public void SetCurrentItem(int index)
        {
            xListView.SelectedIndex = index;
        }
        
        
        
        /// <summary>
		/// Обработчик события изменения выбора элемента таблицы
		/// </summary>
		private void xListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			// Запоминаем выбранный элемент(для доступа из-вне)
            CurrentItem = xListView.SelectedItem;
            // Транслируем событие изменения выбора текущего элемента списка
            if ((CurrentItem != null)&&(CurrentItem_Changed != null)) CurrentItem_Changed(this, new RoutedEventArgs());
		}
        /// <summary>
        /// Обработчик события изменения текста поля фильтрации
        /// </summary>
		private void Filter_TextChanged(object sender, RoutedEventArgs e)
        {
            // Собственно фильтруем список по условию
            xListView.Items.Filter = new Predicate<object>(NameFilter);
        }
        /// <summary>
        /// Условие фильтрации
        /// </summary>
        /// <param name="item">строка списка</param>
        private bool NameFilter(object item)
        {
            // Получаем тип элемента строки
            Type type = item.GetType();
            // Перебираем все поля фильтрации
            for (int i = 0; i < filterTextBox.Length; i++)
            {
                // Получаем значение параметра элемента строки соответствующее параметру фильтрации
                PropertyInfo pm = type.GetProperty(filterTextBox[i].Name);
                var pa = pm.GetValue(item);
                string value = pa.ToString().ToLower();
                // Если значение не содержит текста фильтрации - строка не проходит и не отображается
                if (!value.Contains(filterTextBox[i].Text.ToLower())) return false;
            }    
            return true;
        }


        private string _curName= null;
        private SortAdorner _curAdorner = null;
        private ListSortDirection _curDir = ListSortDirection.Descending; 
        private void Column_Clicked(object sender, MouseButtonEventArgs e)
        {

            StackPanel stk = sender as StackPanel;
            if (stk != null)
            {
                try { AdornerLayer.GetAdornerLayer(stk).Remove(_curAdorner); }
                catch { }
                xListView.Items.SortDescriptions.Clear();
            }

            string name = stk.Name;
            if (name.Equals(_curName))
            {
                if (_curDir == ListSortDirection.Ascending) _curDir = ListSortDirection.Descending;
                else _curDir = ListSortDirection.Ascending;
            }
            else
            {
                _curDir = ListSortDirection.Ascending;
                _curName = name;
            }

            _curAdorner = new SortAdorner(stk, _curDir);
            AdornerLayer.GetAdornerLayer(stk).Add(_curAdorner);

            xListView.Items.SortDescriptions.Add(new SortDescription(name, _curDir));
            //throw new Exception();
        }
        #endregion
        public class SortAdorner : Adorner
        {
            private readonly static Geometry _AscGeometry =
                Geometry.Parse("M 0,0 L 10,0 L 5,5 Z");

            private readonly static Geometry _DescGeometry =
                Geometry.Parse("M 0,5 L 10,5 L 5,0 Z");

            public ListSortDirection Direction { get; private set; }

            public SortAdorner(UIElement element, ListSortDirection dir)
                : base(element)
            { Direction = dir; }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (AdornedElement.RenderSize.Width < 20)
                    return;

                drawingContext.PushTransform(
                    new TranslateTransform(
                      AdornedElement.RenderSize.Width - 15,
                      (AdornedElement.RenderSize.Height - 5) / 2));

                drawingContext.DrawGeometry(Brushes.Green, null,
                    Direction == ListSortDirection.Ascending ?
                      _AscGeometry : _DescGeometry);

                drawingContext.Pop();
            }
        }


        #region Обрабтка событий кнопки "Новый тест"
        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Button.Background = (Brush)FindResource("btnPress");
        }
        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Button.Background = (Brush)FindResource("btnOver");
            if (NewTest_Clicked != null) NewTest_Clicked(this, new RoutedEventArgs());
        }
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button.Background = (Brush)FindResource("btnOver");
        }
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button.Background = (Brush)FindResource("btnIdle");
        }
        #endregion

        #region Контекстное меню
        public event RoutedEventHandler cmUpdate_Click;
        public event RoutedEventHandler cmDelete_Click;
        public event RoutedEventHandler cmPrintReport_Click;
        private void cmUpdate_Clicked(object sender, RoutedEventArgs e)
        {
            if (cmUpdate_Click != null) cmUpdate_Click(this, new RoutedEventArgs());
        }
        private void cmDelete_Clicked(object sender, RoutedEventArgs e)
        {
            if (cmDelete_Click != null) cmDelete_Click(this, new RoutedEventArgs());
        }
        private void cmPrintReport_Clicked(object sender, RoutedEventArgs e)
        {
            if (cmPrintReport_Click != null) cmPrintReport_Click(this, new RoutedEventArgs());
        }
        #endregion       
    }
}
