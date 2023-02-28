cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(CesiumForUnityNative
    VERSION 0.1.0
    LANGUAGES CXX C
)

file(GLOB_RECURSE CESIUMFORUNITYNATIVE_SOURCES CONFIGURE_DEPENDS src/*.cpp ../Shared/src/*.cpp ${REINTEROP_GENERATED_DIRECTORY}/src/*.cpp)
file(GLOB_RECURSE CESIUMFORUNITYNATIVE_HEADERS CONFIGURE_DEPENDS src/*.h ${REINTEROP_GENERATED_DIRECTORY}/src/*.h)

if (${CMAKE_SYSTEM_NAME} STREQUAL "iOS")
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
    PRIVATE
      tidy-static
      enum-flags
)

set_target_properties(
  CesiumForUnityNative-Runtime
    PROPERTIES
        CXX_STANDARD 17
        CXX_STANDARD_REQUIRED YES
        CXX_EXTENSIONS NO
)

target_compile_definitions(
  CesiumForUnityNative-Runtime
  PUBLIC
    TIDY_STATIC
)

# If we're building for the Unity Editor, set a #define accordingly.
if (EDITOR)
  target_compile_definitions(CesiumForUnityNative-Runtime PRIVATE UNITY_EDITOR)
endif()

install(FILES $<TARGET_FILE:CesiumForUnityNative-Runtime> DESTINATION ".")
