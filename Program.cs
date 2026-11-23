using System;
using System.Windows.Forms;

namespace PdfUtilityApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            bool restartForLanguage;
            do
            {
                using var mainForm = new MainForm();
                Application.Run(mainForm);
                restartForLanguage = mainForm.IsLanguageSwitch;
            }
            while (restartForLanguage);
        }
    }
}