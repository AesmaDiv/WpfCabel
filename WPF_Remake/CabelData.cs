using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using xLibrary;

namespace WPF_Try
{
    public class CabelData
    {
        #region ОБЪЯВЛЕНИЯ

        private xAccess.DataValue[] _dataValues;
        private Sortament _sortament = null;
        private float[] _test_R = new float[3];
        private HipotData _hipot_temp = new HipotData();
        private HipotData[] _hipot_data = new HipotData[3];
        private StructureCabel _structureCabel_data = new StructureCabel();
        
        public enum HipotFase { A, B, C, None };
        public enum ROhmFase { AB, BC, CA, None };

        public int ID = -1;
        public DateTime DateTime;

        #endregion

        #region СЕРИАЛИЗУЕМЫЕ ГРУППЫ ДАННЫХ
        [Serializable]
        public class HipotData
        {
            public int[] Time       = new int[0];
            public float[] Voltage  = new float[0];
            public float[] Current  = new float[0];
        }
        
        [Serializable]
        public class StructureCabel
        {
            public StructureCabelRow[] Inserts = new StructureCabelRow[6];
            public StructureCabelRow Extender = new StructureCabelRow();
            public StructureCabel()
            {
                for(int i = 0; i < 6; i++)
                {
                    Inserts[i] = new StructureCabelRow();
                }
            }
            [Serializable]
            public class StructureCabelRow
            {
                public string[] Columns = new string[7];
                public StructureCabelRow()
                {
                    for (int i = 0; i < 7; i++)
                        Columns[i] = "";
                }
            }
        }

        public class Sortament
        {
            public readonly int Num;
            public readonly float Square;
            public string Name
            { get { return Num.ToString() + " - " + Square.ToString() + " мм²"; } }

            public Sortament(int num, float square)
            {
                this.Num = num;
                this.Square = square;
            }
        }
        #endregion

        #region КОНСТРУКТОРЫ

        public CabelData()
        {
            string[] names = Vars.AccessColumnNames.Replace(" ", "").Split(',');
            _dataValues = new xAccess.DataValue[names.Length];

            for (int i = 0; i < _dataValues.Length; i++)
                _dataValues[i] = new xAccess.DataValue(names[i], "");

            for (int i = 0; i < 3; i++)
                _hipot_data[i] = new HipotData();

            _structureCabel_data = new StructureCabel();

            ID = -1;
            DateTime = DateTime.Now;
            SetItemValue("DateTime", DateTime.ToString());
        }

        public CabelData(int ID)
        {
            _dataValues = Vars.Access.GetRecord(ID);

            this.ID = ID;
            DateTime = DateTime.Parse(_dataValues[1].Value);
            GetSortament();

            ParsePoints();
        }

        #endregion

        #region ИНФОРМАЦИЯ О КАБЕЛЕ

        public string Order
        {
            get { return GetItemValue("Order"); }
            set { SetItemValue("Order", value); }
        }
        public string Application
        {
            get { return GetItemValue("Application"); }
            set { SetItemValue("Application", value); }
        }
        public string Plant
        {
            get { return GetItemValue("Plant"); }
            set { SetItemValue("Plant", value); }
        }
        public string Serial
        {
            get { return GetItemValue("Serial"); }
            set { SetItemValue("Serial", value); }
        }
        public string Customer
        {
            get { return GetItemValue("Customer"); }
            set { SetItemValue("Customer", value); }
        }
        public string Field
        {
            get { return GetItemValue("Field"); }
            set { SetItemValue("Field", value); }
        }
        public string Lease
        {
            get { return GetItemValue("Lease"); }
            set { SetItemValue("Lease", value); }
        }
        public string Well
        {
            get { return GetItemValue("Well"); }
            set { SetItemValue("Well", value); }
        }
        public string CabelType
        {
            get { return GetItemValue("CabelType"); }
            set { SetItemValue("CabelType", value); }
        }
        public string CabelSort
        {
            get { return GetItemValue("CabelSort"); }
            set { SetItemValue("CabelSort", value); }
        }
        public string Length
        {
            get { return GetItemValue("Length"); }
            set { SetItemValue("Length", value); }
        }
        public string DaysRun
        {
            get { return GetItemValue("DaysRun"); }
            set { SetItemValue("DaysRun", value); }
        }
        public string Line
        {
            get { return GetItemValue("Line"); }
            set { SetItemValue("Line", value); }
        }
        public string Extention
        {
            get { return GetItemValue("Extention"); }
            set { SetItemValue("Extention", value); }
        }
        public string Temperature
        {
            get { return GetItemValue("Temperature"); }
            set { SetItemValue("Temperature", value); }
        }
        public string Comments
        {
            get { return GetItemValue("Comments"); }
            set { SetItemValue("Comments", value); }
        }
        public float LengthCoeff
        { get { return 1000f / xLibrary.xFunctions.String_ToFloat(Length); } }
        public bool IsStructureDataPresent
        {
            get
            {
                if (_structureCabel_data == null) return false;
                if (_structureCabel_data.Inserts == null) return false;

                if ((_structureCabel_data.Inserts[0].Columns[0] != "") ||
                   (_structureCabel_data.Inserts[1].Columns[0] != "") ||
                   (_structureCabel_data.Extender.Columns[0] != "")) return true;
                            
                return false;
            }
        }


        public bool CheckNecessaryFields()
        {
            string txt = "";

            txt += (CabelType != "") ? "" : "\n    Тип Кабеля";
            txt += (CabelSort != null) ? "" : "\n    Сортамент";
            txt += ((Length != "") && (Length != "0") ? "" : "\n    Длина кабеля");

            bool result = txt == "";
            if (!result) MainFunctions.ShowMessage("ВНИМАНИЕ", "Не заполнены необходимые поля:" + txt, false);

            return result;
        }
        private Sortament GetSortament()
        {
            string str = GetItemValue("CabelSort");
            if (str == "") return null;

            string[] s = str.Replace(" ", "").Replace("мм²", "").Split('-');
            if (s.Length != 2) return null;

            int num = 0;
            float square = 0f;

            if (!int.TryParse(s[0], out num)) return null;
            if (!float.TryParse(s[1], out square)) return null;

            _sortament = new Sortament(num, square);

            return _sortament;
        }
        private void SetSortament(Sortament sortament)
        {
            _sortament = sortament;
            SetItemValue("CabelSort", _sortament == null ? "" : _sortament.Name);
        }

        public void SetItemValue(string name, string value)
        {
            Array.Find(_dataValues, item => item.Name == name).Value = value;
        }
        public string GetItemValue(string name)
        {
            xAccess.DataValue dv = Array.Find(_dataValues, item => item.Name == name);
            return dv == null ? "" : dv.Value == "0" ? "" : dv.Value;
        }

        #endregion

        #region РАБОТА С ДАННЫМИ ОМИЧЕСКОГО СОПРОТИВЛЕНИЯ

        // ???????
        private bool ParsePoints()
        {
            try
            {
                float[] rohm_results = xFunctions.ParseString(GetItemValue("Test_ROhm"), ";");
                _test_R = rohm_results.Length == 3 ? rohm_results : _test_R;

                for (int i = 0; i < 3; i++)
                    HipotData_Deserialize((HipotFase)i);

                StructureCabel_Deserialize();

                return true;
            }
            catch (Exception ex) { return false; }
        }
        private bool UnParsePoints()
        {
            try { SetItemValue("Test_ROhm", xLibrary.xFunctions.ArrayToString(_test_R, ";")); }
            catch (Exception ex) { return false; }
            return true;
        }
        public xAccess.DataValue[] GetAllDataValues()
        {
            UnParsePoints();
            return _dataValues;
        }

        public void ROhm_AddValue(float value, ROhmFase fase)
        {
            if (fase == ROhmFase.None) return;
            _test_R[(int)fase] = value;

            SetItemValue("Test_ROhm", xFunctions.ArrayToString<float>(_test_R, ";"));
        }          
        private float[] ROhm_CalculateFases()
        {
            float[] result = new float[3];
            float ab = _test_R[0], bc = _test_R[1], ca = _test_R[2];

            result[0] = (ab - bc + ca) / 2.0f;
            result[1] = bc - ca + result[0];
            result[2] = ca - result[0];

            return result;
        }
        public void ROhm_FillTable(ref xCustomTable table, [Optional] bool default_values)
        {
            if (default_values)
            {
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 5; j++)
                        table.SetCellValue(i, j, "");

                return;
            }

            float temperature = 23;
            //if (float.TryParse(Temperature, out temperature)) return;

            float length = 1000;
            if (!float.TryParse(Length, out length)) return;

            float square = 1;
            try
            {
                string sortament = GetItemValue("CabelSort").Replace(" ","").Replace("мм²","");
                string[] temp = sortament.Split('-');
                if (temp.Length < 2) return;
                if (!float.TryParse(temp[1], out square)) return;
            }
            catch(Exception ex) { return; }
            // Ri
            float ri = ((temperature - 20f) * 0.0039f + 1) * 0.0175f * length / square;
            float ri1k = ri * LengthCoeff;

            float[] r_i = new float[] { ri, ri1k, 0, 0 };
            float[] r = ROhm_CalculateFases();
            float[] r_1000 = Array.ConvertAll<float, float>(r, f => f * LengthCoeff);

            float[] r_delta = Array.ConvertAll<float, float>(r, f => Math.Abs(1 - f / ri));

            for (int i = 0; i < 3; i++)
            {
                table.SetCellValue(0, i, r_i[i] == 0 ? "--/--" : r_i[i].ToString("0.0000"));
                table.SetCellValue(i + 1, 0, r[i].ToString("0.0000"));
                table.SetCellValue(i + 1, 1, r_1000[i].ToString("0.0000"));
                table.SetCellValue(i + 1, 2, r_delta[i].ToString("0.00%"));
                table.SetCellValue(i + 1, 3, _test_R[i].ToString("0.0000"));
            }

        }

        #endregion

        #region РАБОТА С ДАННЫМИ ВЫСОКОВОЛЬТНОГО ТЕСТА

        public void  Hipot_AddPoint(HipotFase fase, int time, float value_u, float value_i)
        {
            Hipot_AddToData(fase, time, value_u, value_i);
            Hipot_AddToTable(fase, time, value_u, value_i);
        }
        public void  Hipot_AddToData(HipotFase fase, int time, float value_u, float value_i)
        {
            if (_hipot_data[(int)fase].Time.Contains<int>(time)) return;

            xFunctions.AddToArray<int>(ref _hipot_data[(int)fase].Time, time);
            xFunctions.AddToArray<float>(ref _hipot_data[(int)fase].Voltage, value_u);
            xFunctions.AddToArray<float>(ref _hipot_data[(int)fase].Current, value_i);
        }
        public void  Hipot_AddToTable(HipotFase fase, int time, float value_u, float value_i)
        {
            int index_row = (time / 60);

            float i1000 = value_i * LengthCoeff;
            float r1000 = Hipot_CalculateRiz(value_u, value_i, true);

            Vars.hipot_tables[(int)fase].SetCellValue(index_row, 0, value_u.ToString("0.000"));
            Vars.hipot_tables[(int)fase].SetCellValue(index_row, 1, value_i.ToString("0.000"));
            Vars.hipot_tables[(int)fase].SetCellValue(index_row, 2, i1000.ToString("0.000"));
            Vars.hipot_tables[(int)fase].SetCellValue(index_row, 3, r1000.ToString("0.0"));
        }
        public void  Hipot_RemovePoints(HipotFase fase)
        {
            if (fase == HipotFase.None) return;

            _hipot_data[(int)fase] = new HipotData();

            Hipot_FillTable(fase);
        }
        public float Hipot_CalculateRiz(float value_u, float value_i, bool for_1000)
        {
            if (value_u == 0) return 0;
            float coeff = for_1000 ? LengthCoeff : 1;

            return 1000f * coeff * value_u / value_i;
        }

        public void Hipot_FillTable_Old(HipotFase fase)
        {
            int index_table = (int)fase;
            int index_60 = Array.FindIndex<int>(_hipot_data[index_table].Time, secs => secs == 60);
            int index_max = (index_60 > 0) ?
                              Array.IndexOf(_hipot_data[index_table].Current, _hipot_data[index_table].Current.Max<float>()) :
                              -1;

            int rows = Hipot_RebuildTable(fase);
            Hipot_RebuildChart(rows);

            //Vars.hipot_tables[index_table].RowCount = rows + 1;
            //Vars.hipot_tables_protocol[index_table].RowCount = rows + 1;

            xCustomTable table = Vars.hipot_tables[index_table];
            xCustomTable table_protocol = Vars.hipot_tables_protocol[index_table];

            for (int i = 0; i < rows + 1; i++)
            {
                int index = /*(i == 0) ? index_max :*/ index_60 - 1;
                bool is_present = (index >= 0) && (_hipot_data[index_table].Time.Length > index + i);

                float value_u = is_present ? _hipot_data[index_table].Voltage[index + i] : -1f;
                float value_i = is_present ? _hipot_data[index_table].Current[index + i] : -1f;
                float value_i1000 = is_present ? value_i * LengthCoeff : -1f;
                float value_r1000 = is_present ? Hipot_CalculateRiz(value_u, value_i, true) : -1f;

                table.SetCellValue(i, 0, is_present ? value_u.ToString("0.000") : " - ");
                table.SetCellValue(i, 1, is_present ? value_i.ToString("0.000") : " - ");
                table.SetCellValue(i, 2, is_present ? value_i1000.ToString("0.000") : " - ");
                table.SetCellValue(i, 3, is_present ? value_r1000.ToString("0.0") : " - ");

                table_protocol.SetCellValue(i, 0, is_present ? value_u.ToString("0.000") : " - ");
                table_protocol.SetCellValue(i, 1, is_present ? value_i.ToString("0.000") : " - ");
                table_protocol.SetCellValue(i, 2, is_present ? value_i1000.ToString("0.000") : " - ");
                table_protocol.SetCellValue(i, 3, is_present ? value_r1000.ToString("0.0") : " - ");
            }

            MainFunctions.ShowPI(rows == Vars.TestTime_Long, fase, _hipot_data[index_table]);
        }
        public void  Hipot_FillTable(HipotFase fase)
        {
            int index_table = (int)fase;
            int rows = Hipot_RebuildTable(fase);
            int count = _hipot_data[index_table].Time.Length;
            Hipot_RebuildChart(rows);

            xCustomTable table = Vars.hipot_tables[index_table];
            xCustomTable table_protocol = Vars.hipot_tables_protocol[index_table];

            for (int i = 0; i < rows + 1; i++)
            {
                table.SetCellValue(i, 0, " - ");
                table.SetCellValue(i, 1, " - ");
                table.SetCellValue(i, 2, " - ");
                table.SetCellValue(i, 3, " - ");

                table_protocol.SetCellValue(i, 0, " - ");
                table_protocol.SetCellValue(i, 1, " - ");
                table_protocol.SetCellValue(i, 2, " - ");
                table_protocol.SetCellValue(i, 3, " - ");
            }

            for (int i = 0; i < count; i++)
            {
                int index = _hipot_data[index_table].Time[i] / 60;
                
                float value_u = _hipot_data[index_table].Voltage[i];
                float value_i = _hipot_data[index_table].Current[i];
                float value_i1000 = value_i * LengthCoeff;
                float value_r1000 = Hipot_CalculateRiz(value_u, value_i, true);

                table.SetCellValue(index, 0, value_u.ToString("0.000"));
                table.SetCellValue(index, 1, value_i.ToString("0.000"));
                table.SetCellValue(index, 2, value_i1000.ToString("0.000"));
                table.SetCellValue(index, 3, value_r1000.ToString("0.0"));

                table_protocol.SetCellValue(index, 0, value_u.ToString("0.000"));
                table_protocol.SetCellValue(index, 1, value_i.ToString("0.000"));
                table_protocol.SetCellValue(index, 2, value_i1000.ToString("0.000"));
                table_protocol.SetCellValue(index, 3, value_r1000.ToString("0.0"));
            }

            MainFunctions.ShowPI(rows == Vars.TestTime_Long, fase, _hipot_data[index_table]);
        }
        public int   Hipot_RebuildTable(HipotFase fase)
        {
            int index = (int)fase;
            int rows_count = 0;

            if (_hipot_data[index].Time.Length == 0)
            {
                rows_count = Vars.hipot_params.IsWithPolar ? Vars.TestTime_Long : Vars.TestTime_Short;
                MainFunctions.CreateTable_Hipot(Vars.hipot_tables[index], fase == HipotFase.A, rows_count);
                MainFunctions.CreateTable_Hipot(Vars.hipot_tables_protocol[index], fase == HipotFase.A, rows_count);
            }
            else
            {
                int last_time = _hipot_data[index].Time.Max<int>();
                bool is_with_polar = (last_time / 60) > Vars.TestTime_Short;
                rows_count = is_with_polar ? Vars.TestTime_Long : Vars.TestTime_Short;

                MainFunctions.CreateTable_Hipot(Vars.hipot_tables[index], fase == HipotFase.A, rows_count);
                MainFunctions.CreateTable_Hipot(Vars.hipot_tables_protocol[index], fase == HipotFase.A, rows_count);
            }

            return rows_count;
        }
        public void  Hipot_RebuildChart(int max_time)
        {
            Vars.chart.AxisX_Maximum = max_time;
            Vars.chart.AxisX_Divisions = max_time;
            Vars.chart.RestrictZoneCoords = "0,1," + Vars.chart.AxisX_Maximum + ",500";
        }

        public byte[] Hipot_GetRaw(HipotFase fase)
        {
            return HipotData_Serialize(fase);
        }
        public void BuildGraphics()
        {
            for (int i = 0; i < 3; i++)
            {
                Vars.chart.GetChart(i).RemovePoints();
                float[] currents = Array.ConvertAll<float, float>(_hipot_data[i].Current, c => c / Vars.chart.AxisY_Divider);
                Vars.chart.GetChart(i).AddPoints(CreatePoints(_hipot_data[i].Time, currents));
            }
            Vars.chart.RestrictZoneCoords = "0,1," + Vars.chart.AxisX_Maximum + ",500";
            Vars.chart.Refresh();
        }
        public System.Drawing.PointF[] CreatePoints(int[] time, float[] values_i)
        {
            int count = time.Length;

            System.Drawing.PointF[] p = new System.Drawing.PointF[count];
            for (int i = 0; i < count; i++)
                p[i] = CreatePoint((float)time[i], values_i[i]);

            return p;
        }
        public System.Drawing.PointF CreatePoint(float time, float value_i)
        {
            return new System.Drawing.PointF(time / 60f, value_i * LengthCoeff);
        }
        private float[] CalculateRizArray(float[] array_u, float[] array_i, bool for_1000)
        {
            if (array_i.Length != array_u.Length) return new float[0];

            float[] result = new float[array_i.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = Hipot_CalculateRiz(array_u[i], array_i[i], for_1000);

            return result;
        }

        #region  СЕРИАЛИЗАЦИЯ/ДЕСИРИАЛДИЗАЦИЯ ДАННЫХ ВЫСОКОВОЛЬТНОГО ИСПЫТАНИЯ

        private byte[] HipotData_Serialize(HipotFase fase)
        {
            if (fase == HipotFase.None) return new byte[0];

            return xFunctions.Serialize_Bytes<HipotData>(_hipot_data[(int)fase]);
        }

        private void HipotData_Deserialize(HipotFase fase)
        {
            if (fase == HipotFase.None) return;

            byte[] bytes = Vars.Access.GetRawData(ID, Vars.AccessRawDataNames[(int)fase]);

            _hipot_data[(int)fase] = xFunctions.Deserialize_Bytes<HipotData>(bytes) ?? new HipotData();
        }

        #endregion

        #endregion

        #region РАБОТА С ДАННЫМИ ПО СТРУКТУРЕ КАБЕЛЯ
        public byte[] StructureCabel_GetRaw()
        {
            StructureCabel_SaveTable();
            return xFunctions.Serialize_Bytes<StructureCabel>(_structureCabel_data);
        }       
        private void StructureCabel_SaveTable()
        {
            _structureCabel_data = new StructureCabel();

            for (int r = 0; r < 7; r++)
            {
                StructureCabel.StructureCabelRow row;

                if (r == 6) { row = _structureCabel_data.Extender; r = 7; }
                else row = _structureCabel_data.Inserts[r];

                for (int c = 0; c < 7; c++)
                    row.Columns[c] = Vars.structure_table.GetCellValue(r, c);
            }
        }
        private void StructureCabel_Deserialize()
        {
            byte[] bytes = Vars.Access.GetRawData(ID, "StructureCabel");
            _structureCabel_data = xFunctions.Deserialize_Bytes<StructureCabel>(bytes) ?? new StructureCabel();
        }
        public void StructureCabel_FillTable(ref xCustomTable table)
        {
            int count = _structureCabel_data.Inserts.Length;
            for (int r = 0; r < count; r++)
            {
                for (int c = 0; c < 7; c++)
                    table.SetCellValue(r, c, _structureCabel_data.Inserts[r].Columns[c]);
            }
            for (int c = 0; c < 7; c++)
                table.SetCellValue(7, c, _structureCabel_data.Extender.Columns[c]);            
        }
        #endregion

        #region РАБОТА С ДАННЫМИ ТАБЛИЦЫ КАЧАСТВА ИЗОЛЯЦИИ

        public void FillTable_PI(ref xCustomTable table)
        {
            for (int i = 0; i < 2; i++)
            {
                FillCell_PI(ref table, (HipotFase)i);
            }        
        }

        public void FillCell_PI(ref xCustomTable table, HipotFase hipotFase)
        {
            float PI;
            
            PI = CalculatePI(hipotFase);                         // расчитать индекс поляризации

            if (PI != 0)
            {
                table.SetCellValue((int)hipotFase, 0, Convert.ToString(PI));     // записать в таблицу индекс поляризации
                table.SetCellValue((int)hipotFase, 1, Evaluate_PI(PI));          // записать в таблицу оценку качества изоляции
            }
        }
        
        private float CalculatePI(HipotFase fase)
        {
            string str_R;
            float PI = 0;
            float R60 = 0;
            float R600 = 0;
            
            str_R = Vars.hipot_tables[(int)fase].GetCellValue(1, 3).Trim();
            if (str_R != "-" && str_R != "") R60 = Convert.ToSingle(str_R);

            str_R = Vars.hipot_tables[(int)fase].GetCellValue(2, 3).Trim();
            if (str_R != "-" && str_R != "") R600 = Convert.ToSingle(str_R);
            
            if (R60 != 0) PI = R600 / R60;

            return PI;
        }

        private string Evaluate_PI(float PI)
        {
            string evaluate = "";

            if (PI < 1) { evaluate = "Опасное"; }
            if (PI >= 1 && PI < 2) { evaluate = "Несоответствующее"; }
            if (PI >= 2 && PI < 4) { evaluate = "Хорошее"; }
            if (PI > 4) { evaluate = "Отличное"; }
            
            return evaluate;
        }

        #endregion
    }
}
