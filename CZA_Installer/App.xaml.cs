 
using System.IO;
using System.Windows;

namespace CZA_Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
           if (MainWindow == null)  MainWindow = new MainWindow();
             var mainWindow = MainWindow as MainWindow;
           
            // If arguments are passed (i.e., file(s) opened with the app)
            if (e.Args.Length > 0)
            {
                string filePath = e.Args[0];
                  if (File.Exists(filePath) && Path.GetExtension(filePath).ToLower() == ".cza")  mainWindow?.LoadFile(filePath);  
                  
            }
            mainWindow.Show();

        }
    }

}
