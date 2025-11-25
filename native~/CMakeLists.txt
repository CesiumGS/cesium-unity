cmake_minimum_required(VERSION 3.18)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(CesiumForUnityNative
    VERSION 0.1.0
    LANGUAGES CXX C
)

# By default, vcpkg (or is it cmake?) will try to link against the Debug version
# of the vcpkg libraries when we build the RelWithDebInfo configuration. Tell it
# to prefer Release instead. vcpkg Release builds DO have debug symbols, just like
# RelWithDebInfo.
SET(CMAKE_MAP_IMPORTED_CONFIG_RELWITHDEBINFO "RelWithDebInfo;Release;")

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

if(NOT ${CMAKE_SYSTEM_NAME} MATCHES "WindowsStore")
  # Link with MSVC static runtime library. This is not supported on UWP / WindowsStore.
  set(CESIUM_MSVC_STATIC_RUNTIME_ENABLED ON)
  set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

  # This setting applies to tidy-html5
  set(USE_STATIC_RUNTIME ON)
endif()

set(BUILD_SHARED_LIB OFF)
set(SUPPORT_CONSOLE_APP OFF)
set(CMAKE_POLICY_VERSION_MINIMUM "3.5")
add_subdirectory(extern/tidy-html5 EXCLUDE_FROM_ALL)

if (${CMAKE_SYSTEM_NAME} STREQUAL "Emscripten")
  # The version of Emscripten included with Unity doesn't include sys/utime.h,
  # so tell tidy-html5 not to use it.
  target_compile_definitions(
    tidy-static
    PUBLIC
      HAS_FUTIME=0
  )
endif()

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

# vcpkg setup
set(ENV{VCPKG_OVERLAY_PORTS} "${CMAKE_CURRENT_LIST_DIR}/vcpkg/ports")
set(ENV{VCPKG_OVERLAY_TRIPLETS} "${CMAKE_CURRENT_LIST_DIR}/vcpkg/triplets")

include(${CMAKE_CURRENT_SOURCE_DIR}/extern/cesium-native/cmake/detect-vcpkg-triplet.cmake)

if (NOT VCPKG_TRIPLET)
    if (DEFINED ENV{VCPKG_TRIPLET})
        set(VCPKG_TRIPLET "$ENV{VCPKG_TRIPLET}")
    elseif(DETECTED_VCPKG_TRIPLET_ERROR)
        message(FATAL_ERROR "${DETECTED_VCPKG_TRIPLET_ERROR}")
    elseif(CESIUM_WASM64 AND "${DETECTED_VCPKG_TRIPLET}" STREQUAL "wasm32-emscripten")
        set(VCPKG_TRIPLET wasm64-emscripten-unity)
    else()
        set(VCPKG_TRIPLET "${DETECTED_VCPKG_TRIPLET}-unity")
    endif()
endif()

# We never need any of cesium-native's header files to be installed.
set(CESIUM_INSTALL_HEADERS OFF)

# We only need cesium-native's static lib files to be installed on iOS and Wasm.
if (NOT IOS AND NOT ${CMAKE_SYSTEM_NAME} STREQUAL "Emscripten")
    set(CESIUM_INSTALL_STATIC_LIBS OFF)
endif()

add_subdirectory(extern/swl-variant)

# if(CESIUM_TARGET_WASM AND EXISTS extern/blend2d)
#   # fix blend2d not working from vcpkg install
#   set(BLEND2D_EXTERNAL_ASMJIT OFF)
#   set(BLEND2D_STATIC ON)
#   add_subdirectory(extern/blend2d)
#   install(TARGETS blend2d) # TODO: just install static lib, not headers
#   set_target_properties(blend2d PROPERTIES EXCLUDE_FROM_ALL 0 EXCLUDE_FROM_DEFAULT_BUILD 0)
# endif()

# We don't need cesium-native's Curl-based asset accessor, because Cesium for Unity supplies
# its own built on .NET libraries.
set(CESIUM_DISABLE_CURL ON)

# In Emscripten builds _only_, using libjpeg-turbo leads to multiply-defined symbols because
# Unity itself appears to include this library. So disable it for Wasm builds.
if (${CMAKE_SYSTEM_NAME} STREQUAL "Emscripten")
  set(CESIUM_DISABLE_LIBJPEG_TURBO ON)
endif()

add_subdirectory(extern/cesium-native)

# Don't build the cesium-native tests by default.
set_target_properties(cesium-native-tests PROPERTIES EXCLUDE_FROM_ALL 1 EXCLUDE_FROM_DEFAULT_BUILD 1)

# Enum flags is a header-only library without a CMakeLists.txt
file(GLOB ENUM_FLAGS_HEADERS ${CMAKE_CURRENT_LIST_DIR}/extern/enum-flags/include/*.hpp)
add_library(enum-flags INTERFACE ${ENUM_FLAGS_HEADERS})

add_subdirectory(Runtime)

if (EDITOR)
  add_subdirectory(Editor)
endif()

set(LIB_INSTALL_DIR ${CMAKE_INSTALL_LIBDIR})

# Specify all targets that need to compile bitcode
if (${CMAKE_SYSTEM_NAME} STREQUAL "iOS" OR ${CMAKE_SYSTEM_NAME} STREQUAL "Emscripten")
  install(TARGETS tidy-static)
  set_target_properties(tidy-static PROPERTIES EXCLUDE_FROM_ALL 0 EXCLUDE_FROM_DEFAULT_BUILD 0)
endif()
