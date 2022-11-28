using System;
using System.Diagnostics;
using System.IO;

namespace SoundPackCreator
{
    public class Wavpack
    {
        readonly string Compressor;
        readonly string Decompressor;

        public Wavpack()
        {
            // The path of the compressor and decompressor in storage
            Compressor = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "wavpack.exe");
            Decompressor = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "wvunpack.exe");

            if (!(File.Exists(Compressor) & File.Exists(Decompressor)))
                throw new Exception("The files wavpack.exe and wavunpack.exe doesn't exists.");
        }

        // Will compress the file from wav to a wv file
        public void Compress(string wav, string wv)
        {
            if (!File.Exists(wav))
                throw new FileNotFoundException(wav);

            if (Path.GetExtension(wav).ToLower() != ".wav")
                throw new ArgumentException("The first parameter need to be a .wav file");

            // Execute the program
            Process Process = new Process();

            Process.StartInfo.FileName = Compressor;
            Process.StartInfo.Arguments = "-y \"" + wav + "\" \"" + wv + "\""; // the "y" parameter force to answer "yes"
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = false;
            Process.StartInfo.RedirectStandardError = false;
            Process.StartInfo.CreateNoWindow = true;

            Process.Start();
            Process.WaitForExit();
        }

        // Will decompress the file from wv to a wav file
        public void Uncompress(string wv, string wav)
        {
            if (!File.Exists(wv))
                throw new FileNotFoundException(wv);

            if (Path.GetExtension(wv).ToLower() != ".wv")
                throw new ArgumentException("The first parameter need to be a .wv file");

            // Execute the program
            Process Process = new Process();

            Process.StartInfo.FileName = Decompressor;
            Process.StartInfo.Arguments = "-y \"" + wv + "\" \"" + wav + "\"";
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = false;
            Process.StartInfo.RedirectStandardError = false;
            Process.StartInfo.CreateNoWindow = true;

            Process.Start();
            Process.WaitForExit();
        }
    }
}