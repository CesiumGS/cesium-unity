using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Build
{
    public class Package
    {
        public void Run()
        {
            Unity? unity = Unity.FindUnity();
            if (unity == null)
                throw new Exception("Could not find Unity!");
            Cmake cmake = new Cmake();

            Console.WriteLine("**** Using Unity at " + unity.ExecutablePath);

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Environment.SetEnvironmentVariable("CESIUM_PACKAGE_TEMP_PATH", tempPath);

            Console.WriteLine("**** Output directory " + tempPath);

            string runtimeCscRspPath = Path.Combine(Utility.PackageRoot, "Runtime", "csc.rsp");
            string runtimeCscRsp = File.ReadAllText(runtimeCscRspPath, Encoding.UTF8);
            string editorCscRspPath = Path.Combine(Utility.PackageRoot, "Editor", "csc.rsp");
            string editorCscRsp = File.ReadAllText(editorCscRspPath, Encoding.UTF8);

            try
            {
                Directory.CreateDirectory(tempPath);

                string outputPackagePath = Path.Combine(tempPath, "package");
                Directory.CreateDirectory(outputPackagePath);

                Console.WriteLine("**** Modifying the csc.rsp files to write generated files to disk");
                string generatedRuntimePath = Path.Combine(tempPath, "generated", "Runtime");
                Directory.CreateDirectory(generatedRuntimePath);
                string generatedEditorPath = Path.Combine(tempPath, "generated", "Editor");
                Directory.CreateDirectory(generatedEditorPath);

                File.AppendAllText(runtimeCscRspPath, "-generatedfilesout:\"" + generatedRuntimePath + "\"" + Environment.NewLine, Encoding.UTF8);
                File.AppendAllText(editorCscRspPath, "-generatedfilesout:\"" + generatedEditorPath + "\"" + Environment.NewLine, Encoding.UTF8);

                string sceneDirectory = Path.Combine(Utility.ProjectRoot, "Assets", "Scenes");
                Directory.CreateDirectory(sceneDirectory);
                string emptyScenePath = Path.Combine(sceneDirectory, "Empty.unity");
                if (!File.Exists(emptyScenePath))
                {
                    Console.WriteLine("**** Creating an empty scene");
                    using (StreamWriter emptyScene = new StreamWriter(emptyScenePath))
                    {
                        emptyScene.WriteLine("%YAML 1.1");
                        emptyScene.WriteLine("%TAG !u! tag:unity3d.com,2011:");
                    }
                }

                Console.WriteLine("**** Compiling C# code for the Editor");
                unity.Run(new[]
                {
                    "-batchmode",
                    "-nographics",
                    "-projectPath",
                    Utility.ProjectRoot,
                    "-executeMethod",
                    "CesiumForUnity.BuildCesiumForUnity.CompileForEditorAndExit"
                });

                Console.WriteLine("**** Adding generated files (for the Editor) to the package");
                string platformEditorConditional = "UNITY_EDITOR";
                if (OperatingSystem.IsWindows())
                    platformEditorConditional = "UNITY_EDITOR_WIN";
                else if (OperatingSystem.IsMacOS())
                    platformEditorConditional = "UNITY_EDITOR_OSX";
                else if (OperatingSystem.IsLinux())
                    platformEditorConditional = "UNITY_EDITOR_LINUX";

                AddGeneratedFiles(platformEditorConditional, generatedRuntimePath, Path.Combine(outputPackagePath, "Runtime", "generated"));
                AddGeneratedFiles(platformEditorConditional, generatedEditorPath, Path.Combine(outputPackagePath, "Editor", "generated"));

                // Clean the generated code directories.
                Directory.Delete(generatedRuntimePath, true);
                Directory.CreateDirectory(generatedRuntimePath);
                Directory.Delete(generatedEditorPath, true);
                Directory.CreateDirectory(generatedEditorPath);

                Console.WriteLine("**** Compiling C++ code for the Editor");

                List<string> configureArgs = new List<string>()
                {
                    "-B",
                    "native~/build",
                    "-S",
                    "native~",
                    "-DCMAKE_BUILD_TYPE=RelWithDebInfo"
                };

                List<string> buildArgs = new List<string>()
                {
                    "--build",
                    "native~/build",
                    "--target",
                    "install",
                    "-j" + (Environment.ProcessorCount + 1),
                    "--config",
                    "RelWithDebInfo"
                };

                if (OperatingSystem.IsMacOS())
                {
                    // On macOS, we must build the native code twice, once for x86_64 and once for arm64.
                    // In theory we can build universal binaries, but some of our third party libraries don't
                    // handle this well.
                    configureArgs[1] = "native~/build-x64";
                    var x64ConfigureArgs = configureArgs.Concat(new[]
                    {
                        "-DCMAKE_OSX_ARCHITECTURES=x86_64",
                        "-DCMAKE_INSTALL_PREFIX=" + Path.Combine(Utility.PackageRoot, "Editor", "x86_64")
                    });
                    Utility.Run("cmake", x64ConfigureArgs);

                    buildArgs[1] = "native~/build-x64";
                    Utility.Run("cmake", buildArgs);

                    configureArgs[1] = "native~/build-arm64";
                    var armConfigureArgs = configureArgs.Concat(new[]
                    {
                        "-DCMAKE_OSX_ARCHITECTURES=arm64",
                        "-DCMAKE_INSTALL_PREFIX=" + Path.Combine(Utility.PackageRoot, "Editor", "arm64")
                    });
                    Utility.Run("cmake", armConfigureArgs);

                    buildArgs[1] = "native~/build-arm64";
                    Utility.Run("cmake", buildArgs);
                }
                else
                {
                    // On other platforms, just build once.
                    Utility.Run("cmake", configureArgs);
                    Utility.Run("cmake", buildArgs);
                }

                if (OperatingSystem.IsWindows())
                {
                    Console.WriteLine("**** Compiling for Windows Player");
                    unity.Run(new[]
                    {
                        "-batchmode",
                        "-nographics",
                        "-projectPath",
                        Utility.ProjectRoot,
                        "-buildTarget",
                        "Win64",
                        "-executeMethod",
                        "CesiumForUnity.BuildCesiumForUnity.CompileForWindowsAndExit"
                    });

                    Console.WriteLine("**** Adding generated files (for the Windows Player) to the package");
                    AddGeneratedFiles("!UNITY_EDITOR && UNITY_STANDALONE_WIN", generatedRuntimePath, Path.Combine(outputPackagePath, "Runtime", "generated"));

                    // Clean the generated code directory.
                    Directory.Delete(generatedRuntimePath, true);
                    Directory.CreateDirectory(generatedRuntimePath);

                    // TODO: we're currently only building for Android on Windows. This should be an option, or a separate build command.
                    Console.WriteLine("**** Compiling for Android Player");
                    unity.Run(new[]
                    {
                        "-batchmode",
                        "-nographics",
                        "-projectPath",
                        Utility.ProjectRoot,
                        "-buildTarget",
                        "Android",
                        "-executeMethod",
                        "CesiumForUnity.BuildCesiumForUnity.CompileForAndroidAndExit"
                    });

                    Console.WriteLine("**** Adding generated files (for the Android Player) to the package");
                    AddGeneratedFiles("!UNITY_EDITOR && UNITY_ANDROID", generatedRuntimePath, Path.Combine(outputPackagePath, "Runtime", "generated"));
                }
                else if (OperatingSystem.IsMacOS())
                {
                    Console.WriteLine("**** Compiling for macOS Player");
                    unity.Run(new[]
                    {
                        "-batchmode",
                        "-nographics",
                        "-projectPath",
                        Utility.ProjectRoot,
                        "-buildTarget",
                        "OSXUniversal",
                        "-executeMethod",
                        "CesiumForUnity.BuildCesiumForUnity.CompileForMacAndExit"
                    });

                    Console.WriteLine("**** Adding generated files (for the macOS Player) to the package");
                    AddGeneratedFiles("!UNITY_EDITOR && UNITY_STANDALONE_OSX", generatedRuntimePath, Path.Combine(outputPackagePath, "Runtime", "generated"));

                    // Clean the generated code directory.
                    Directory.Delete(generatedRuntimePath, true);
                    Directory.CreateDirectory(generatedRuntimePath);
                     
                    Console.WriteLine("**** Compiling for iOS Player");
                    unity.Run(new[]
                    {
                        "-batchmode",
                        "-nographics",
                        "-projectPath",
                        Utility.ProjectRoot,
                        "-buildTarget",
                        "iOS",
                        "-executeMethod",
                        "CesiumForUnity.BuildCesiumForUnity.CompileForIOSAndExit"
                    });

                    Console.WriteLine("**** Adding generated files (for the iOS Player) to the package");
                    AddGeneratedFiles("!UNITY_EDITOR && UNITY_IOS", generatedRuntimePath, Path.Combine(outputPackagePath, "Runtime", "generated"));
                }

                Console.WriteLine("**** Copying the rest of the package");
                CopyPackageContents(Utility.PackageRoot, outputPackagePath);

                Console.WriteLine("**** Building the package");
                unity.Run(new[]
                {
                    "-batchmode",
                    "-nographics",
                    "-projectPath",
                    Utility.ProjectRoot,
                    "-executeMethod",
                    "CesiumForUnity.BuildCesiumForUnity.PackAndExit"
                });
            }
            finally
            {
                // Restore the original contents of the rsp files.
                File.WriteAllText(runtimeCscRspPath, runtimeCscRsp, Encoding.UTF8);
                File.WriteAllText(editorCscRspPath, editorCscRsp, Encoding.UTF8);

                // Delete the temp directory.
                Directory.Delete(tempPath, true);
            }
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
                "CHANGES.md",
                "CHANGES.md.meta",
                "Runtime.meta",
                "Tests.meta",
                "ThirdParty.json",
                "ThirdParty.json.meta"
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
                "Editor/csc.rsp",
                "Editor/csc.rsp.meta",
                "Runtime/ConfigureReinterop.cs",
                "Runtime/ConfigureReinterop.cs.meta",
                "Runtime/csc.rsp",
                "Runtime/csc.rsp.meta"
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
    }
}