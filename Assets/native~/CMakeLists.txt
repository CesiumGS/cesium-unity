cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(CesiumForUnityNative
    VERSION 0.1.0
    LANGUAGES CXX C
)

add_subdirectory(extern/cesium-native EXCLUDE_FROM_ALL)

option(BUILD_SHARED_LIB OFF)
option( SUPPORT_CONSOLE_APP OFF)
add_subdirectory(extern/tidy-html5 EXCLUDE_FROM_ALL)

file(GLOB CESIUMFORUNITYNATIVE_SOURCES CONFIGURE_DEPENDS src/*.cpp generated/src/*.cpp)
file(GLOB CESIUMFORUNITYNATIVE_HEADERS CONFIGURE_DEPENDS src/*.h generated/src/*.h)

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
        generated/src
        generated/include
        # tidy-html5 doesn't use target_include_directories, so add it manually
        extern/tidy-html5/include
)

target_link_libraries(
    CesiumForUnityNative
    PUBLIC
        Cesium3DTilesSelection
    PRIVATE
        tidy-static
)

set_target_properties(
    CesiumForUnityNative
    PROPERTIES
        CXX_STANDARD 17
        CXX_STANDARD_REQUIRED YES
        CXX_EXTENSIONS NO
)

if (EDITOR)
  target_compile_definitions(CesiumForUnityNative PRIVATE UNITY_EDITOR)
endif()

install(FILES $<TARGET_FILE:CesiumForUnityNative> DESTINATION ".")
