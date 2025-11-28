using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;


namespace WordToPDF.Helper
{
    public class ProgressService
    {
        private static string path = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/progress.txt");

        public static void SetProgress(int value)
        {
            File.WriteAllText(path, value.ToString());
        }

        public static int GetProgress()
        {
            if (!File.Exists(path)) return 0;
            return int.Parse(File.ReadAllText(path));
        }
    }
}