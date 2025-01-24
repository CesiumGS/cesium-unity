#pragma once

#include <CesiumImpl.h>

namespace DotNet::CesiumForUnity {
class TestReinterop;
}

namespace CesiumForUnityNative {

class TestReinteropImpl {
public:
  static bool CallThrowAnExceptionFromCppAndCatchIt(
      const DotNet::CesiumForUnity::TestReinterop& instance);
  static bool CallThrowAnExceptionFromCppAndDontCatchIt(
      const DotNet::CesiumForUnity::TestReinterop& instance);
};

} // namespace CesiumForUnityNative
