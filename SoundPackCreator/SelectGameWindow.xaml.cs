using Microsoft.Win32;
using SoundPackCreator.Properties;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SoundPackCreator
{
    public partial class SelectGameWindow : Window
    {
        const int DETECT_DELAY = 700; // delay in milliseconds between each detection of the game

        // Name of compatible games
        readonly string[] GameNames = { "DDNet", "teeworlds" };

        // The PID of each process that the user rejected installation in
        ArrayList IgnorePid = new ArrayList();

        // Events
        public event EventHandler OnGameDetected;
        public event EventHandler OnGameSelected;

        // Timer to execute the detection loop
        private DispatcherTimer DetectTimer = new DispatcherTimer();

        public SelectGameWindow()
        {
            InitializeComponent();

            OnGameDetected += new EventHandler(OnDetected);

            // Try to be seen in this world of attention
            Focus();
            Activate();

            // Init and start the detection loop
            DetectTimer.Interval = TimeSpan.FromMilliseconds(700);
            DetectTimer.Tick += new EventHandler(Detect);
            DetectTimer.Start();
        }

        private void Detect_window_Closed(object sender, System.EventArgs e)
        {
            // Stop the loop of detection
            DetectTimer.Stop();
        }

        // Will try in the background to detect the game process of the user
        private void Detect(object sender, EventArgs e)
        {
            bool DetectContinue = true;

            foreach (string gameName in GameNames)
            {
                // Try to find the game with his process name
                Process[] Processes = Process.GetProcessesByName(gameName);

                foreach (Process process in Processes)
                {
                    // If this process id is in the ignore list, just ignore it, yes
                    if (IgnorePid.Contains(process.Id)) continue;

                    // This process is not ignored, stop the loop
                    DetectContinue = false;

                    string GamePath = process.MainModule.FileName; // the path of the .exe file of the process

                    // Send an event of detection
                    OnGameEventArgs Args = new OnGameEventArgs(process, GamePath);

                    EventHandler handler = OnGameDetected;
                    handler?.Invoke(this, Args);

                    break;
                }

                if (!DetectContinue) break;
            }
        }

        // Ask the user where is the .exe file of the game
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            DetectTimer.Stop();

            // Ask the user to select the .exe file of the game
            OpenFileDialog GameFileSelector = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Executable game (*.exe)|*.exe"
            };

            string InitialDirectory = Settings.Default.GameFilePath;

            if (InitialDirectory.Length == 0)
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Programs);

            GameFileSelector.InitialDirectory = InitialDirectory;

            if ((bool)GameFileSelector.ShowDialog())
            {
                string FilePath = GameFileSelector.FileName;
                string FileFolderPath = Path.GetDirectoryName(FilePath);

                Settings.Default.GameFilePath = FileFolderPath; // update settings

                // Verify if audio folder exists
                string AudioFolderPath = FileFolderPath + PackInstaller.SoundPathFromGame;

                if (!Directory.Exists(AudioFolderPath))
                {
                    string message = localization.ResourceManager.GetString("dialog_error_select_game_folder-not-exists_message");
                    string title = localization.ResourceManager.GetString("dialog_error_select_game_folder-not-exists_title");

                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    DetectTimer.Start();

                    return;
                }

                // Send selected event
                Process Game = new Process();
                Game.StartInfo.FileName = FilePath;

                OnGameEventArgs Args = new OnGameEventArgs(Game, FilePath);
                EventHandler handler = OnGameSelected;
                handler?.Invoke(this, Args);
                Close();
            }
            else DetectTimer.Start();
        }

        // When a game process is detected
        // Ask the user to select this game or ignore it
        private void OnDetected(object sender, EventArgs e)
        {
            DetectTimer.Stop();

            OnGameEventArgs args = (OnGameEventArgs)e;

            Process Game = args.Game;
            string GamePath = args.Path;
            int GamePID = Game.Id;

            // Put the window's game at the back
            ShowWindow(Game.MainWindowHandle, 6); // 6 = MINIMIZE / 3 = MAXIMIZE

            // Ask to install in this game or ignore it
            string message = localization.ResourceManager.GetString("dialog_question_select-game_message") + GamePath + "\n\nPID : \n" + GamePID.ToString();
            string title = localization.ResourceManager.GetString("dialog_question_select-game_title");

            MessageBoxResult AskInstallResult = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);

            if (AskInstallResult == MessageBoxResult.No)
            {
                // Ignore this process and restart the detection's loop
                IgnorePid.Add(GamePID);

                DetectTimer.Start();

                return;
            }

            // Send selected event
            OnGameEventArgs Args = new OnGameEventArgs(Game, GamePath);
            EventHandler handler = OnGameSelected;
            handler?.Invoke(this, Args);
            Close();
        }

        [DllImport("USER32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }

    class OnGameEventArgs : EventArgs
    {
        public Process Game;
        public string Path;

        public OnGameEventArgs(Process game, string path)
        {
            Game = game;
            Path = path;
        }
    }
}