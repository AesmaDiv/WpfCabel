using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Try
{
    public class Journal
    {
        private string _folderPath = @".\Logs\";
        private string _fileName = "";
        private string _fullPath = "";
        private string[] _lines = new string[0];

        public string[] Lines
        {
            get { return _lines; }
            set { }
        }

        public Journal()
        {
            string today = DateTime.Now.ToString("yyyy.MM.dd");
            _fileName = today + ".log";
            if (!Directory.Exists(_folderPath)) Directory.CreateDirectory(_folderPath);
            Add("Программа запущена.");
        }
        public void Add(string message, [System.Runtime.InteropServices.Optional]string time_format)
        {
            if (time_format == "") time_format = "HH:mm:ss.ff";
            string time = DateTime.Now.ToString(time_format);
            _fullPath = _folderPath + _fileName;
            try
            {
                if (File.Exists(_fullPath))
                {
                    using (StreamWriter sw = File.AppendText(_fullPath))
                    {
                        sw.WriteLine(time + " --- " + message);
                    };
                }
                else
                {
                    using (StreamWriter sw = File.CreateText(_fullPath))
                    {
                        sw.WriteLine(time + " --- " + message);
                    };
                }
            }
            catch (Exception ex)
            { }
        }
        public string[] Read()
        {
            string[] result = new string[0];
            try
            {
                if(File.Exists(_fullPath))
                    using (StreamReader sr = new StreamReader(_fullPath))
                    {
                        while (!sr.EndOfStream)
                        {
                            int length = result.Length;
                            Array.Resize<string>(ref result, length + 1);
                            result[length] = sr.ReadLine();
                        }
                    }
            }
            catch (Exception ex)
            { }
            return result;
        }
    }
}
