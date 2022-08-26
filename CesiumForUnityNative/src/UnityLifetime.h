#pragma once

#include <DotNet/UnityEngine/Application.h>

namespace CesiumForUnityNative {

class UnityLifetime {
public:
  template <typename T> static void Destroy(const T& o) {
    // In the Editor, we must use DestroyImmediate because Destroy won't
    // actually destroy the object.
    if (DotNet::UnityEngine::Application::isEditor())
      DotNet::UnityEngine::Object::DestroyImmediate(o);
    else
      DotNet::UnityEngine::Object::Destroy(o);
  }
};

} // namespace CesiumForUnityNative
