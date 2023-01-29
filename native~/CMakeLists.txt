cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

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

option(EDITOR "Whether to build with Editor support." ON)
set(REINTEROP_GENERATED_DIRECTORY "generated-Editor" CACHE STRING "The subdirectory of each native library in which the Reinterop-generated code is found.")

# Static libraries are eventually built into shared libraries, so we need
# position independent code.
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

set(BUILD_SHARED_LIB OFF)
set( SUPPORT_CONSOLE_APP OFF)
add_subdirectory(extern/tidy-html5 EXCLUDE_FROM_ALL)

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
        Cesium3DTilesSelection
        CesiumAsync
        CesiumGeospatial
        CesiumGeometry
        CesiumGltf
        CesiumGltfReader
        CesiumJsonReader
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
    )
    install(TARGETS tidy-static)
    install(TARGETS ${ALL_TARGETS} PUBLIC_HEADER EXCLUDE_FROM_ALL)
    set_target_properties(cesium-native-tests PROPERTIES EXCLUDE_FROM_ALL 1 EXCLUDE_FROM_DEFAULT_BUILD 1)
    set_target_properties(tidy-static PROPERTIES EXCLUDE_FROM_ALL 0 EXCLUDE_FROM_DEFAULT_BUILD 0)
    set_target_properties(${ALL_TARGETS} PROPERTIES XCODE_ATTRIBUTE_ENABLE_BITCODE "YES")
endif()


