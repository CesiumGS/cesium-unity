# =============================================================================
# Cesium for Unity — Custom s2geometry overlay port
# =============================================================================
#
# This overlay adds a patch on top of the standard vcpkg s2geometry port to
# fix a build failure on Android.
#
# CUSTOMIZATION HISTORY:
#   Created to fix an Android Clang 12 (NDK r23) build failure introduced
#   in s2geometry 0.13.1.
#
# BACKGROUND:
#   s2geometry 0.13.1 added [[nodiscard]] to the SpinLockHolder class:
#
#     class [[nodiscard]] ABSL_SCOPED_LOCKABLE SpinLockHolder { ... };
#
#   ABSL_SCOPED_LOCKABLE expands to __attribute__((scoped_lockable)) on Clang.
#   Clang 12 (used by Unity's Android NDK r23) has a bug where mixing a C++
#   standard attribute ([[nodiscard]]) with a GNU-style attribute
#   (__attribute__(...)) between the "class" keyword and the class name causes
#   the class to be treated as anonymous, producing:
#
#     error: declaration of anonymous class must be a definition
#     error: unknown type name 'SpinLockHolder'
#
#   Clang 14+ (Unreal Engine's Android NDK r25) handles this syntax correctly.
#   We patch spinlock.h to remove [[nodiscard]] from the class declaration,
#   which is a minor quality-of-life annotation only (not a correctness issue).

if(VCPKG_TARGET_IS_WINDOWS)
    vcpkg_check_linkage(ONLY_STATIC_LIBRARY)
endif()

vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO google/s2geometry
    REF v${VERSION}
    SHA512 4ddfff2f44c0e98b2a110da57335fe119788f32e3924c8bdbe9afffbad5e037fdfe64f88f664b025a86134e17f14f6195107035b258fde06f946972f1f0456a8
    HEAD_REF main
    PATCHES
        # Taken from the upstream vcpkg port: fixes use of unqualified math
        # functions that MSVC does not find via ADL.
        fix-msvc-build.patch
        # Fixes a Clang 12 (Android NDK r23) build failure: Clang 12 cannot
        # parse a class declaration that combines a C++ standard attribute
        # ([[nodiscard]]) with a GNU attribute (__attribute__((scoped_lockable)))
        # between the "class" keyword and the class name.
        fix-android-clang12-spinlock.patch
)

vcpkg_cmake_configure(
    SOURCE_PATH "${SOURCE_PATH}"
    OPTIONS
        -DBUILD_EXAMPLES=OFF
        -DBUILD_TESTS=OFF
)
vcpkg_cmake_install()
vcpkg_cmake_config_fixup(PACKAGE_NAME s2)

file(REMOVE_RECURSE "${CURRENT_PACKAGES_DIR}/debug/include")

file(INSTALL "${SOURCE_PATH}/LICENSE" DESTINATION "${CURRENT_PACKAGES_DIR}/share/s2geometry" RENAME copyright)
file(INSTALL "${CURRENT_PORT_DIR}/usage" DESTINATION "${CURRENT_PACKAGES_DIR}/share/s2geometry")
