cmake_minimum_required(VERSION 3.18)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

# Make sure we use the same runtime library as cesium-native so our binaries are compatible
set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

project(CesiumForUnityNative
    VERSION 0.1.0
    LANGUAGES CXX C
)

# When building for the Editor, both Runtime and Editor assemblies are
# written to the Editor directory so that Unity won't load them in
# a standalone build.
if (CMAKE_INSTALL_PREFIX_INITIALIZED_TO_DEFAULT)
  set(CMAKE_INSTALL_PREFIX "${CMAKE_CURRENT_LIST_DIR}/../Editor" CACHE PATH "Installed to the Editor directory." FORCE)
endif()

option(CESIUM_TRACING_ENABLED "Whether to enable the Cesium performance tracing framework (CESIUM_TRACE_* macros)." OFF)
option(EDITOR "Whether to build with Editor support." ON)
set(REINTEROP_GENERATED_DIRECTORY "generated-Editor" CACHE STRING "The subdirectory of each native library in which the Reinterop-generated code is found.")

if (CESIUM_TRACING_ENABLED)
    add_compile_definitions(CESIUM_TRACING_ENABLED=1)
endif()

# Static libraries are eventually built into shared libraries, so we need
# position independent code.
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

set(BUILD_SHARED_LIB OFF)
set( SUPPORT_CONSOLE_APP OFF)
add_subdirectory(extern/tidy-html5 EXCLUDE_FROM_ALL)

# Patch tidy-html5's sprtf.c to work on UWP. UWP doesn't have lstrlen, but
# it's totally unnecessary because simply passing -1 as the length will do the same thing.
# So just swap in the -1.
if (${CMAKE_SYSTEM_NAME} MATCHES "WindowsStore")
  get_target_property(tidySources tidy-static "SOURCES")
  list(REMOVE_ITEM tidySources "src/sprtf.c")
  file(READ "extern/tidy-html5/src/sprtf.c" sprtSource)
  string(REPLACE "int len = (int)lstrlen(ps);" "int len = -1;" sprtSource "${sprtSource}")
  file(CONFIGURE OUTPUT "${CMAKE_CURRENT_BINARY_DIR}/extern/tidy-html5/sprtf.c" CONTENT "${sprtSource}")
  list(APPEND tidySources "${CMAKE_CURRENT_BINARY_DIR}/extern/tidy-html5/sprtf.c")
  set_target_properties(tidy-static PROPERTIES "SOURCES" "${tidySources}")
endif()

if (${CMAKE_SYSTEM_NAME} STREQUAL "Android")
  set(HTTPLIB_USE_OPENSSL_IF_AVAILABLE OFF)
  set(ANDROID_ABI ${CMAKE_ANDROID_ARCH_ABI})
  set(ANDROID_NDK ${CMAKE_ANDROID_NDK})
endif()

add_subdirectory(extern/cesium-native EXCLUDE_FROM_ALL)

# Enum flags is a header-only library without a CMakeLists.txt
file(GLOB ENUM_FLAGS_HEADERS ${CMAKE_CURRENT_LIST_DIR}/extern/enum-flags/include/*.hpp)
add_library(enum-flags INTERFACE ${ENUM_FLAGS_HEADERS})

add_subdirectory(Runtime)

if (EDITOR)
  add_subdirectory(Editor)
endif()

# Specify all targets that need to compile bitcode
if (${CMAKE_SYSTEM_NAME} STREQUAL "iOS")
    set (ALL_TARGETS
        CesiumForUnityNative-Runtime
        Async++
        Cesium3DTilesContent
        Cesium3DTilesReader
        Cesium3DTilesSelection
        CesiumAsync
        CesiumGeometry
        CesiumGeospatial
        CesiumGltf
        CesiumGltfContent
        CesiumGltfReader
        CesiumJsonReader
        CesiumRasterOverlays
        CesiumUtility
        draco_attributes
        draco_compression_attributes_dec
        draco_compression_bit_coders
        draco_compression_entropy
        draco_compression_decode
        draco_compression_mesh_dec
        draco_compression_point_cloud_dec
        draco_core
        draco_mesh
        draco_metadata
        draco_metadata_dec
        draco_point_cloud
        draco_points_dec
        draco_static
        expected-lite
        GSL
        httplib
        ktx_read
        meshoptimizer
        modp_b64
        spdlog
        sqlite3
        s2geometry
        tinyxml2
        tidy-static
        uriparser
        webpdecode
        webpdecoder
        webpdspdecode
        webputils
        webputilsdecode
        zlibstatic
    )
    set(MESHOPT_BUILD_SHARED_LIBS OFF BOOL FORCE)
    install(TARGETS tidy-static)
    install(TARGETS ${ALL_TARGETS} PUBLIC_HEADER EXCLUDE_FROM_ALL)
    set_target_properties(cesium-native-tests PROPERTIES EXCLUDE_FROM_ALL 1 EXCLUDE_FROM_DEFAULT_BUILD 1)
    set_target_properties(tidy-static PROPERTIES EXCLUDE_FROM_ALL 0 EXCLUDE_FROM_DEFAULT_BUILD 0)
    set_target_properties(${ALL_TARGETS} PROPERTIES XCODE_ATTRIBUTE_ENABLE_BITCODE "YES")
endif()


