using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace xLibrary
{
    public static class xFunctions
    {
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public static void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex) { obj = null; }
            finally { GC.Collect(); }
        }

        #region КОНВЕРТАЦИЯ
        public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
        {
            if (source == null) return null;

            var hBitmap = source.GetHbitmap();
            var result = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(hBitmap);

            return result;
        }
        public static System.Drawing.Bitmap BitmapSourceToBitmap(BitmapSource srs)
        {
            int width = srs.PixelWidth;
            int height = srs.PixelHeight;
            int stride = width * ((srs.Format.BitsPerPixel + 7) / 8);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(height * stride);
                srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
                using (var btm = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format1bppIndexed, ptr))
                {
                    // Clone the bitmap so that we can dispose it and
                    // release the unmanaged memory at ptr
                    return new System.Drawing.Bitmap(btm);
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
        public static double FormatDouble(double value, int digits_after_dot)
        {
            int temp = (int)(value * Math.Pow(10, digits_after_dot));
            return temp / Math.Pow(10, digits_after_dot);
        }
        public static float[] ConvertArray_DoubleToFloat(double[] input)
        {
            return Array.ConvertAll<double, float>(input, delegate (double d) { return (float)d; });
        }
        public static double[] ConvertArray_FloatToDouble(float[] input)
        {
            return Array.ConvertAll<float, double>(input, delegate (float f) { return (double)f; });
        }
        public static double[] ConvertArray_BytesToDoubles(byte[] bytes, [Optional]int range_count)
        {
            if (bytes == null) return null;

            if (range_count == 0) range_count = bytes.Length / 4;

            if (bytes.Length < (range_count - 1) * 4) return null;

            try
            {
                double[] result = new double[range_count];
                for (int i = 0; i < range_count; i++)
                    result[i] = (double)BitConverter.ToSingle(bytes, i * 4);

                return result;
            }
            catch (Exception ex) { return null; }
        }
        public static float[] ConvertArray_BytesToFloats(byte[] bytes, [Optional]int range_count)
        {
            if (bytes == null) return null;
            if (range_count == 0) range_count = bytes.Length / sizeof(float);
            if (bytes.Length % sizeof(float) > 0) return null;

            if (bytes.Length < (range_count - 1) * sizeof(float)) return null;

            try
            {
                float[] result = new float[range_count];
                Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
                
                return result;
            }
            catch (Exception ex) { return null; }
        }
        public static Single[] ParseBytes_ToSingle(byte[] bytes)
        {
            try
            {
                byte[][] temp = ArrayToMultiDimentionArray<byte>(bytes, sizeof(Single));
                Single[] result = new Single[temp.Length];

                for (int i = 0; i < temp.Length; i++)
                    result[i] = BitConverter.ToSingle(temp[i], 0);

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static string BytesToHex(byte[] bytes)
        {
            string[] temp = Array.ConvertAll<byte, string>(bytes, delegate (byte b) { return b.ToString("X2"); });
            return ArrayToString<string>(temp, " ");
        }
        public static float GetDecimalValue(string text)
        {
            float result = -1;
            Regex r = new Regex(@"\d+[.]\d+");
            Match m = r.Match(text);
            if(m.Success) { result = Convert.ToSingle(m.Value); }
            return result;
        }

        #endregion

        #region МАССИВЫ
        public static T[] CombineArrays<T>(T[] array_1, T[] array_2)
        {
            try
            {
                T[] result = new T[array_1.Length + array_2.Length];
                Array.Copy(array_1, 0, result, 0, array_1.Length);
                Array.Copy(array_2, 0, result, array_1.Length, array_2.Length);

                return result;
            }
            catch (Exception ex) { return new T[] { default(T) }; }
        }
        public static void CombineArrays<T>(ref T[] array_1, T[] array_2)
        {
            int index = array_1.Length;
            Array.Resize<T>(ref array_1, array_1.Length + array_2.Length);
            Array.Copy(array_2, 0, array_1, index, array_2.Length);
        }
        public static void CombineArrays<T>(T[] array_1, T[] array_2, out T[] result)
        {
            result = CombineArrays<T>(array_1, array_2);
        }

        public static void AddToArray<T>(ref T[] array, T value)
        {
            int length = array.Length;
            Array.Resize<T>(ref array, ++length);
            array[--length] = value;
        }
        public static void UpdateArray<T>(ref T[] array, T value, ref int index, int maxLength)
        {
            int length = array.Length;

            if (length == maxLength)
            {
                array[index] = value;
                index = (index == maxLength - 1) ? 0 : index + 1;
            }
            else AddToArray<T>(ref array, value);
        }
        public static T[] TakeFromArray<T>(T[] array, int index, int count)
        {
            if ((index + count) > array.Length) return null;

            T[] result = new T[count];
            for (int i = 0; i < count; i++)
                result[i] = array[i + index];

            return result;
        }
        public static float GetAverage(float[] array)
        {
            float result = 0;

            try
            {
                for (int i = 0; i < array.Length; i++)
                    result += array[i];
                result /= (float)array.Length;
            }
            catch (Exception ex) { }

            return result;
        }
        public static T[][] ArrayToMultiDimentionArray<T>(T[] arr, int numberOfElements)
        {
            try
            {
                int length = arr.Length / numberOfElements;
                T[][] result = new T[length][];

                for (int i = 0; i < length; i++)
                {
                    T[] temp = new T[numberOfElements];
                    Array.Copy(arr, i * numberOfElements, temp, 0, numberOfElements);
                    result[i] = temp;
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static string ArrayToString<T>(T[] elements, string separator)
        {
            string result = "";

            for (int i = 0; i < elements.Length; i++)
                result += elements[i].ToString() + separator;

            return RemoveLast(result, separator);
        }
        public static float[] ParseString(string string_values, string separator)
        {
            try
            {
                if (string_values.Last<char>() == separator.Last<char>()) string_values = RemoveLast(string_values, separator);
                string[] str = string_values.Replace(" ", "").Split(separator.ToArray<char>());

                return Array.ConvertAll<string, float>(str, float.Parse);
            }
            catch (Exception ex) { return new float[0]; }
        }
        public static string RemoveLast(string text, string last)
        {
            return text.Remove(text.LastIndexOf(last));
        }
        #endregion

        public static float String_ToFloat(string str)
        {
            float result = 0;
            float.TryParse(String_FixDecimalSeparator(str), out result);

            return result;
        }
        public static string String_FixDecimalSeparator(string value_string)
        {
            char ch = Convert.ToChar(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            return value_string.Replace(',', ch).Replace('.', ch);
        }
        public static FrameworkElement GetChildFromUIECollection(UIElementCollection parent, string name)
        {
            foreach (FrameworkElement item in parent)
                if (item.Name == name) return item;

            return null;        
        }

        public static bool Serialize<T>(T obj, string file_name)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (Stream fStream = new FileStream(file_name,
                       FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    serializer.Serialize(fStream, obj);
                }
                return true;
            }
            catch(Exception ex) { return false; }
        }

        public static T Deserialize_Bytes<T>(byte[] bytes)
        {
            if (bytes == null) return default(T);
            if (bytes.Length == 0) return default(T);
            try
            {
                T result = default(T);
                IFormatter formatter = new BinaryFormatter();
                using (var ms = new MemoryStream(bytes))
                    try { result = (T)formatter.Deserialize(ms); }
                    catch (ArgumentException ex) { }

                return result;
            }
            catch (Exception ex) { return default(T); }
        }
        public static T Deserialize<T>(string file_name)
        {
            try
            {
                T result;
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (Stream fStream = File.OpenRead(file_name))
                {
                    result = (T)serializer.Deserialize(fStream);
                }
                return result;
            }
            catch (Exception ex) { return default(T); }

        }
        public static byte[] Serialize_Bytes<T>(T obj)
        {
            try
            {
                byte[] result = new byte[0];
                IFormatter formatter = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, obj);
                    result = ms.ToArray();
                }
                return result;
            }
            catch (Exception ex) { return null; }
        }
         
        public static string Serialize_String<T>(T obj)
        {
            try
            {
                string result = "";

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (var sw = new StringWriter())
                {
                    serializer.Serialize(sw, obj);
                    result = sw.ToString();
                }
                return result;
            }
            catch (Exception ex) { return null; }
        }

    }
}
