cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(CesiumForUnityNative
    VERSION 0.1.0
    LANGUAGES CXX C
)

get_filename_component(toolchainFile "${CMAKE_TOOLCHAIN_FILE}" NAME)
if(toolchainFile STREQUAL "Emscripten.cmake")
    set(CESIUM_TARGET_WASM ON)
endif()

file(GLOB_RECURSE CESIUMFORUNITYNATIVE_SOURCES CONFIGURE_DEPENDS src/*.cpp ../Shared/src/*.cpp ${REINTEROP_GENERATED_DIRECTORY}/src/*.cpp)
file(GLOB_RECURSE CESIUMFORUNITYNATIVE_HEADERS CONFIGURE_DEPENDS src/*.h ${REINTEROP_GENERATED_DIRECTORY}/src/*.h)

if (${CMAKE_SYSTEM_NAME} STREQUAL "iOS" OR CESIUM_TARGET_WASM)
    add_library(CesiumForUnityNative-Runtime STATIC)
else()
    add_library(CesiumForUnityNative-Runtime SHARED)
endif()

target_sources(
  CesiumForUnityNative-Runtime
    PRIVATE
        ${CESIUMFORUNITYNATIVE_SOURCES}
        ${CESIUMFORUNITYNATIVE_HEADERS}
)

target_include_directories(
  CesiumForUnityNative-Runtime
    PRIVATE
        src
        include
        ../Shared/src
        ${REINTEROP_GENERATED_DIRECTORY}/src
        ${REINTEROP_GENERATED_DIRECTORY}/include
        # tidy-html5 doesn't use target_include_directories, so add it manually
        ../extern/tidy-html5/include
        # same with enum-flags
        ../extern/enum-flags/include)

target_link_libraries(
  CesiumForUnityNative-Runtime
    PUBLIC
      Cesium3DTilesSelection
      CesiumAsync
      CesiumIonClient
    PRIVATE
      tidy-static
      enum-flags
      swl-variant
)

# cpp-httplib, as of v0.26.0, seems to be missing a required dependency on CoreServices on macOS, so add it here.
if (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
  target_link_libraries(
    CesiumForUnityNative-Runtime
    PRIVATE
      "-framework CoreServices"
  )
endif()

set_target_properties(
  CesiumForUnityNative-Runtime
    PROPERTIES
        CXX_STANDARD 20
        CXX_STANDARD_REQUIRED YES
        CXX_EXTENSIONS NO
)

if (MSVC)
  target_compile_options(
    CesiumForUnityNative-Runtime
    PRIVATE
      /bigobj
  )
endif()

target_compile_definitions(
  CesiumForUnityNative-Runtime
  PUBLIC
    TIDY_STATIC
    SWL_VARIANT_NO_CONSTEXPR_EMPLACE
)

# If we're building for the Unity Editor, set a #define accordingly.
if (EDITOR)
  target_compile_definitions(CesiumForUnityNative-Runtime PRIVATE UNITY_EDITOR)
endif()

install(FILES $<TARGET_FILE:CesiumForUnityNative-Runtime> DESTINATION ".")
