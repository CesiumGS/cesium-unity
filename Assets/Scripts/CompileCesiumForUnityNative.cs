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
    internal class CompileCesiumForUnityNative
    {
        [InitializeOnLoadMethod]
        static void OnProjectLoadedInEditor()
        {
            UnityEngine.Debug.Log("Project loaded in Unity Editor");
        }

        private static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null)
        {
            return callerFilePath == null ? "" : callerFilePath;
        }
    }

    class MyAssetPostprocessor : AssetPostprocessor
    {
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
    }

    class MyCustomBuildProcessor : IPostprocessBuildWithReport, IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPostprocessBuild(BuildReport report)
        {
            UnityEngine.Debug.Log("MyCustomBuildProcessor.OnPostprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
        }

        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            UnityEngine.Debug.Log("OnPostprocessBuild");
        }

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

        private static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null)
        {
            return callerFilePath == null ? "" : callerFilePath;
        }
    }

    class MyIPostBuildPlayerScriptDLLs : IPostBuildPlayerScriptDLLs
    {
        public int callbackOrder => 0;

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

        private void Configure_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null)
        {
            return callerFilePath == null ? "" : callerFilePath;
        }
    }

    [InitializeOnLoad]
    public class BuildPP
    {
        static BuildPP()
        {
            CompilationPipeline.assemblyCompilationFinished += CompilationEventReceiver.OnAssemblyCompilationFinished;
        }
    }
    public static class CompilationEventReceiver
    {
        static public void OnAssemblyCompilationFinished(string filename, CompilerMessage[] CompilerMessages)
        {
            UnityEngine.Debug.Log("assemblyCompilationFinished " + filename);
            // I see control passed here every time the scripts are rebuilt,
            // even when I switch to Unity from VS...
        }
    }
}
#endif