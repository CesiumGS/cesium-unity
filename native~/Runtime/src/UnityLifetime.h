#pragma once

#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Object.h>

#if UNITY_EDITOR
#include <DotNet/UnityEditor/EditorApplication.h>
#endif

namespace CesiumForUnityNative {

class UnityLifetime {
public:
  template <typename T> static void Destroy(const T& o) {
#if UNITY_EDITOR
    // In the Editor, we must use DestroyImmediate because Destroy won't
    // actually destroy the object.
    if (!DotNet::UnityEditor::EditorApplication::isPlaying()) {
      DotNet::UnityEngine::Object::DestroyImmediate(o);
      return;
    }
#endif

    DotNet::UnityEngine::Object::Destroy(o);
  }
};

} // namespace CesiumForUnityNative
