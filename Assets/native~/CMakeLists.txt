cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(CesiumForUnityNative
    VERSION 0.1.0
    LANGUAGES CXX C
)

option(EDITOR "Whether to build with Editor support." ON)
set(REINTEROP_GENERATED_DIRECTORY "generated-Editor" CACHE STRING "The subdirectory of each native library in which the Reinterop-generated code is found.")

# Static libraries are eventually built into shared libraries, so we need
# position independent code.
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

if (${CMAKE_SYSTEM_NAME} STREQUAL "Android")
  set(CESIUM_ARCHITECTURE "aarch64")
  set(HTTPLIB_USE_OPENSSL_IF_AVAILABLE OFF)
  set(ANDROID_ABI ${CMAKE_ANDROID_ARCH_ABI})
  set(ANDROID_NDK ${CMAKE_ANDROID_NDK})
endif()

add_subdirectory(extern/cesium-native EXCLUDE_FROM_ALL)
add_subdirectory(Runtime)

if (EDITOR)
  add_subdirectory(Editor)
endif()
