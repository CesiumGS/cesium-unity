using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Build
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Command listPlatformsCommand = new("list-platforms")
            {
                Description = "Lists the supported package platforms and exits."
            };

            Command packageCommand = new("package")
            {
                Description = "Packages the plugin",
            };

            Option<string> unityVersionOption = new("--unity-version")
            {
                Description = "The version of Unity to use for the build."
            };

            Option<DirectoryInfo> unityBasePathOption = new("--unity-base-path")
            {
                Description = "The base path to the Unity installation. A subdirectory matching the specified Unity version should be found in this directory."
            };

            Option<FileInfo> unityExecutableOption = new("--unity-executable")
            {
                Description = "The path to the Unity executable to use."
            };

            Option<List<string>> platformsOption = new("--platform")
            {
                Description = "Specifies a platform for which to package. Can be specified multiple times to include multiple platforms.",
                Arity = ArgumentArity.OneOrMore,
                Required = true,
            };

            packageCommand.Add(unityVersionOption);
            packageCommand.Add(unityExecutableOption);
            packageCommand.Add(unityBasePathOption);
            packageCommand.Add(platformsOption);

            RootCommand rootCommand = new("Cesium for Unity Build Tool")
            {
                listPlatformsCommand,
                packageCommand
            };

            listPlatformsCommand.SetAction(parseResult =>
            {
                foreach (string platform in Package.SupportedPlatforms)
                {
                    Console.WriteLine(platform);
                }
                return 0;
            });

            packageCommand.SetAction(parseResult =>
            {
                List<string> platforms =parseResult.GetRequiredValue(platformsOption);
                foreach (string platform in platforms)
                {
                    if (!Package.SupportedPlatforms.Contains(platform))
                    {
                        Console.Error.WriteLine($"Error: Unsupported platform '{platform}'.");
                        return 1;
                    }
                    Console.WriteLine($"Including platform: {platform}");
                }

                new Package().Run(new Package.Options()
                {
                    UnityVersion = parseResult.GetValue(unityVersionOption),
                    UnityExecutablePath = parseResult.GetValue(unityExecutableOption),
                    UnityBasePath = parseResult.GetValue(unityBasePathOption),
                    Platforms = platforms
                });

                return 0;
            });

            ParseResult parseResult = rootCommand.Parse(args);
            return parseResult.Invoke();
        }
    }
}
