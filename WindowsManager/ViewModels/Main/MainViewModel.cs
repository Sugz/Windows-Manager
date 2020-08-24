using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using WindowsManager.Helpers;
using WindowsManager.Views;
using Forms = System.Windows.Forms;

namespace WindowsManager.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {

        #region Fields

        private OptionsWindow _HotKeysWindow;
        private SettingsManager _SettingsManager;
        private bool _SettingsExist;
        private ForegroundWindowHook _ForegroundWindowHook;
        private IntPtr _CurrentForegroundWindow = IntPtr.Zero;

        #endregion Fields


        #region Properties

        public List<Screen> Screens { get; private set; } = new List<Screen>(); 

        #endregion Properties


        #region Commands

        private RelayCommand _ShowSplitWindowsCommand;
        private RelayCommand _ShowOptionsWindowsCommand;
        private RelayCommand _ExitAppCommand;

        public RelayCommand ShowSplitWindowsCommand => _ShowSplitWindowsCommand ??= new RelayCommand
        (
            () => Screens.ForEach(x => x.ShowSplitters())
        );

        public RelayCommand ShowOptionsWindowsCommand => _ShowOptionsWindowsCommand ??= new RelayCommand
        (
            () =>
            {
                _HotKeysWindow ??= new OptionsWindow();
                _HotKeysWindow.Show();
            }
        );

        public RelayCommand ExitAppCommand => _ExitAppCommand ??= new RelayCommand(() => Application.Current.Shutdown());

        #endregion Commands


        #region Constructor

        public MainViewModel()
        {
            CheckSettings();
            GetScreens();
            SetForegroundWindowHook();
            InitializeHotKeysHost();
            CreateHotKeys();
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


        private void SetForegroundWindowHook()
        {
            _ForegroundWindowHook = new ForegroundWindowHook();
            _ForegroundWindowHook.ForegroundWindowChanged += OnForegroundWindowChanged;
            _ForegroundWindowHook.Start();
            _CurrentForegroundWindow = NativeMethods.GetForegroundWindow();
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

        private void OnForegroundWindowChanged(object sender, ForegroundWindowChangedEventArgs e)
        {
            _CurrentForegroundWindow = e.Handle;
        }

        #endregion Event Handlers


        #region Cleanup

        public override void Cleanup()
        {
            _ForegroundWindowHook.ForegroundWindowChanged -= OnForegroundWindowChanged;
            _ForegroundWindowHook.Stop();
            CleanupHotKeysHost();
            base.Cleanup();
        }

        #endregion Cleanup

    }
}


//TODO: change screen (Win + NumPad1 / NumPad2 / etc for each screens )
//TODO: change rect (Win + PageUp / PageDown)
//TODO: move (Win + Left / Up / Right / Down)
//TODO: resize (Win + Ctrl + Left / Up / Right / Down)
//TODO: symmetrique resize (Win + Alt + Left / Up / Right / Down)
//TODO: fill rect (Win + Enter)
//TODO: fill neighboor rect (Win + Shift + Left / Up / Right / Down)