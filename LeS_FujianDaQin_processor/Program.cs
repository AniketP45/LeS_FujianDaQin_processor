using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FujianDaQin_Routine;

namespace LeS_FujianDaQin_processor
{
    internal class Program
    {
        static string processor_name = Convert.ToString(ConfigurationManager.AppSettings["PROCESSOR_NAME"]) + " : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        static void Main(string[] args)
        {
            FujianDaQin FujianDaQin = new FujianDaQin();
            FujianDaQin.LogText = "====================================";
            FujianDaQin.LogText = processor_name + " Process Started...";
            SetCulture(FujianDaQin);
            FujianDaQin.StartProcess();
            FujianDaQin.LogText = processor_name + " Process Completed...";
            FujianDaQin.LogText = "====================================";
            Environment.Exit(0);
        }
        public static void SetCulture(FujianDaQin _Routine)
        {
            System.Globalization.CultureInfo _defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            _Routine.LogText = "Default regional setting - " + _defaultCulture.DisplayName;
            _Routine.LogText = "Current regional setting - " + System.Threading.Thread.CurrentThread.CurrentCulture.DisplayName;
        }
    }
}
