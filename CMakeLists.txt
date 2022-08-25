cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(cesium-unity
    VERSION 0.1.0
    LANGUAGES CXX C
)

# Install to the Assets directory.
set(CMAKE_INSTALL_PREFIX ${CMAKE_CURRENT_LIST_DIR}/Assets)

# TODO: This is running "publish" on a normal build.
#       Ideally we'd only copy the C# DLL to Assets on install.
add_custom_target(
    CesiumForUnity ALL
    dotnet publish CesiumForUnity -c $<CONFIG>
    WORKING_DIRECTORY "${CMAKE_CURRENT_LIST_DIR}"
    VERBATIM
)

add_subdirectory(CesiumForUnityNative)

add_dependencies(CesiumForUnityNative CesiumForUnity)