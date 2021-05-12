using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xLibrary;

namespace WPF_Try
{
    public static class AccessFuncs
    {
        public static bool Connect()
        {
            Vars.Access = new xLibrary.xAccess(Vars.AccessPath);
            Vars.Access.TableName = Vars.AccessTable;
            Vars.Access.ColumnNames = Vars.AccessColumnNames;

            if (!Vars.Access.Connect()) MainFunctions.ShowMessage("ОШИБКА", "Не удалось подключиться к БД", false);

            return Vars.Access.IsOpen;
        }
        public static void Disconnect()
        {
            if (Vars.Access == null) return;
            Vars.Access.Disconnect();
        }
        public static bool AddRecord()
        {
            bool result = false;
            xLibrary.xAccess.DataValue[] Values = Vars.CurrentCabel.GetAllDataValues();

            Values = Array.FindAll(Values, item => item.Name != "RecID");

            Vars.CurrentCabel.ID = Vars.Access.InsertRecord(Values);
            result = (Vars.CurrentCabel.ID > 0);

            if (result) MainFunctions.ShowMessage("Сохранение БД", "Данные успешно сохранены", false);
            else MainFunctions.ShowMessage("Сохранение БД", "Не удалось сохранить данные в БД", false);

            return result;
        }
        public static bool UpdateRecord()
        {
            int id = Vars.CurrentCabel.ID;

            xLibrary.xAccess.DataValue[] values = Vars.CurrentCabel.GetAllDataValues();
            values = Array.FindAll(values, item => item.Name != "RecID");

            if (!Vars.Access.UpdateRecord(id, values)) { MainFunctions.ShowMessage("Обновление БД", "Не удалось обновить данные в БД", false); return false; }

            for (int i = 0; i < 3; i++)
                if (!Vars.Access.UpdateRawData(id, Vars.CurrentCabel.Hipot_GetRaw((CabelData.HipotFase)i), Vars.AccessRawDataNames[i]))
                {
                    MainFunctions.ShowMessage("Обновление БД", "Не удалось обновить данные в БД:\n" + Vars.AccessRawDataNames[i], false);
                    return false;
                }

            if (!Vars.Access.UpdateRawData(id, Vars.CurrentCabel.StructureCabel_GetRaw(), "StructureCabel"))
            {
                MainFunctions.ShowMessage("Обновление БД", "Не удалось обновить данные в БД:\n" + "StructureCabel", false);
                return false;
            }
            
            MainFunctions.ShowMessage("Обновление БД", "Данные успешно обновлены", false);

            return true;
        }
        public static void DeleteRecord(ref xLibrary.xTestList testlist)
        {
            Vars.TestListRow item = testlist.CurrentItem as Vars.TestListRow;
            Vars.Access.DeleteRecord(item.RecID);
            MainFunctions.FillTestList(ref testlist);
        }

    }
}
