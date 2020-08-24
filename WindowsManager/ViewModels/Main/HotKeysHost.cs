using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Windows.Input;
using WindowsManager.Helpers;
using Forms = System.Windows.Forms;

namespace WindowsManager.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {

        #region Fields

        private static readonly SerialCounter _IdGen = new SerialCounter(0);
        private WndProcWindow _WndProcWindow;

        #endregion Fields


        #region Properties

        public ObservableCollection<HotKey> HotKeys { get; set; } = new ObservableCollection<HotKey>();

        #endregion Properties


        #region Initialization

        private void InitializeHotKeysHost()
        {
            _WndProcWindow = new WndProcWindow();
            _WndProcWindow.WndProcCalled += GetHotKeyMessage;
        }


        private void GetHotKeyMessage(object sender, Forms.Message message)
        {
            // Only interested in hotkey messages so skip others
            if (message.Msg != NativeMethods.WM_HOTKEY)
                return;

            // Get hotkey id and execute it
            int id = message.WParam.ToInt32();
            if (HotKeys.FirstOrDefault(x => x.Id == id) is HotKey hotKey)
                hotKey.Execute();
        }

        #endregion Initialization


        #region Win Global HotKey Interop

        private void RegisterHotKey(HotKey hotKey)
        {
            NativeMethods.RegisterHotKey(_WndProcWindow.Handle, hotKey.Id, (int)hotKey.Modifiers, KeyInterop.VirtualKeyFromKey(hotKey.Key));
            int error = Marshal.GetLastWin32Error();
            if (error != 0)
            {
                Exception e = new Win32Exception(error);

                if (error == 1409)
                    throw new HotKeyAlreadyRegisteredException(e.Message, hotKey, e);
                else
                    throw e;
            }
        }

        private void UnregisterHotKey(HotKey hotKey)
        {
            NativeMethods.UnregisterHotKey(_WndProcWindow.Handle, hotKey.Id);
            int error = Marshal.GetLastWin32Error();
            if (error != 0)
                throw new Win32Exception(error);
        }

        #endregion Win Global HotKey Interop


        #region Manage HotKeys

        private void OnHotkeyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (HotKeys.FirstOrDefault(x => x.Equals(sender)) is HotKey hotKey)
            {
                if (e.PropertyName == "IsEnable")
                {
                    if (hotKey.IsEnable)
                        RegisterHotKey(hotKey);
                    else
                        UnregisterHotKey(hotKey);
                }
                else if (e.PropertyName == "Key" || e.PropertyName == "Modifiers")
                {
                    if (hotKey.IsEnable)
                    {
                        UnregisterHotKey(hotKey);
                        RegisterHotKey(hotKey);
                    }
                }
            }
        }


        /// <summary>
        /// Adds an hotKey.
        /// </summary>
        /// <param name="hotKey">The hotKey which will be added. Must not be null and can be registed only once.</param>
        public void AddHotKey(HotKey hotKey)
        {
            if (hotKey == null)
                throw new ArgumentNullException(nameof(hotKey));
            if (hotKey.Key == 0)
                throw new ArgumentNullException("hotKey.Key");
            if (HotKeys.Any(x => x.Equals(hotKey)))
                throw new HotKeyAlreadyRegisteredException("HotKey already registered!", hotKey);

            if (hotKey.IsEnable)
                RegisterHotKey(hotKey);

            hotKey.PropertyChanged += OnHotkeyPropertyChanged;
            HotKeys.Add(hotKey);
        }


        /// <summary>
        /// Removes an hotKey
        /// </summary>
        /// <param name="hotKey">The hotKey to be removed</param>
        /// <returns>True if success, otherwise false</returns>
        public bool RemoveHotKey(HotKey hotKey)
        {
            HotKey _hotKey = HotKeys.FirstOrDefault(h => h.Equals(hotKey));
            if (_hotKey != null)
            {
                _hotKey.PropertyChanged -= OnHotkeyPropertyChanged;
                if (_hotKey.IsEnable)
                    UnregisterHotKey(_hotKey);

                return HotKeys.Remove(_hotKey);
            }

            return false;
        }

        #endregion Manage HotKeys


        #region Cleanup

        private void CleanupHotKeysHost()
        {
            _WndProcWindow.WndProcCalled -= GetHotKeyMessage;
            for (int i = HotKeys.Count() - 1; i >= 0; i--)
                RemoveHotKey(HotKeys.ElementAt(i));

            _WndProcWindow.ReleaseHandle();
            _WndProcWindow.DestroyHandle();
        }

        #endregion Cleanup
    }


    [Serializable]
    public class HotKeyAlreadyRegisteredException : Exception
    {
        public HotKey Hotkey { get; private set; }
        public HotKeyAlreadyRegisteredException(string message, HotKey hotkey) : base(message) { Hotkey = hotkey; }
        public HotKeyAlreadyRegisteredException(string message, HotKey hotKey, Exception inner) : base(message, inner) { Hotkey = hotKey; }
        protected HotKeyAlreadyRegisteredException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }

    }


    internal class SerialCounter
    {
        internal SerialCounter(int start)
        {
            Current = start;
        }

        internal int Current { get; private set; }

        internal int Next()
        {
            return ++Current;
        }
    }
}
