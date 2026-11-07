using System;
using System.Configuration;
using System.Windows.Forms;
using DailyDataSample.Business;
using DailyDataSample.Forms;

namespace DailyDataSample
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var connectionString = ConfigurationManager
                .ConnectionStrings["DailyDataDb"]
                .ConnectionString;

            var dailyDataBll = new DailyDataBll(connectionString);
            var targetId = 1;

            using (var form = new DailyDataEditForm(targetId, dailyDataBll))
            {
                Application.Run(form);
            }
        }
    }
}
