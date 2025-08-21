#pragma once

#include <CesiumUtility/Assert.h>

#include <atomic>
#include <cstdint>
#include <type_traits>

namespace CesiumForUnityNative {

template <typename TDerived> class CesiumImpl {
public:
  /**
   * @brief Adds a counted reference to this object. Use
   * {@link CesiumUtility::IntrusivePointer} instead of calling this method
   * directly.
   */
  void addReference() const /*noexcept*/ { ++this->_referenceCount; }

  /**
   * @brief Removes a counted reference from this object. When the last
   * reference is removed, this method will delete this instance. Use
   * {@link CesiumUtility::IntrusivePointer} instead of calling this method
   * directly.
   */
  void releaseReference() const /*noexcept*/ {
    CESIUM_ASSERT(this->_referenceCount > 0);
    const int32_t references = --this->_referenceCount;
    if (references == 0) {
      delete static_cast<const TDerived*>(this);
    }
  }

  /**
   * @brief Returns the current reference count of this instance.
   */
  std::int32_t getReferenceCount() const noexcept {
    return this->_referenceCount;
  }

  // Prevent copying of impl classes
  CesiumImpl(CesiumImpl&&) = delete;
  CesiumImpl(const CesiumImpl&) = delete;
  CesiumImpl& operator=(CesiumImpl&&) = delete;
  CesiumImpl& operator=(const CesiumImpl&) = delete;

private:
  CesiumImpl() noexcept = default;
  ~CesiumImpl() noexcept { CESIUM_ASSERT(this->_referenceCount == 0); }

  friend TDerived;

  mutable std::atomic<std::int32_t> _referenceCount{0};
};

} // namespace CesiumForUnityNative
