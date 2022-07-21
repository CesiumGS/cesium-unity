#include <TestOxidize/System/Object.h>
#include <TestOxidize/UnityEngine/Camera.h>
#include <TestOxidize/UnityEngine/GameObject.h>

#include <cassert>

void start() {
  TestOxidize::UnityEngine::GameObject go;
  void* pFoo = go.GetHandle().GetRaw();
  assert(pFoo != nullptr);

  TestOxidize::UnityEngine::Camera camera =
      TestOxidize::UnityEngine::Camera::main();
  void* pCamera = camera.GetHandle().GetRaw();
  assert(pCamera != nullptr);
  TestOxidize::System::Object o = camera;
  void* pO = o.GetHandle().GetRaw();
  assert(pO != nullptr);
  assert(pO != pCamera);
}

void stop() {}
