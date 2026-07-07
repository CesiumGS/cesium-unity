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
    }
}
