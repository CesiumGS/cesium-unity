#include "TestReinteropImpl.h"

#include <DotNet/CesiumForUnity/TestReinterop.h>

namespace CesiumForUnityNative {

bool TestReinteropImpl::CallThrowAnExceptionFromCppAndCatchIt(
    const DotNet::CesiumForUnity::TestReinterop& instance) {
  try {
    instance.ThrowAnException();
  } catch (...) {
    return true;
  }

  return false;
}

bool TestReinteropImpl::CallThrowAnExceptionFromCppAndDontCatchIt(
    const DotNet::CesiumForUnity::TestReinterop& instance) {
  instance.ThrowAnException();
  return false;
}

} // namespace CesiumForUnityNative
