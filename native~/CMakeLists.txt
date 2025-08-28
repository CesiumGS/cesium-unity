cmake_minimum_required(VERSION 3.18)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(CesiumForUnityNative
    VERSION 0.1.0
    LANGUAGES CXX C
)

get_filename_component(toolchainFile "${CMAKE_TOOLCHAIN_FILE}" NAME)
if(toolchainFile STREQUAL "Emscripten.cmake")
    set(CESIUM_TARGET_WASM ON)
    # Include the toolchain directly as ezvcpkg will overwrite the 
    # toolchain before it's loaded
    include(${CMAKE_TOOLCHAIN_FILE})
endif()

option(CESIUM_WASM64 "Enable 64-bit WebAssembly target" OFF)

if(CESIUM_TARGET_WASM)
    # vcpkg will attempt to second-guess our CMAKE_C_COMPILER setting, choosing to go with the value of CC instead.
    # While normally this is the correct value to go with, for wasm we need to be using emcc and em++.
    # So we set CC and CXX to emcc and em++ here so vcpkg will pick them up properly.
    # Does this make sense? No. Does it work? Somehow. ¯\_(ツ)_/¯
    set(ENV{CC} ${CMAKE_C_COMPILER})
    set(ENV{CXX} ${CMAKE_CXX_COMPILER})

    set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY BOTH)
    set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE BOTH)
    set(CMAKE_FIND_ROOT_PATH_MODE_PACKAGE BOTH)
    set(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)

    # Make sure all files are compiled with -pthread and other required flags.
    # HAS_FUTIME is for tidy-html5 to keep it from using <sys/utime.h>, which is not available with Emscripten.
    if(CESIUM_WASM64)
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -sMEMORY64=1 -fwasm-exceptions -mbulk-memory -mnontrapping-fptoint -sSUPPORT_LONGJMP=wasm -pthread -DHAS_FUTIME=0")
        set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -sMEMORY64=1 -fwasm-exceptions -mbulk-memory -mnontrapping-fptoint -sSUPPORT_LONGJMP=wasm -pthread -DHAS_FUTIME=0")
    else()
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fwasm-exceptions -mbulk-memory -mnontrapping-fptoint -sSUPPORT_LONGJMP=wasm -pthread -DHAS_FUTIME=0")
        set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fwasm-exceptions -mbulk-memory -mnontrapping-fptoint -sSUPPORT_LONGJMP=wasm -pthread -DHAS_FUTIME=0")
    endif()
endif()

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

if (CESIUM_TARGET_WASM)
  set(CMAKE_INSTALL_PREFIX "${CMAKE_CURRENT_LIST_DIR}/../Plugins/WebGL")
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
    else()
        set(VCPKG_TRIPLET "${DETECTED_VCPKG_TRIPLET}-unity")
    endif()
endif()

if(CESIUM_TARGET_WASM)
    if(CESIUM_WASM64)
        # vcpkg will attempt to make this wasm32-emscripten if we don't declare it
        set(VCPKG_TARGET_TRIPLET wasm64-emscripten-unity)
    else()
        # vcpkg will attempt to make this wasm32-emscripten if we don't declare it
        set(VCPKG_TARGET_TRIPLET wasm32-emscripten-unity)
    endif()
endif()

# We never need any of cesium-native's header files to be installed.
set(CESIUM_INSTALL_HEADERS OFF)

# We only need cesium-native's static lib files to be installed on iOS.
if (NOT IOS AND NOT CESIUM_TARGET_WASM)
    set(CESIUM_INSTALL_STATIC_LIBS OFF)
endif()

add_subdirectory(extern/swl-variant)

set(CESIUM_DISABLE_CURL ON)
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

if (CESIUM_TARGET_WASM)
  set(LIB_INSTALL_DIR "${CMAKE_INSTALL_PREFIX}")
else()
  set(LIB_INSTALL_DIR ${CMAKE_INSTALL_LIBDIR})  
endif()

# Specify all targets that need to compile bitcode
if (${CMAKE_SYSTEM_NAME} STREQUAL "iOS" OR CESIUM_TARGET_WASM)
  install(TARGETS tidy-static)
  set_target_properties(tidy-static PROPERTIES EXCLUDE_FROM_ALL 0 EXCLUDE_FROM_DEFAULT_BUILD 0)
endif()
