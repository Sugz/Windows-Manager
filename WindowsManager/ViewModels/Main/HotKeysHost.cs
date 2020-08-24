using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
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

        private static readonly SerialCounter _IdGen = new SerialCounter(1);
        private WndProcWindow _WndProcWindow;

        #endregion Fields


        #region Properties

        public Dictionary<int, HotKey> HotKeys { get; set; } = new Dictionary<int, HotKey>(); 

        #endregion Properties


        #region Initialization

        private void InitializeHotKeysHost()
        {
            _WndProcWindow = new WndProcWindow();
            _WndProcWindow.WndProcCalled += GetHotKey;
        }


        private void GetHotKey(object sender, Forms.Message message)
        {
            // Only interested in hotkey messages so skip others
            if (message.Msg != NativeMethods.WM_HOTKEY)
                return;

            // Get hotkey id and execute it
            int id = message.WParam.ToInt32();
            if (HotKeys.ContainsKey(id))
                HotKeys[id].Execute();
        }

        #endregion Initialization


        #region Win Global HotKey Interop

        private void RegisterHotKey(int id, HotKey hotKey)
        {
            NativeMethods.RegisterHotKey(_WndProcWindow.Handle, id, (int)hotKey.Modifiers, KeyInterop.VirtualKeyFromKey(hotKey.Key));
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

        private void UnregisterHotKey(int id)
        {
            NativeMethods.UnregisterHotKey(_WndProcWindow.Handle, id);
            int error = Marshal.GetLastWin32Error();
            if (error != 0)
                throw new Win32Exception(error);
        }

        #endregion Win Global HotKey Interop


        #region Manage HotKeys

        private void OnHotkeyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var kvPair = HotKeys.FirstOrDefault(h => h.Value == sender);
            if (kvPair.Value != null)
            {
                if (e.PropertyName == "IsEnable")
                {
                    if (kvPair.Value.IsEnable)
                        RegisterHotKey(kvPair.Key, kvPair.Value);
                    else
                        UnregisterHotKey(kvPair.Key);
                }
                else if (e.PropertyName == "Key" || e.PropertyName == "Modifiers")
                {
                    if (kvPair.Value.IsEnable)
                    {
                        UnregisterHotKey(kvPair.Key);
                        RegisterHotKey(kvPair.Key, kvPair.Value);
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
                throw new ArgumentNullException("value");
            if (hotKey.Key == 0)
                throw new ArgumentNullException("value.Key");
            if (HotKeys.Values.Any(x => x.Equals(hotKey)))
                throw new HotKeyAlreadyRegisteredException("HotKey already registered!", hotKey);

            int id = _IdGen.Next();
            if (hotKey.IsEnable)
                RegisterHotKey(id, hotKey);

            hotKey.PropertyChanged += OnHotkeyPropertyChanged;
            HotKeys[id] = hotKey;
        }


        /// <summary>
        /// Removes an hotKey
        /// </summary>
        /// <param name="hotKey">The hotKey to be removed</param>
        /// <returns>True if success, otherwise false</returns>
        public bool RemoveHotKey(HotKey hotKey)
        {
            var kvPair = HotKeys.FirstOrDefault(h => h.Value == hotKey);
            if (kvPair.Value != null)
            {
                kvPair.Value.PropertyChanged -= OnHotkeyPropertyChanged;
                if (kvPair.Value.IsEnable)
                    UnregisterHotKey(kvPair.Key);

                return HotKeys.Remove(kvPair.Key);
            }
            return false;
        }

        #endregion Manage HotKeys


        #region Cleanup

        private void CleanupHotKeysHost()
        {
            _WndProcWindow.WndProcCalled -= GetHotKey;
            _WndProcWindow.ReleaseHandle();
            _WndProcWindow.DestroyHandle();
            for (int i = HotKeys.Count() - 1; i >= 0; i--)
                RemoveHotKey(HotKeys.Values.ElementAt(i));
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
