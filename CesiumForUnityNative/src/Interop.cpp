#include "Interop.h"

#include "Bindings.h"

#include <CesiumUtility/ScopeGuard.h>

using namespace CesiumForUnity;
using namespace CesiumUtility;
using namespace System::Runtime::InteropServices;

std::string Interop::convert(System::String& s) {
  void* p = Marshal::StringToCoTaskMemUTF8(s);
  ScopeGuard sg([p]() { Marshal::FreeCoTaskMem(p); });
  return std::string(static_cast<char*>(p));
}
