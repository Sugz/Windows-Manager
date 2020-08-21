using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using WindowsManager.Helpers;
using Forms = System.Windows.Forms;

namespace WindowsManager.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        #region Fields

        private SettingsManager _SettingsManager;
        private bool _SettingsExist; 

        #endregion Fields


        #region Properties

        public List<Screen> Screens { get; private set; } = new List<Screen>(); 

        #endregion Properties


        #region Commands

        private RelayCommand _ShowSplitWindowsCommand;
        private RelayCommand _ExitAppCommand;

        public RelayCommand ShowSplitWindowsCommand => _ShowSplitWindowsCommand ??= new RelayCommand
        (
            () => Screens.ForEach(x => x.ShowSplitters())
        );

        public RelayCommand ExitAppCommand => _ExitAppCommand ??= new RelayCommand(() => Application.Current.Shutdown());

        #endregion Commands


        #region Constructor

        public MainViewModel()
        {
            CheckSettings();
            GetScreens();
        }

        #endregion Constructor


        #region Initialisation

        private void CheckSettings()
        {
            if (_SettingsManager is null)
                _SettingsManager = SimpleIoc.Default.GetInstance<SettingsManager>();

            _SettingsExist = _SettingsManager.SettingsExist();
        }


        private void LoadSettings()
        {
            if (_SettingsManager.Load() is IEnumerable<XElement> screenXElements)
            {
                foreach (XElement screenXElement in screenXElements)
                {
                    int index = int.Parse(screenXElement.Attribute("Index").Value);

                    List<XElement> rectXElements = screenXElement.Elements().ToList();
                    for (int i = 0; i < rectXElements.Count; i++)
                    {
                        List<double> rectValues = rectXElements[i].Value.Split(',').Select(x => double.Parse(x)).ToList();
                        Screens[index].Rects[i] = new Rect(rectValues[0], rectValues[1], rectValues[2], rectValues[3]);
                    }
                }
            }
        }


        private void GetScreens()
        {
            Forms.Screen[] allScreens = Forms.Screen.AllScreens;
            for (int i = 0; i < allScreens.Length; i++)
            {
                Screen screen = new Screen(allScreens[i], i, _SettingsExist);
                screen.SplitWindowHide += HideSplitters;
                Screens.Add(screen);
            }

            if (_SettingsExist)
                LoadSettings();
        }

        #endregion Initialisation


        #region Event Handlers

        private void HideSplitters(object sender, EventArgs e)
        {
            List<XElement> serializedScreens = new List<XElement>();
            foreach (Screen screen in Screens)
            {
                screen.HideSplitters();
                serializedScreens.Add(screen.Serialize());
            }

            if (_SettingsManager is null)
                _SettingsManager = SimpleIoc.Default.GetInstance<SettingsManager>();

            _SettingsManager.Save(serializedScreens);
        } 

        #endregion Event Handlers

    }
}


//TODO: change screen (Win + NumPad1 / NumPad2 / etc for each screens )
//TODO: change rect (Win + PageUp / PageDown)
//TODO: move (Win + Left / Up / Right / Down)
//TODO: resize (Win + Ctrl + Left / Up / Right / Down)
//TODO: symmetrique resize (Win + Alt + Left / Up / Right / Down)
//TODO: fill rect (Win + Enter)
//TODO: fill neighboor rect (Win + Shift + Left / Up / Right / Down)