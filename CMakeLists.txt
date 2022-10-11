cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(cesium-unity
    VERSION 0.1.0
    LANGUAGES CXX C
)

# When building for the Editor, both Runtime and Editor assemblies are
# written to the Editor directory so that Unity won't load them in
# a standalone build.
set(CMAKE_INSTALL_PREFIX ${CMAKE_CURRENT_LIST_DIR}/Assets/Editor)

add_subdirectory(Assets/native~)
