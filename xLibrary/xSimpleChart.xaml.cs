using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Navigation;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace xLibrary
{
    public partial class xSimpleChart : UserControl
    {
        #region СВОЙСТВА КОМПОНЕНТА

        #region СВОЙСТВА ОБЩИЕ
        [Category("xOptions")]
        public int MarginTop
        {
            get { return _marginTop; }
            set
            {
                _marginTop = value;
                Refresh();
            }
        }
        [Category("xOptions")]
        public int MarginLeft
        {
            get { return _marginLeft; }
            set
            {
                _marginLeft = value;
                Refresh();
            }
        }
        [Category("xOptions")]
        public int MarginRight
        {
            get { return _marginRight; }
            set
            {
                _marginRight = value;
                Refresh();
            }
        }
        [Category("xOptions")]
        public int MarginBottom
        {
            get { return _marginBottom; }
            set
            {
                _marginBottom = value;
                Refresh();
            }
        }
        [Category("xOptions")]
        public Color BackColor
        {
            get { return _background; }
            set { _background = value; }
        }
        [Category("xOptions")]
        public Color PenColor_Grid
        {
            get { return _penGrid.Color; }
            set
            {
                _penGrid.Color = value;
                Refresh();
            }
        }
        [Category("xOptions")]
        public float PenWidth_Grid
        {
            get { return _penGrid.Width; }
            set
            {
                _penGrid.Width = value;
                Refresh();
            }
        }
        [Category("xOptions")]
        public Font Font
        {
            get { return _font; }
            set
            {
                _font = value;
                Refresh();
            }
        }
        [Category("xOptions")]
        public Color FontColorX
        {
            get { return _fontColorX; }
            set
            {
                _fontColorX = value;
                Refresh();
            }
        }
        [Category("xOptions")]
        public Color FontColorY
        {
            get { return _fontColorY; }
            set
            {
                _fontColorY = value;
                Refresh();
            }
        }

        public int FrontChartIndex
        {
            get { return _front_chart; }
            set
            {
                _front_chart = (value < 0) ? 0 : value;
                _front_chart = (value > _charts.Length - 1) ? 0 : value;
            }
        }
        public bool ShowMark
        {
            get { return cross.Visibility == System.Windows.Visibility.Visible; }
            set { cross.Visibility = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden; }
        }
        public PointF MarkPosition
        {
            get { return _pointMark; }
            set { MoveMark(value); }
        }
        [Category("xOptions")]
        public string RestrictZoneCoords
        {
            get { return xFunctions.ArrayToString<float>(_restrictZoneCoords, ","); }
            set
            {
                _restrictZoneCoords = Array.ConvertAll<string, float>(value.Split(','), float.Parse);
                Refresh();
            }
        }

        #endregion

        #region СВОЙСТВА СЕТКИ ГРАФИКА
        public enum AxisName { X, Y };
        [Category("xChart")]
        public bool AxisX_IsLogarithmic
        {
            get { return _bLogX; }
            set
            {
                _bLogX = value;
                RecalculateDivisionsValues(AxisName.X);
                Refresh();
            }
        }
        [Category("xChart")]
        public bool AxisY_IsLogarithmic
        {
            get { return _bLogY; }
            set
            {
                _bLogY = value;
                RecalculateDivisionsValues(AxisName.Y);
                Refresh();
            }
        }
        [Category("xChart")]
        public int AxisX_Divisions
        {
            get { return _divX; }
            set
            {
                _divX = value;
                RecalculateDivisionsValues(AxisName.X);
                Refresh();
            }
        }
        [Category("xChart")]
        public int AxisY_Divisions
        {
            get { return _divY; }
            set
            {
                if (_bLogY) return;
                _divY = value;
                RecalculateDivisionsValues(AxisName.Y);
                Refresh();
            }
        }
        [Category("xChart")]
        public float AxisY_Divider
        {
            get { return _dividerY; }
            set
            {
                _dividerY = value;
                //RecalculateDivisionsValues(AxisName.Y);
                Refresh();
            }
        }

        [Category("xChart")]
        public float AxisX_Minimum
        {
            get { return _minX; }
            set
            {
                _minX = _bLogX ? ((value < 1) ? 1 : value) : value;
                RecalculateDivisionsValues(AxisName.X);
                Refresh();
            }
        }
        [Category("xChart")]
        public float AxisX_Maximum
        {
            get { return _maxX; }
            set
            {
                _maxX = _bLogX ? ((value < 1) ? 1 : value) : value;
                RecalculateDivisionsValues(AxisName.X);
                Refresh();
            }
        }
        [Category("xChart")]
        public float AxisY_Minimum
        {
            get { return _minY; }
            set
            {
                _minY = _bLogY ? ((value < 1) ? 1 : value) : value;
                RecalculateDivisionsValues(AxisName.Y);
                Refresh();
            }
        }
        [Category("xChart")]
        public float AxisY_Maximum
        {
            get { return _maxY; }
            set
            {
                _maxY = _bLogY ? ((value < 1) ? 1 : value) : value;
                RecalculateDivisionsValues(AxisName.Y);
                Refresh();
            }
        }
        #endregion

        #region ВСЕ ПЕРЕМЕННЫЕ
        private int _width;
        private int _height;

        private int _marginTop = 20;
        private int _marginLeft = 20;
        private int _marginRight = 20;
        private int _marginBottom = 20;

        private bool _bLogX = false;
        private bool _bLogY = false;
        private bool _bShowMark = false;
        private bool _bShowZone = false;
        private bool _bIsDashed = true;
        
        private int _divX = 10;
        private int _divY = 10;
        private float _dividerY = 1f;

        private float _minX = 0;
        private float _maxX = 10;

        private float _minY = 0;
        private float _maxY = 10;

        private float[] _divisionValuesX;
        private float[] _divisionValuesY;
        
        private Color _background = Color.Transparent;

        private Pen _penGrid = new Pen(Color.Black, 1f);        

        private Font _font = new Font(System.Drawing.FontFamily.GenericSansSerif, 8, System.Drawing.FontStyle.Regular);
        private Color _fontColorX = Color.Black;
        private Color _fontColorY = Color.Black;
        
        private PointTranslator _translator;
        private Rectangle _drawZone;

        private Chart[] _charts = new Chart[0];
        private int _front_chart = 0;
        private PointF _pointMark;

        private float[] _restrictZoneCoords = { };
        private Brush _restrictZoneBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(127, 255, 255, 0));
        #endregion

        #region СОБЫТИЯ
        public event EventHandler OnEvent;
        public class ChartEventArgs : EventArgs
        {
            public Point screenCoords;
            public PointF chartCoords;
        };
        #endregion

        #endregion

        public xSimpleChart()
        {
            InitializeComponent();
        }
        public void Refresh()
        {
            if (bdrCanvases.ActualWidth == 0) return;
            using (Bitmap bmp = new Bitmap(
                            (int)bdrCanvases.ActualWidth,
                            (int)bdrCanvases.ActualHeight,
                            PixelFormat.Format32bppRgb))
            {
                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    gfx.Clear(_background);
                    RefreshChart(gfx);
                    gfx.Dispose();
                }
                bdrCanvases.Background = new System.Windows.Media.ImageBrush(xFunctions.ToBitmapSource(bmp))
                { TileMode = System.Windows.Media.TileMode.None, Stretch = System.Windows.Media.Stretch.Fill };
                
                bmp.Dispose();
            }

            //cnvMarks.Width = bdrCanvases.ActualWidth - MarginLeft - MarginRight;
            //cnvMarks.Height = bdrCanvases.ActualHeight - MarginTop - MarginBottom;
            cnvMarks.Margin = new System.Windows.Thickness(MarginLeft, MarginTop, MarginRight, MarginBottom);
        }
        

        /********************************************************************************************************/
        public void AddChart(Chart chart)
        {
            xFunctions.AddToArray<Chart>(ref _charts, chart);
        }
        public void AddChart(PointF[] points, Pen pen)
        {
            AddChart((new Chart() { Points = points, Pen = pen }));
        }
        public Chart GetChart(int index)
        {
            if (_charts.Length <= index) return null;
            return _charts[index];
        }
        public void RemoveCharts()
        {
            _charts = new Chart[0];
        }
        /********************************************************************************************************/
        public Bitmap DrawToBitmap(int width, int height)
        {
            Bitmap result = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            using(Graphics gfx = Graphics.FromImage(result))
            {
                gfx.Clear(Color.White);

                float grid_width = _penGrid.Width;               
                Color pen = _penGrid.Color;
                Color fontX = _fontColorX;
                Color fontY = _fontColorY;

                for(int i = 0; i < 3; i++)
                    if(_charts[i] != null)
                        _charts[i].Pen.Width = 2;

                _penGrid.Width = 0.1f;
                _penGrid.Color = Color.Black;
                _fontColorX = Color.Black;
                _fontColorY = Color.Black;
                
                RefreshChart(gfx);

                _penGrid.Width = grid_width;
                _penGrid.Color = pen;
                _fontColorX = fontX;
                _fontColorY = fontY;

                for (int i = 0; i < 3; i++)
                    if (_charts[i] != null)
                        _charts[i].Pen.Width = 3;
            }
            return result;
        }
        /********************************************************************************************************/
        public void RecalculateAxisParams(float maxValuePoints, AxisName axis)
        {
            CalculateFloat(maxValuePoints, axis);
        }
        private void RecalculateDivisionsValues(AxisName axis)
        {
            float[] result = new float[0];
            float minimumValue;
            float maximumValue;
            bool isLogarithmic;
            int count;

            switch (axis)
            {
                default:
                    isLogarithmic = _bLogX;
                    minimumValue = _minX;
                    maximumValue = _maxX;
                    count = _bLogX ? 0 : _divX;
                    break;
                case AxisName.Y:
                    isLogarithmic = _bLogY;
                    minimumValue = _minY;
                    maximumValue = _maxY;
                    count = _bLogY ? 0 : _divY;
                    break;
            }

            try
            {
                if (isLogarithmic)
                {
                    int powerMin = (int)Math.Log10(minimumValue);
                    int powerMax = (int)Math.Log10(maximumValue);
                    result = new float[(powerMax - powerMin) * 9 + 1];
                    count = 0;
                    for (int i = powerMin; i < powerMax; i++)
                        for (int j = 1; j != 10; j++)
                        {
                            result[count] = (float)j * (float)Math.Pow(10, i);
                            count++;
                        }
                    result[result.Length - 1] = maximumValue;
                }
                else
                {
                    result = new float[count + 1];
                    float price = (maximumValue - minimumValue) / (float)count;
                    for (int i = 0; i < count + 1; i++)
                    {
                        result[i] = minimumValue + (float)i * price;
                    }
                }
            }
            catch (Exception ex)
            {
                result = new float[0];
            }

            switch (axis)
            {
                case AxisName.X:
                    _divisionValuesX = result;
                    Size s = System.Windows.Forms.TextRenderer.MeasureText("0", _font);
                    if (_marginBottom < s.Height) _marginBottom = s.Height;
                    break;
                case AxisName.Y:
                    _divisionValuesY = result;
                    int l0 = GetLengthOfMostLongValue(_divisionValuesY) + 10;
                    /*if (marginLeft < l0)*/
                    _marginLeft = l0;
                    break;
            }
        }
        private Point RecalculateOriginPoint()
        {
            Point result = new Point(0, 0);
            _translator = new PointTranslator(this);
            result = _translator.Point_ToScreenCoords(new PointF(_minX, _minY));

            return result;
        }
        private int GetLengthOfMostLongValue(float[] values)
        {
            int result = 0;
            for (int i = 0; i < values.Length; i++)
            {
                int length = System.Windows.Forms.TextRenderer.MeasureText(values[i].ToString(), _font).Width;
                result = result < length ? length : result;
            }
            return result;
        }
        /********************************************************************************************************/
        private void DrawZone(Graphics gfx)
        {
            gfx.ScaleTransform(1, -1);
            try
            {
                if ((_restrictZoneCoords != null) && (_restrictZoneCoords.Length == 4))
                {
                    PointF r0 = new PointF(_restrictZoneCoords[0], _restrictZoneCoords[1]);
                    PointF r1 = new PointF(_restrictZoneCoords[2], _restrictZoneCoords[3]);
                    PointF p0 = _translator.Point_ToScreenCoords(r0);
                    PointF p1 = _translator.Point_ToScreenCoords(r1);
                    gfx.FillRectangle(_restrictZoneBrush, p0.X, p0.Y, p1.X, p1.Y);
                }
            }
            catch (Exception ex) { }
            gfx.ScaleTransform(1, -1);
        }
        private void DrawGrid(Graphics gfx)
        {
            _penGrid.DashStyle = _bIsDashed ? DashStyle.Dash : DashStyle.Solid;
            _penGrid.DashPattern = new float[] { 3, 3};

            DrawZone(gfx);                             
            
            #region Ось X
            #region Линии сетки
            gfx.ScaleTransform(1, -1);
            for (int i = 1; i < _divisionValuesX.Length - 1; i++)
            {
                PointF pMin = _translator.Point_ToScreenCoords(new PointF(_divisionValuesX[i], _minY));
                PointF pMax = _translator.Point_ToScreenCoords(new PointF(_divisionValuesX[i], _maxY));
                gfx.DrawLine(_penGrid, pMin, pMax);
            }
            gfx.ScaleTransform(1, -1);
            #endregion
            #region Подписи делений
            for (int i = 0; i < _divisionValuesX.Length; i++)
            {
                PointF p = _translator.Point_ToScreenCoords(new PointF(_divisionValuesX[i], _minY));

                string str = (_divisionValuesX[i]+1).ToString();
                Size s = System.Windows.Forms.TextRenderer.MeasureText(str, _font);
                float x = p.X - s.Width / 2 + 2;
                float y = -1 * (p.Y - 4);

                gfx.DrawString(str.ToString(), _font, new SolidBrush(_fontColorX), new PointF(x, y));
            }
            #endregion
            #endregion

            #region Ось Y
            #region Логарифмический
            if (_bLogY)
            {
                #region Линии сетки
                gfx.ScaleTransform(1, -1);
                for (int i = 0; i < _divisionValuesY.Length - 1; i++)
                {
                    PointF pMin = _translator.Point_ToScreenCoords(new PointF(_minX, _divisionValuesY[i]));
                    PointF pMax = _translator.Point_ToScreenCoords(new PointF(_maxX, _divisionValuesY[i]));
                    gfx.DrawLine(_penGrid, pMin, pMax);
                }
                gfx.ScaleTransform(1, -1);
                #endregion
                #region Подписи делений
                for (int i = 0; i < _divisionValuesY.Length; i++)
                {
                    double tmp = Math.Log10(_divisionValuesY[i]) % 1;
                    if (tmp > 0) continue;
                    PointF p = _translator.Point_ToScreenCoords(new PointF(0, _divisionValuesY[i]));

                    string str = (_divisionValuesY[i] * _dividerY).ToString();
                    Size s = System.Windows.Forms.TextRenderer.MeasureText(str, _font);
                    float x = -s.Width;
                    float y = -1 * (p.Y + s.Height / 2);

                    gfx.DrawString(str, _font, new SolidBrush(_fontColorX), new PointF(x, y));
                }
                #endregion
            }
            #endregion
            #region Линейный
            else
            {
                #region Линии сетки
                gfx.ScaleTransform(1, -1);
                for (int i = 1; i < _divisionValuesY.Length - 1; i++)
                {
                    PointF pMin = _translator.Point_ToScreenCoords(new PointF(_minX, _divisionValuesY[i]));
                    PointF pMax = _translator.Point_ToScreenCoords(new PointF(_maxX, _divisionValuesY[i]));
                    gfx.DrawLine(_penGrid, pMin, pMax);
                }
                gfx.ScaleTransform(1, -1);
                #endregion
                #region Подписи делений
                for (int i = 0; i < _divisionValuesY.Length; i++)
                {
                    PointF p = _translator.Point_ToScreenCoords(new PointF(_minX, _divisionValuesY[i]));

                    string str = _divisionValuesY[i].ToString();
                    Size s = System.Windows.Forms.TextRenderer.MeasureText(str, _font);
                    float x = p.X - s.Width;
                    float y = -p.Y - s.Height / 2;

                    gfx.DrawString(str.ToString(), _font, new SolidBrush(_fontColorY), new PointF(x, y));
                }
                #endregion
            }
            #endregion
            #endregion

            #region Окантовка области графика
            gfx.ScaleTransform(1, -1);
            Point corner = _translator.Point_ToScreenCoords(new PointF(_minX, _minY));
            Size size = new Size(_width, _height);
            _drawZone = new Rectangle(corner, size);
            gfx.DrawRectangle(new Pen(_penGrid.Color, _penGrid.Width), _drawZone);
            gfx.Clip = new Region(_drawZone);
            gfx.ScaleTransform(1, -1);
            #endregion
        }
        /********************************************************************************************************/
        private Bitmap SaveToImage()
        {
            Bitmap result = new Bitmap((int)bdrCanvases.ActualWidth, (int)bdrCanvases.ActualHeight, PixelFormat.Format24bppRgb);
            //pictureBox.DrawToBitmap(result, new Rectangle(0, 0, result.Width, result.Height));

            return result;
        }
        private Bitmap CropImage(Bitmap source, Rectangle section)
        {
            Bitmap result = new Bitmap(section.Width, section.Height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(result);

            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);

            return result;
        }
        private void RefreshChart(Graphics gfx)
        {
            GraphicsContainer container = gfx.BeginContainer();
            try
            {
                _width = (int)gfx.VisibleClipBounds.Width - _marginLeft - _marginRight;// (int)bdrCanvases.ActualWidth - _marginLeft - _marginRight;
                _height = (int)gfx.VisibleClipBounds.Height - _marginTop - _marginBottom;// (int)bdrCanvases.ActualHeight - _marginTop - _marginBottom;

                Point origin = RecalculateOriginPoint();
                gfx.TranslateTransform(_marginLeft + origin.X, _height + _marginTop + origin.Y);

                DrawGrid(gfx);
                for (int i = 0; i < _charts.Length; i++)
                {
                    if (i == _front_chart) continue;
                    _charts[i].Draw(gfx, _translator);
                }
                _charts[_front_chart].Draw(gfx, _translator);
            }
            catch (Exception ex) { }
            gfx.EndContainer(container);
        }
        private void MoveMark(PointF point)
        {
            _pointMark = point;

            if (_pointMark == null) return;
            _pointMark.X = (_pointMark.X < _minX) ? _minX : _pointMark.X;
            _pointMark.Y = (_pointMark.Y < _minY) ? _minY : _pointMark.Y;

            Point p = _translator.Point_ToScreenCoords(_pointMark);

            cross.Center = new System.Windows.Point(p.X, p.Y);
        }
        /********************************************************************************************************/
        private void OnEventBroadcast(ChartEventArgs e)
        {
            if (OnEvent != null) OnEvent(this, e);
        }
        /********************************************************************************************************/
        private void CalculateFloat(float maxValuePoints, AxisName axis)
        {
            String temp = maxValuePoints.ToString();
            int point_index = -1;
            int first_digit_index = 0;
            int input_length, length_difference;
            int divisions = 1;

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
            divisions = CalculateDivisions(temp);
            length_difference = temp.Length - input_length;

            if (first_digit_index > 0)
                for (int i = 0; i < (first_digit_index - length_difference); i++)
                    temp = "0" + temp;
            if (point_index > 0)
                temp = temp.Insert(point_index, ".");

            switch (axis)
            {
                default:
                    AxisX_Maximum = float.Parse(temp);
                    AxisX_Divisions = divisions;
                    break;
                case AxisName.Y:
                    AxisY_Maximum = float.Parse(temp);
                    AxisY_Divisions = divisions;
                    break;
            }
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
            //divisions = CalculateDivisions(temp);

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
        /********************************************************************************************************/
        public class Chart
        {
            private Pen _penLines = new Pen(Color.Red, 3f);
            private PointF[] _points = new PointF[0];
            private Pen _pen = new Pen(Color.Black, 1);

            public PointF[] Points
            {
                get { return _points; }
                set { _points = value; }
            }
            public Pen Pen
            {
                get { return _pen; }
                set { _pen = value; }
            }

            public void AddPoints(PointF[] points)
            {
                xFunctions.CombineArrays<PointF>(ref _points, points);
            }
            public void AddPoint(PointF point)
            {
                xFunctions.AddToArray<PointF>(ref _points, point);
            }
            public void RemovePoints()
            {
                _points = new PointF[0];
            }
            public void RemovePoint()
            {
                Array.Resize<PointF>(ref _points, _points.Length - 1);
            }

            public void Draw(Graphics gfx, PointTranslator translator)
            {
                gfx.ScaleTransform(1, -1);

                if (_points.Length > 1)
                {
                    Point[] pY = translator.Points_ToScreenCoords(_points);
                    gfx.DrawLines(_pen, pY);
                }
                gfx.ScaleTransform(1, -1);
            }
        }
        public class PointTranslator
        {
            private float _coefficientX = 1;
            private float _coefficientY = 1;
            
            private float _minX = 0;
            private float _minY = 0;
            
            private float _maxX = 0;
            private float _maxY = 0;
            
            private bool _bLogX = false;
            private bool _bLogY = false;

            public PointTranslator(xSimpleChart chart)
            {
                _minX = chart._minX;
                _minY = chart._minY;
                
                _maxX = chart._maxX;
                _maxY = chart._maxY;
                
                _bLogX = chart._bLogX;
                _bLogY = chart._bLogY;

                _coefficientX = (float)chart._width / (_maxX - _minX);

                if (_bLogY) _coefficientY = (float)chart._height / (float)(Math.Log10(_maxY) - Math.Log10(_minY));
                else _coefficientY = (float)chart._height / (_maxY - _minY);
            }
            public Point Point_ToScreenCoords(PointF realCoords)
            {
                Point result = new Point();

                result.X = (int)((realCoords.X - _minX) * _coefficientX);

                if (_bLogY) result.Y = (int)(Math.Log10(realCoords.Y > 0 ? realCoords.Y : 1) * _coefficientY);
                else result.Y = (int)((realCoords.Y - _minY) * _coefficientY);
                
                return result;
            }
            public Point[] Points_ToScreenCoords(PointF[] realCoords)
            {
                Point[] result = new Point[realCoords.Length];

                for (int i = 0; i < result.Length; i++)
                    result[i] = Point_ToScreenCoords(realCoords[i]);

                return result;
            }
            public PointF Point_ToRealCoords(Point screenCoords)
            {
                PointF result = new PointF();

                result.X = _minX + (float)screenCoords.X / _coefficientX;

                if (_bLogY) result.Y = (float)Math.Pow(10, (float)screenCoords.Y / _coefficientY);
                else result.Y = _minY + (float)screenCoords.Y / _coefficientY;

                return result;
            }
            public PointF[] Points_ToRealCoords(Point[] screenCoords)
            {
                PointF[] result = new PointF[screenCoords.Length];
                for (int i = 0; i < result.Length; i++)
                    result[i] = Point_ToRealCoords(screenCoords[i]);

                return result;
            }
        }
        
        private void bdrCanvases_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (bdrCanvases.Width == 0) return;
            Refresh();
        }
    }
}
