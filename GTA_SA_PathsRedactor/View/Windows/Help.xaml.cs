using System;
using System.Windows;

namespace GTA_SA_PathsRedactor.View
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        private static HelpWindow? s_existWindow;

        public HelpWindow()
        {
            InitializeComponent();

            if (s_existWindow == null)
                s_existWindow = this;
        }

        public static HelpWindow? ExistWindow => s_existWindow;

        private void Window_Closed(object sender, EventArgs e)
        {
            if (s_existWindow == this)
            {
                s_existWindow = null;
            }
        }
    }
}
