using System.IO;
using System.Windows;

namespace SoundPackCreator
{
    public partial class App : Application
    {
        private void OnOpen(object sender, StartupEventArgs e)
        {
            MainWindow window;

            if (e.Args.Length > 0)
            {
                string arg = e.Args[0];

                if (File.Exists(arg))
                {
                    window = new MainWindow(arg);
                }
                else
                {
                    window = new MainWindow();
                }
            }
            else
            {
                window = new MainWindow();
            }

            window.Show();
        }
    }
}
