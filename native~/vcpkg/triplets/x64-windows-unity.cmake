include("${CMAKE_CURRENT_LIST_DIR}/shared/common.cmake")

set(VCPKG_TARGET_ARCHITECTURE x64)
set(VCPKG_CRT_LINKAGE static)
set(VCPKG_LIBRARY_LINKAGE static)

# In Debug builds on Windows, the default _ITERATOR_DEBUG_LEVEL=2 makes things _really_ slow,
# for little benefit. Use a more reasonable debug level.
set(VCPKG_CXX_FLAGS_DEBUG "/D_ITERATOR_DEBUG_LEVEL=1")
set(VCPKG_C_FLAGS_DEBUG "/D_ITERATOR_DEBUG_LEVEL=1")