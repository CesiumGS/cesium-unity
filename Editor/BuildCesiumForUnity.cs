using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace CesiumForUnity
{
    public class BuildCesiumForUnity
    {
        public static void CompileForEditorAndExit()
        {
            CompilationPipeline.compilationFinished += OnEditorCompilationFinished;
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
        }

        private static void OnEditorCompilationFinished(object o)
        {
            CompilationPipeline.compilationFinished -= OnEditorCompilationFinished;
            EditorApplication.Exit(0);
        }

        public static void CompileForAndroidAndExit()
        {
            CompileCesiumForUnityNative.ExitAfterCompile = true;

            string buildPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(buildPath);
            try
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64;
                BuildPlayer(BuildTargetGroup.Android, BuildTarget.Android, Path.Combine(buildPath, "Android"));
            }
            finally
            {
                Directory.Delete(buildPath, true);
            }
            EditorApplication.Exit(0);
        }

        public static void CompileForWebAndExit()
        {
            CompileCesiumForUnityNative.ExitAfterCompile = true;

            string buildPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(buildPath);
            try
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);
                PlayerSettings.WebGL.threadsSupport = true;
                BuildPlayer(BuildTargetGroup.WebGL, BuildTarget.WebGL, Path.Combine(buildPath, "WebGL"));
            }
            finally
            {
                Directory.Delete(buildPath, true);
            }
            EditorApplication.Exit(0);
        }

        public static void CompileForUWPAndExit()
        {
            CompileCesiumForUnityNative.ExitAfterCompile = true;

            string buildPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(buildPath);
            try
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.WSA, ScriptingImplementation.IL2CPP);
                BuildPlayer(BuildTargetGroup.WSA, BuildTarget.WSAPlayer, Path.Combine(buildPath, "UWP"));
            }
            finally
            {
                Directory.Delete(buildPath, true);
            }
            EditorApplication.Exit(0);
        }

        public static void CompileForIOSAndExit()
        {
            CompileCesiumForUnityNative.ExitAfterCompile = true;

            string buildPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(buildPath);
            try
            {
                BuildPlayer(BuildTargetGroup.iOS, BuildTarget.iOS, Path.Combine(buildPath, "iOS"), false);
                // Check that the XCode project is able to build
                System.Diagnostics.Process p = System.Diagnostics.Process.Start("xcodebuild",
                  $"-project {Path.Combine(buildPath, "iOS/game/Unity-iPhone.xcodeproj")} CODE_SIGNING_ALLOWED=NO");
                p.WaitForExit();
                if (p.ExitCode != 0)
                    throw new Exception("xcodebuild failed");
            }
            finally
            {
                if (Directory.Exists(Path.Combine(buildPath, "iOS")))
                    Directory.Delete(Path.Combine(buildPath, "iOS"), true);
                Directory.Delete(buildPath, true);
            }
            EditorApplication.Exit(0);
        }

        public static void CompileForWindowsAndExit()
        {
            CompileCesiumForUnityNative.ExitAfterCompile = true;

            string buildPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(buildPath);
            try
            {
                BuildPlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, Path.Combine(buildPath, "Windows"));
            }
            finally
            {
                Directory.Delete(buildPath, true);
            }
            EditorApplication.Exit(0);
        }

        public static void CompileForMacAndExit()
        {
            CompileCesiumForUnityNative.ExitAfterCompile = true;

            string buildPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(buildPath);
            try
            {
                BuildPlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, Path.Combine(buildPath, "Mac"));
            }
            finally
            {
                Directory.Delete(buildPath, true);
            }
            EditorApplication.Exit(0);
        }

        public static void PackAndExit()
        {
            string tempPath = Environment.GetEnvironmentVariable("CESIUM_PACKAGE_TEMP_PATH");
            if (string.IsNullOrEmpty(tempPath))
            {
                Debug.Log("Cannot pack because the CESIUM_PACKAGE_TEMP_PATH environment variable is not set.");
                return;
            }

            string packSource = Path.Combine(tempPath, "package");
            string packTarget = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            
            Debug.Log("Packing directory " + packSource + " to output directory " + packTarget);

            PackRequest request = Client.Pack(packSource, packTarget);
            EditorApplication.update += () =>
            {
                if (request.IsCompleted)
                {
                    Debug.Log("Packing complete, exiting.");
                    EditorApplication.Exit(0);
                }
            };
        }

        private static void BuildPlayer(BuildTargetGroup targetGroup, BuildTarget target, string outputPath, bool deleteProject = true)
        {
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions()
            {
                locationPathName = Path.Combine(outputPath, "game"),
                targetGroup = targetGroup,
                target = target,
                scenes = new[] { "Assets/Scenes/Empty.unity" }
            });

            if (report.summary.totalErrors > 0)
            {
                StringBuilder errorReport = new StringBuilder();
                errorReport.AppendLine("BuildPlayer failed:");
                foreach (BuildStep step in report.steps)
                {
                    var errorMessages = step.messages.Where(message => message.type == LogType.Error || message.type == LogType.Exception);
                    if (!errorMessages.Any())
                        continue;

                    errorReport.AppendLine("  In step " + step.name + ":");

                    foreach (BuildStepMessage message in errorMessages)
                    {
                        errorReport.AppendLine("    - " + message.content);
                    }
                }
                throw new Exception(errorReport.ToString());
            }

            // We don't actually need the built project; delete it.
            if (deleteProject && Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
        }
    }
}
