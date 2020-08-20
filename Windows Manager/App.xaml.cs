using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WindowsManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon _TaskbarIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _TaskbarIcon = (TaskbarIcon)FindResource("TaskbarIcon");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _TaskbarIcon.Dispose();
            base.OnExit(e);
        }
    }
}
