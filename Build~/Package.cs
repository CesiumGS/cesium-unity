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
        public class Options
        {
            public string? UnityVersion { get; set; }
            public FileInfo? UnityExecutablePath { get; set; }
            public DirectoryInfo? UnityBasePath { get; set; }
            public required List<string> Platforms { get; set; }
        }

        public static string[] SupportedPlatforms = new[]
        {
            "Editor",
            "Android",
            "iOS",
            "Linux",
            "macOS",
            "UWP",
            "Web",
            "Windows",
        };

        public void Run(Options options)
        {
            Unity? unity = Unity.FindUnity(options.UnityExecutablePath, options.UnityVersion, options.UnityBasePath);
            if (unity == null)
                throw new Exception("Could not find Unity!");
            Cmake cmake = new Cmake();

            Console.WriteLine("**** Using Unity at " + unity.ExecutablePath);

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Environment.SetEnvironmentVariable("CESIUM_PACKAGE_TEMP_PATH", tempPath);

            Console.WriteLine("**** Output directory " + tempPath);

            string cscRspPath = Path.Combine(Utility.PackageRoot, "Source", "csc.rsp");
            string cscRsp = File.ReadAllText(cscRspPath, Encoding.UTF8);

            try
            {
                Directory.CreateDirectory(tempPath);

                string outputPackagePath = Path.Combine(tempPath, "package");
                Directory.CreateDirectory(outputPackagePath);

                string outputGeneratedPath = Path.Combine(outputPackagePath, "Source", "generated");
                Directory.CreateDirectory(outputGeneratedPath);

                Console.WriteLine("**** Modifying the csc.rsp file to write generated files to disk");
                string generatedBasePath = Path.Combine(tempPath, "generated~");
                Directory.CreateDirectory(generatedBasePath);

                File.AppendAllText(cscRspPath, "-generatedfilesout:\"" + generatedBasePath + "\"" + Environment.NewLine, Encoding.UTF8);

                string generatedPath = Path.Combine(generatedBasePath, "Reinterop");
                Environment.SetEnvironmentVariable("CESIUM_GENERATED_CODE_PATH_DELETE_BEFORE_BUILD", generatedPath);

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

                // Disable Unity audio, because we don't need it and because it seems to take 10-20 minutes
                // to time out on macOS on GitHub Actions every time we start up Unity.
                string projectSettingsDirectory = Path.Combine(Utility.ProjectRoot, "ProjectSettings");
                Directory.CreateDirectory(projectSettingsDirectory);
                string audioManagerPath = Path.Combine(projectSettingsDirectory, "AudioManager.asset");
                if (!File.Exists(audioManagerPath))
                {
                    Console.WriteLine("**** Creating AudioManager.asset to disable Unity audio");
                    using (StreamWriter audioManager = new StreamWriter(audioManagerPath))
                    {
                        audioManager.WriteLine("%YAML 1.1");
                        audioManager.WriteLine("%TAG !u! tag:unity3d.com,2011:");
                        audioManager.WriteLine("--- !u!11 &1");
                        audioManager.WriteLine("AudioManager:");
                        audioManager.WriteLine("  m_DisableAudio: 1");
                    }
                }

                if (options.Platforms.Contains("Editor"))
                {
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

                    AddGeneratedFiles(platformEditorConditional, generatedPath, outputGeneratedPath);

                    // Clean the generated code directories.
                    Directory.Delete(generatedPath, true);
                    Directory.CreateDirectory(generatedPath);

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
                        configureArgs = configureArgs.Concat(new[]
                        {
                            "-DCMAKE_OSX_DEPLOYMENT_TARGET=10.15"
                        }).ToList();

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
                        string? containerImage = Environment.GetEnvironmentVariable("CESIUM_NATIVE_BUILD_CONTAINER");
                        if (OperatingSystem.IsLinux() && !string.IsNullOrEmpty(containerImage))
                            RunCmakeInContainer(containerImage, configureArgs, buildArgs);
                        else
                        {
                            Utility.Run("cmake", configureArgs);
                            Utility.Run("cmake", buildArgs);
                        }
                    }
                }

                if (options.Platforms.Contains("UWP"))
                {
                    Console.WriteLine("**** Compiling for Universal Windows Platform Player");
                    unity.Run(new[]
                    {
                        "-batchmode",
                        "-nographics",
                        "-projectPath",
                        Utility.ProjectRoot,
                        "-buildTarget",
                        "WindowsStoreApps",
                        "-executeMethod",
                        "CesiumForUnity.BuildCesiumForUnity.CompileForUWPAndExit"
                    });

                    Console.WriteLine("**** Adding generated files (for the UWP Player) to the package");
                    AddGeneratedFiles("!UNITY_EDITOR && UNITY_WSA", generatedPath, outputGeneratedPath);

                    // Clean the generated code directory.
                    Directory.Delete(generatedPath, true);
                    Directory.CreateDirectory(generatedPath);
                }

                if (options.Platforms.Contains("Windows"))
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
                    AddGeneratedFiles("!UNITY_EDITOR && UNITY_STANDALONE_WIN", generatedPath, outputGeneratedPath);

                    // Clean the generated code directory.
                    Directory.Delete(generatedPath, true);
                    Directory.CreateDirectory(generatedPath);
                }

                if (options.Platforms.Contains("Android"))
                {
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
                    AddGeneratedFiles("!UNITY_EDITOR && UNITY_ANDROID", generatedPath, outputGeneratedPath);

                    // Clean the generated code directory.
                    Directory.Delete(generatedPath, true);
                    Directory.CreateDirectory(generatedPath);
                }


                if (options.Platforms.Contains("Web"))
                {
                    Console.WriteLine("**** Compiling for Web Player");
                    unity.Run(new[]
                    {
                        "-batchmode",
                        "-nographics",
                        "-projectPath",
                        Utility.ProjectRoot,
                        "-buildTarget",
                        "WebGL",
                        "-executeMethod",
                        "CesiumForUnity.BuildCesiumForUnity.CompileForWebAndExit"
                    });

                    Console.WriteLine("**** Adding generated files (for the Web Player) to the package");
                    AddGeneratedFiles("!UNITY_EDITOR && UNITY_WEBGL", generatedPath, outputGeneratedPath);

                    // Clean the generated code directory.
                    Directory.Delete(generatedPath, true);
                    Directory.CreateDirectory(generatedPath);
                }

                if (options.Platforms.Contains("Linux"))
                {
                    Console.WriteLine("**** Compiling for Linux Player");
                    unity.Run(new[]
                    {
                        "-batchmode",
                        "-nographics",
                        "-projectPath",
                        Utility.ProjectRoot,
                        "-buildTarget",
                        "Linux64",
                        "-executeMethod",
                        "CesiumForUnity.BuildCesiumForUnity.CompileForLinuxAndExit"
                    });

                    Console.WriteLine("**** Adding generated files (for the Linux Player) to the package");
                    AddGeneratedFiles("!UNITY_EDITOR && UNITY_STANDALONE_LINUX", generatedPath, outputGeneratedPath);

                    // Clean the generated code directory.
                    Directory.Delete(generatedPath, true);
                    Directory.CreateDirectory(generatedPath);
                }

                if (options.Platforms.Contains("macOS"))
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
                    AddGeneratedFiles("!UNITY_EDITOR && UNITY_STANDALONE_OSX", generatedPath, outputGeneratedPath);

                    // Clean the generated code directory.
                    Directory.Delete(generatedPath, true);
                    Directory.CreateDirectory(generatedPath);
                }

                if (options.Platforms.Contains("iOS"))
                {
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
                    AddGeneratedFiles("!UNITY_EDITOR && UNITY_IOS", generatedPath, outputGeneratedPath);

                    // Clean the generated code directory.
                    Directory.Delete(generatedPath, true);
                    Directory.CreateDirectory(generatedPath);
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
                File.WriteAllText(cscRspPath, cscRsp, Encoding.UTF8);

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

        private static void RunCmakeInContainer(
            string containerImage,
            IEnumerable<string> configureArgs,
            IEnumerable<string> buildArgs)
        {
            string packageRoot = Utility.PackageRoot;
            string ezvcpkgHostPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ezvcpkg");

            // Write cmake commands to a script file to avoid shell quoting complexity.
            string scriptPath = Path.Combine(Path.GetTempPath(), $"cesium-cmake-{Guid.NewGuid():N}.sh");
            File.WriteAllText(scriptPath,
                "#!/bin/bash\n" +
                "set -e\n" +
                "dnf install -q -y dnf-plugins-core\n" +
                "dnf config-manager --set-enabled powertools\n" +
                "dnf module enable -y llvm-toolset\n" +
                "dnf install -q -y clang make nasm git curl zip unzip tar kernel-headers perl-IPC-Cmd\n" +
                "curl -fsSL https://github.com/Kitware/CMake/releases/download/v3.31.12/cmake-3.31.12-linux-x86_64.tar.gz | tar -xz -C /usr/local --strip-components=1\n" +
                "curl -fsSL https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip -o /tmp/awscliv2.zip && unzip -q /tmp/awscliv2.zip -d /tmp && /tmp/aws/install && rm -rf /tmp/awscliv2.zip /tmp/aws\n" +
                $"cmake {string.Join(' ', configureArgs)}\n" +
                $"cmake {string.Join(' ', buildArgs)}\n",
                new UTF8Encoding(false));

            try
            {
                ProcessStartInfo dockerInfo = new ProcessStartInfo("docker");
                dockerInfo.WorkingDirectory = packageRoot;

                dockerInfo.ArgumentList.Add("run");
                dockerInfo.ArgumentList.Add("--rm");
                dockerInfo.ArgumentList.Add("--workdir");
                dockerInfo.ArgumentList.Add(packageRoot);
                dockerInfo.ArgumentList.Add("-v");
                dockerInfo.ArgumentList.Add($"{packageRoot}:{packageRoot}");
                dockerInfo.ArgumentList.Add("-v");
                dockerInfo.ArgumentList.Add($"{ezvcpkgHostPath}:/root/.ezvcpkg");
                dockerInfo.ArgumentList.Add("-v");
                dockerInfo.ArgumentList.Add($"{scriptPath}:/tmp/cesium-build.sh:ro");
                dockerInfo.ArgumentList.Add("-e");
                dockerInfo.ArgumentList.Add("CC=clang");
                dockerInfo.ArgumentList.Add("-e");
                dockerInfo.ArgumentList.Add("CXX=clang++");

                foreach (string envVarName in new[] {
                    "VCPKG_BINARY_SOURCES", "AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY",
                    "AWS_REGION", "CESIUM_VCPKG_RELEASE_ONLY" })
                {
                    string? value = Environment.GetEnvironmentVariable(envVarName);
                    if (!string.IsNullOrEmpty(value))
                    {
                        dockerInfo.ArgumentList.Add("-e");
                        dockerInfo.ArgumentList.Add($"{envVarName}={value}");
                    }
                }

                dockerInfo.ArgumentList.Add(containerImage);
                dockerInfo.ArgumentList.Add("bash");
                dockerInfo.ArgumentList.Add("/tmp/cesium-build.sh");

                Utility.RunAndLog(dockerInfo);
            }
            finally
            {
                try { File.Delete(scriptPath); } catch (Exception) { }
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
                "LICENSE",
                "LICENSE.meta",
                "package.json",
                "package.json.meta",
                "Editor.meta",
                "Plugins.meta",
                "README.md",
                "README.md.meta",
                "CHANGES.md",
                "CHANGES.md.meta",
                "Source.meta",
                "Tests.meta",
                "EditorTests.meta",
                "ThirdParty.json",
                "ThirdParty.json.meta",
                ".npmrc"
            };

            foreach (string file in filesToCopy)
            {
                string completeSource = Path.Combine(sourcePath, file);
                if (File.Exists(completeSource))
                {
                    File.Copy(completeSource, Path.Combine(targetPath, file));
                }
            }

            string[] pathsToCopy = new[]
            {
                "Source",
                "Tests",
                "EditorTests",
                "Documentation~",
                "Plugins",
                "Editor"
            };

            foreach (string pathToCopy in pathsToCopy)
            {
                string completeSource = Path.Combine(sourcePath, pathToCopy);
                if (Directory.Exists(completeSource))
                {
                    CopyDirectory(completeSource, Path.Combine(targetPath, pathToCopy));
                }
            }

            // Remove build-related sources files that don't make sense in the published package
            string[] filesToDelete = new[]
            {
                "Source/Editor/CompileCesiumForUnityNative.cs",
                "Source/Editor/CompileCesiumForUnityNative.cs.meta",
                "Source/Editor/BuildCesiumForUnity.cs",
                "Source/Editor/BuildCesiumForUnity.cs.meta",
                "Source/Editor/ConfigureReinteropEditor.cs",
                "Source/Editor/ConfigureReinteropEditor.cs.meta",
                "Source/Runtime/ConfigureReinterop.cs",
                "Source/Runtime/ConfigureReinterop.cs.meta",
                "Source/csc.rsp",
                "Source/csc.rsp.meta"
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