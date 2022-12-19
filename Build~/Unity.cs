using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Build
{
    public class Unity
    {
        public readonly string ExecutablePath;

        public Unity(string executablePath)
        {
            this.ExecutablePath = executablePath;
        }

        public void Run(IEnumerable<string> args)
        {
            string log = Path.GetTempFileName();
            File.WriteAllText(log, "", Encoding.UTF8);

            try
            {
                using (Process unity = new Process())
                {
                    unity.OutputDataReceived += (sender, e) =>
                    {
                        Console.WriteLine(e.Data);
                        //Console.Flush();
                    };
                    unity.ErrorDataReceived += (sender, e) =>
                    {
                        Console.WriteLine(e.Data);
                        //log.Flush();
                    };

                    ProcessStartInfo startInfo = new ProcessStartInfo(this.ExecutablePath);
                    startInfo.WorkingDirectory = Utility.PackageRoot;
                    foreach (string arg in args)
                        startInfo.ArgumentList.Add(arg);
                    startInfo.ArgumentList.Add("-logFile");
                    startInfo.ArgumentList.Add(log);

                    unity.StartInfo = startInfo;
                    unity.StartInfo.RedirectStandardOutput = true;
                    unity.StartInfo.RedirectStandardError = true;
                    unity.Start();
                    unity.BeginOutputReadLine();
                    unity.BeginErrorReadLine();

                    using (StreamReader reader = new StreamReader(log, Encoding.UTF8, false, new FileStreamOptions()
                    {
                        Mode = FileMode.Open,
                        Access = FileAccess.Read,
                        Share = FileShare.ReadWrite
                    }))
                    {
                        string? output = null;
                        while (!unity.HasExited)
                        {
                            output = reader.ReadLine();
                            if (output != null)
                                Console.WriteLine(output);
                            else
                                Thread.Sleep(1);
                        }

                        output = reader.ReadLine();
                        if (output != null)
                            Console.WriteLine(output);
                    }

                    // This should return immediately.
                    unity.WaitForExit();

                    if (unity.ExitCode != 0)
                    {
                        throw new Exception($"An error (code {unity.ExitCode}) occurred while executing:{Environment.NewLine}{startInfo.FileName} {string.Join(' ', startInfo.ArgumentList)}");
                    }
                }
            }
            finally
            {
                // Unity seems to keep the log file open longer than expected sometimes, preventing
                // us from deleting it.
                // Don't let this fail the build, but do retry a few times.
                for (int i = 0; i < 5; ++i)
                {
                    try
                    {
                        File.Delete(log);
                        break;
                    }
                    catch (IOException)
                    {
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        private class UnityVersion
        {
            public string Name = "";
            public string FullPath = "";
            public int Major;
            public int Minor;
            public int Patch;
        }

        public static Unity? FindUnity(string? version = null)
        {
            DirectoryInfo? unityDir = null;

            if (OperatingSystem.IsWindows())
            {
                if (version != null)
                {
                    string path = $"C:\\Program Files\\Unity\\Hub\\Editor\\{version}\\Editor\\Unity.exe";
                    if (File.Exists(path))
                        return new Unity(path);
                }

                unityDir = new DirectoryInfo("C:\\Program Files\\Unity\\Hub\\Editor\\");
            }
            else if (OperatingSystem.IsMacOS())
            {
                if (version != null)
                {
                    string path = "/Applications/Unity/Hub/Editor/{version}/Unity.app/Contents/MacOS/Unity";
                    if (File.Exists(path))
                        return new Unity(path);
                }

                unityDir = new DirectoryInfo("/Applications/Unity/Hub/Editor/");
            }

            if (unityDir == null)
                return null;

            DirectoryInfo[] subDirectories = unityDir.GetDirectories();
            List<UnityVersion?> versions = subDirectories.Select((di) =>
            {
                string[] parts = di.Name.Split(".", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3)
                    return null;
                return new UnityVersion()
                {
                    Name = di.Name,
                    FullPath = di.FullName,
                    Major = int.Parse(parts[0]),
                    Minor = int.Parse(parts[1]),
                    Patch = int.Parse(parts[2], System.Globalization.NumberStyles.HexNumber)
                };
            }).ToList();

            versions.Sort((a, b) =>
            {
                if (a == null && b == null)
                    return 0;
                else if (a == null)
                    return -1;
                else if (b == null)
                    return 1;
                else if (a.Major != b.Major)
                    return a.Major.CompareTo(b.Major);
                else if (a.Minor != b.Minor)
                    return a.Minor.CompareTo(b.Minor);
                else if (a.Patch != b.Patch)
                    return a.Patch.CompareTo(b.Patch);
                else
                    return 0;
            });

            if (versions.Count == 0 || versions.Last() == null)
                return null;

            if (OperatingSystem.IsWindows())
                return new Unity(Path.Combine(versions.Last()!.FullPath, "Editor", "Unity.exe"));
            else if (OperatingSystem.IsMacOS())
                return new Unity(Path.Combine(versions.Last()!.FullPath, "Unity.app", "Contents", "MacOS", "Unity"));

            return null;
        }
    }
}
