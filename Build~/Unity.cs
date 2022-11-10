using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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

            try
            {
                int result = Utility.Run(this.ExecutablePath, args.Concat(new[]
                {
                    "-logFile",
                    log
                }), false);

                // Some part of Unity, maybe its crash reporter, will sometimes continue to
                // write to the log after the process has ended. That will prevent us from
                // being able to read it. Just keep trying.
                string? logText = null;
                while (logText == null)
                {
                    try
                    {
                        logText = File.ReadAllText(log);
                    }
                    catch (IOException)
                    {
                    }
                }

                Console.WriteLine(logText);

                if (result != 0)
                    throw new Exception("An error occurred while executing Unity.");
            }
            finally
            {
                File.Delete(log);
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
                    string path = "/Applications/Unity/Hub/Editor/{version}/Unity.app";
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
                return new Unity(Path.Combine(versions.Last()!.FullPath, "Unity.app"));

            return null;
        }
    }
}
