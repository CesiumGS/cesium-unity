cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(CesiumForUnityNative
    VERSION 0.1.0
    LANGUAGES CXX C
)

option(EDITOR "Whether to build with Editor support." ON)

if (EDITOR)
  set(REINTEROP_TARGET_NAME "Editor")
  set(CMAKE_INSTALL_PREFIX ${CMAKE_CURRENT_LIST_DIR}/../Editor)
else()
  set(REINTEROP_TARGET_NAME "x64")
  #set(CMAKE_INSTALL_PREFIX ${CMAKE_CURRENT_LIST_DIR}/../Plugins/x64)
endif()

set(REINTEROP_GENERATED_DIRECTORY "generated-${REINTEROP_TARGET_NAME}" CACHE STRING "The suffix for the generated code folder.")

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

file(GLOB CESIUMFORUNITYNATIVE_SOURCES CONFIGURE_DEPENDS src/*.cpp ${REINTEROP_GENERATED_DIRECTORY}/src/*.cpp)
file(GLOB CESIUMFORUNITYNATIVE_HEADERS CONFIGURE_DEPENDS src/*.h ${REINTEROP_GENERATED_DIRECTORY}/src/*.h)

add_library(CesiumForUnityNative SHARED)

target_sources(
    CesiumForUnityNative
    PRIVATE
        ${CESIUMFORUNITYNATIVE_SOURCES}
        ${CESIUMFORUNITYNATIVE_HEADERS}
)

target_include_directories(
    CesiumForUnityNative
    PRIVATE
        src
        include
        ${REINTEROP_GENERATED_DIRECTORY}/src
        ${REINTEROP_GENERATED_DIRECTORY}/include
)

target_link_libraries(
    CesiumForUnityNative
    PUBLIC
        Cesium3DTilesSelection
)

set_target_properties(
    CesiumForUnityNative
    PROPERTIES
        CXX_STANDARD 17
        CXX_STANDARD_REQUIRED YES
        CXX_EXTENSIONS NO
)

# If we're building for the Unity Editor, set a #define accordingly.
if ("${REINTEROP_GENERATED_DIRECTORY}" STREQUAL "generated-Editor")
  target_compile_definitions(CesiumForUnityNative PRIVATE UNITY_EDITOR)
endif()

install(FILES $<TARGET_FILE:CesiumForUnityNative> DESTINATION ".")
