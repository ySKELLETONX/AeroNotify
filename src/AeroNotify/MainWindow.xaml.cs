using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace AeroNotify
{
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public MainWindow()
        {
            ConfigService.Load();

            var debugValue = Convert.ToInt32(ConfigService.Get("AeroNotify:Debug"));

            if (debugValue == 1)
            {
                AllocConsole();
                Console.WriteLine("Modo Debug Ativado");
            }
            else
            {
                FreeConsole();
            }

            InitializeComponent();
        }
    }
}
