using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xLibrary
{
    public class xAxis
    {
        public enum AxisName { Flow, Lift, Power, Eff };

        private int[] _dividers = new int[] { 4, 5, 6, 8, 10 };
        private int _divisions = 0;
        private float _max_value = 0;
        private string _dot = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        private float _length = 1;
        private AxisName _name;

        public int Divisions
        { get { return _divisions; } }
        public float PerDivision
        { get { return _max_value / _divisions; } }
        public float MaxValue
        { get { return _max_value; } }
        public float Length
        { set { _length = value; } }
        public AxisName Name
        { get { return _name; } }

        public xAxis(float max_value, AxisName name, int prescision, [System.Runtime.InteropServices.Optional] int divisions)
        {
            _name = name;
            _max_value = (float)Math.Round(max_value, prescision);
            _divisions = divisions;
            if (_divisions > 0) _dividers = new int[] { divisions };
            Calculate();
        }
        public float Translate_ToScreen(float real_value)
        { return real_value * _length / _max_value; }
        public float Translate_ToReal(float screen_value)
        {
            float coef = _length / _max_value;
            float result = screen_value / coef;
            return result;
        }

        private void Calculate()
        {
            #region ТРАНСФОРМАЦИЯ ЧИСЛА В СТРОКУ ДЛЯ ВЫЧИСЛЕНИЙ
            // Округляю до заданного кол-ва знаков после запятой (prescision) и преобр.в строку 
            string str_max_value = _max_value.ToString();
            // Получаю индекс разделителя дробной части
            int index_dot = str_max_value.IndexOf(_dot);
            // Удаляю разделитель дробной части
            str_max_value = str_max_value.Replace(_dot, "");
            // Получаю индекс первой значимой (не нулевой) цифры (далее - ПЗЦ)
            int index_first_nonzero = str_max_value.IndexOf(str_max_value.First(c => c != '0'));

            string temp_string = "";
            bool b_only_one_digit = (index_first_nonzero + 1 == str_max_value.Length);
            bool b_only_two_digit = (index_first_nonzero + 2 == str_max_value.Length);
            int length_difference = 0;
            int length_difference_corrector = 0;

            // Если ПЗЦ последний в строке, то.. 
            if (b_only_one_digit) temp_string = str_max_value;
            // Если ПЗЦ не последний, то..
            else
            {
                // если ПЗЦ >= минимального делителя, то беру только эту цифру
                if (int.Parse("" + str_max_value[index_first_nonzero]) >= _dividers[0])
                {
                    temp_string = str_max_value.Substring(0, index_first_nonzero + 1);
                    b_only_two_digit = false;
                }
                // если ПЗЦ < минимального делителя, то беру эту и следующую цифру
                else
                    temp_string = str_max_value.Substring(0, index_first_nonzero + 2);
            }
            #endregion

            #region ВЫЧИСЛЕНИЕ
            // Преобр.в целочисленное
            int temp_int = int.Parse(temp_string);
            int index = -1;
            // Если только одна значимая цифра
            if (b_only_one_digit)
            {
                // Если кол-во делений задано заранее
                if (_dividers.Length == 1)
                {
                    // Добавляю ещё одну значимую цифру
                    int t = temp_int * 10;
                    do
                    {
                        // Проверяю на кратность
                        index = GetDividerIndex(t);
                        // если не кратно, увеличиваю на 1 и проверяю опять
                        if (index < 0) t++;
                    }
                    while (index < 0);
                    // Запоминаю делитель
                    _divisions = _dividers[index];
                    // Запоминаю полученное число
                    temp_int = t;
                    // Инкремирую корректор длины, потому что длинна числа была увеличена
                    length_difference_corrector++;
                }
                else
                {
                    // Получаю индекс наибольшего делителя, остаток от деления на который даёт ноль
                    index = GetDividerIndex(temp_int);
                    // Если делитель не найден, принимаю число за делитель
                    if (index < 0) _divisions = temp_int;
                    // Если делитель найден, то используем его
                    else _divisions = _dividers[index];
                }
            }
            // Если только две значимых цифры
            if (b_only_two_digit)
            {
                do
                {
                    // Получаю индекс наибольшего делителя, остаток от деления на который даёт ноль
                    index = GetDividerIndex(temp_int);
                    // Если делитель не найден, увеличиваю число на 1 и опять ищу подходящий делитель
                    if (index < 0) temp_int++;
                }
                while (index < 0);

                _divisions = _dividers[index];
            }
            // Если значимых цифр больше двух
            if (!b_only_one_digit && !b_only_two_digit)
            {
                do
                {
                    // Увеличиваю число на 1
                    temp_int++;
                    // Получаю индекс наибольшего делителя, остаток от деления на который даёт ноль
                    index = GetDividerIndex(temp_int);
                }
                // Если делитель не найден опять ищу подходящий делитель
                while (index < 0);

                _divisions = _dividers[index];
            }
            #endregion

            #region ОБРАТНАЯ ТРАНСФОРМАЦИЯ СТРОКИ В ЧИСЛО
            // Проверяю увеличилась ли длинна числа (учитывая корректор)
            length_difference = temp_int.ToString().Length - int.Parse(temp_string).ToString().Length - length_difference_corrector;
            // Если индекс ПЗЦ > 0 (1 > число > 0), то добавляю нули в начало строки
            if (index_first_nonzero > 0) temp_string = temp_int.ToString().PadLeft(temp_string.Length + length_difference_corrector).Replace(' ', '0');
            // в противном случае, добавляю нули в конец строки
            else temp_string = temp_int.ToString().PadRight(temp_string.Length + length_difference).Replace(' ', '0');
            // Если индекс точки > 0 (число имеет дробную часть)
            if (index_dot > 0)
            {
                // На всякий-який, добавляю нули в конец строки, чтоб длина строки числа была больше чем индекс точки + 2
                if (temp_string.Length < index_dot + 2) temp_string = temp_string.PadRight(index_dot + 2).Replace(' ', '0');
                // Если индекс ПЗЦ > 0, вставляю точку на её место
                if (index_first_nonzero > 0) temp_string = temp_string.Insert(index_dot, _dot);
                // если нет, вставляю точку с учётом разницы длин исходного и полученного чисел
                else temp_string = temp_string.Insert(index_dot + length_difference + length_difference_corrector, _dot);
            }
            else temp_string = temp_int.ToString().PadRight(str_max_value.Length + length_difference).Replace(' ', '0');
            // Преобр.строку в число
            _max_value = float.Parse(temp_string);
            #endregion
        }
        private float[] GetDivisionReminders(int input)
        {
            float[] result = new float[_dividers.Length];
            // Получаю остатки от деления для каждого делителя
            for (int i = 0; i < result.Length; i++)
                result[i] = input % _dividers[i];
            return result;
        }
        private int GetDividerIndex(int input)
        {
            int result = -1;
            // Получаю остатки от деления для каждого делителя
            float[] temp = GetDivisionReminders(input);
            // Получаю индекс наибольшего делителя, остаток от деления на который даёт ноль
            result = Array.LastIndexOf(temp, Array.Find(temp, v => v == 0));

            return result;
        }
    }
}
