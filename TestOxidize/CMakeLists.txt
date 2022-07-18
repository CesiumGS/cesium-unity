cmake_minimum_required(VERSION 3.15)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(TestOxidize
    VERSION 0.1.0
    LANGUAGES CXX C
)

file(GLOB TESTOXIDIZE_SOURCES src/*.cpp generated/src/*.cpp)
file(GLOB TESTOXIDIZE_HEADERS src/*.h generated/src/*.h)

add_library(TestOxidize STATIC)

target_sources(
    TestOxidize
    PRIVATE
        ${TESTOXIDIZE_SOURCES}
        ${TESTOXIDIZE_HEADERS}
)

target_include_directories(
    TestOxidize
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
    TestOxidize
    PROPERTIES
        CXX_STANDARD 17
        CXX_STANDARD_REQUIRED YES
        CXX_EXTENSIONS NO
)

# install(FILES $<TARGET_FILE:CesiumForUnityNative> DESTINATION ${CMAKE_SYSTEM_NAME})
