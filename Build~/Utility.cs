using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Build
{
    public class Utility
    {
        public static int Run(string executable, IEnumerable<string> args, bool throwOnError = true)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(executable);
            startInfo.WorkingDirectory = Utility.PackageRoot;
            foreach (string arg in args)
                startInfo.ArgumentList.Add(arg);

            return Utility.RunAndLog(startInfo, null, throwOnError);
        }

        public static int RunAndLog(ProcessStartInfo startInfo, TextWriter? log = null, bool throwOnError = true)
        {
            if (log == null)
                log = Console.Out;

            using (Process configure = new Process())
            {
                configure.OutputDataReceived += (sender, e) =>
                {
                    log.WriteLine(e.Data);
                    log.Flush();
                };
                configure.ErrorDataReceived += (sender, e) =>
                {
                    log.WriteLine(e.Data);
                    log.Flush();
                };
                configure.StartInfo = startInfo;
                configure.StartInfo.RedirectStandardOutput = true;
                configure.StartInfo.RedirectStandardError = true;
                configure.Start();
                configure.BeginOutputReadLine();
                configure.BeginErrorReadLine();
                configure.WaitForExit();

                if (throwOnError && configure.ExitCode != 0)
                {
                    throw new Exception($"An error occurred while executing:{Environment.NewLine}{startInfo.FileName} {startInfo.Arguments}");
                }

                return configure.ExitCode;
            }
        }

        public static string ProjectRoot
        {
            get
            {
                // Assumes this package is in the project's Packages/com.cesium.unity directory or similar.
                return Path.GetFullPath(Path.Combine(PackageRoot, "..", ".."));
            }
        }

        public static string PackageRoot
        {
            get
            {
                return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(GetSourceFilePathName()) ?? "", ".."));
            }
        }

        private static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null)
        {
            return callerFilePath == null ? "" : callerFilePath;
        }
    }
}
