using System;
using System.Collections.Generic;
using System.IO;

namespace Build
{
    public class Cmake
    {
        public readonly string ExecutablePath;

        public Cmake(string? executablePath = null)
        {
            if (executablePath != null)
                this.ExecutablePath = executablePath;
            else if (OperatingSystem.IsMacOS() && File.Exists("/Applications/CMake.app/Contents/bin/cmake"))
                this.ExecutablePath = "/Applications/CMake.app/Contents/bin/cmake";
            else
                // Expect it to be in the path
                this.ExecutablePath = "cmake";
        }

        public int Run(IEnumerable<string> args)
        {
            return Utility.Run(this.ExecutablePath, args);
        }
    }
}
