using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif

namespace CesiumForUnity
{
    internal struct PlatformToBuild
    {
        public BuildTargetGroup platformGroup;
        public BuildTarget platform;
        public bool isDevelopment;
        public bool isCleanBuild;
    }

    /// <summary>
    /// When the user builds a Player (built game) in the Unity Editor, this class manages
    /// automatically compiling a suitable version of the native C++ CesiumForUnityNative
    /// shared library to go with it.
    /// </summary>
    internal class CompileCesiumForUnityNative :
        AssetPostprocessor,
        IPreprocessBuildWithReport,
        IPostBuildPlayerScriptDLLs
    {
        internal class LibraryToBuild
        {
            public BuildTarget Platform = BuildTarget.StandaloneWindows64;
            public BuildTargetGroup PlatformGroup = BuildTargetGroup.Standalone;
            public string Cpu = null;
            public string SourceDirectory = "";
            public string BuildDirectory = "build";
            public string GeneratedDirectoryName = "generated-Unknown";
            public string Configuration = "RelWithDebInfo";
            public string InstallDirectory = "";
            public bool CleanBuild = false;
            public string Toolchain;
            public List<string> ExtraConfigureArgs = new List<string>();
            public List<string> ExtraBuildArgs = new List<string>();
        }

        // This field is static because OnPreprocessBuild and OnPreprocessAsset are called on different
        // instances of this class.
        private static Dictionary<string, LibraryToBuild> importsInProgress = new Dictionary<string, LibraryToBuild>();

        /// <summary>
        /// At the start of the build, create placeholders for the CesiumForUnityNative shared
        /// libraries that will be produced during the build.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is necessary because, if a shared library is not present at the start of the build,
        /// Unity won't pick it up if it is created during the build. However, as long as it exists,
        /// Unity will pick up the latest version.
        /// </para>
        /// <para>
        /// The shared library assets are imported synchronously, and Unity will call
        /// `OnPreprocessAsset` at the start of the import in order to allow us to set
        /// the import settings.
        /// </para>
        /// </remarks>
        /// <param name="report"></param>
        public void OnPreprocessBuild(BuildReport report)
        {
            importsInProgress.Clear();

            AssetDatabase.StartAssetEditing();
            try
            {
                LibraryToBuild[] libraries = GetLibrariesToBuildForPlatform(report.summary, finalLibrariesOnly: true);
                foreach (LibraryToBuild library in libraries)
                {
                    CreatePlaceholders(library, "CesiumForUnityNative-Runtime");
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                importsInProgress.Clear();
            }
        }

        internal static void CreatePlaceholders(LibraryToBuild libraryToBuild, string sharedLibraryName)
        {
            Directory.CreateDirectory(libraryToBuild.InstallDirectory);

            string libraryFilename = GetSharedLibraryFilename(sharedLibraryName, libraryToBuild.Platform);
            string libraryPath = Path.Combine(libraryToBuild.InstallDirectory, libraryFilename);
            if (!File.Exists(libraryPath))
                File.WriteAllText(libraryPath, "This is not a real shared library, it is a placeholder.", Encoding.UTF8);

            string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string importPath = Path.GetRelativePath(projectPath, libraryPath).Replace("\\", "/");
            importsInProgress.Add(importPath, libraryToBuild);
            AssetDatabase.ImportAsset(importPath, ImportAssetOptions.ForceSynchronousImport);
        }

        private static string GetSharedLibraryFilename(string baseName, BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return $"{baseName}.dll";
                case BuildTarget.iOS:
                    return $"lib{baseName}.a";
                case BuildTarget.StandaloneOSX:
                    return $"lib{baseName}.dylib";
                default:
                    // Assume Linux-ish
                    return $"lib{baseName}.so";
            }
        }

        /// <summary>
        /// This Unity message is invoked at the start of the asset imports initiated in OnPreprocessBuild
        /// above and allows us to configure how the placeholder shared libraries are imported.
        /// </summary>
        private void OnPreprocessAsset()
        {
            LibraryToBuild libraryToBuild;
            if (!importsInProgress.TryGetValue(assetPath, out libraryToBuild))
                return;

            PluginImporter importer = this.assetImporter as PluginImporter;
            if (importer == null)
                return;

            CompileCesiumForUnityNative.ConfigurePlugin(libraryToBuild, importer);
        }

        private static void ConfigurePlugin(LibraryToBuild library, PluginImporter importer)
        {
            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(false);
            importer.SetCompatibleWithPlatform(library.Platform, true);

            if (library.Platform == BuildTarget.Android)
            {
                importer.SetPlatformData(BuildTarget.Android, "CPU", library.Cpu == "arm64" ? "ARM64" : library.Cpu);
            }
            else if (library.Platform == BuildTarget.StandaloneOSX)
            {
                if (library.Cpu != null)
                    importer.SetPlatformData(BuildTarget.StandaloneOSX, "CPU", library.Cpu == "arm64" ? "ARM64" : library.Cpu);
            }
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths,
            bool didDomainReload)
        {
            // On macOS, the settings in OnPreprocessAsset above seem to be ignored.
            // So reapply here as well.
            foreach (string imported in importedAssets)
            {
                LibraryToBuild libraryToBuild;
                if (!importsInProgress.TryGetValue(imported, out libraryToBuild))
                    continue;

                PluginImporter importer = AssetImporter.GetAtPath(imported) as PluginImporter;
                if (importer == null)
                    continue;

                CompileCesiumForUnityNative.ConfigurePlugin(libraryToBuild, importer);
            }
        }

        public int callbackOrder => 0;

        /// <summary>
        /// Invoked after the managed script assemblies are compiled, including the CesiumForUnity
        /// managed code. Building the CesiumForUnity assembly will generate C++ code via Reinterop,
        /// so we implement this method in order to compile that generated C++ code to a shared library
        /// and inject it into the in-progress Player build.
        /// </summary>
        /// <param name="report"></param>
        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            LibraryToBuild[] libraries = GetLibrariesToBuildForPlatform(report.summary, finalLibrariesOnly: false);
            foreach (LibraryToBuild library in libraries)
            {
                BuildNativeLibrary(library);
            }

            if (report.summary.platform == BuildTarget.StandaloneOSX)
            {
                // On macOS, build for both ARM64 and x64, and then use the lipo tool to combine
                // the libraries for the two CPUs into a single library.
                List<string> args = new List<string>();
                args.Add("-create");
                foreach (LibraryToBuild library in libraries)
                {
                    args.Add(Path.Combine(library.InstallDirectory, "libCesiumForUnityNative-Runtime.dylib"));
                }
                args.Add("-output");
                args.Add(Path.GetFullPath(Path.Combine(libraries[0].InstallDirectory, "..", "libCesiumForUnityNative-Runtime.dylib")));
                Process p = Process.Start("lipo", string.Join(' ', args));
                p.WaitForExit();
                if (p.ExitCode != 0)
                    throw new Exception("lipo failed");

                foreach (LibraryToBuild library in libraries)
                {
                    Directory.Delete(library.InstallDirectory, true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="summary"></param>
        /// <param name="finalLibrariesOnly">
        /// True if only the final libraries shipped to end users should be included, not any intermediate libraries created along the way.
        /// This affects the macOS libraries, where two libraries are built (for x64 and ARM64), but then they are compiled into a
        /// single library supporting both platforms.
        /// </param>
        /// <returns></returns>
        private static LibraryToBuild[] GetLibrariesToBuildForPlatform(BuildSummary summary, bool finalLibrariesOnly)
        {
            List<LibraryToBuild> result = new List<LibraryToBuild>();

            if (summary.platform == BuildTarget.StandaloneOSX)
            {
                if (finalLibrariesOnly)
                {
                    result.Add(GetLibraryToBuild(summary));
                }
                else
                {
                    result.Add(GetLibraryToBuild(summary, "x86_64"));
                    result.Add(GetLibraryToBuild(summary, "arm64"));
                }
            }
            else if (summary.platform == BuildTarget.Android)
            {
                // We support ARM64 and x86_64. If any other architectures are enabled, log a warning.
                AndroidArchitecture supported = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64;
                if ((PlayerSettings.Android.targetArchitectures & ~supported) != 0)
                    UnityEngine.Debug.LogWarning("Cesium for Unity only supports the ARM64 and x86_64 CPU architectures on Android. Other architectures will not work.");

                if (PlayerSettings.Android.targetArchitectures.HasFlag(AndroidArchitecture.ARM64))
                    result.Add(GetLibraryToBuild(summary, "arm64"));
                if (PlayerSettings.Android.targetArchitectures.HasFlag(AndroidArchitecture.X86_64))
                    result.Add(GetLibraryToBuild(summary, "x86_64"));
            }
            else
            {
                result.Add(GetLibraryToBuild(summary));
            }

            return result.ToArray();
        }

        public static LibraryToBuild GetLibraryToBuild(BuildSummary summary, string cpu = null)
        {
            return GetLibraryToBuild(new PlatformToBuild()
            {
                platform = summary.platform,
                platformGroup = summary.platformGroup,
                isDevelopment = summary.options.HasFlag(BuildOptions.Development),
                isCleanBuild = summary.options.HasFlag(BuildOptions.CleanBuildCache)
            }, cpu);
        }

        public static LibraryToBuild GetLibraryToBuild(PlatformToBuild platform, string cpu = null)
        {
            string sourceFilename = GetSourceFilePathName();
            string packagePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilename), $".."));
            string nativeDirectory = Path.Combine(packagePath, "native~");

            string platformDirectoryName = GetDirectoryNameForPlatform(platform);

            LibraryToBuild library = new LibraryToBuild();
            library.Platform = platform.platform;
            library.PlatformGroup = platform.platformGroup;
            library.Cpu = cpu;
            library.SourceDirectory = nativeDirectory;
            library.BuildDirectory = Path.Combine(nativeDirectory, $"build-{platformDirectoryName}");
            library.GeneratedDirectoryName = $"generated-{platformDirectoryName}";
            library.Configuration = platform.isDevelopment
                ? "Debug"
                : "RelWithDebInfo";
            library.InstallDirectory = GetInstallDirectoryForPlatform(platform, packagePath);
            library.CleanBuild = platform.isCleanBuild;

            if (IsEditor(platform))
                library.ExtraConfigureArgs.Add("-DEDITOR=on");

            if (platform.platformGroup == BuildTargetGroup.Android)
            {
                library.Toolchain = $"extern/android-toolchain.cmake";
                if (cpu == null)
                    cpu = "arm64";
                if (cpu == "x86_64")
                    library.ExtraConfigureArgs.Add("-DCMAKE_ANDROID_ARCH_ABI=x86_64");
                else
                    library.ExtraConfigureArgs.Add("-DCMAKE_ANDROID_ARCH_ABI=arm64-v8a");
            }

            if (platform.platformGroup == BuildTargetGroup.iOS)
            {
                library.Toolchain = "extern/ios-toolchain.cmake";
                library.ExtraConfigureArgs.Add("-GXcode");
                library.ExtraConfigureArgs.Add("-DCMAKE_SYSTEM_NAME=iOS");
                library.ExtraConfigureArgs.Add("-DCMAKE_SYSTEM_PROCESSOR=aarch64");
                library.ExtraConfigureArgs.Add("-DCMAKE_OSX_ARCHITECTURES=arm64");
            }

            if (platform.platform == BuildTarget.StandaloneOSX)
            {
                if (cpu != null)
                    library.ExtraConfigureArgs.Add("-DCMAKE_OSX_ARCHITECTURES=" + cpu);
            }

            if (cpu != null)
            {
                library.InstallDirectory = Path.Combine(library.InstallDirectory, cpu);
                library.BuildDirectory += "-" + cpu;
            }

            return library;
        }

        private static bool IsEditor(PlatformToBuild platform)
        {
            return IsEditor(platform.platformGroup, platform.platform);
        }

        private static bool IsEditor(BuildTargetGroup platformGroup, BuildTarget platform)
        {
            return platformGroup == BuildTargetGroup.Unknown && platform == BuildTarget.NoTarget;
        }

        private static bool IsIOS(BuildTargetGroup platformGroup, BuildTarget platform){
            return platformGroup == BuildTargetGroup.iOS && platform == BuildTarget.iOS;
        }

        private static string GetDirectoryNameForPlatform(PlatformToBuild platform)
        {
            return GetDirectoryNameForPlatform(platform.platformGroup, platform.platform);
        }

        private static string GetDirectoryNameForPlatform(BuildTargetGroup platformGroup, BuildTarget platform)
        {
            if (IsEditor(platformGroup, platform))
                return "Editor";
            else if (IsIOS(platformGroup, platform))
                return "iOS";
            return platformGroup.ToString();
        }

        private static string GetInstallDirectoryForPlatform(PlatformToBuild platform, string packagePath)
        {
            if (IsEditor(platform))
                return Path.Combine(packagePath, "Editor");
            return Path.Combine(packagePath, "Plugins", GetDirectoryNameForPlatform(platform));
        }

        internal static void BuildNativeLibrary(LibraryToBuild library)
        {
            if (library.CleanBuild && library.BuildDirectory.Length > 2 && Directory.Exists(library.BuildDirectory))
                Directory.Delete(library.BuildDirectory, true);
            Directory.CreateDirectory(library.BuildDirectory);

            try
            {
                string logFilename = Path.Combine(library.BuildDirectory, "build.log");
                string projectPath = Path.Combine(Application.dataPath, "..");
                string logDisplayName = Path.GetRelativePath(projectPath, logFilename);

                EditorUtility.DisplayProgressBar($"Building CesiumForUnityNative", $"See {logDisplayName}.", 0.0f);

                using (StreamWriter log = new StreamWriter(logFilename, false, Encoding.UTF8))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = false;
                    if (library.Platform == BuildTarget.StandaloneOSX || library.Platform == BuildTarget.iOS){
                        startInfo.FileName = File.Exists("/Applications/CMake.app/Contents/bin/cmake") ? "/Applications/CMake.app/Contents/bin/cmake" : "cmake";
                    }
                    else {
                        startInfo.FileName = "cmake";
                    }
                    startInfo.CreateNoWindow = true;
                    startInfo.WorkingDirectory = library.SourceDirectory;
                    startInfo.RedirectStandardError = true;
                    startInfo.RedirectStandardOutput = true;
                    ConfigureEnvironmentVariables(startInfo.Environment, library);

                    List<string> args = new List<string>()
                    {
                        "-B",
                        library.BuildDirectory,
                        "-S",
                        library.SourceDirectory,
                        "-DEDITOR=false",
                        $"-DCMAKE_BUILD_TYPE={library.Configuration}",
                        $"-DCMAKE_INSTALL_PREFIX=\"{library.InstallDirectory}\"",
                        $"-DREINTEROP_GENERATED_DIRECTORY={library.GeneratedDirectoryName}",
                    };
                    args.AddRange(library.ExtraConfigureArgs);
                    
                    if (library.Toolchain != null)
                        args.Add($"-DCMAKE_TOOLCHAIN_FILE=\"{library.Toolchain}\"");

                    startInfo.Arguments = string.Join(' ', args);

                    RunAndLog(startInfo, log, logFilename);

                    args = new List<string>()
                    {
                        "--build",
                        $"\"{library.BuildDirectory}\"",
                        "--config",
                        library.Configuration,
                        "--parallel",
                        (Environment.ProcessorCount + 1).ToString(),
                        "--target",
                        "install"
                    };
                    args.AddRange(library.ExtraBuildArgs);
                    startInfo.Arguments = string.Join(' ', args);
                    RunAndLog(startInfo, log, logFilename);
                 
                    if(library.Platform == BuildTarget.iOS)
                        AssetDatabase.Refresh();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void RunAndLog(ProcessStartInfo startInfo, StreamWriter log, string logFilename)
        {
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
                configure.Start();
                configure.BeginOutputReadLine();
                configure.BeginErrorReadLine();
                configure.WaitForExit();

                if (configure.ExitCode != 0)
                {
                    UnityEngine.Debug.LogError($"An error occurred while building CesiumForUnityNative. See {logFilename} for details. The command-line was:{Environment.NewLine}{startInfo.FileName} {startInfo.Arguments}");
                }
            }
        }

        private static void ConfigureEnvironmentVariables(IDictionary<string, string> environment, LibraryToBuild library)
        {
            // CMake can't deal with back slashes (Windows) in the ANDROID_NDK_ROOT environment variable.
            // So replace them with forward slashes.
            string ndkRoot = environment.ContainsKey("ANDROID_NDK_ROOT") ? environment["ANDROID_NDK_ROOT"] : null;
#if UNITY_ANDROID
            if (ndkRoot == null)
            {
                // We're building for Android but don't have a known NDK root. Try asking Unity for it.
                ndkRoot = AndroidExternalToolsSettings.ndkRootPath;
            }
#endif

            // On Windows, use the make program included in the NDK. Because Visual Studio (which is usually
            // the default) won't work to build for Android.
            if (library.Platform == BuildTarget.Android && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                library.ExtraConfigureArgs.Add("-G \"Unix Makefiles\"");

                if (!string.IsNullOrEmpty(ndkRoot))
                {
                    string make = Path.Combine(ndkRoot, "prebuilt", "windows-x86_64", "bin", "make.exe").Replace('\\', '/');
                    library.ExtraConfigureArgs.Add($"-DCMAKE_MAKE_PROGRAM=\"{make}\"");
                }
            }

            if (!string.IsNullOrEmpty(ndkRoot))
                environment["ANDROID_NDK_ROOT"] = ndkRoot.Replace('\\', '/');
        }

        private static string GetSourceFilePathName([CallerFilePath] string callerFilePath = null)
        {
            return callerFilePath == null ? "" : callerFilePath;
        }
    }
}
