using System;
using System.Text;

namespace CesiumForUnity
{
    /// <summary>
    /// Shared utilities for building native code in a Linux container.
    /// Used by both the Build~ tool and the Unity Editor's CompileCesiumForUnityNative.
    /// </summary>
    internal static class LinuxContainerBuild
    {
        /// <summary>
        /// Environment variable names that are passed through to the container during a build.
        /// </summary>
        internal static readonly string[] PassthroughEnvVars = new[]
        {
            "VCPKG_BINARY_SOURCES", "AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY",
            "AWS_REGION", "CESIUM_VCPKG_RELEASE_ONLY"
        };

        /// <summary>
        /// Creates the content of the bash script that installs build tools and runs CMake inside the container.
        /// </summary>
        internal static string CreateBuildScript(string configureArgs, string buildArgs)
        {
            return "#!/bin/bash\n" +
                "set -e\n" +
                "dnf install -q -y dnf-plugins-core\n" +
                "dnf config-manager --set-enabled powertools\n" +
                "dnf module enable -y llvm-toolset\n" +
                "dnf install -q -y clang make nasm git curl zip unzip tar kernel-headers perl-core ninja-build python3 pkgconfig autoconf automake libtool patch\n" +
                "curl -fsSL https://github.com/Kitware/CMake/releases/download/v3.31.12/cmake-3.31.12-linux-x86_64.tar.gz | tar -xz -C /usr/local --strip-components=1\n" +
                "curl -fsSL https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip -o /tmp/awscliv2.zip && unzip -q /tmp/awscliv2.zip -d /tmp && /tmp/aws/install && rm -rf /tmp/awscliv2.zip /tmp/aws\n" +
                $"cmake {configureArgs}\n" +
                $"cmake {buildArgs}\n";
        }

        /// <summary>
        /// Creates the docker command-line arguments needed to run the build script in the container.
        /// </summary>
        internal static string CreateDockerArguments(
            string workingDirectory,
            string packageRoot,
            string ezvcpkgHostPath,
            string scriptPath,
            string containerImage)
        {
            StringBuilder dockerArgsBuilder = new StringBuilder("run --rm");
            dockerArgsBuilder.Append($" --workdir {QuoteArgument(workingDirectory)}");
            dockerArgsBuilder.Append($" -v {QuoteArgument($"{packageRoot}:{packageRoot}")}");
            dockerArgsBuilder.Append($" -v {QuoteArgument($"{ezvcpkgHostPath}:/root/.ezvcpkg")}");
            dockerArgsBuilder.Append($" -v {QuoteArgument($"{scriptPath}:/tmp/cesium-build.sh:ro")}");
            dockerArgsBuilder.Append(" -e CC=clang");
            dockerArgsBuilder.Append(" -e CXX=clang++");

            foreach (string envVarName in PassthroughEnvVars)
            {
                string? value = Environment.GetEnvironmentVariable(envVarName);
                if (!string.IsNullOrEmpty(value))
                    dockerArgsBuilder.Append($" -e {QuoteArgument($"{envVarName}={value}")}");
            }

            dockerArgsBuilder.Append($" {QuoteArgument(containerImage)} bash /tmp/cesium-build.sh");
            return dockerArgsBuilder.ToString();
        }

        private static string QuoteArgument(string value)
        {
            return $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
        }
    }
}
