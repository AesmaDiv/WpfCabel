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
using System.Xml;
using System.Windows.Markup; 
using System.IO;
using CardinalSplineTest;

namespace xLibrary
{
    /// <summary>
    /// Логика взаимодействия для xChart.xaml
    /// </summary>
    public partial class xChart : UserControl
    {
        public double _margin = 50;
        public string title = "";
        public bool bShowMarkers = false;
        public bool bShowGraphics = true;
        private AxisCollection arrayAxis;
        private ChartCollection[] chartCollections = new ChartCollection[2];
        private CrossMark[] _markers = new CrossMark[0];
        private Dot[] _dots = new Dot[0];
        private double _nom;

        public xChart()
        {
            InitializeComponent();
            arrayAxis = new AxisCollection(cnvGrid, _margin);
            chartCollections[0] = new ChartCollection(cnvLayer1, _margin);
            chartCollections[1] = new ChartCollection(cnvLayer2, _margin);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            ReFresh();
        }
        public void ReFresh()
        {
            ClearCanvases();
            if (!bShowGraphics) return;
            //MoveMarkers();
            DrawTitle();
            arrayAxis.UpdateCanvas(cnvGrid);
            arrayAxis.Draw();
            chartCollections[0].Update(arrayAxis);
            chartCollections[1].Update(arrayAxis);
            chartCollections[0].Draw();
            chartCollections[1].Draw();
            DrawMarkers();

            /*if (chartCollection.Count < 6)*/
            stkResults.Visibility = Visibility.Collapsed;
            /*else stkResults.Visibility = Visibility.Visible;*/
        }

        #region Оси - Добавление и удаление
        public void AddAxis(double size, string _name)
        {
            Axis axis = new Axis(size, _name);
            arrayAxis.Add(axis);
            InvalidateVisual();
        }
        public void AddAxis(double size, string _name, string _division_format)
        {
            Axis axis = new Axis(size, _name);
            axis.parameters.division_format = _division_format;
            arrayAxis.Add(axis);
            InvalidateVisual();
        }
        public void DeleteAxis(string _name)
        {
            arrayAxis.Delete(_name);
            InvalidateVisual();
        }
        public void DeleteAllAxises()
        {
            arrayAxis = new AxisCollection(cnvGrid, _margin);
            cnvGrid.Children.Clear();
            InvalidateVisual();
        }
        #endregion
        #region Графики - Добавление и удаление
        public void AddChart(int index, Point[] points, int axisY_index)
        {
            AddChart(index, points, axisY_index, Colors.White, 1, false);
        }
        public void AddChart(int index, Point[] points, int axisY_index, Color color, double thickness, bool isDashed)
        {
            if (index > 1) return;
            chartCollections[index].Add(points, arrayAxis, axisY_index, color, thickness, isDashed);
            InvalidateVisual();
        }
        public void AddChart(int index, Point[] points, int axisY_index, Color color, double thickness, bool isDashed, string name)
        {
            if (index > 1) return;
            chartCollections[index].Add(points, arrayAxis, axisY_index, color, thickness, isDashed, name);
            InvalidateVisual();
        }
        public void DeleteChart(string name)
        {
            chartCollections[0].Delete(name);
            chartCollections[1].Delete(name);
            InvalidateVisual();
        }
        #endregion
        #region Заголовок и Зона Номинала
        public void DrawTitle()
        {
            Label label = new Label();
            label.Content = title;
            label.Padding = new Thickness(0);

            label.Foreground = Brushes.White;
            label.Background = Brushes.Transparent;

            label.FontFamily = new FontFamily("Arial");
            label.FontSize = 14;

            label.HorizontalContentAlignment = HorizontalAlignment.Left;
            label.VerticalContentAlignment = VerticalAlignment.Top;

            label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Rect measureRect = new Rect(label.DesiredSize);
            label.Arrange(measureRect);

            Canvas.SetLeft(label, _margin);
            Canvas.SetTop(label, 0);

            cnvGrid.Children.Add(label);
        }
        public void SetNominal(double min, double nom, double max)
        {
            arrayAxis.SetNominal(min, nom, max);
            _nom = nom;
        }
        #endregion

        #region Маркер
        public void AddMarker(string name, Brush color)
        {
            int length = _markers.Length;
            Array.Resize<CrossMark>(ref _markers, ++length);
            _markers[--length] = new CrossMark(name, color);
            _markers[--length].Center = new Point(100, 100);
            cnvMarks.Children.Clear();
            if (_markers.Length > 0)
                for (int i = 0; i < _markers.Length; i++)
                    cnvMarks.Children.Add(_markers[i]);
        }
        public void RemoveMarker(string axisY_name)
        {
            xLibrary.CrossMark marker = Array.Find<CrossMark>(_markers, item => item.Name == axisY_name);
            if (marker != null) cnvMarks.Children.Remove(marker);
        }
        public void MoveMarkers(string name, int axisX_index, int axisY_index, Point position)
        {
            xLibrary.CrossMark marker = Array.Find<CrossMark>(_markers, item => item.Name == name);
            if (marker == null) return;
            
            Point transPoint = new Point();
            transPoint.X = arrayAxis.TranslateCoord(axisX_index , position.X);
            transPoint.Y = arrayAxis.TranslateCoord(axisY_index, position.Y);
            marker.Center = transPoint;
            ReFresh();
        }
        private void DrawMarkers()
        {
            try
            {
                if (_markers.Length > 0)
                    for (int i = 0; i < _markers.Length; i++)
                        _markers[i].Visibility = bShowMarkers ? Visibility.Visible : Visibility.Hidden;
            }
            catch (Exception ex) { }
        }
        #endregion

        #region Точки
        public void AddPoint(string name)
        {
            xLibrary.CrossMark marker = Array.Find<CrossMark>(_markers, item => item.Name == name);
            if (marker == null) return;
            int index = _dots.Length;
            Array.Resize<Dot>(ref _dots, index + 1);
            _dots[index] = new Dot();
            _dots[index].Center = marker.Center;
            _dots[index].Fill = marker.Fill;
            cnvMarks.Children.Add(_dots[index]);
        }
        public void RemovePoint(int count)
        {
            int last_index = _dots.Length - 1;
            if (last_index < count - 1) return;
            for (int i = 0; i < count; i++ )
            {
                Dot dot = _dots[last_index - i];
                cnvMarks.Children.Remove(dot);
            }
            Array.Resize<Dot>(ref _dots, _dots.Length - count);
        }
        public void RemoveAllPoints()
        {
            cnvMarks.Children.RemoveRange(2, cnvMarks.Children.Count - 2);
            _dots = new Dot[0];
        }
        #endregion

        #region Очистка холста
        public void ClearChartCollection(int index)
        {
            if (index > 1) return;
            chartCollections[index].Clear();
        }
        public void ClearLayer(int index)
        {
            switch (index)
            {
                case 0:
                    cnvGrid.Children.Clear();
                    break;
                case 1:
                    cnvLayer1.Children.Clear();
                    break;
                case 2:
                    cnvLayer2.Children.Clear();
                    break;
                case 3:
                    cnvMarks.Children.Clear();
                    break;
            }
        }
        public void ClearCanvases()
        {
            for (int i = 0; i < 3; i++)
                ClearLayer(i);
        }
        public void ClearAll()
        {
            arrayAxis = new AxisCollection(cnvGrid, _margin);
            chartCollections[0] = new ChartCollection(cnvLayer1, _margin);
            chartCollections[1] = new ChartCollection(cnvLayer1, _margin);
            title = "";
            ClearCanvases();
            InvalidateVisual();
        }
        #endregion

        public string GetMaximumKPD(int index)
        {
            if (index > 1) return "";
            if ((_nom < 0)||(chartCollections[index].Count < 6)) return "";

            Point p_max_etalonKPD = chartCollections[index].GetMaximum("etalonKPD");
            Point p_max_factKPD = chartCollections[index].GetMaximum("graphKPD");
            string str_max_etalonKPD = p_max_etalonKPD.Y.ToString("#.#%") + ";" + p_max_etalonKPD.X.ToString("#.#") + ";";
            string str_max_factKPD = p_max_factKPD.Y.ToString("#.#%") + ";" + p_max_factKPD.X.ToString("#.#") + ";";
            string str_nom_etalonKPD = chartCollections[index].GetValueY("etalonKPD", (int)_nom).ToString("#.#%") + ";";
            string str_nom_factKPD = chartCollections[index].GetValueY("graphKPD", (int)_nom).ToString("#.#%") + ";";

            return str_nom_etalonKPD + str_nom_factKPD + str_max_etalonKPD + str_max_factKPD;
        }


        

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Mouse.SetCursor(Cursors.Cross);
            Point point = e.GetPosition(cnvMarks);
            mark.Content = (arrayAxis.RetranslateCoord(0, point.X)).ToString("#.##") + " : " + (arrayAxis.RetranslateCoord(3, point.Y)).ToString("#%");
        }

        #region Классы Осей и Графиков и их Коллекций
        private class Axis
        {
            public AxisParameters parameters;

            public Axis(double size)
            {
                parameters = new AxisParameters(size);
            }
            public Axis(double size, string _name)
            {
                parameters = new AxisParameters(size, _name);
            }
            public class AxisParameters
            {
                public double virtual_size;
                public double real_size;
                public int divisions;
                public double per_division;
                public double coeff;
                public string name;
                public string division_format = "{0:0}";

                public AxisParameters(double max_value)
                {
                    virtual_size = Double.Parse(CalculateFloat(max_value.ToString()));
                    per_division = virtual_size / divisions;
                }
                public AxisParameters(double max_value, string _name)
                {
                    virtual_size = Double.Parse(CalculateFloat(max_value.ToString()));
                    per_division = virtual_size / divisions;
                    name = _name;
                }
                public void CalculateCoeffs(double container_size)
                {
                    real_size = container_size;
                    coeff = real_size / virtual_size;
                }

                private String CalculateFloat(String input)
                {
                    String temp = input;
                    int point_index = -1;
                    int first_digit_index = 0;
                    int input_length, length_difference;

                    point_index = temp.IndexOf('.');

                    if (point_index > 0)
                    {
                        temp = temp.Replace(".", "");


                        for (int i = 0; i < temp.Length; i++)
                            if (temp[i] != '0') { first_digit_index = i; break; }

                        temp = temp.Substring(first_digit_index);

                    }

                    input_length = temp.Length;
                    temp = CalculateInt(temp);
                    length_difference = temp.Length - input_length;

                    if (first_digit_index > 0)
                        for (int i = 0; i < (first_digit_index - length_difference); i++)
                            temp = "0" + temp;
                    if (point_index > 0)
                        temp = temp.Insert(point_index, ".");

                    return temp;
                }
                private String CalculateInt(String input)
                {
                    String result = "";
                    String temp = input;

                    if (temp.Length > 1)
                    {
                        int first_digit = int.Parse(temp[0].ToString());
                        int second_digit = int.Parse(temp[1].ToString());
                        int digits = first_digit * 10 + second_digit;
                        int power = temp.Length - 1;
                        if (first_digit < 5)
                        {
                            if (second_digit < 5) temp = ((first_digit * 10 + 5) * Math.Pow(10, power - 1)).ToString();
                            else temp = ((first_digit + 1) * Math.Pow(10, power)).ToString();
                        }
                        else temp = ((first_digit + 1) * Math.Pow(10, power)).ToString();
                    }
                    result = temp;
                    divisions = CalculateDivisions(temp);

                    return result;
                }
                private int CalculateDivisions(String input)
                {
                    int result = 0;
                    string temp = input.Replace("0", "");
                    int first_digit = int.Parse(temp[0].ToString());
                    if (first_digit < 5)
                    {
                        if (temp.Length > 1) result = first_digit * 2 + 1;
                        else result = first_digit * 2;
                    }
                    else result = first_digit;
                    if (first_digit == 1)
                        if (temp.Length > 1) result = 15;
                        else result = 10;

                    return result;
                }
            }
        }
        private class AxisCollection
        {
            private Axis[] _axisArray;          // Коллекция осей
            private double _margin;
            private Canvas _canvas;
            private double _rangeMin, _rangeNom, _rangeMax;

            public AxisCollection(Canvas canvas, double margin)
            {
                _canvas = canvas;
                _margin = margin;
                _axisArray = new Axis[4];
            }
            public void Add(Axis axis)
            {
                int index = _axisArray.Count(s => s != null);
                if (index == 4) return;
                _axisArray[index] = axis;
            }
            public void Add(double size)
            {
                Axis axis = new Axis(size);
                this.Add(axis);
            }
            public void Delete(string name)
            {
                if (_axisArray.Length == 0) return;
                _axisArray = _axisArray.Where(a => a.parameters.name != name).ToArray();
            }

            public Axis GetAxis(int index)
            {
                return _axisArray[index];
            }
            public void SetNominal(double min, double nom, double max)
            {
                _rangeMin = min;
                _rangeNom = nom;
                _rangeMax = max;
            }
            public void UpdateCanvas(Canvas canvas)
            {
                _canvas = canvas;
            }
            public void Draw()
            {

                if (_axisArray[0] == null) return; else _DrawMainX();
                if (_axisArray[1] == null) return; else _DrawMainY();
                if (_axisArray[2] == null) return; else _DrawSecondaryY(2);
                if (_axisArray[3] == null) return; else _DrawSecondaryY(3);

            }
            public double TranslateCoord(int axis_index, double coord)
            {
                if (_axisArray[axis_index] == null) return 0;
                if (axis_index == 0) return coord * _axisArray[0].parameters.coeff + _margin;
                else return _canvas.ActualHeight - coord * _axisArray[axis_index].parameters.coeff - _margin;
            }
            public double TranslateCoord(string axis_name, double coord)
            {
                int axis_index = Array.FindIndex<Axis>(_axisArray, axis => axis.parameters.name == axis_name);
                return TranslateCoord(axis_index, coord);
            }
            public double RetranslateCoord(int axis_index, double coord)
            {
                if (_axisArray[axis_index] == null) return 0;
                if (axis_index == 0) return (coord - _margin) / _axisArray[0].parameters.coeff;
                else return (_canvas.ActualHeight - coord - _margin) / _axisArray[axis_index].parameters.coeff;
            }
            public double RetranslateCoord(string axis_name, double coord)
            {
                int axis_index = Array.FindIndex<Axis>(_axisArray, axis => axis.parameters.name == axis_name);
                return RetranslateCoord(axis_index, coord);
            }

            private void _DrawMainX()
            {
                _axisArray[0].parameters.CalculateCoeffs(_canvas.ActualWidth - 2 * _margin);
                _DrawNominal();
                Line l = new Line();
                l.Stroke = Brushes.White;
                l.X1 = _margin;
                l.Y1 = _canvas.ActualHeight - _margin;
                l.X2 = _canvas.ActualWidth - _margin;
                l.Y2 = _canvas.ActualHeight - _margin;

                _canvas.Children.Add(l);

                for (int i = 0; i <= _axisArray[0].parameters.divisions; i++)
                {
                    Line div = new Line();
                    div.Stroke = Brushes.White;
                    div.X1 = _margin + i * _axisArray[0].parameters.coeff * _axisArray[0].parameters.per_division;
                    div.Y1 = _canvas.ActualHeight - _margin;
                    div.X2 = _margin + i * _axisArray[0].parameters.coeff * _axisArray[0].parameters.per_division;
                    div.Y2 = _canvas.ActualHeight - _margin - 5;

                    _canvas.Children.Add(div);

                    div = new Line();
                    div.Stroke = Brushes.White;
                    div.StrokeThickness = 0.5;
                    div.X1 = _margin + i * _axisArray[0].parameters.coeff * _axisArray[0].parameters.per_division;
                    div.Y1 = _canvas.ActualHeight - _margin;
                    div.X2 = _margin + i * _axisArray[0].parameters.coeff * _axisArray[0].parameters.per_division;
                    div.Y2 = _margin;

                    _canvas.Children.Add(div);

                    string strDiv = String.Format(_axisArray[0].parameters.division_format, i * _axisArray[0].parameters.per_division);

                    _DrawText_Division(div.X1, div.Y1, strDiv, Brushes.White, -0.5, 0);
                }
                _DrawText_AxisName(0, _axisArray[0].parameters.name, Brushes.White);
            }
            private void _DrawMainY()
            {
                _axisArray[1].parameters.CalculateCoeffs(_canvas.ActualHeight - 2 * _margin);

                Line l = new Line();
                l.Stroke = Brushes.White;
                l.X1 = _margin;
                l.Y1 = _canvas.ActualHeight - _margin;
                l.X2 = _margin;
                l.Y2 = _margin;

                _canvas.Children.Add(l);

                for (int i = 1; i <= _axisArray[1].parameters.divisions; i++)
                {
                    Line div = new Line();
                    div.Stroke = Brushes.White;
                    div.X1 = _margin;
                    div.Y1 = _canvas.ActualHeight - _margin - i * _axisArray[1].parameters.coeff * _axisArray[1].parameters.per_division;
                    div.X2 = _canvas.ActualWidth - _margin;
                    div.Y2 = _canvas.ActualHeight - _margin - i * _axisArray[1].parameters.coeff * _axisArray[1].parameters.per_division;

                    _canvas.Children.Add(div);

                    div = new Line();
                    div.Stroke = Brushes.White;
                    div.StrokeThickness = 0.5;
                    div.X1 = _margin;
                    div.Y1 = _canvas.ActualHeight - _margin - i * _axisArray[1].parameters.coeff * _axisArray[1].parameters.per_division;
                    div.X2 = _margin + 5;
                    div.Y2 = _canvas.ActualHeight - _margin - i * _axisArray[1].parameters.coeff * _axisArray[1].parameters.per_division;

                    _canvas.Children.Add(div);

                    string strDiv = String.Format(_axisArray[1].parameters.division_format, i * _axisArray[1].parameters.per_division);

                    _DrawText_Division(div.X1 - 3, div.Y1, strDiv, Brushes.White, -1, -.5);
                }
                _DrawText_AxisName(1, _axisArray[1].parameters.name, Brushes.White);
            }
            private void _DrawSecondaryY(int array_index)
            {
                _axisArray[array_index].parameters.divisions = _axisArray[1].parameters.divisions;
                _axisArray[array_index].parameters.per_division = _axisArray[array_index].parameters.virtual_size / _axisArray[array_index].parameters.divisions;
                _axisArray[array_index].parameters.CalculateCoeffs(_canvas.ActualHeight - 2 * _margin);

                Line l = new Line();
                l.Stroke = Brushes.White;
                l.X1 = _canvas.ActualWidth - _margin;
                l.Y1 = _canvas.ActualHeight - _margin;
                l.X2 = _canvas.ActualWidth - _margin;
                l.Y2 = _margin;

                _canvas.Children.Add(l);

                for (int i = 1; i <= _axisArray[1].parameters.divisions; i++)
                {
                    Line div = new Line();
                    div.Stroke = Brushes.White;
                    div.X1 = _canvas.ActualWidth - _margin;
                    div.Y1 = _canvas.ActualHeight - _margin - i * _axisArray[1].parameters.coeff * _axisArray[1].parameters.per_division;
                    div.X2 = _canvas.ActualWidth - _margin - 5;
                    div.Y2 = _canvas.ActualHeight - _margin - i * _axisArray[1].parameters.coeff * _axisArray[1].parameters.per_division;

                    _canvas.Children.Add(div);
                    string strDiv = String.Format(_axisArray[array_index].parameters.division_format, i * _axisArray[array_index].parameters.per_division);

                    if (_axisArray[3] == null)
                        _DrawText_Division(div.X1 + 3, div.Y1, strDiv, Brushes.White, 0, -.5);
                    else if (array_index == 2)
                        _DrawText_Division(div.X1 + 3, div.Y1, strDiv, Brushes.Red, 0, -1.1);
                    else
                        _DrawText_Division(div.X1 + 3, div.Y1, strDiv, Brushes.Green, 0, 0.1);
                }
                _DrawText_AxisName(array_index, _axisArray[array_index].parameters.name, Brushes.White);

            }
            private void _DrawText_Division(double x, double y, string text, Brush color, double x_offset_coeff, double y_offset_coeff)
            {

                Label label = new Label();
                label.Content = text;
                label.Padding = new Thickness(0);

                label.Foreground = color;
                label.Background = Brushes.Transparent;

                label.HorizontalContentAlignment = HorizontalAlignment.Left;
                label.VerticalContentAlignment = VerticalAlignment.Top;

                label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                Rect measureRect = new Rect(label.DesiredSize);
                label.Arrange(measureRect);

                Canvas.SetLeft(label, x + (measureRect.Width) * x_offset_coeff);
                Canvas.SetTop(label, y + (measureRect.Height) * y_offset_coeff);

                _canvas.Children.Add(label);
            }
            private void _DrawText_AxisName(int axis_index, string text, Brush color)
            {

                Label label = new Label();
                label.Content = text;
                label.Padding = new Thickness(0);

                label.Foreground = color;
                label.Background = Brushes.Transparent;

                label.HorizontalContentAlignment = HorizontalAlignment.Left;
                label.VerticalContentAlignment = VerticalAlignment.Top;

                label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                Rect measureRect = new Rect(label.DesiredSize);
                label.Arrange(measureRect);

                switch (axis_index)
                {
                    case 1:
                        label.RenderTransform = new RotateTransform(-90, 0, 0);
                        Canvas.SetLeft(label, 0);
                        Canvas.SetTop(label, (_canvas.ActualHeight + measureRect.Width) / 2);
                        break;

                    case 2:
                        if (_axisArray[3] == null)
                        {
                            label.RenderTransform = new RotateTransform(-90, measureRect.Width, measureRect.Height);
                            Canvas.SetRight(label, 0);
                            Canvas.SetTop(label, (_canvas.ActualHeight - measureRect.Width) / 2 - measureRect.Height);
                        }
                        else
                        {
                            label.RenderTransform = new RotateTransform(-90, measureRect.Width, measureRect.Height);
                            Canvas.SetRight(label, 0);
                            Canvas.SetTop(label, _margin - measureRect.Height);
                        }
                        break;

                    case 3:
                        label.RenderTransform = new RotateTransform(-90, measureRect.Width, measureRect.Height);
                        Canvas.SetRight(label, 0);
                        Canvas.SetBottom(label, _margin + measureRect.Width);
                        break;

                    default:
                        Canvas.SetLeft(label, (_canvas.ActualWidth - measureRect.Width) / 2);
                        Canvas.SetBottom(label, 0);
                        break;

                }

                _canvas.Children.Add(label);
            }
            private void _DrawNominal()
            {
                if ((_rangeMin == 0) || (_rangeNom == 0) || (_rangeMax == 0)) return;

                Rectangle r = new Rectangle();
                r.Width = TranslateCoord(0, _rangeMax) - TranslateCoord(0, _rangeMin);
                r.Height = _canvas.ActualHeight - 2 * _margin;
                r.Fill = Brushes.DarkGray;

                Canvas.SetLeft(r, TranslateCoord(0, _rangeMin));
                Canvas.SetTop(r, _margin);

                _canvas.Children.Add(r);

                Line l;
                double range;
                for (int i = 0; i < 3; i++)
                {
                    l = new Line();
                    range = TranslateCoord(0, _rangeMin);
                    if (i == 1) range = TranslateCoord(0, _rangeNom);
                    if (i == 2) range = TranslateCoord(0, _rangeMax);

                    l.X1 = range;
                    l.X2 = range;
                    l.Y1 = _canvas.ActualHeight - _margin;
                    l.Y2 = _margin;
                    l.Stroke = Brushes.White;
                    l.StrokeThickness = 3;

                    _canvas.Children.Add(l);
                }
            }
        }
        private class Chart
        {
            private CardinalSplineShape _shape;
            private AxisCollection _axises;
            private Dot[] _dots;
            private Point[] _points;
            private Point[] _pointsScreen;
            private Canvas _canvas;
            private double _margin;
            private int _axisY_index;
            private Color _color;
            private double _thickness;
            private bool _isDashed;
            private System.Drawing.Bitmap _bmp;

            public string _name = "";

            public Chart(Canvas canvas, double margin, int axisY_index)
            {
                _canvas = canvas;
                _margin = margin;
                _axisY_index = axisY_index;
                _color = Colors.White;
                _thickness = 1;
            }
            public Chart(Canvas canvas, double margin, int axisY_index, Color color, double thickness, bool isDashed)
            {
                _canvas = canvas;
                _margin = margin;
                _axisY_index = axisY_index;
                _color = color;
                _thickness = thickness;
                _isDashed = isDashed;
            }
            public Chart(Canvas canvas, double margin, int axisY_index, Color color, double thickness, bool isDashed, string name)
            {
                _canvas = canvas;
                _margin = margin;
                _axisY_index = axisY_index;
                _color = color;
                _thickness = thickness;
                _isDashed = isDashed;
                _name = name;
            }
            public void AddPoint(Point point)
            {
                Dot d = new Dot();
                d.Center = point;
                Array.Resize(ref _dots, _dots.Length + 1);
                _dots[_dots.Length - 1] = d;
            }
            public void AddPoints(Point[] points, AxisCollection axisArray)
            {
                _points = new Point[points.Length];
                _points = points;
                _axises = axisArray;
                Update(axisArray);
            }
            public void Update(AxisCollection axisArray)
            {
                _axises = axisArray;
                PointCollection pc = new PointCollection();
                _pointsScreen = new Point[_points.Length];
                _dots = new Dot[_points.Length];
                for (int i = 0; i < _points.Length; i++)
                {
                    _pointsScreen[i] = new Point(_axises.TranslateCoord(0, _points[i].X), _axises.TranslateCoord(_axisY_index, _points[i].Y));
                    _dots[i] = new Dot();
                    _dots[i].Center = _pointsScreen[i];
                    _dots[i].Fill = _isDashed ? Brushes.Transparent : new SolidColorBrush(_color);
                    pc.Add(_pointsScreen[i]);
                }
                _shape = new CardinalSplineShape(_color, _thickness, _isDashed);

                _shape.Points = pc;

            }
            public void Draw()
            {
                if ((_dots == null) || (_dots.Length < 2)) return;

                for (int i = 0; i < _dots.Length; i++)
                {
                    _canvas.Children.Add(_dots[i]);
                }
                _canvas.Children.Add(_shape);


                Point p0 = new Point(50, 50);
                Point p1 = new Point(250, 250);
                Point p2 = new Point(450, 150);
                Point p3 = new Point(550, 50);

                Line l0 = new Line();
                l0.X1 = p0.X;
                l0.Y1 = p0.Y;
                l0.X2 = p1.X;
                l0.Y2 = p1.Y;
                l0.Stroke = Brushes.Blue;
                Line l1 = new Line();
                l1.X1 = p1.X;
                l1.Y1 = p1.Y;
                l1.X2 = p2.X;
                l1.Y2 = p2.Y;
                l1.Stroke = Brushes.Blue;
                Line l2 = new Line();
                l2.X1 = p2.X;
                l2.Y1 = p2.Y;
                l2.X2 = p3.X;
                l2.Y2 = p3.Y;
                l2.Stroke = Brushes.Blue;


                QuadraticBezierSegment q = new QuadraticBezierSegment(p1,p2,true);
                PathFigure f = new PathFigure(p0, new[] { q }, false);
                PathGeometry g = new PathGeometry(new PathFigure[] { f });
                System.Windows.Shapes.Path p = new System.Windows.Shapes.Path();
                p.Data = g;
                p.Stroke = Brushes.Red;

                _canvas.Children.Add(p);
                _canvas.Children.Add(l0);
                _canvas.Children.Add(l1);
                _canvas.Children.Add(l2);
            }

            


            public void DrawSplineOnly()
            {
                //if ((_dots == null) || (_dots.Length < 2)) return;

                //for (int i = 0; i < _dots.Length; i++)
                //{
                //    _canvas.Children.Add(_dots[i]);
                //}
                _canvas.Children.Add(_shape);
            }
            public void Clear()
            {
                if ((_shape == null) || (_dots == null)) return;
                _canvas.Children.Remove(_shape);
                for (int i = 0; i < _dots.Length; i++)
                    if (_dots[i] != null) _canvas.Children.Remove(_dots[i]);
            }
            public void Delete()
            {
                Clear();
                _dots = new Dot[0];
                _shape = new CardinalSplineShape();
            }
            public Point GetMaximum()
            {
                _bmp = _CreateBitmap();
                Point result = new Point();

                System.Drawing.Color color;
                System.Drawing.Color compare = System.Drawing.Color.FromArgb(_color.A, _color.R, _color.G, _color.B);
                string str = "";
                Point p = new Point();

                for (int y = 0; y < _bmp.Height; y++)
                    for (int x = 0; x < _bmp.Width; x++)
                    {
                        color = _bmp.GetPixel(x, y);
                        if (color == compare)
                        {
                            str = color.ToString();
                            p.X = x;
                            p.Y = y;
                            goto Found;
                        }

                    }
            Found:
                result.X = _axises.RetranslateCoord(0, p.X);
                result.Y = _axises.RetranslateCoord(_axisY_index, p.Y);

                return result;
            }
            public double GetValueY(int x)
            {
                _bmp = _CreateBitmap();
                Point result = new Point();
                int virt_x = (int)_axises.TranslateCoord(0, x);

                System.Drawing.Color color;
                System.Drawing.Color compare = System.Drawing.Color.FromArgb(_color.A, _color.R, _color.G, _color.B);
                string str = "";
                Point p = new Point();

                for (int y = 0; y < _bmp.Height; y++)
                {
                    color = _bmp.GetPixel(virt_x, y);
                    if (color == compare)
                    {
                        str = color.ToString();
                        p.X = virt_x;
                        p.Y = y;
                        goto Found;
                    }

                }
            Found:
                result.Y = _axises.RetranslateCoord(_axisY_index, p.Y);

                return result.Y;
            }
            public double GetValueX(int y)
            {
                _bmp = _CreateBitmap();
                Point result = new Point();
                int virt_y = (int)_axises.TranslateCoord(_axisY_index, y);

                System.Drawing.Color color;
                System.Drawing.Color compare = System.Drawing.Color.FromArgb(_color.A, _color.R, _color.G, _color.B);
                string str = "";
                Point p = new Point();

                for (int x = 0; x < _bmp.Width; x++)
                {
                    color = _bmp.GetPixel(x, virt_y);
                    if (color == compare)
                    {
                        str = color.ToString();
                        p.X = x;
                        p.Y = virt_y;
                        goto Found;
                    }

                }
            Found:
                result.X = _axises.RetranslateCoord(0, p.X);

                return result.X;
            }

            private System.Drawing.Bitmap _CreateBitmap()
            {
                System.Drawing.Bitmap result;
                UIElement[] collection = new UIElement[_canvas.Children.Count];
                for (int i = 0; i < collection.Length; i++)
                {
                    var el = _canvas.Children[i];
                    collection[i] = el;
                }


                if (_isDashed) { _shape.StrokeDashArray = new DoubleCollection() { }; }
                _shape.StrokeThickness = 3;
                _canvas.Children.Clear();
                _canvas.Children.Add(_shape);
                _canvas.UpdateLayout();
                result = _ConvertToBitmapSource(_canvas);
                _canvas.Children.Clear();
                if (_isDashed) { _shape.StrokeDashArray = new DoubleCollection() { 5, 2 }; }
                _shape.StrokeThickness = _thickness;

                foreach (UIElement el in collection)
                    _canvas.Children.Add(el);

                return result;
            }
            private System.Drawing.Bitmap _ConvertToBitmapSource(UIElement element)
            {
                var target = new RenderTargetBitmap((int)(element.RenderSize.Width), (int)(element.RenderSize.Height), 96, 96, PixelFormats.Pbgra32);
                target.Render(element);

                var encoder = new PngBitmapEncoder();
                var outputFrame = BitmapFrame.Create(target);
                encoder.Frames.Add(outputFrame);
                System.IO.Stream stream = new System.IO.MemoryStream();
                // -- Сохранение битмапа в поток.
                encoder.Save(stream);
                // -- или в файл
                //using (FileStream file = File.OpenWrite(_name + "TestImage.png"))
                //{
                //    encoder.Save(file);
                //}

                return new System.Drawing.Bitmap(stream);
            }
        }
        private class ChartCollection
        {
            private Chart[] _chartArray;
            private Canvas _canvas;
            private double _margin;
            public int Count
            {
                get { return _chartArray.Length; }
                set { }
            }

            public ChartCollection(Canvas canvas, double margin)
            {
                _canvas = canvas;
                _margin = margin;
                _chartArray = new Chart[0];
            }
            public void Add(Point[] points, AxisCollection axisArray, int axisY_index)
            {
                Add(points, axisArray, axisY_index, Colors.White, 1, false);
            }
            public void Add(Point[] points, AxisCollection axisArray, int axisY_index, Color color, double thicknes, bool isDashed)
            {
                Chart c = new Chart(_canvas, _margin, axisY_index, color, thicknes, isDashed);
                c.AddPoints(points, axisArray);
                xFunctions.AddToArray<Chart>(ref _chartArray, c);
            }
            public void Add(Point[] points, AxisCollection axisArray, int axisY_index, Color color, double thicknes, bool isDashed, string name)
            {
                Chart c = new Chart(_canvas, _margin, axisY_index, color, thicknes, isDashed, name);
                Array.Resize(ref _chartArray, _chartArray.Length + 1);
                c.AddPoints(points, axisArray);
                _chartArray[_chartArray.Length - 1] = c;
            }
            public void Update(AxisCollection axisArray)
            {
                for (int i = 0; i < _chartArray.Length; i++)
                    _chartArray[i].Update(axisArray);
            }
            public void Draw()
            {
                for (int i = 0; i < _chartArray.Length; i++)
                    _chartArray[i].Draw();
            }
            public void Clear()
            {
                _chartArray = new Chart[0];
                _canvas.Children.Clear();
                //for (int i = 0; i < _chartArray.Length; i++)
                //    _chartArray[i].Clear();
            }
            public void Delete(string name)
            {
                if (_chartArray.Length == 0) return;
                _chartArray = _chartArray.Where(a => a._name != name).ToArray();
            }
            public Point GetMaximum(string name)
            {
                return _chartArray.First(n => n._name == name).GetMaximum();
            }
            public double GetValueY(string name, int x)
            {
                return _chartArray.First(n => n._name == name).GetValueY(x);
            }
            public double GetValueX(string name, int y)
            {
                return _chartArray.First(n => n._name == name).GetValueX(y);
            }
        }
        #endregion
    }
}
