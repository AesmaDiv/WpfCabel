using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace xLibrary
{
    public partial class xChartSpline : UserControl
    {
        #region Графики
        public class Chart
        {
            public Pen _pen;

            private Point[] _pointsScreen = new Point[0];
            private PointF[] _pointsReal = new PointF[0];

            public bool isEthalon = false;

            public Pen Pen
            { get { return _pen; } }
            public Point[] ScreenPoints
            { get { return _pointsScreen; } }


            public Chart(PointF[] points, Pen pen, bool is_ethalon)
            {
                _pen = new Pen(pen.Color) { Width = is_ethalon ? 1 : 3 };

                _pointsReal = new PointF[points.Length];
                isEthalon = is_ethalon;

                for (int i = 0; i < _pointsReal.Length; i++)
                    _pointsReal[i] = new PointF((float)points[i].X, (float)points[i].Y);
            }
            public void CalculateScreenPoints(xAxis axis_x, xAxis axis_y)
            {
                _pointsScreen = new Point[_pointsReal.Length];
                for (int i = 0; i < _pointsReal.Length; i++)
                    _pointsScreen[i] = Translate_ToScreen(_pointsReal[i], axis_x, axis_y);
            }
            public Point Translate_ToScreen(PointF point, xAxis axis_x, xAxis axis_y)
            {
                return new Point((int)axis_x.Translate_ToScreen(point.X), (int)axis_y.Translate_ToScreen(point.Y));
            }
        }
        #endregion
        #region ПЕРЕМЕННЫЕ
        private int _pointCount = 7;
        private int _lineCurrentStep = 0;
        private int[] _lineStepScreenCoords = new int[0];

        private Dot[] _dotsLift = new Dot[0];
        private Dot[] _dotsPower = new Dot[0];

        private xAxis[] _axises = new xAxis[4];
        private Chart[] _chartsEthalon = new Chart[3];
        private Chart[] _chartsTest = new Chart[3];
        private CurrentValues _currentValues = new CurrentValues() { Flow = 0, Lift = 0, Power = 0, Eff = 0 };

        private Size _sizeGrid = new Size(),
                     _sizeChart = new Size();

        private Font _font = new Font(new FontFamily("Calibri"), 10, System.Drawing.FontStyle.Regular);// SystemFonts.DefaultFont;

        private System.Windows.Thickness _margin = new System.Windows.Thickness(20, 40, 40, 50);

        private float _coefLift_Upper = 1.05f,
                      _coefLift_Bottom = 0.9f,
                      _coefPower_Upper = 1.1f,
                      _coefPower_Bottom = 0.8f;

        private bool _bCheckGridSize
        {
            get { return (_sizeGrid != null) && (_sizeGrid.Width > 0) && (_sizeGrid.Height > 0); }
        }
        private bool _bCheckChartSize
        {
            get { return (_sizeChart != null) && (_sizeChart.Width > 0) && (_sizeChart.Height > 0); }
        }
        private bool _bCheckAxisPresence
        {
            get { return !(Array.Exists<xAxis>(_axises, a => a == null)); }
        }
        private bool _bCheckEthalonChartsPresence
        {
            get { return Array.FindAll<Chart>(_chartsEthalon, ch => ch == null).Length == 0; }
        }
        private bool _bCheckTestChartsPresence
        {
            get { return Array.FindAll<Chart>(_chartsTest, ch => ch == null).Length == 0; }
        }

        public string Title = "ТЕСТ-1-ЭПУ";
        public bool EthalonOnly = true;
        public bool TestViewMode
        {
            set
            {
                EthalonOnly = value;
                cnvChart.Visibility = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                if (value == true) CalculateLineSteps();
            }
        }
        public int PointsCount
        {
            get { return _pointCount; }
            set { _pointCount = value; }
        }
        public float ZoneMinimum = 0,
                     ZoneNominal = 0,
                     ZoneMaximum = 0;
        public CurrentValues CurrentVals
        {
            get { return _currentValues; }
            set
            {
                _currentValues = value;
                MoveCrosses();
            }
        }
        public struct CurrentValues
        {
            public float Flow;
            public float Lift;
            public float Power;
            public float Eff;
        }
        

        public System.Windows.Thickness xMargin
        {
            get { return _margin; }
            set { _margin = value; }
        }
        #endregion

        public xChartSpline()
        {
            InitializeComponent();
            TestViewMode = false;
        }
        /****************************************************************************************************************/
        public void AddAxis(float max_value, xAxis.AxisName axis_name, int prescision)
        {
            xAxis axis;
            max_value = (float)Math.Round(max_value, 4);
            if ((int)axis_name > 1) axis = new xAxis(max_value, axis_name, prescision, _axises[1].Divisions);
            else axis = new xAxis(max_value, axis_name, prescision);
            _axises[(int)axis_name] = axis;
        }
        public xAxis GetAxis(xAxis.AxisName axis_name)
        {
            return _axises[(int)axis_name];
        }
        /****************************************************************************************************************/
        public void AddChart(PointF[] points, xAxis.AxisName axis_name, Pen pen, bool is_ethalon)
        {
            int indexChart = (int)axis_name - 1;
            if (indexChart < 0) return;
            if (is_ethalon)
                _chartsEthalon[indexChart] = new Chart(points, pen, is_ethalon);
            else
                _chartsTest[indexChart] = new Chart(points, pen, is_ethalon);
        }
        public Chart GetChart(xAxis.AxisName axis_name, bool is_ethalon)
        {
            if (axis_name == xAxis.AxisName.Flow) return null;
            if (is_ethalon) return _chartsEthalon[(int)axis_name - 1];
            else return _chartsTest[(int)axis_name - 1];
        }
        /****************************************************************************************************************/
        public void ClearEthalonCharts()
        {
            _chartsEthalon = new Chart[3];
        }
        public void ClearTestCharts()
        {
            _chartsTest = new Chart[3];
        }
        /****************************************************************************************************************/
        public float GetChartValue(xAxis.AxisName chart_name, float flow_value, bool is_ethalon)
        {
            if (chart_name == xAxis.AxisName.Flow) return 0;
            return GetChartValues(chart_name, new float[] { flow_value }, is_ethalon)[0];
        }
        public float[] GetChartValues(xAxis.AxisName chart_name, float[] flow_values, bool is_ethalon)
        {
            if (chart_name == xAxis.AxisName.Flow) return new float[] { 0f };
            return GetValuesY_Real(is_ethalon ? _chartsEthalon[(int)chart_name - 1] :
                                                _chartsTest[(int)chart_name - 1],
                                                flow_values,
                                                _axises[(int)chart_name]);
        }
        /****************************************************************************************************************/
        public void AddDot(float value_x, float value_y, xAxis.AxisName axis_name, [System.Runtime.InteropServices.Optional] bool b_move_line)
        {
            int screen_x = (int)_axises[0].Translate_ToScreen(value_x);
            int screen_y = 0;
            Dot dot = new Dot();

            switch (axis_name)
            {
                case xAxis.AxisName.Lift:
                    screen_y = _sizeChart.Height - (int)_axises[1].Translate_ToScreen(value_y);
                    dot.Center = new System.Windows.Point(screen_x, screen_y);
                    dot.Fill = System.Windows.Media.Brushes.Blue;
                    dot.Name = "dot_lift";
                    cnvChart.Children.Add(dot);
                    break;

                case xAxis.AxisName.Power:
                    screen_y = _sizeChart.Height - (int)_axises[2].Translate_ToScreen(value_y);
                    dot.Center = new System.Windows.Point(screen_x, screen_y);
                    dot.Fill = System.Windows.Media.Brushes.Red;
                    dot.Name = "dot_power";
                    cnvChart.Children.Add(dot);
                    break;

                default:
                    return;
            }

            if (b_move_line)
            {
                _lineCurrentStep++;
                MoveLine();
            }
        }
        public void RemoveLastDots()
        {
            if (cnvChart.Children.Count < 5) return;
            cnvChart.Children.RemoveRange(cnvChart.Children.Count - 2, 2);
            _lineCurrentStep--;
            MoveLine();
        }
        public void RemoveAllDots()
        {
            if (cnvChart.Children.Count < 5) return;
            cnvChart.Children.RemoveRange(4, cnvChart.Children.Count - 4);
            _lineCurrentStep = 0;
            MoveLine();
        }
        /****************************************************************************************************************/
        public void Redraw()
        {
            if ((!_bCheckChartSize) || (!_bCheckChartSize) || (!_bCheckAxisPresence)) return;
            using (Bitmap bmp = DrawAllToBitmap(_sizeGrid))
            {
                img.Source = null;
                img.Source = xFunctions.ToBitmapSource(bmp);
                bmp.Dispose();
            }

        }
        /****************************************************************************************************************/
        public bool DrawAllToFile(string filename, Size size, [Optional]Color background, [Optional]Pen pen, [Optional] Brush brush)
        {
            try
            {
                if (!_bCheckAxisPresence) return false;
                if ((filename == null) && (filename == "")) return false;

                using (Bitmap bmp = DrawAllToBitmap(size, background, pen, brush))
                {
                    if (bmp != null) bmp.Save(filename);
                    bmp.Dispose();
                }
                return true;
            }
            catch(Exception ex)
            { return false; }
        }
        public Bitmap DrawAllToBitmap(Size size, [Optional]Color background, [Optional]Pen pen, [Optional] Brush brush)
        {
            if (!_bCheckAxisPresence) return null;
            Bitmap result = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Size chart_size = new Size(size.Width - (int)(_margin.Left + _margin.Right), size.Height - (int)(_margin.Top + _margin.Bottom));
                
            DrawGrid(ref result, chart_size, background, pen, brush);
            DrawEthalons(ref result, chart_size);
            if(!EthalonOnly) DrawTestCharts(ref result, chart_size);

            

            return result;
        }
        /****************************************************************************************************************/
        private void DrawEthalons(ref Bitmap bitmap, [Optional] Size size)
        {
            if (size.Width == 0)
            {
                if (!_bCheckChartSize) return;
                size = _sizeChart;                
            }
            if (!_bCheckEthalonChartsPresence) return;
                
            for (int i = 0; i < 3; i++)
                DrawChart(ref bitmap, _chartsEthalon[i], _axises[i + 1]);
        }
        private void DrawTestCharts(ref Bitmap bitmap, [Optional] Size size)
        {
            if (size.Width == 0)
            {
                if (!_bCheckChartSize) return;
                size = _sizeChart;
            }
            if (!_bCheckTestChartsPresence) return;
                
            for (int i = 0; i < 3; i++)
                DrawChart(ref bitmap, _chartsTest[i], _axises[i + 1]);
        }
        /****************************************************************************************************************/
        private void DrawGrid(ref Bitmap bitmap, [Optional] Size chart_size, [Optional]Color background, [Optional]Pen pen, [Optional] Brush brush)
        {
            if (chart_size.Width == 0) chart_size = _sizeChart;
            if (background.Name == "0") background = Color.Black;
            if (pen == null) pen = new Pen(Color.White, 1) { DashPattern = new[] { 5f, 2f } };
            if (brush == null) brush = Brushes.White;

            using (Graphics gfx = Graphics.FromImage(bitmap))
            {

                gfx.Clear(background);

                GraphicsContainer container = gfx.BeginContainer();

                SizeF string_size;

                float division_value = 0;
                string division_string = "";
                int division_coord = 0;
                int division_string_coord_x = 0;
                int division_string_coord_y = 0;

                _axises[0].Length = chart_size.Width;
                _axises[1].Length = chart_size.Height;
                _axises[2].Length = chart_size.Height;
                _axises[3].Length = chart_size.Height;

                gfx.TranslateTransform((float)_margin.Left, (float)_margin.Top);
                gfx.Clear(background);

                #region Рабочая зона и допуски(номинал)
                int coord_minimum = (int)_axises[0].Translate_ToScreen((float)ZoneMinimum);
                int coord_nominal = (int)_axises[0].Translate_ToScreen((float)ZoneNominal);
                int coord_maximum = (int)_axises[0].Translate_ToScreen((float)ZoneMaximum);

                Rectangle rect_yellow_zone = new Rectangle();

                rect_yellow_zone.Width = coord_maximum - coord_minimum;
                rect_yellow_zone.Height = chart_size.Height;
                rect_yellow_zone.Location = new Point(coord_minimum, 0);

                gfx.FillRectangle(Brushes.DarkGray, rect_yellow_zone);

                if ((_chartsEthalon[0] != null) && (_chartsEthalon[1] != null))
                {
                    _chartsEthalon[0].CalculateScreenPoints(_axises[0], _axises[1]);
                    _chartsEthalon[1].CalculateScreenPoints(_axises[0], _axises[2]);

                    Pen pen_range_lift = new Pen(Color.Blue) { DashPattern = new[] { 5f, 5f } };
                    Pen pen_range_power = new Pen(Color.Red) { DashPattern = new[] { 5f, 5f } };

                    Point[] point_range_lift = GetAllowedRangePoints(_chartsEthalon[0].ScreenPoints, _coefLift_Upper, _coefLift_Bottom, rect_yellow_zone);
                    Point[] point_range_power = GetAllowedRangePoints(_chartsEthalon[1].ScreenPoints, _coefPower_Upper, _coefPower_Bottom, rect_yellow_zone);

                    GraphicsContainer range_container = gfx.BeginContainer();

                    gfx.Clip = new Region(rect_yellow_zone);

                    gfx.FillClosedCurve(Brushes.Yellow, point_range_lift);
                    gfx.FillClosedCurve(Brushes.Yellow, point_range_power);

                    gfx.DrawClosedCurve(pen_range_lift, point_range_lift);
                    gfx.DrawClosedCurve(pen_range_power, point_range_power);

                    gfx.EndContainer(range_container);
                }
                gfx.DrawLine(pen, new Point(coord_nominal, 0), new Point(coord_nominal, chart_size.Height));
                #endregion
                #region Ось X
                for (int x = 0; x <= _axises[0].Divisions; x++)
                {
                    pen.DashStyle = (x == 0) || (x == _axises[0].Divisions) ? DashStyle.Solid : DashStyle.Dash;

                    division_value = (float)(x * _axises[0].PerDivision);
                    division_coord = (int)_axises[0].Translate_ToScreen(division_value);

                    gfx.DrawLine(pen,
                                 new Point(division_coord, 0),
                                 new Point(division_coord, chart_size.Height));

                    division_string = division_value.ToString("0");
                    string_size = gfx.MeasureString(division_string, _font);
                    division_string_coord_x = (int)(division_coord - string_size.Width / 2);
                    division_string_coord_y = (int)(chart_size.Height + 2);

                    gfx.DrawString(division_string,
                                   _font,
                                   brush,
                                   new Point(division_string_coord_x, division_string_coord_y));
                }

                #endregion
                #region Ось Y
                for (int y = 0; y <= _axises[1].Divisions; y++)
                {
                    pen.DashStyle = (y == 0) || (y == _axises[1].Divisions) ? DashStyle.Solid : DashStyle.Dash;

                    division_value = (float)(y * _axises[1].PerDivision);
                    division_coord = (int)_axises[1].Translate_ToScreen(division_value);

                    gfx.DrawLine(pen,
                                 new Point(0, division_coord),
                                 new Point(chart_size.Width, division_coord));

                    if ((y == 0) || (y == _axises[1].Divisions)) continue;

                    division_string = division_value.ToString("0");
                    string_size = gfx.MeasureString(division_string, _font);
                    division_string_coord_x = (int)(-string_size.Width - 2);
                    division_string_coord_y = (int)(chart_size.Height - division_coord);

                    gfx.DrawString(division_string,
                                   _font,
                                   brush,
                                   new Point(division_string_coord_x, division_string_coord_y - (int)string_size.Width / 2));

                    division_value = _axises[2].Translate_ToReal(division_coord);
                    division_string = division_value.ToString("0.00");
                    string_size = gfx.MeasureString(division_string, _font);
                    division_string_coord_x = (int)(chart_size.Width + 2);

                    gfx.DrawString(division_string,
                                   _font,
                                   brush,
                                   new Point(division_string_coord_x, division_string_coord_y - (int)string_size.Height));

                    division_value = _axises[3].Translate_ToReal(division_coord);
                    division_string = division_value.ToString("0%");
                    string_size = gfx.MeasureString(division_string, _font);

                    gfx.DrawString(division_string,
                                   _font,
                                   brush,
                                   new Point(division_string_coord_x, division_string_coord_y));
                }


                #endregion
                #region Подписи графика и осей
                Font title_font = new Font(new FontFamily("Calibri"), 12, System.Drawing.FontStyle.Bold);
                string_size = gfx.MeasureString(Title, title_font);
                division_string_coord_x = 0;
                division_string_coord_y = (int)(-string_size.Height - 18);
                gfx.DrawString(Title,
                               title_font,
                               brush,
                               new Point(division_string_coord_x, division_string_coord_y));

                division_string = "Расход (м3/сутки)";
                string_size = gfx.MeasureString(division_string, _font);
                division_string_coord_x = (int)(chart_size.Width / 2 - string_size.Width / 2);
                division_string_coord_y = (int)(chart_size.Height + string_size.Height + 4);
                gfx.DrawString(division_string,
                               _font,
                               brush,
                               new Point(division_string_coord_x, division_string_coord_y));

                division_string = "Напор (метр)";
                string_size = gfx.MeasureString(division_string, _font);
                division_string_coord_x = 0;
                division_string_coord_y = (int)(-string_size.Height - 2);
                gfx.DrawString(division_string,
                               _font,
                               brush,
                               new Point(division_string_coord_x, division_string_coord_y));

                division_string = "Потр.мощность (кВт)";
                string_size = gfx.MeasureString(division_string, _font);
                division_string_coord_x = (int)(chart_size.Width - string_size.Width);
                division_string_coord_y = (int)(-string_size.Height - 2) * 2;
                gfx.DrawString(division_string,
                               _font,
                               brush,
                               new Point(division_string_coord_x, division_string_coord_y));

                division_string = "КПД (%)";
                string_size = gfx.MeasureString(division_string, _font);
                division_string_coord_x = (int)(chart_size.Width - string_size.Width);
                division_string_coord_y = (int)(-string_size.Height - 2);
                gfx.DrawString(division_string,
                               _font,
                               brush,
                               new Point(division_string_coord_x, division_string_coord_y));
                #endregion

                gfx.EndContainer(container);

                gfx.Dispose();
            }
        }
        private void DrawChart(ref Bitmap bitmap, Chart chart, xAxis axis_y, [Optional] Pen pen, [Optional] bool for_calculation)
        {
            try
            {
                if (pen == null) pen = chart.Pen;
                chart.CalculateScreenPoints(_axises[0], axis_y);

                using (Graphics gfx = Graphics.FromImage(bitmap))
                {
                    GraphicsContainer container = gfx.BeginContainer();

                    if (!for_calculation)
                    {
                        gfx.ScaleTransform(1, -1);
                        gfx.TranslateTransform((float)_margin.Left, -bitmap.Height + (float)_margin.Bottom);
                    }

                    if (chart.ScreenPoints.Length > 2)
                            gfx.DrawCurve(pen, chart.ScreenPoints);

                    gfx.EndContainer(container);
                    gfx.Dispose();
                }
            }
            catch (Exception ex) { }
        }
        /****************************************************************************************************************/
        private float[] GetValuesY_Real(Chart chart, float[] real_value_x, xAxis axis_y)
        {
            float[] result = new float[real_value_x.Length];
            int[] screen_values_x = new int[result.Length];

            for (int i = 0; i < screen_values_x.Length; i++)
                screen_values_x[i] = (int)_axises[0].Translate_ToScreen(real_value_x[i]);

            int[] screen_values_y = GetValuesY_Screen(chart, axis_y, screen_values_x);

            for (int i = 0; i < result.Length; i++)
                result[i] = axis_y.Translate_ToReal(screen_values_y[i]);

            return result;
        }
        private int[] GetValuesY_Screen(Chart chart, xAxis axis_y, int[] screen_value_x)
        {
            int[] result = new int[screen_value_x.Length];
            Bitmap bmp = new Bitmap(800, 800, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            DrawChart(ref bmp, chart, axis_y, Pens.White, true);
            for(int i=0; i<result.Length;i++)
                result[i] = GetIntersection(bmp, screen_value_x[i], axis_y.Name);

            bmp.Dispose();

            return result;
        }
        private int GetIntersection(Bitmap bmp, int input_coord, xAxis.AxisName axis_name)
        {
            try
            {
                int range = ((int)axis_name > 0) ? bmp.Height : bmp.Width;
                for (int output_coord = 0; output_coord <= range; output_coord++)
                {
                    Color pixel = ((int)axis_name > 0) ? bmp.GetPixel(input_coord, output_coord) : bmp.GetPixel(output_coord, input_coord);
                    if ((pixel.R == 0) &&
                        (pixel.G == 0) &&
                        (pixel.B == 0))
                        continue;

                    return output_coord;
                }
            }
            catch (Exception ex) { }

            return 0;
        }
        /****************************************************************************************************************/
        private void CalculateLineSteps()
        {
            _lineStepScreenCoords = new int[0];
            float step = (float)_chartsEthalon[0].ScreenPoints[0].X / (float)(_pointCount - 1);
            for (int i = 0; i < _pointCount; i++)
                xFunctions.AddToArray<int>(ref _lineStepScreenCoords, (int)(_chartsEthalon[0].ScreenPoints[0].X - step * i));

            _lineCurrentStep = 0;
            MoveLine();
        }
        private void MoveLine()
        {
            if (_lineCurrentStep <= _pointCount - 1)
            {
                lineFixPoint.X1 = _lineStepScreenCoords[_lineCurrentStep];
                lineFixPoint.X2 = _lineStepScreenCoords[_lineCurrentStep];
            }
        }
        private void MoveCrosses()
        {
            float coord_flow    = _axises[0].Translate_ToScreen(_currentValues.Flow);
            float coord_lift    = _sizeChart.Height - _axises[1].Translate_ToScreen(_currentValues.Lift);
            float coord_power   = _sizeChart.Height - _axises[2].Translate_ToScreen(_currentValues.Power);
            float coord_eff     = _sizeChart.Height - _axises[3].Translate_ToScreen(_currentValues.Eff);

            crossLift.Center    = new System.Windows.Point(coord_flow, coord_lift);
            crossPower.Center   = new System.Windows.Point(coord_flow, coord_power);
            crossEff.Center     = new System.Windows.Point(coord_flow, coord_eff);
        }
        /****************************************************************************************************************/
        private Point[] GetAllowedRangePoints(Point[] points, float coef_upper, float coef_bottom, Rectangle rectangle)
        {
            Point[] result = new Point[points.Length * 2];
            int index = 0;
            float coef = 1;
            for (int i = 0; i < result.Length; i++)
            {
                index = i < points.Length ? i : result.Length - i - 1;
                coef = i < points.Length ? coef_upper : coef_bottom;
                result[i] = new Point(points[index].X, (int)(rectangle.Height - points[index].Y * coef));
            }

            return result;
        }
        /****************************************************************************************************************/
        private void cnvChart_MouseMove(object sender, MouseEventArgs e)
        {
            //System.Windows.Point mouse_screen_point = e.GetPosition(cnvChart);
            
            //int screen_Flow     = (int)mouse_screen_point.X;
            //int screen_Lift     = (int)cnvChart.ActualHeight - GetValueY_Screen(_chartsEthalon[0], _axises[1], screen_Flow);
            //int screen_Power    = (int)cnvChart.ActualHeight - GetValueY_Screen(_chartsEthalon[1], _axises[2], screen_Flow);
            //int screen_Eff      = (int)cnvChart.ActualHeight - GetValueY_Screen(_chartsEthalon[2], _axises[3], screen_Flow);

            //CurrentVals = new CurrentValues()
            //{
            //    Flow = _axises[0].Translate_ToReal(screen_Flow),
            //    Lift = _axises[1].Translate_ToReal(screen_Lift),
            //    Power = _axises[2].Translate_ToReal(screen_Power),
            //    Eff = _axises[3].Translate_ToReal(screen_Eff)
            //};

            

            
        }
        private void xChartSpline_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            bdrBackground.Width = e.NewSize.Width;
            bdrBackground.Height = e.NewSize.Height;

            img.Width = bdrBackground.Width - 5;
            img.Height = bdrBackground.Height - 5;

            _sizeGrid.Width = (int)img.Width;
            _sizeGrid.Height = (int)img.Height;

            _sizeChart.Width = _sizeGrid.Width - (int)(_margin.Left + _margin.Right);
            _sizeChart.Height = _sizeGrid.Height - (int)(_margin.Top + _margin.Bottom);

            cnvChart.Width = _sizeChart.Width;
            cnvChart.Height = _sizeChart.Height;
            cnvChart.Margin = _margin;

            Redraw();
        }

    }
}
