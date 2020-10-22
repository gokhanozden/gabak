//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Windows.Forms;

namespace GABAK
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}