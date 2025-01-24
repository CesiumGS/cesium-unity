#pragma once

#include <CesiumImpl.h>

namespace DotNet::CesiumForUnity {
class TestReinterop;
}

namespace CesiumForUnityNative {

class TestReinteropImpl {
public:
  static bool CallThrowAnExceptionFromCpp(
      const DotNet::CesiumForUnity::TestReinterop& instance);
};

} // namespace CesiumForUnityNative
