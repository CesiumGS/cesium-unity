cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(cesium-unity
    VERSION 0.1.0
    LANGUAGES CXX C
)

if(APPLE)
    set(CMAKE_OSX_ARCHITECTURES "x86_64")
endif()

# Install to the Assets directory.
set(CMAKE_INSTALL_PREFIX ${CMAKE_CURRENT_LIST_DIR}/Assets)

add_subdirectory(CesiumForUnityNative)
