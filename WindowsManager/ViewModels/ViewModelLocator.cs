using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using WindowsManager.Helpers;

namespace WindowsManager.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SettingsManager>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();


        public static void Cleanup()
        {
            ServiceLocator.Current.GetInstance<MainViewModel>().Cleanup();
        }
    }
}
