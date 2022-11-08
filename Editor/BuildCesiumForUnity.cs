using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEngine;

namespace CesiumForUnity
{
    public class BuildCesiumForUnity
    {
        [MenuItem("Cesium/Build/All")]
        public static void BuildAll()
        {
            string sourcePath = GetSourceFilePathName();
            string packagePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourcePath), ".."));

            // Create working directories for the build
            string buildPath = Path.Combine(packagePath, "built~");
            if (Directory.Exists(buildPath))
                Directory.Delete(buildPath, true);
            Directory.CreateDirectory(buildPath);

            string generatedPath = Path.Combine(packagePath, "generated~");
            if (Directory.Exists(generatedPath))
                Directory.Delete(generatedPath, true);
            Directory.CreateDirectory(generatedPath);
            string generatedRuntimePath = Path.Combine(generatedPath, "Runtime");
            Directory.CreateDirectory(generatedRuntimePath);
            string generatedEditorPath = Path.Combine(generatedPath, "Editor");
            Directory.CreateDirectory(generatedEditorPath);

            // Store the original contents of the response files so we can restore them later.
            string runtimeCscRspPath = Path.Combine(packagePath, "Runtime", "csc.rsp");
            string originalRuntimeCscRsp = File.ReadAllText(runtimeCscRspPath, Encoding.UTF8);
            string editorCscRspPath = Path.Combine(packagePath, "Editor", "csc.rsp");
            string originalEditorCscRsp = File.ReadAllText(editorCscRspPath, Encoding.UTF8);

            try
            {
                // Tell the C# compiler to write the generated code to disk.
                File.AppendAllText(runtimeCscRspPath, "-generatedfilesout:\"" + generatedRuntimePath + "\"" + Environment.NewLine, Encoding.UTF8);
                File.AppendAllText(editorCscRspPath, "-generatedfilesout:\"" + generatedEditorPath + "\"" + Environment.NewLine, Encoding.UTF8);

                // Trigger a recompile of the managed code, invoking Reinterop and generating code.
                AssetDatabase.Refresh();

                // Compile the native code for the Editor.
                //CompileCesiumForUnityNative.BuildNativeLibrary(CompileCesiumForUnityNative.GetLibraryToBuild(new PlatformToBuild()
                //{
                //    platformGroup = BuildTargetGroup.Unknown,
                //    platform = BuildTarget.NoTarget
                //});

                // Add the generated files (for the Editor) to the package
                AddGeneratedFiles("UNITY_EDITOR", Path.Combine(packagePath, "generated~", "Runtime"), Path.Combine(buildPath, "Runtime", "generated"));
                AddGeneratedFiles("UNITY_EDITOR", Path.Combine(packagePath, "generated~", "Editor"), Path.Combine(buildPath, "Editor", "generated"));

                // Build the Windows player
                BuildPlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, Path.Combine(buildPath, "Windows"));

                // Add the generated files for the Windows player to the package
                AddGeneratedFiles("!UNITY_EDITOR && UNITY_STANDALONE_WIN", Path.Combine(packagePath, "generated~", "Runtime"), Path.Combine(buildPath, "Runtime", "generated"));
                AddGeneratedFiles("!UNITY_EDITOR && UNITY_STANDALONE_WIN", Path.Combine(packagePath, "generated~", "Editor"), Path.Combine(buildPath, "Editor", "generated"));

                // Build the Android player
                BuildPlayer(BuildTargetGroup.Android, BuildTarget.Android, Path.Combine(buildPath, "Android"));

                // Add the generated files for the Android player to the package
                AddGeneratedFiles("!UNITY_EDITOR && UNITY_ANDROID", Path.Combine(packagePath, "generated~", "Runtime"), Path.Combine(buildPath, "Runtime", "generated"));
                AddGeneratedFiles("!UNITY_EDITOR && UNITY_ANDROID", Path.Combine(packagePath, "generated~", "Editor"), Path.Combine(buildPath, "Editor", "generated"));

                // TODO: build additional players and only build supported platforms (i.e. don't try to build for Windows on macOS)

                // Copy additional code and assets to the package
                CopyPackageContents(packagePath, buildPath);

                // Create the package .tar.gz in the package's root directory.
                Client.Pack(buildPath, packagePath);
            }
            finally
            {
                // Clean up
                File.WriteAllText(Path.Combine(packagePath, "Runtime", "csc.rsp"), originalRuntimeCscRsp, Encoding.UTF8);
                File.WriteAllText(Path.Combine(packagePath, "Runtime", "csc.rsp"), originalEditorCscRsp, Encoding.UTF8);
                if (Directory.Exists(buildPath))
                    Directory.Delete(buildPath, true);
                if (Directory.Exists(generatedPath))
                    Directory.Delete(generatedPath, true);
            }
        }

        private static void BuildPlayer(BuildTargetGroup targetGroup, BuildTarget target, string outputPath)
        {
            BuildPipeline.BuildPlayer(new BuildPlayerOptions()
            {
                locationPathName = Path.Combine(outputPath, "game"),
                targetGroup = targetGroup,
                target = target,
                //options = BuildOptions.BuildScriptsOnly
            });

            // We don't actually need the built project; delete it.
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
        }

        private static void CopyPackageContents(string sourcePath, string targetPath)
        {
            string[] filesToCopy = new[]
            {
                "Editor.meta",
                "LICENSE",
                "LICENSE.meta",
                "package.json",
                "package.json.meta",
                "Plugins.meta",
                "README.md",
                "README.md.meta",
                "Runtime.meta",
                "Tests.meta"
            };

            foreach (string file in filesToCopy)
            {
                File.Copy(Path.Combine(sourcePath, file), Path.Combine(targetPath, file));
            }

            string[] pathsToCopy = new[]
            {
                "Editor",
                "Runtime",
                "Tests",
                "Documentation~",
                "Plugins"
            };

            foreach (string pathToCopy in pathsToCopy)
            {
                CopyDirectory(Path.Combine(sourcePath, pathToCopy), Path.Combine(targetPath, pathToCopy));
            }

            // Remove build-related sources files that don't make sense in the published package
            string[] filesToDelete = new[]
            {
                "Editor/CompileCesiumForUnityNative.cs",
                "Editor/CompileCesiumForUnityNative.cs.meta",
                "Editor/BuildCesiumForUnity.cs",
                "Editor/BuildCesiumForUnity.cs.meta",
                "Editor/ConfigureReinterop.cs",
                "Editor/ConfigureReinterop.cs.meta",
                "Runtime/ConfigureReinterop.cs",
                "Runtime/ConfigureReinterop.cs.meta"
            };
            
            foreach (string fileToDelete in filesToDelete)
            {
                string path = Path.Combine(targetPath, fileToDelete);
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        private static void CopyDirectory(string source, string destination)
        {
            TraverseDirectoryRecursively(
                source,
                destination,
                (file, destination) =>
                {
                    file.CopyTo(Path.Combine(destination, file.Name));
                },
                (directory, destination) =>
                {
                    Directory.CreateDirectory(destination);
                });
        }

        private static void TraverseDirectoryRecursively(string directory, string builtPath, Action<FileInfo, string> fileCallback, Action<DirectoryInfo, string> directoryCallback)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(directory);
            directoryCallback(sourceDir, builtPath);

            DirectoryInfo[] directories = sourceDir.GetDirectories();
            FileInfo[] files = sourceDir.GetFiles();

            foreach (FileInfo file in files)
            {
                fileCallback(file, builtPath);
            }

            foreach (DirectoryInfo dir in directories)
            {
                TraverseDirectoryRecursively(dir.FullName, Path.Combine(builtPath, dir.Name), fileCallback, directoryCallback);
            }
        }

        private static void AddGeneratedFiles(string condition, string source, string target)
        {
            TraverseDirectoryRecursively(
                source,
                target,
                (file, destination) =>
                {
                    string content = File.ReadAllText(file.FullName, Encoding.UTF8);
                    Directory.CreateDirectory(destination);
                    string destFile = Path.Combine(destination, file.Name);
                    using (StreamWriter writer = File.AppendText(destFile))
                    {
                        writer.WriteLine("#if " + condition);
                        writer.WriteLine(content);
                        writer.WriteLine("#endif");
                    }

                    // Create a .meta file if one doesn't already exist
                    string metaFile = destFile + ".meta";
                    if (!File.Exists(metaFile))
                    {
                        using (StreamWriter metaWriter = new StreamWriter(metaFile))
                        {
                            metaWriter.WriteLine("fileFormatVersion: 2");
                            metaWriter.WriteLine("guid: " + Guid.NewGuid().ToString("N"));
                            metaWriter.WriteLine("MonoImporter:");
                            metaWriter.WriteLine("  externalObjects: {}");
                            metaWriter.WriteLine("  serializedVersion: 2");
                            metaWriter.WriteLine("  defaultReferences: []");
                            metaWriter.WriteLine("  executionOrder: 0");
                            metaWriter.WriteLine("  icon: {instanceID: 0}");
                            metaWriter.WriteLine("  userData: ");
                            metaWriter.WriteLine("  assetBundleName: ");
                            metaWriter.WriteLine("  assetBundleVariant: ");
                        }
                    }
                },
                (directory, destination) =>
                {
                    Directory.CreateDirectory(destination);

                    // Create a .meta file for the directory if one doesn't already exist
                    string dirMetaFile = destination + ".meta";
                    if (!File.Exists(dirMetaFile))
                    {
                        using (StreamWriter metaWriter = new StreamWriter(dirMetaFile))
                        {
                            metaWriter.WriteLine("fileFormatVersion: 2");
                            metaWriter.WriteLine("guid: " + Guid.NewGuid().ToString("N"));
                            metaWriter.WriteLine("folderAsset: yes");
                            metaWriter.WriteLine("DefaultImporter:");
                            metaWriter.WriteLine("  externalObjects: {}");
                            metaWriter.WriteLine("  userData: ");
                            metaWriter.WriteLine("  assetBundleName: ");
                            metaWriter.WriteLine("  assetBundleVariant: ");
                        }
                    }
                });
        }

        private static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null)
        {
            return callerFilePath == null ? "" : callerFilePath;
        }
    }
}
