using System;
using System.Windows.Forms;

namespace WindowsManager.Helpers
{
    public sealed class WndProcWindow : NativeWindow
    {
        public event EventHandler<Message> WndProcCalled;


        public WndProcWindow()
        {
            CreateHandle(new CreateParams());
        }


        protected override void WndProc(ref Message m)
        {
            EventHandler<Message> handler = WndProcCalled;
            handler?.Invoke(this, m);

            base.WndProc(ref m);
        }

    }
}
