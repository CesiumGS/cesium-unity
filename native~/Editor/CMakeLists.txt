cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(CesiumForUnityNative-Editor
    VERSION 0.1.0
    LANGUAGES CXX C
)

file(GLOB_RECURSE CESIUMFORUNITYNATIVEEDITOR_SOURCES CONFIGURE_DEPENDS src/*.cpp ../Shared/src/*.cpp ${REINTEROP_GENERATED_DIRECTORY}/src/*.cpp)
file(GLOB_RECURSE CESIUMFORUNITYNATIVEEDITOR_HEADERS CONFIGURE_DEPENDS src/*.h ${REINTEROP_GENERATED_DIRECTORY}/src/*.h)

add_library(CesiumForUnityNative-Editor SHARED)

target_sources(
    CesiumForUnityNative-Editor
    PRIVATE
        ${CESIUMFORUNITYNATIVEEDITOR_SOURCES}
        ${CESIUMFORUNITYNATIVEEDITOR_HEADERS}
)

target_include_directories(
    CesiumForUnityNative-Editor
    PRIVATE
        src
        include
        ../Shared/src
        ${REINTEROP_GENERATED_DIRECTORY}/src
        ${REINTEROP_GENERATED_DIRECTORY}/include
        # enum-flags doesn't use target_include_directories, so add it manually
        ../extern/enum-flags/include
)

target_link_libraries(
    CesiumForUnityNative-Editor
    PUBLIC
        CesiumIonClient
    PRIVATE
        enum-flags
)

set_target_properties(
    CesiumForUnityNative-Editor
    PROPERTIES
        CXX_STANDARD 17
        CXX_STANDARD_REQUIRED YES
        CXX_EXTENSIONS NO
)

# If we're building for the Unity Editor, set a #define accordingly.
if (EDITOR)
  target_compile_definitions(CesiumForUnityNative-Editor PRIVATE UNITY_EDITOR)
endif()

install(FILES $<TARGET_FILE:CesiumForUnityNative-Editor> DESTINATION ".")
