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
            string buildPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(buildPath);
            try
            {
                BuildPlayer(BuildTargetGroup.Android, BuildTarget.Android, Path.Combine(buildPath, "Android"));
            }
            finally
            {
                Directory.Delete(buildPath, true);
            }
            EditorApplication.Exit(0);
        }

        public static void CompileForWindowsAndExit()
        {
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

        private static void BuildPlayer(BuildTargetGroup targetGroup, BuildTarget target, string outputPath)
        {
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions()
            {
                locationPathName = Path.Combine(outputPath, "game"),
                targetGroup = targetGroup,
                target = target,
                scenes = new[] { "Assets/Scenes/Empty.unity" }
            });
            if (report.summary.totalErrors > 0)
                throw new Exception("Build failed");

            // We don't actually need the built project; delete it.
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
        }
    }
}
