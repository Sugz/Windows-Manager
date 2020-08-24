using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using WindowsManager.ViewModels;

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
            ViewModelLocator.Cleanup();
            base.OnExit(e);
        }
    }
}
