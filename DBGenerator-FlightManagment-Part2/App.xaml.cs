using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DBGenerator_FlightManagment_Part2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));
        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Info("~~~~~~~~~~ Started Logging.......");
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            log.Info("~~~~~~~~~~ Finishing Logging!");
            base.OnExit(e);
        }
    }
}
