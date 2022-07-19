cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(TestOxidizeNative
    VERSION 0.1.0
    LANGUAGES CXX C
)

file(GLOB TESTOXIDIZENATIVE_SOURCES src/*.cpp generated/src/*.cpp)
file(GLOB TESTOXIDIZENATIVE_HEADERS src/*.h generated/src/*.h)

add_library(TestOxidizeNative SHARED)

target_sources(
  TestOxidizeNative
    PRIVATE
        ${TESTOXIDIZENATIVE_SOURCES}
        ${TESTOXIDIZENATIVE_HEADERS}
)

target_include_directories(
  TestOxidizeNative
    PUBLIC
        include
        generated/include
    PRIVATE
        src
        generated/src
)

# target_link_libraries(
#     TestOxidize
#     PUBLIC
#         Cesium3DTilesSelection
# )

set_target_properties(
  TestOxidizeNative
    PROPERTIES
        CXX_STANDARD 17
        CXX_STANDARD_REQUIRED YES
        CXX_EXTENSIONS NO
)

# install(FILES $<TARGET_FILE:CesiumForUnityNative> DESTINATION ${CMAKE_SYSTEM_NAME})
