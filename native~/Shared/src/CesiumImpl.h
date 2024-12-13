#pragma once

#include <CesiumUtility/ReferenceCounted.h>

namespace CesiumForUnityNative {

template <typename TDerived>
class CesiumImpl : public CesiumUtility::ReferenceCountedThreadSafe<TDerived> {
public:
  CesiumImpl() = default;

  // Prevent copying of impl classes
  CesiumImpl(CesiumImpl&&) = delete;
  CesiumImpl(const CesiumImpl&) = delete;
  CesiumImpl& operator=(CesiumImpl&&) = delete;
  CesiumImpl& operator=(const CesiumImpl&) = delete;
};

} // namespace CesiumForUnityNative
