#include <TestOxidize/UnityEngine/GameObject.h>

#include <cassert>

void start() {
  TestOxidize::UnityEngine::GameObject go;
  void* pFoo = go.GetHandle().GetRaw();
  assert(pFoo != nullptr);
}

void stop() {}
