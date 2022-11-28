using NAudio.Wave;
using System;
using System.IO;

namespace SoundPackCreator
{
    // Use the lib of NAudio. Thanks to them
    // https://github.com/naudio/NAudio

    internal class AudioConverter
    {
        internal static void MediaToWav(string source, string wav)
        {
            using (MediaFoundationReader reader = new MediaFoundationReader(source))
            {
                WaveFileWriter.CreateWaveFile(wav, reader);
            }
        }

        // This method detect automatically which type of file is in source
        public static bool AudioToWav(string source, string wav)
        {
            string SoundExtension = Path.GetExtension(source);

            switch (SoundExtension.ToLower())
            {
                case ".wav":
                case ".wv":
                    return false;

                case ".mp3":
                    Mp3ToWav(source, wav);
                    break;

                case ".mp4":
                    Mp4ToWav(source, wav);
                    break;

                case ".m4a":
                    M4aToWav(source, wav);
                    break;

                case ".wma":
                    WmaToWav(source, wav);
                    break;

                case ".aac":
                    AacToWav(source, wav);
                    break;

                case ".flac":
                    FlacToWav(source, wav);
                    break;

                case ".aiff":
                    AiffToWav(source, wav);
                    break;

                default:
                    throw new ArgumentException("The first parameter need to be WAV/WV/MP3/MP4/M4A/WMA/AAC/FLAC/AIFF file");
            }

            return true;
        }

        public static void Mp3ToWav(string source, string wav)
        {
            MediaToWav(source, wav);
        }

        public static void Mp4ToWav(string source, string wav)
        {
            MediaToWav(source, wav);
        }

        public static void M4aToWav(string source, string wav)
        {
            MediaToWav(source, wav);
        }

        public static void WmaToWav(string source, string wav)
        {
            MediaToWav(source, wav);
        }

        public static void AacToWav(string source, string wav)
        {
            MediaToWav(source, wav);
        }

        public static void FlacToWav(string source, string wav)
        {
            MediaToWav(source, wav);
        }

        public static void AiffToWav(string source, string wav)
        {
            using (AiffFileReader reader = new AiffFileReader(source))
            {
                using (WaveFileWriter writer = new WaveFileWriter(wav, reader.WaveFormat))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead = 0;

                    do
                    {
                        bytesRead = reader.Read(buffer, 0, buffer.Length); // Reads the content of the file for 4096 bytes and write them into the buffer
                        writer.Write(buffer, 0, bytesRead); // Write the buffer content to the file
                    } while (bytesRead > 0); // If there is nothing more to read, its finished
                }
            }
        }
    }
}