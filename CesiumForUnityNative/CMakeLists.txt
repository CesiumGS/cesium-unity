cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(CesiumForUnityNative
    VERSION 0.1.0
    LANGUAGES CXX C
)

# Install to 
set(CMAKE_INSTALL_PREFIX ${CMAKE_CURRENT_LIST_DIR}/../CesiumForUnityNativeBindings/native)

add_subdirectory(extern/cesium-native EXCLUDE_FROM_ALL)

file(GLOB CESIUMFORUNITYNATIVE_SOURCES src/*.cpp generated/src/*.cpp)
file(GLOB CESIUMFORUNITYNATIVE_HEADERS src/*.h generated/src/*.h)

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
        generated/src
)

target_link_libraries(
    CesiumForUnityNative
    PUBLIC
        Cesium3DTilesSelection
)

install(FILES $<TARGET_FILE:CesiumForUnityNative> DESTINATION ${CMAKE_SYSTEM_NAME})
