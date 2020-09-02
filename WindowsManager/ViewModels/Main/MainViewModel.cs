using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        private bool _SettingsExist = false;
        private ForegroundWindowHook _ForegroundWindowHook;
        private IntPtr _CurrentForegroundWindow = IntPtr.Zero;
        private Rectangle _ScreensBounds;
        private bool _StartWithWindow;

        #endregion Fields


        #region Properties

        public List<Screen> Screens { get; private set; } = new List<Screen>();


        /// <summary>
        /// Gets or sets wheter the app start with window or not.
        /// </summary>
        public bool StartWithWindow
        {
            get => _StartWithWindow;
            set
            {
                Set(ref _StartWithWindow, value);

                // The path to the key where Windows looks for startup applications
                using RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (_StartWithWindow)
                    key.SetValue(Constants.Product, Process.GetCurrentProcess().MainModule.FileName);
                else
                    key.DeleteValue(Constants.Product, false);

            }
        }

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
            LoadHotKeysSettings();
            CreateHotKeys();
        }

        #endregion Constructor


        #region Initialisation

        private void CheckSettings()
        {
            // check if the app start with windows
            using RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            StartWithWindow = key.GetValue(Constants.Product) != null;

            // define settings manager
            if (_SettingsManager is null)
                _SettingsManager = SimpleIoc.Default.GetInstance<SettingsManager>();

            // load settings if any
            if (_SettingsManager.SettingsExist)
            {
                _SettingsExist = true;
                _SettingsManager.Load();
                _SettingsExist = true;
            }
        }


        private void LoadScreensSettings()
        {
            foreach (XElement screenXElement in _SettingsManager.Screens)
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


        private void GetScreens()
        {
            Rectangle screensBounds = new Rectangle();
            Forms.Screen[] allScreens = Forms.Screen.AllScreens;
            for (int i = 0; i < allScreens.Length; i++)
            {
                Screen screen = new Screen(allScreens[i], i, _SettingsExist);
                screensBounds = Rectangle.Union(screensBounds, allScreens[i].Bounds);
                screen.SplitWindowHide += HideSplitters;
                Screens.Add(screen);
            }

            _ScreensBounds = screensBounds;

            if (_SettingsExist)
                LoadScreensSettings();

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

            _SettingsManager.Screens = serializedScreens;
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