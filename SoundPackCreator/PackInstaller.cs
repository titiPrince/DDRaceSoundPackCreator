using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace SoundPackCreator
{
    internal class PackInstaller
    {
        // Audio folder from the main directory of the game
        // Luckily its the same path for DDNet and Teeworld clients
        public static string SoundPathFromGame { get; } = @"\data\audio\";

        // Will install the pack from "packPath" to the game from "game"
        public static void Install(string packPath, Process game)
        {
            using (ZipArchive PackFile = ZipFile.Open(packPath, ZipArchiveMode.Read))
            {
                string GamePath = game.StartInfo.FileName;
                string GameSoundPath = Path.GetDirectoryName(GamePath) + SoundPathFromGame;

                // List each file of the pack
                foreach (ZipArchiveEntry entry in PackFile.Entries)
                {
                    string ExtractFilePath = GameSoundPath + entry.Name;

                    // Extract the file from the archive to the audio folder of the game
                    entry.ExtractToFile(ExtractFilePath, true);
                }



                // Restart the game to apply the pack
                try
                {
                    if (!game.CloseMainWindow())
                    {
                        game.Kill();
                    }

                    game.WaitForExit();
            
                    Process.Start(GamePath);
                }
                catch { }
            }
        }
    }
}
