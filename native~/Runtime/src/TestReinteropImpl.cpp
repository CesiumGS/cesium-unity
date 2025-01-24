#include "TestReinteropImpl.h"

#include <DotNet/CesiumForUnity/TestReinterop.h>

namespace CesiumForUnityNative {

bool TestReinteropImpl::CallThrowAnExceptionFromCpp(
    const DotNet::CesiumForUnity::TestReinterop& instance) {
  try {
    instance.ThrowAnException();
  } catch (...) {
    return true;
  }

  return false;
}

} // namespace CesiumForUnityNative
