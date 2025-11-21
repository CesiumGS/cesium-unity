include(${CMAKE_CURRENT_LIST_DIR}/common.cmake)

if(NOT EMSCRIPTEN_ROOT AND DEFINED ENV{EMSDK})
   set(EMSCRIPTEN_ROOT "$ENV{EMSDK}/upstream/emscripten")
   if(NOT EXISTS "${EMSCRIPTEN_ROOT}/cmake/Modules/Platform/Emscripten.cmake")
      set(EMSCRIPTEN_ROOT "$ENV{EMSDK}/emscripten")
   endif()
endif()
