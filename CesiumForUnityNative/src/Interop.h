#pragma once

#include <string>

namespace System {
struct String;
}

namespace CesiumForUnity {

class Interop {
public:
  static std::string convert(System::String& s);
};

} // namespace CesiumForUnity
