using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xLibrary;

namespace WPF_Try
{
    public class AccessDictionary
    {
        private xAccess _access;

        public AccessDictionary(string path, string table, string column)
        {
            _access = new xAccess(path);
            _access.TableName = table;
            _access.ColumnNames = column;
        }
        public string[] Read()
        {
            if (!_access.Connect()) return null;
            string[] result = _access.GetUniqItemsList(_access.ColumnNames);
            _access.Disconnect();

            return result;
        }
        public void Add(string value)
        {
            if (!_access.Connect()) return;
            int id = _access.GetRecordContains(_access.ColumnNames, value);
            if (id < 0) _access.InsertRecord(new xAccess.DataValue[] { new xAccess.DataValue() { Name = "Name", Value = value } });
            _access.Disconnect();
        }
        public void Delete(string value)
        {
            if (!_access.Connect()) return;
            int id = _access.GetRecordContains(_access.ColumnNames, value);
            if (id > 0) = _access.DeleteRecord(id);
            _access.Disconnect();
        }

    }
}
