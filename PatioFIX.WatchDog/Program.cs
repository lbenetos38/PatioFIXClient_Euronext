using System;
using System.ServiceProcess;
using System.Windows.Forms;

namespace PatioFIX.WatchDog
{
    internal static class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            bool startGui = false;


            if (args.Length > 0)
            {
                #region
                foreach (var arg in args)
                {
                    string _arg = arg.ToLowerInvariant();

                    if (String.Compare("gui", _arg, StringComparison.Ordinal) == 0 || String.Compare("-gui", _arg, StringComparison.Ordinal) == 0)
                    {
                        startGui = true;
                    }
                }
                #endregion
            }


            if (startGui == true)
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                System.Threading.Thread.CurrentThread.Name = "PatioFIX.WatchDog (GUI)";
                Application.Run(new DebugForm());
            }
            else
            {
                ServiceBase.Run(new Actuator());
            }
        }
    }
}