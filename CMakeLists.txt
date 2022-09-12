cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(cesium-unity
    VERSION 0.1.0
    LANGUAGES CXX C
)

option(EDITOR "Whether to build with Editor support." ON)

# Static libraries are eventually built into shared libraries, so we need
# position independent code.
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# Install to the Assets directory.
if (EDITOR)
    set(CMAKE_INSTALL_PREFIX ${CMAKE_CURRENT_LIST_DIR}/Assets/Editor)
else()
    set(CMAKE_INSTALL_PREFIX ${CMAKE_CURRENT_LIST_DIR}/Assets/NonEditor)
endif()

add_subdirectory(CesiumForUnityNative)
