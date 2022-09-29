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

namespace CesiumForUnity
{
    internal class CompileCesiumForUnityNative
    {
        [InitializeOnLoadMethod]
        static void OnProjectLoadedInEditor()
        {
            UnityEngine.Debug.Log("Project loaded in Unity Editor");
        }
    }

    class MyCustomBuildProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPostprocessBuild(BuildReport report)
        {
            UnityEngine.Debug.Log("MyCustomBuildProcessor.OnPostprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
        }

        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            UnityEngine.Debug.Log(pathToBuiltProject);
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
            string installPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilename), $"../Plugins/{folder}"));

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = "cmake";
            startInfo.Arguments = $"-B build-{folder} -S . -DCMAKE_BUILD_TYPE={configuration} -DCMAKE_INSTALL_PREFIX=\"{installPath}\" -DREINTEROP_GENERATED_DIRECTORY=\"generated-{folder}\"";
            if (toolchain != null)
                startInfo.Arguments += $" -DCMAKE_TOOLCHAIN_FILE=\"{toolchain}\"";
            startInfo.CreateNoWindow = false;
            startInfo.WorkingDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilename), "../native~"));
            Process configure = Process.Start(startInfo);
            configure.WaitForExit();

            startInfo.Arguments = $"--build build-{folder} -j14 --config {configuration} --target install";
            Process install = Process.Start(startInfo);
            install.WaitForExit();
        }

        private static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null)
        {
            return callerFilePath == null ? "" : callerFilePath;
        }
    }

    [InitializeOnLoad]
    public class BuildPP //
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