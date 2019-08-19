using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication
{
    public class ErrorService
    {
        public static string LogError(Exception ex)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string code = new string(Enumerable.Repeat(chars, 20).Select(s => s[random.Next(s.Length)]).ToArray());
            DateTime actual = DateTime.Now;
            string message = string.Format("Time: {0}", actual.ToString("dd/MM/yyyy HH:mm:ss"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Code: {0}", code + "_" + actual.ToString("ddMMyyyyHHmmss"));
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/ErrorLog/" + actual.ToString("dd-MM-yyyy") + ".txt");
            try
            {
                using (System.IO.StreamWriter writer = System.IO.File.AppendText(path))
                {
                    writer.WriteLine(message);
                    writer.Close();
                }
            }
            catch { }
            return code + "_" + actual.ToString("ddMMyyyyHHmmss");
        }

        public static string LogErrorMessage(Exception ex)
        {
            DateTime actual = DateTime.Now;
            string message = string.Format("Time: {0}", actual.ToString("dd/MM/yyyy HH:mm:ss"));
            message += "<br/>";
            message += "-----------------------------------------------------------";
            message += "<br/>";
            message += string.Format("Message: {0}", ex.Message);
            message += "<br/>";
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += "<br/>";
            message += string.Format("Source: {0}", ex.Source);
            message += "<br/>";
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += "<br/>";
            message += "-----------------------------------------------------------";
            message += "<br/>";
            return message;
        }
    }
}