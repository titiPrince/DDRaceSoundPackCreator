using Microsoft.Win32;
using SoundPackCreator.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SoundPackCreator
{
    public partial class MainWindow : Window
    {
        public string test = null;

        // Where each temporary file will be write
        // Any file here can be overwrite
        private readonly string TEMP_FOLDER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DDPackCreator\Temp\";

        // List of items of the ListBox
        public List<object> SoundsListItems { get; }

        // Where the current pack is temporary saved
        private Dictionary<string, string> Pack = new Dictionary<string, string>();

        // Sounds players
        private MediaPlayer NewSoundPlayer = new MediaPlayer();
        private SoundPlayer DefaultSoundPlayer;

        private DispatcherTimer SliderTimer = new DispatcherTimer();

        // The wav compresser/uncompresser
        private Wavpack Wavpack;

        // Other windows
        private SelectGameWindow SelectGameWindow;
        private Credits CreditsWindow = new Credits();

        // The paths where the current pack is saved in the storage
        private string PackSavePath = "";
        private string TempPackSavePath = "";

        private bool NewSoundPlayerIsPlaying = false;
        private bool PackIsSaved = true;

        public MainWindow(string openFile = null)
        {
            InitializeComponent();

            // Verify if the temp folder exists
            if (!Directory.Exists(TEMP_FOLDER_PATH))
            {
                string message = "The folder \"Temp\" is missing. Please start the repair program.";
                string title = "Temp folder missing";

                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Verify if wavpack.exe and wavunpack.exe exists
            try
            {
                Wavpack = new Wavpack();
            }
            catch (Exception)
            {
                string message = "Some important files are missing. Please start the repair program.";
                string title = "Files missing";

                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Associate the ListBox dynamically
            SoundsListItems = new List<object>();
            SoundsList.ItemsSource = SoundsListItems;

            // Init the timer
            SliderTimer.Interval = TimeSpan.Zero;
            SliderTimer.Tick += new EventHandler(Time_tick);

            // Init the sound player
            NewSoundPlayer.MediaEnded += new EventHandler(New_sound_player_finished);

            // List of each name of default-sounds
            string[] SoundsName = {
                "foley_body_impact-01", "foley_body_impact-02", "foley_body_impact-03", "foley_body_splat-01",
                "foley_body_splat-02", "foley_body_splat-03", "foley_body_splat-04", "foley_dbljump-01", "foley_dbljump-02",
                "foley_dbljump-03", "foley_foot_left-01", "foley_foot_left-02", "foley_foot_left-03", "foley_foot_left-04",
                "foley_foot_right-01", "foley_foot_right-02", "foley_foot_right-03", "foley_foot_right-04", "foley_land-01",
                "foley_land-02", "foley_land-03", "foley_land-04", "hook_attach-01", "hook_attach-02", "hook_attach-03",
                "hook_loop-01", "hook_loop-02", "hook_noattach-01", "hook_noattach-02", "hook_noattach-03", "music_menu",
                "sfx_ctf_cap_pl", "sfx_ctf_drop", "sfx_ctf_grab_en", "sfx_ctf_grab_pl", "sfx_ctf_rtn", "sfx_hit_strong-01",
                "sfx_hit_strong-02", "sfx_hit_weak-01", "sfx_hit_weak-02", "sfx_hit_weak-03", "sfx_msg-client", "sfx_msg-highlight",
                "sfx_msg-server", "sfx_pickup_arm-01", "sfx_pickup_arm-02", "sfx_pickup_arm-03", "sfx_pickup_arm-04", "sfx_pickup_gun",
                "sfx_pickup_hrt-01", "sfx_pickup_hrt-02", "sfx_pickup_launcher", "sfx_pickup_ninja", "sfx_pickup_sg", "sfx_skid-01",
                "sfx_skid-02", "sfx_skid-03", "sfx_skid-04", "sfx_spawn_wpn-01", "sfx_spawn_wpn-02", "sfx_spawn_wpn-03", "vo_teefault_cry-01",
                "vo_teefault_cry-02", "vo_teefault_ninja-01", "vo_teefault_ninja-02", "vo_teefault_ninja-03", "vo_teefault_ninja-04",
                "vo_teefault_pain_long-01", "vo_teefault_pain_long-02", "vo_teefault_pain_short-01", "vo_teefault_pain_short-02",
                "vo_teefault_pain_short-03", "vo_teefault_pain_short-04", "vo_teefault_pain_short-05", "vo_teefault_pain_short-06",
                "vo_teefault_pain_short-07", "vo_teefault_pain_short-08", "vo_teefault_pain_short-09", "vo_teefault_pain_short-10",
                "vo_teefault_pain_short-11", "vo_teefault_pain_short-12", "vo_teefault_sledge-01", "vo_teefault_sledge-02",
                "vo_teefault_sledge-03", "vo_teefault_spawn-01", "vo_teefault_spawn-02", "vo_teefault_spawn-03",
                "vo_teefault_spawn-04", "vo_teefault_spawn-05", "vo_teefault_spawn-06", "vo_teefault_spawn-07", "wp_flump_explo-01",
                "wp_flump_explo-02", "wp_flump_explo-03", "wp_flump_launch-01", "wp_flump_launch-02", "wp_flump_launch-03",
                "wp_gun_fire-01", "wp_gun_fire-02", "wp_gun_fire-03", "wp_hammer_hit-01", "wp_hammer_hit-02", "wp_hammer_hit-03",
                "wp_hammer_swing-01", "wp_hammer_swing-02", "wp_hammer_swing-03", "wp_laser_bnce-01", "wp_laser_bnce-02",
                "wp_laser_bnce-03", "wp_laser_fire-01", "wp_laser_fire-02", "wp_laser_fire-03", "wp_ninja_attack-01",
                "wp_ninja_attack-02", "wp_ninja_attack-03", "wp_ninja_attack-04", "wp_ninja_hit-01", "wp_ninja_hit-02",
                "wp_ninja_hit-03", "wp_ninja_hit-04", "wp_noammo-01", "wp_noammo-02", "wp_noammo-03", "wp_noammo-04",
                "wp_noammo-05", "wp_shotty_fire-01", "wp_shotty_fire-02", "wp_shotty_fire-03",
                "wp_switch-01", "wp_switch-02", "wp_switch-03"
            };

            // Add names to the ListBox and the pack dictionary
            foreach (string Name in SoundsName)
            {
                Pack.Add(Name, null);
                SoundsListItems.Add(new SoundsListItem { Name = Name, Edited = "#00FFFFFF" });
            }

            if (openFile != null)
            {
                bool opened = OpenPack(openFile);

                if (!opened)
                {
                    string Message = localization.ResourceManager.GetString("dialog_error_open_selected-pack-corrupted_message");
                    string Title = localization.ResourceManager.GetString("dialog_error_open_selected-pack-corrupted_message");

                    MessageBox.Show(Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }  
            }
        }

        // Show to the user if the pack is saved
        public void SetSaveState(bool saved)
        {
            string Save_state = "*";
            if (saved) Save_state = "";

            if (PackSavePath.Length == 0) Title = "DDrace SoundPack Creator";

            else Title = "DDSPC - " + Path.GetFileNameWithoutExtension(PackSavePath) + Save_state;

            PackIsSaved = saved;
        }

        // Show to the user if the window is currently loading
        public void SetLoadState(bool loading)
        {
            // Change the mouse cursor
            if (loading) Mouse.SetCursor(Cursors.AppStarting);
            else Mouse.SetCursor(Cursors.Arrow);

            Window.IsEnabled = !loading;
        }

        // Remove each data of the current pack
        public void Reset()
        {
            SoundsList.SelectedItem = null;

            // Remove each values of the pack dictionary
            string[] PackKeys = new string[131];
            Pack.Keys.CopyTo(PackKeys, 0);

            foreach (string key in PackKeys)
            {
                Pack[key] = null;
            }

            // Set the color of each items of the ListBox to white
            ItemCollection SoundItems = SoundsList.Items;

            foreach (SoundsListItem item in SoundItems)
            {
                item.Edited = "#00FFFFFF";
            }

            SoundsList.Items.Refresh();

            // Clear all UI inputs
            Author_input.Clear();
            Description_input.Clear();
            New_sound_name.Content = "";
            New_sound_name.ToolTip = "";

            // Refresh UI
            New_sound_close_sound_button.IsEnabled = false;
            New_sound_open_sound_button.IsEnabled = false;
            New_sound_slider.IsEnabled = false;
            New_sound_play_button.IsEnabled = false;

            // Set save state
            PackSavePath = "";

            SetSaveState(true);
        }

        // Ask the user where save the pack
        public void SaveAs()
        {
            // Ask where save the pack
            SaveFileDialog SavePackDialog = new SaveFileDialog
            {
                Filter = "DDraceSoundPack file (*.ddsp)|*.ddsp",
                Title = localization.ResourceManager.GetString("dialog_select_save-pack_title")
            };

            string InitialDirectory = Settings.Default.SavePackFolderPath;

            if (InitialDirectory.Length == 0)
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (PackSavePath.Length > 0)
                SavePackDialog.FileName = Path.GetFileNameWithoutExtension(PackSavePath);

            SavePackDialog.InitialDirectory = InitialDirectory;

            if ((bool)SavePackDialog.ShowDialog())
            {
                string FileName = SavePackDialog.FileName;

                Settings.Default.SavePackFolderPath = Path.GetDirectoryName(FileName); // Update settings

                PackSavePath = FileName;

                Save();
            }
        }

        // Write the pack in the storage, at the location selected by "SaveAs"
        public void Save()
        {
            // If there are now way where save the file, ask the user
            if (PackSavePath.Length == 0)
            {
                SaveAs();
                return;
            }

            SetLoadState(true);
            try
            {
                using (FileStream ZipFile = File.Open(PackSavePath, FileMode.Create))
                {
                    using (ZipArchive Archive = new ZipArchive(ZipFile, ZipArchiveMode.Create))
                    {
                        // Write the Author file
                        ZipArchiveEntry AuthorFile = Archive.CreateEntry("Author");
                        using (StreamWriter writer = new StreamWriter(AuthorFile.Open()))
                        {
                            writer.Write(Author_input.Text);
                        }

                        // Write the Description file
                        ZipArchiveEntry DescriptionFile = Archive.CreateEntry("Description");
                        using (StreamWriter writer = new StreamWriter(DescriptionFile.Open()))
                        {
                            writer.Write(Description_input.Text);
                        }

                        // Write all edited sounds file
                        foreach (KeyValuePair<string, string> Sound in Pack)
                        {
                            if (Sound.Value == null) continue; // if the default-sound is not edited, continue

                            string OutputWavName = TEMP_FOLDER_PATH + Sound.Key + ".wav";
                            string OutputWvName = Sound.Key + ".wv";

                            string SourceFile = Sound.Value;

                            // Convert the sound to .WAV format if needed to the temp folder
                            bool SourceConverted = AudioConverter.AudioToWav(SourceFile, OutputWavName);

                            if (!SourceConverted) OutputWavName = SourceFile;

                            // Compress the sound to .WV file in the temp folder
                            Wavpack.Compress(OutputWavName, TEMP_FOLDER_PATH + OutputWvName);

                            // Add the compressed file to the archive
                            Archive.CreateEntryFromFile(TEMP_FOLDER_PATH + OutputWvName, OutputWvName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string message = localization.ResourceManager.GetString("dialog_error_save_message");
                string title = localization.ResourceManager.GetString("dialog_error_save_title");

                MessageBox.Show(message + e, title, MessageBoxButton.OK, MessageBoxImage.Error);
                SetLoadState(false);

                return;
            }

            SetSaveState(true);
            SetLoadState(false);
        }

        // Show the SelectGame window and hide this window
        private void PrepareInstallPack()
        {
            SetLoadState(true);

            SelectGameWindow = new SelectGameWindow();
            SelectGameWindow.Closed += new EventHandler(SelectGameWindow_Closed);

            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;

            SelectGameWindow.OnGameSelected += new EventHandler(GameSelected);

            SelectGameWindow.Show();
            SelectGameWindow.Activate();
        }

        // When the SelectGame window is closed
        // Display back this window
        private void SelectGameWindow_Closed(object sender, EventArgs e)
        {
            SetLoadState(false);
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
        }

        // When the game in select from the SelectGame window
        // Start the installation
        private void GameSelected(object sender, EventArgs e)
        {
            OnGameEventArgs Args = (OnGameEventArgs)e;

            if (!Settings.Default.AllGamePath.Contains(Args.Path))
                Settings.Default.AllGamePath.Add(Args.Path);

            Args.Game.StartInfo.FileName = Args.Path;

            InstallPack(Args.Game);
        }

        // Verify if the pack is saved and start to install it
        private void InstallPack(Process game)
        {
            // Verify if the pack is saved
            if (!File.Exists(PackSavePath))
            {
                string message = localization.ResourceManager.GetString("dialog_error_install_pack-not-exists_message");
                string title = localization.ResourceManager.GetString("dialog_error_install_pack-not-exists_title");

                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Install the pack for this game
            PackInstaller.Install(PackSavePath, game);

            if (TempPackSavePath.Length != 0) PackSavePath = TempPackSavePath;
            TempPackSavePath = "";
        }

        // Try to open a pack from the input path
        public bool OpenPack(string packPath)
        {
            // If the file doesn't exists, cancel
            if (!File.Exists(packPath))
            {
                string message = localization.ResourceManager.GetString("dialog_error_open_selected-pack-not-exists_message");
                string title = localization.ResourceManager.GetString("dialog_error_open_selected-pack-not-exists_title");

                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }

            SetLoadState(true);
            Reset();

            SoundsList.SelectedItem = null;

            try
            {
                using (ZipArchive OpenPackFile = ZipFile.OpenRead(packPath))
                {
                    foreach (ZipArchiveEntry Entry in OpenPackFile.Entries)
                    {
                        string Name = Path.GetFileNameWithoutExtension(Entry.Name);
                        string Extension = Path.GetExtension(Entry.Name);

                        // Get author name
                        if (Name.Equals("Author", StringComparison.Ordinal))
                        {
                            StreamReader reader = new StreamReader(Entry.Open());
                            Author_input.Text = reader.ReadToEnd();
                            reader.Close();
                        }

                        // Get Description text
                        else if (Name.Equals("Description", StringComparison.Ordinal))
                        {
                            StreamReader reader = new StreamReader(Entry.Open());
                            Description_input.Text = reader.ReadToEnd();
                            reader.Close();
                        }

                        // If there is something else in the file, just ignore it
                        if (!Extension.Equals(".wv", StringComparison.OrdinalIgnoreCase)) continue;

                        // Extract the file to the temp folder
                        string ExtractDestination = Path.GetFullPath(Path.Combine(TEMP_FOLDER_PATH, Entry.FullName));
                        Entry.ExtractToFile(ExtractDestination, true);

                        // Uncompress the file to the same place
                        string UncompressDestination = ExtractDestination.Replace(".wv", ".wav");
                        Wavpack.Uncompress(ExtractDestination, UncompressDestination);

                        // Update the Pack dictionary
                        if (!Pack.ContainsKey(Name)) continue;

                        Pack[Name] = UncompressDestination;

                        // Update the UI
                        ItemCollection SoundItems = SoundsList.Items;

                        foreach (SoundsListItem Item in SoundItems)
                        {
                            if (Name.Equals(Item.Name, StringComparison.Ordinal))
                            {
                                Item.Edited = "#5fd69d00";
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                string message = localization.ResourceManager.GetString("dialog_error_open_selected-pack-corrupted_message");
                string title = localization.ResourceManager.GetString("dialog_error_open_selected-pack-corrupted_title");

                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

                Reset();
                return false;
            }

            PackSavePath = packPath;

            SoundsList.Items.Refresh();
            SetSaveState(true);
            SetLoadState(false);

            return true;
        }

        // Try to open a pack from a .ddsp file selected by the user
        private bool OpenPackByUser()
        {
            SetLoadState(true);

            // Reset all input and ask for save if the pack is not saved
            if (!PackIsSaved)
            {
                string message = localization.ResourceManager.GetString("dialog_warning_pack-not-saved_message1");
                string title = localization.ResourceManager.GetString("dialog_warning_pack-not-saved_message1");

                MessageBoxResult MsgBoxResult = MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (MsgBoxResult == MessageBoxResult.Yes) Save();
                else if (MsgBoxResult == MessageBoxResult.Cancel) return false;
            }

            // Select the pack file
            OpenFileDialog SelectPackDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "DDraceSoundPack file (*.ddsp)|*.ddsp",
                CheckFileExists = true,
                CheckPathExists = true
            };

            string InitialDirectory = Settings.Default.OpenPackFolderPath;

            if (InitialDirectory.Length == 0)
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            SelectPackDialog.InitialDirectory = InitialDirectory;

            if (!(bool)SelectPackDialog.ShowDialog())
            {
                SetLoadState(false);
                return false;
            }

            string OpenPackPath = SelectPackDialog.FileName;

            return OpenPack(OpenPackPath);
        }

        // Ask to save the pack if is not saved, save settings and stop threads
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Ask to save the pack if is not saved
            if (!PackIsSaved)
            {
                string message = localization.ResourceManager.GetString("dialog_warning_pack-not-saved_message1");
                string title = localization.ResourceManager.GetString("dialog_warning_pack-not-saved_title1");

                MessageBoxResult MsgBoxResult = MessageBox.Show(message, title, MessageBoxButton.YesNoCancel);

                if (MsgBoxResult == MessageBoxResult.Yes)
                {
                    Save();
                }
                else if (MsgBoxResult == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Save settings
            Settings.Default.Save();

            // Stop
            SliderTimer.Stop();

            Application.Current.Shutdown();
        }

        // Timer event to refresh the slider 
        private void Time_tick(object sender, EventArgs e)
        {
            // Update the slider only if mouse doesn't interact with the slider and if the sound player is playing
            if (!New_sound_slider.IsMouseCaptureWithin && NewSoundPlayerIsPlaying)
            {
                New_sound_slider.Value = NewSoundPlayer.Position.Ticks;
            }

            // Update the maximum of the slider
            if (NewSoundPlayer.NaturalDuration.HasTimeSpan)
            {
                New_sound_slider.Maximum = NewSoundPlayer.NaturalDuration.TimeSpan.Ticks;
            }
        }

        // When the user select a default-sound from the ListBox
        // Update the UI depending of which default-sound is selected
        private void SoundsList_SelectionChanged(object sender, MouseButtonEventArgs e)
        {
            SoundsListItem Select = (SoundsListItem)SoundsList.SelectedItem;

            if (Select == null) return;

            New_sound_open_sound_button.IsEnabled = true;

            string Sound_path = Pack[Select.Name];

            if (Sound_path != null) // The default-sound have been edited and selectioned again
            {
                New_sound_name.Content = Path.GetFileName(Sound_path);
                New_sound_name.ToolTip = Sound_path;

                New_sound_close_sound_button.IsEnabled = true;

                Uri Sound_uri = new Uri(Sound_path);

                NewSoundPlayer.Close();
                NewSoundPlayer.Open(Sound_uri);
                NewSoundPlayer.Position = TimeSpan.Zero;
                New_sound_slider.Value = 0;
                New_sound_play_button.IsEnabled = true;
                NewSoundPlayerIsPlaying = false;
                New_sound_play_button.Content = "⏵";
                New_sound_slider.IsEnabled = true;
            }
            else // The default-sound is not modified
            {
                New_sound_name.Content = "";
                New_sound_name.ToolTip = "";

                New_sound_close_sound_button.IsEnabled = false;

                NewSoundPlayer.Close();
                NewSoundPlayer.Position = TimeSpan.Zero;
                New_sound_slider.Value = 0;
                New_sound_play_button.IsEnabled = false;
                NewSoundPlayerIsPlaying = false;
                New_sound_play_button.Content = "⏵";
                New_sound_slider.IsEnabled = false;
            }

        }

        // When the "⏵" button of a default-sound is clicked
        // Play the default-sound
        private void Sound_play_button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            string name = button.DataContext.ToString().Replace(" ", "_");

            UnmanagedMemoryStream SoundStream = Default_sounds.Get(name); // get the default-sound stream from ressources

            DefaultSoundPlayer = new System.Media.SoundPlayer(SoundStream);
            DefaultSoundPlayer.Play(); // play the sound
        }

        // When the user click on the button "..."
        // Ask the user to select a new sound file, and add it to the pack
        private void New_sound_open_sound_button_Click(object sender, RoutedEventArgs e)
        {
            // Ask the user to select a new sound file
            OpenFileDialog SoundFileSelector = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Sound Files(*.wav;*.wv;*.mp3;*.mp4;*.m4a;*.wma;*.aac;*.flac;*.aiff)|*.wav;*.wv;*.mp3;*.mp4;*.m4a;*.wma;*.aac;*.flac;*.aiff|Wav (*.wav)|*.wav|Wv (*.wv)|*.wv|Mp3 (*.mp3)|*.mp3|Mp4 (*.mp4)|*.mp4|M4a (*.m4a)|*.m4a|Wma (*.wma)|*.wma|Aac (*.aac)|*.aac|Flac (*.flac)|*.flac|Aiff (*.aiff)|*.aiff"
            };

            string InitialDirectory = Settings.Default.OpenSoundFolderPath;

            if (InitialDirectory.Length == 0)
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

            SoundFileSelector.InitialDirectory = InitialDirectory;

            if ((bool)SoundFileSelector.ShowDialog())
            {
                Settings.Default.OpenSoundFolderPath = Path.GetDirectoryName(SoundFileSelector.FileName); // update settings

                // Update UI
                New_sound_name.Content = Path.GetFileName(SoundFileSelector.FileName);
                New_sound_name.ToolTip = SoundFileSelector.FileName;

                New_sound_close_sound_button.IsEnabled = true;

                // Init the sound player
                Uri Sound_uri = new Uri(SoundFileSelector.FileName);

                NewSoundPlayer.Close();
                NewSoundPlayer.Open(Sound_uri);

                // Update UI
                NewSoundPlayer.Position = TimeSpan.Zero;
                New_sound_slider.Value = 0;
                New_sound_play_button.IsEnabled = true;
                NewSoundPlayerIsPlaying = false;
                New_sound_play_button.Content = "⏵";
                New_sound_slider.IsEnabled = true;

                SoundsListItem Select = (SoundsListItem)SoundsList.SelectedItem;

                if (Select == null) return;

                Select.Edited = "#5fd69d00";
                Pack[Select.Name] = SoundFileSelector.FileName;

                SetSaveState(false);

                SoundsList.Items.Refresh();
            }
        }

        // When the "X" button is pressed
        // Remove the sound selected by the user before and update the UI
        private void New_sound_close_sound_button_Click(object sender, RoutedEventArgs e)
        {
            // Stop the thread of the slider and the sound player
            SliderTimer.Stop();
            NewSoundPlayer.Stop();

            // Update UI
            New_sound_name.Content = "";
            New_sound_name.ToolTip = "";
            New_sound_close_sound_button.IsEnabled = false;
            New_sound_play_button.IsEnabled = false;
            New_sound_slider.IsEnabled = false;

            SoundsListItem Select = (SoundsListItem)SoundsList.SelectedItem;

            if (Select == null) return;

            Select.Edited = "#00FFFFFF";
            Pack[Select.Name] = null; // Remove the sound selected by the user

            SetSaveState(false);

            SoundsList.Items.Refresh();
        }

        // When the user press the "⏵" button
        // Start or pause the sound and start or stop the SliderTimer depending of the state of the sound player
        private void New_sound_play_button_Click(object sender, RoutedEventArgs e)
        {
            // If the source sound file of the player is empty, just do nothing
            if (NewSoundPlayer.Source == null) return;

            if (NewSoundPlayerIsPlaying) // if the sound player is already playing
            {
                NewSoundPlayer.Pause();
                SliderTimer.Stop();

                // Update UI
                NewSoundPlayerIsPlaying = false;
                New_sound_play_button.Content = "⏵";
            }
            else // if the sound player is in pause or stopped
            {
                SliderTimer.Start();
                NewSoundPlayer.Play();

                // Update UI
                NewSoundPlayerIsPlaying = true;
                New_sound_play_button.Content = "❚❚";
            }
        }

        // When the mouse stops being pressed on the slider
        // Update the sound player position depending of the position of the slider
        private void New_sound_slider_LostMouseCapture(object sender, MouseEventArgs e)
        {
            NewSoundPlayer.Position = TimeSpan.FromTicks((long)New_sound_slider.Value);
        }

        // When the sound player finished to play the sound
        // Update the UI and some states
        private void New_sound_player_finished(object sender, EventArgs e)
        {
            NewSoundPlayer.Stop();
            SliderTimer.Stop();

            // Update UI
            NewSoundPlayer.Position = TimeSpan.Zero;
            New_sound_slider.Value = 0;
            New_sound_play_button.Content = "⏵";
            NewSoundPlayerIsPlaying = false;
        }

        // When the Author text input changed
        // Update the UI to show that the pack is not saved
        private void Author_input_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetSaveState(false);
        }

        // When the Description text input changed
        // Update the UI to show that the pack is not saved
        private void Description_input_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetSaveState(false);
        }

        // When the "New" button from the submenu of "File" is clicked
        // Ask to save if the pack is not saved and reset the UI
        private void Menu_new_Click(object sender, RoutedEventArgs e)
        {
            // Ask to save if the pack is not saved
            if (!PackIsSaved)
            {
                string message = localization.ResourceManager.GetString("dialog_warning_pack-not-saved_message1");
                string title = localization.ResourceManager.GetString("dialog_warning_pack-not-saved_title1");

                MessageBoxResult MsgBoxResult = MessageBox.Show(message, title, MessageBoxButton.YesNoCancel);

                if (MsgBoxResult == MessageBoxResult.Yes)
                {
                    Save();
                }
                else if (MsgBoxResult == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            // Just reset everything
            Reset();
        }

        // When the "Open" button from the submenu of "File" is clicked
        // Try to open a pack from a .ddsp file selected by the user
        private void Menu_open_Click(object sender, RoutedEventArgs e)
        {
            OpenPackByUser();
        }

        // When the "Save" button from the submenu of "File" is clicked
        // Try to save the pack
        private void Menu_save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        // When the "Save as" button from the submenu of "File" is clicked
        // Ask the user where save the pack and try to save it
        private void Menu_save_as_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        // When the "Quit" button from the submenu of "File" is clicked
        // Ask the user to save the pack if it is not saved and close the program
        private void Menu_quit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // When the "Install current" button from the submenu of "Install" is clicked
        // Ask the user to save the pack if it is not saved and install the pack
        private void Menu_install_current_Click(object sender, RoutedEventArgs e)
        {
            // Ask the user to save the pack if it is not saved
            if (PackSavePath.Length == 0)
            {
                string AskSaveMessage = localization.ResourceManager.GetString("dialog_warning_install_need-save-before_message1");
                string AskSaveTitle = localization.ResourceManager.GetString("dialog_warning_install_need-save-before_title1");

                MessageBoxResult AskSaveDialog = MessageBox.Show(AskSaveMessage, AskSaveTitle, MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                if (AskSaveDialog == MessageBoxResult.OK) SaveAs();
                return;
            }

            PrepareInstallPack();
        }

        // When the "Open and install" button from the submenu of "Install" is clicked
        // Try to open a pack from a .ddsp file selected by the user and install it directly
        private void Menu_open_install_Click(object sender, RoutedEventArgs e)
        {
            // If the pack have been open correctly then install
            if (OpenPackByUser())
                PrepareInstallPack();
        }

        // When the "Restore default" button from the submenu of "Install" is clicked
        // Open a new window who will try to restore the default sound pack of the game
        private void Menu_install_default_Click(object sender, RoutedEventArgs e)
        {
            string DefaultPackPath = Path.Combine(Directory.GetCurrentDirectory(), TEMP_FOLDER_PATH, "Default.ddsp");

            using (FileStream DefaultPackFile = new FileStream(DefaultPackPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                // Get the default pack file from ressources
                byte[] DefaultPack = Properties.Resources.DefaultSoundPack;

                try
                {
                    // Write the default pack to the storage
                    DefaultPackFile.Write(DefaultPack, 0, DefaultPack.Length);
                }
                catch (Exception)
                {
                    string message = localization.ResourceManager.GetString("dialog_error_install_default_error-occured_message");
                    string title = localization.ResourceManager.GetString("dialog_error_install_default_error-occured_title");

                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            TempPackSavePath = PackSavePath;
            PackSavePath = DefaultPackPath;

            PrepareInstallPack();
        }

        // When the "Credits" button from the submenu of "Help" is clicked
        // Show the credits window
        private void Menu_help_credits_Click(object sender, RoutedEventArgs e)
        {
            if (CreditsWindow.IsLoaded)
                CreditsWindow.Focus();
            else
                CreditsWindow = new Credits();
            CreditsWindow.Show();
        }

        // When "New" shortcut is executed by the user
        private void Bind_new_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Menu_new_Click(sender, e);
        }

        // When "Open" shortcut is executed by the user
        private void Bind_open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Menu_open_Click(sender, e);
        }

        // When "Save" shortcut is executed by the user
        private void Bind_save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Menu_save_Click(sender, e);
        }

        // When "Save as" shortcut is executed by the user
        private void Bind_save_as_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Menu_save_as_Click(sender, e);
        }
    }

    public class SoundsListItem
    {
        public string Name { get; set; } // Name of the default-sound
        public string Edited { get; set; } // The color of the ListItem background
    }
}