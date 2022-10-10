cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(CesiumForUnityNative
    VERSION 0.1.0
    LANGUAGES CXX C
)

file(GLOB CESIUMFORUNITYNATIVE_SOURCES CONFIGURE_DEPENDS src/*.cpp ../../native~/src/*.cpp ${REINTEROP_GENERATED_DIRECTORY}/src/*.cpp)
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
        ../../native~/src
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
