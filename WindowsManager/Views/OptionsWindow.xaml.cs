using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WindowsManager.Views
{
    /// <summary>
    /// Interaction logic for HotKeysWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
        }

       
        private void OnNumericUpDownKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ExplorerSize.Focus();
        }
    }
}
