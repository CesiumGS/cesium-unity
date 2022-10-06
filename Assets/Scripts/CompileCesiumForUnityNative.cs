#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text;

namespace CesiumForUnity
{
    /// <summary>
    /// When the user builds a Player (built game) in the Unity Editor, this class manages
    /// automatically compiling a suitable version of the native C++ CesiumForUnityNative
    /// shared library to go with it.
    /// </summary>
    class CompileCesiumForUnityNative :
        AssetPostprocessor,
        IPreprocessBuildWithReport,
        IPostBuildPlayerScriptDLLs
    {
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
            UnityEngine.Debug.Log("OnPreprocessBuild");

            string sourceFilename = GetSourceFilePathName();
            string assetsPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilename), $".."));
            string pluginPath = Path.Combine(assetsPath, "Plugins", "x64", "CesiumForUnityNative.dll");
            File.WriteAllText(pluginPath, "This is not a real DLL, it is a placeholder.", Encoding.UTF8);
            string relativePath = Path.Combine("Assets", Path.GetRelativePath(Application.dataPath, pluginPath));
            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceSynchronousImport);
        }

        /// <summary>
        /// This Unity message is invoked at the start of the asset import initiated in OnPreprocessBuild
        /// above and allows us to configure how the shared library is imported.
        /// </summary>
        private void OnPreprocessAsset()
        {
            PluginImporter? importer = this.assetImporter as PluginImporter;
            if (importer != null && assetPath.Contains("Plugins/x64/CesiumForUnityNative.dll"))
            {
                importer.SetCompatibleWithAnyPlatform(true);
                importer.SetExcludeEditorFromAnyPlatform(true);
                importer.SetCompatibleWithEditor(false);
            }
        }

        public int callbackOrder => 0;

        /// <summary>
        /// Invoked after the managed script assemblies are compiled, including the CesiumForUnity
        /// managed code. Building the CesiumForUnity assembly will generate C++ code via Reinterop,
        /// so we handle this method in order to compile that generated C++ code to a shared library
        /// and inject it into the in-progress Player build.
        /// </summary>
        /// <param name="report"></param>
        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            string folder;
            string? toolchain = null;
            switch (report.summary.platformGroup)
            {
                case BuildTargetGroup.Android:
                    folder = "Android";
                    toolchain = "extern/android-toolchain.cmake";
                    break;
                case BuildTargetGroup.Standalone:
                    folder = "x64";
                    break;
                default:
                    folder = "unknown";
                    break;
            }

            string configuration = report.summary.options.HasFlag(BuildOptions.Development) ? "Debug" : "RelWithDebInfo";

            BuildNativeLibrary(configuration, folder, toolchain);
        }

        private void BuildNativeLibrary(string configuration, string folder, string? toolchain)
        {
            string sourceFilename = GetSourceFilePathName();
            string scriptsDirectory = Path.GetDirectoryName(sourceFilename);
            string nativeDirectory = Path.GetFullPath(Path.Combine(scriptsDirectory, "../native~"));
            string buildDirectory = Path.Combine(nativeDirectory, $"build-{folder}");
            Directory.CreateDirectory(buildDirectory);

            try
            {
                string logFilename = Path.Combine(buildDirectory, "build.log");
                string logDisplayName = Path.Combine("Assets", Path.GetRelativePath(Application.dataPath, logFilename));
                if (EditorUtility.DisplayCancelableProgressBar("Building CesiumForUnityNative", $"See {logDisplayName}.", 0.0f))
                    return;

                string installPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilename), $"../Plugins/{folder}"));

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = false;
                startInfo.FileName = "cmake.exe";
                startInfo.Arguments = $"-B build-{folder} -S . -DEDITOR=false -DCMAKE_BUILD_TYPE={configuration} -DCMAKE_INSTALL_PREFIX=\"{installPath}\" -DREINTEROP_GENERATED_DIRECTORY=\"generated-{folder}\"";
                if (toolchain != null)
                    startInfo.Arguments += $" -DCMAKE_TOOLCHAIN_FILE=\"{toolchain}\"";
                startInfo.CreateNoWindow = false;
                startInfo.WorkingDirectory = nativeDirectory;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                string? ndkRoot = startInfo.Environment.ContainsKey("ANDROID_NDK_ROOT") ? startInfo.Environment["ANDROID_NDK_ROOT"] : null;
                if (!string.IsNullOrEmpty(ndkRoot))
                {
                    ndkRoot = ndkRoot.Replace('\\', '/');
                    startInfo.Environment["ANDROID_NDK_ROOT"] = ndkRoot;
                }

                using (Process configure = new Process())
                using (StreamWriter log = new StreamWriter(logFilename, false, Encoding.UTF8))
                {
                    configure.OutputDataReceived += (sender, e) =>
                    {
                        log.WriteLine(e.Data);
                    };
                    configure.ErrorDataReceived += (sender, e) =>
                    {
                        log.WriteLine(e.Data);
                    };
                    configure.StartInfo = startInfo;
                    configure.Start();
                    configure.BeginOutputReadLine();
                    configure.BeginErrorReadLine();
                    configure.WaitForExit();
                }

                startInfo.Arguments = $"--build build-{folder} -j14 --config {configuration} --target install";

                using (Process install = new Process())
                using (StreamWriter log = new StreamWriter(logFilename, true, Encoding.UTF8))
                {
                    install.OutputDataReceived += (sender, e) =>
                    {
                        log.WriteLine(e.Data);
                    };
                    install.ErrorDataReceived += (sender, e) =>
                    {
                        log.WriteLine(e.Data);
                    };
                    install.StartInfo = startInfo;
                    install.Start();
                    install.BeginOutputReadLine();
                    install.BeginErrorReadLine();
                    install.WaitForExit();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null)
        {
            return callerFilePath == null ? "" : callerFilePath;
        }
    }
}
#endif
