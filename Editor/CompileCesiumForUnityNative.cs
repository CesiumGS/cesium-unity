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
            public string SourceDirectory = "";
            public string BuildDirectory = "build";
            public string GeneratedDirectoryName = "generated-Unknown";
            public string Configuration = "RelWithDebInfo";
            public string InstallDirectory = "";
            public bool CleanBuild = false;
            public string? Toolchain;
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
                CreatePlaceholders(
                    GetLibraryToBuild(report.summary),
                    "CesiumForUnityNative-Runtime"
                );
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                importsInProgress.Clear();
            }
        }

        internal static void CreateLibraryPlaceholders(params PlatformToBuild[] platforms)
        {
            importsInProgress.Clear();

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (PlatformToBuild platform in platforms)
                {
                    CreatePlaceholders(
                        GetLibraryToBuild(platform),
                        "CesiumForUnityNative-Runtime"
                    );
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
            LibraryToBuild? libraryToBuild;
            if (!importsInProgress.TryGetValue(this.assetPath, out libraryToBuild))
                return;

            PluginImporter? importer = this.assetImporter as PluginImporter;
            if (importer == null)
                return;

            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(false);
            importer.SetCompatibleWithPlatform(libraryToBuild.Platform, true);
            
            if (libraryToBuild.Platform == BuildTarget.Android)
            {
                importer.SetPlatformData(BuildTarget.Android, "CPU", "ARM64");
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
            BuildNativeLibrary(GetLibraryToBuild(report.summary));
        }

        public static LibraryToBuild GetLibraryToBuild(BuildSummary summary)
        {
            return GetLibraryToBuild(new PlatformToBuild()
            {
                platform = summary.platform,
                platformGroup = summary.platformGroup,
                isDevelopment = summary.options.HasFlag(BuildOptions.Development),
                isCleanBuild = summary.options.HasFlag(BuildOptions.CleanBuildCache)
            });
        }

        public static LibraryToBuild GetLibraryToBuild(PlatformToBuild platform)
        {
            string sourceFilename = GetSourceFilePathName();
            string packagePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilename), $".."));
            string nativeDirectory = Path.Combine(packagePath, "native~");

            string platformDirectoryName = GetDirectoryNameForPlatform(platform);

            LibraryToBuild library = new LibraryToBuild();
            library.Platform = platform.platform;
            library.PlatformGroup = platform.platformGroup;
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
                library.Toolchain = "extern/android-toolchain.cmake";
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

        private static string GetDirectoryNameForPlatform(PlatformToBuild platform)
        {
            return GetDirectoryNameForPlatform(platform.platformGroup, platform.platform);
        }

        private static string GetDirectoryNameForPlatform(BuildTargetGroup platformGroup, BuildTarget platform)
        {
            if (IsEditor(platformGroup, platform))
                return "Editor";
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
                    if (library.Platform == BuildTarget.StandaloneOSX){
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
                        "14",
                        "--target",
                        "install"
                    };
                    args.AddRange(library.ExtraBuildArgs);
                    startInfo.Arguments = string.Join(' ', args);
                    RunAndLog(startInfo, log, logFilename);
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
            string? ndkRoot = environment.ContainsKey("ANDROID_NDK_ROOT") ? environment["ANDROID_NDK_ROOT"] : null;
            if (ndkRoot != null)
            {
                // On Windows, use the make program included in the NDK. Because Visual Studio (which is usually
                // the default) won't work to build for Android.
                if (library.Platform == BuildTarget.Android && Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    library.ExtraConfigureArgs.Add("-G \"Unix Makefiles\"");
                    string make = Path.Combine(ndkRoot, "prebuilt", "windows-x86_64", "bin", "make.exe").Replace('\\', '/');
                    library.ExtraConfigureArgs.Add($"-DCMAKE_MAKE_PROGRAM=\"{make}\"");
                }

                environment["ANDROID_NDK_ROOT"] = ndkRoot.Replace('\\', '/');
            }
        }

        private static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null)
        {
            return callerFilePath == null ? "" : callerFilePath;
        }
    }
}
