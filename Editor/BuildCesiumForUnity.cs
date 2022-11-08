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
            string buildPath = Path.Combine(packagePath, "built~");
            Directory.Delete(buildPath, true);
            Directory.CreateDirectory(buildPath);

            // This _should_ happen automatically when we call BuildPlayer for each platform, but sadly
            // the placeholder asset import doesn't work right when invoked this way. So we do it manually
            // here.
            CompileCesiumForUnityNative.CreateLibraryPlaceholders(
                new PlatformToBuild()
                {
                    platformGroup = BuildTargetGroup.Standalone,
                    platform = BuildTarget.StandaloneWindows64,
                },
                new PlatformToBuild()
                {
                    platformGroup = BuildTargetGroup.Android,
                    platform = BuildTarget.Android,
                });

            AddGeneratedFiles("UNITY_EDITOR", Path.Combine(packagePath, "generated~", "Runtime"), Path.Combine(buildPath, "Runtime", "generated"));
            AddGeneratedFiles("UNITY_EDITOR", Path.Combine(packagePath, "generated~", "Editor"), Path.Combine(buildPath, "Editor", "generated"));

            BuildPipeline.BuildPlayer(new BuildPlayerOptions()
            {
                locationPathName = Path.Combine(buildPath, "Windows-x64", "game.exe"),
                targetGroup = BuildTargetGroup.Standalone,
                target = BuildTarget.StandaloneWindows64,
                //options = BuildOptions.BuildScriptsOnly
            });

            Directory.Delete(Path.Combine(buildPath, "Windows-x64"), true);

            AddGeneratedFiles("!UNITY_EDITOR && UNITY_STANDALONE_WIN", Path.Combine(packagePath, "generated~", "Runtime"), Path.Combine(buildPath, "Runtime", "generated"));
            AddGeneratedFiles("!UNITY_EDITOR && UNITY_STANDALONE_WIN", Path.Combine(packagePath, "generated~", "Editor"), Path.Combine(buildPath, "Editor", "generated"));

            BuildPipeline.BuildPlayer(new BuildPlayerOptions()
            {
                locationPathName = Path.Combine(buildPath, "Android", "game"),
                targetGroup = BuildTargetGroup.Android,
                target = BuildTarget.Android,
                //options = BuildOptions.BuildScriptsOnly
            });

            Directory.Delete(Path.Combine(buildPath, "Android"), true);

            AddGeneratedFiles("!UNITY_EDITOR && UNITY_ANDROID", Path.Combine(packagePath, "generated~", "Runtime"), Path.Combine(buildPath, "Runtime", "generated"));
            AddGeneratedFiles("!UNITY_EDITOR && UNITY_ANDROID", Path.Combine(packagePath, "generated~", "Editor"), Path.Combine(buildPath, "Editor", "generated"));

            CopyPackageContents(packagePath, buildPath);

            Client.Pack(buildPath, buildPath);
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
                "Editor/BuildCesiumForUnity.cs.meta"
            };
            
            foreach (string fileToDelete in filesToDelete)
            {
                File.Delete(Path.Combine(targetPath, fileToDelete));
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
