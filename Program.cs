using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Windows.Forms;

namespace FlightPlanManager
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var config = new LoggingConfiguration();
            var logfile = new FileTarget("logfile")
            {
                FileName = "current.log",
                ArchiveFileName = "archive.{#}.log",
                ArchiveEvery = FileArchivePeriod.Month,
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                MaxArchiveFiles = 1,
                AutoFlush = true,
                Layout = "${longdate} ${message} ${exception:format=tostring}"
            };

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.MainForm());
        }
    }
}