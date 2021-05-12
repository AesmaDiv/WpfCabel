using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.OleDb;
using System.Reflection;
using System.Runtime.InteropServices;

namespace xLibrary
{
    public class xAccess
    {
        private OleDbConnection _con;
        private OleDbCommand _cmd;
        private OleDbDataAdapter _adapter;
        private string _path;
        
        public class DataValue
        {
            public string Name;
            public string Value;

            public DataValue() { }
            public DataValue(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }
        public string TableName;
        public string ColumnNames;
        public string[] RowPropertyNames = new string[0];
       
        public xAccess(string path) { _path = path; }
        public bool IsOpen
        {
            get
            {
                if (_con == null) return false;
                return (_con.State == ConnectionState.Open);
            }
        }
        public bool Connect()
        {
            try
            {
                if (IsOpen) return true;

                _con = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _path + ";");
                _con.Open();

                return IsOpen;
            }
            catch (OleDbException ex) { return false; }
        }
        public void Disconnect()
        {
            if (IsOpen) _con.Close();
        }

        public DataTable ReadTable()
        {   
            try
            {
                DataTable result = new DataTable();
                _cmd = new OleDbCommand("SELECT " + ColumnNames + " FROM " + TableName, _con);

                _adapter = new OleDbDataAdapter(_cmd);
                _adapter.Fill(result);

                return result;
            }
            catch (OleDbException ex) { return null; }
        }
        public DataValue[] GetRecord(int id, [Optional]string table)
        {
            if (id < 0) return null;
            if ((table == null) || (table == "")) table = TableName;

            try
            {
                DataValue[] result = new DataValue[0];
                _cmd = new OleDbCommand("Select " + ColumnNames + " From " + table + " Where " + ColumnNames.Split(',')[0] + "=" + id + ";", _con);              
            
                OleDbDataReader reader = _cmd.ExecuteReader();
                result = new DataValue[reader.FieldCount];
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result[i] = new DataValue();
                        result[i].Name = reader.GetName(i);
                        result[i].Value = reader[i].ToString();
                    }
                }
                reader.Close();

                return result;
            }
            catch (OleDbException ex) { return null; }
            catch (Exception ex) { return null; }
        }

        public T[] GetTestListRows<T>([Optional]string[] property_names, [Optional]string table)
        {
            if ((table == null) || (table == "")) table = TableName;
            if (property_names == null) property_names = RowPropertyNames;

            if (IsOpen)
            {
                try
                {
                    T[] result = new T[0];
                    T row;

                    _cmd = new OleDbCommand("Select * From " + table  + ";", _con);
                    OleDbDataReader reader = _cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        row = Activator.CreateInstance<T>();
                        for (int i=0; i<property_names.Length; i++)
                        {
                            PropertyInfo property_info = row.GetType().GetProperty(property_names[i]);
                            if (property_info == null) continue;

                            property_info.SetValue(row, Convert.ChangeType(reader[property_names[i]], property_info.PropertyType), null);
                        }
                        xFunctions.AddToArray<T>(ref result, row);
                    }
                    reader.Close();

                    return result;
                }
                catch (OleDbException ex) { return null; }
                catch (Exception ex) { return null; }
            }
            return null;
        }
        public object GetItemValue(int id, string column, [Optional]string table)
        {
            if ((column == "") || (id < 0)) return null;
            if ((table == null) || (table == "")) table = TableName;

            try
            {
                _cmd = new OleDbCommand("Select " + column + " From " + table + " Where " + ColumnNames.Split(',')[0] + "=" + id + ";", _con);

                OleDbDataReader reader = _cmd.ExecuteReader();
                object result = null;
                while (reader.Read())
                {
                    result = reader[0];
                }
                reader.Close();

                return result;
            }
            catch (OleDbException ex) { return null; }
            catch (Exception ex) { return null; }
        }
        public string[] GetUniqItemsList(string column, [Optional]string table)
        {
            if ((table == null) || (table == "")) table = TableName;

            if (IsOpen)
            {
                try
                {
                    string[] result = new string[0];
                   _cmd = new OleDbCommand("Select Distinct " + column + " From " + table + ";", _con);

                    OleDbDataReader reader = _cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        xFunctions.AddToArray<string>(ref result, reader[0].ToString());
                    }
                    reader.Close();

                    return result;
                }
                catch (OleDbException ex) { return null; }
                catch (Exception ex) { return null; }
            }
            return null;
        }
        public int GetRecordContains(string column, string value, [Optional]string table)
        {
            if ((column == "") || (value == "")) return -1;
            if ((table == null) || (table == "")) table = TableName;

            try
            {
                _cmd = new OleDbCommand("Select " + ColumnNames.Split(',')[0] + " From " + table + " Where " + column + "='" + value + "';", _con);
            
                int result = -1;
                OleDbDataReader reader = _cmd.ExecuteReader();
                while (reader.Read())
                {
                    result = Convert.ToInt16(reader[0].ToString());
                }
                reader.Close();

                return result;
            }
            catch (OleDbException ex) { return -1; }
            catch (Exception ex) { return -2; }
        }
        public byte[] GetRawData(int id, [Optional]string column, [Optional]string table)
        {
            if (id < 0) return null;
            if ((table == null) || (table == "")) table = TableName;
            if ((column == null) || (column == "")) column = "RawData";

            try
            {
                object obj = GetItemValue(id, column);

                if (obj == null) return null;

                byte[] bytes = obj as byte[];
                return obj as byte[];
            }
            catch(OleDbException ex) { return null; }
            catch(Exception ex) { return null; }
        }
        

        public float[] GetParsedArray_Float(string column, int id, [Optional]string table)
        {
            if ((table == null) || (table == "")) table = TableName;
            
            try
            { 
                string[] parsed_str = GetParsedArray_String(id, column, table);
                float[] result = new float[parsed_str.Length];

                for (int i = 0; i < parsed_str.Length; i++)
                    if ((parsed_str[i] == "") || (parsed_str[i] == "-.--") || (parsed_str[i] == "-,--")) result[i] = -1.0f;
                        else result[i] = float.Parse(parsed_str[i]);
                
                return result;
            }
            catch (OleDbException ex) { return null; }
            catch (Exception ex) { return null; }
        }
        public string[] GetParsedArray_String(int id, string column, [Optional]string table)
        {
            if ((table == null) || (table == "")) table = TableName;

            try
            {
                string _str = GetItemValue(id, column, table).ToString();
                return _str.Replace(" ", "").Split(';');
            }
            catch (OleDbException ex) { return null; }
            catch (Exception ex) { return null; }
        }

        public int InsertRecord(DataValue[] values, [Optional]byte[] rawdata, [Optional]string table)
        {
            if ((table == null) || (table == "")) table = TableName;

            int result = -1;
            if ((values == null) || (values.Length < 1)) return result;

            try
            {
                string all_names = "", all_values = "";
                for (int i = 0; i < values.Length; i++)
                {
                    all_names += "[" + values[i].Name + "], ";
                    all_values += "'" + values[i].Value + "', ";
                }
                all_names = all_names.Remove(all_names.LastIndexOf(','));
                all_values = all_values.Remove(all_values.LastIndexOf(','));

                if (rawdata == null) _cmd = new OleDbCommand(@"Insert Into [" + table + "] (" + all_names + ") Values (" + all_values + ");", _con);
                else
                {
                    OleDbParameter par = new OleDbParameter("@RawData",OleDbType.Binary, rawdata.Length);
                    par.Value = rawdata;
                    _cmd = new OleDbCommand(@"Insert Into " + table + " (" + all_names + ", RawData) Values (" + values + ", @RawData);", _con);
                    _cmd.Parameters.Add(par);
                }

                _cmd.ExecuteNonQuery();
                _cmd = new OleDbCommand(@"Select @@Identity;", _con);
                result = (int)_cmd.ExecuteScalar(); /*_cmd.ExecuteNonQuery();*/

                return result;
            }
            catch (OleDbException ex) { return -5; }
        }
        public bool UpdateRecord(int id, [Optional]DataValue[] values, [Optional]byte[] rawdata, [Optional]string table)
        {
            if (id < 0) return false;
            if ((table == null) || (table == "")) table = TableName;

            try
            { 
                _cmd = new OleDbCommand();
                _cmd.Connection = _con;

                string set = "";
                if ((values != null) && (values.Length > 0))
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i].Name == ColumnNames.Split(',')[0]) continue;
                        set += "[" + values[i].Name + "]=" + "'" + values[i].Value + "', ";
                    }
                }
                set = xFunctions.RemoveLast(set, ",");    
                if ((rawdata != null) && (rawdata.Length > 0))
                {
                    OleDbParameter par = new OleDbParameter("@RawData", OleDbType.LongVarBinary, 2500);
                    par.Value = rawdata;
                    _cmd.Parameters.Add(par);
                    set += @", [RawData] = @RawData";
                }

                _cmd.CommandText = @"Update " + table + " Set " + set + " Where " + ColumnNames.Split(',')[0] + "=" + id + ";";
                _cmd.ExecuteNonQuery();

                return true;
            }
            catch (OleDbException ex) { return false; }
            catch (Exception ex) { return false; }  
        }
        public bool DeleteRecord(int id, [Optional]string table)
        {
            if (id < 0) return false;
            if ((table == null) || (table == "")) table = TableName;

            try
            {
                _cmd = new OleDbCommand("Delete * From " + table + " Where " + ColumnNames.Split(',')[0] + "=" + id + ";", _con);
                _cmd.ExecuteNonQuery();
                return true;
            }
            catch (OleDbException ex) { return false; }
        }

        public bool UpdateRawData(int id, byte[] rawdata, [Optional] string column, [Optional]string table)
        {
            if (id < 0) return false;
            if ((table == null) || (table == "")) table = TableName;
            if ((column == null) || (column == "")) column = "RawData";

            try
            {
                
                _cmd = new OleDbCommand();
                _cmd.Connection = _con;

                OleDbParameter par = new OleDbParameter("@" + column, OleDbType.LongVarBinary, 2500);
                par.Value = rawdata;
                _cmd.Parameters.Add(par);

                _cmd.CommandText = @"Update " + table + " Set [" + column + "] = @" + column + " Where " + ColumnNames.Split(',')[0] + "=" + id + ";";

                _cmd.ExecuteNonQuery();
                
                return true;
            }
            catch (OleDbException ex) { return false; }
            catch (Exception ex) { return false; }
        }
    }
}
