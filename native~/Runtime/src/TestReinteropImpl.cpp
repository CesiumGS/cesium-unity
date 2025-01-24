#include "TestReinteropImpl.h"

#include <DotNet/CesiumForUnity/TestReinterop.h>

#include <stdexcept>

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

bool TestReinteropImpl::ThrowCppStdException(
    const DotNet::CesiumForUnity::TestReinterop& instance) {
  throw std::exception("An exceptional hello from C++!");
}

bool TestReinteropImpl::ThrowOtherCppExceptionType(
    const DotNet::CesiumForUnity::TestReinterop& instance) {
  throw "This is a dodgy exception.";
}

} // namespace CesiumForUnityNative
