#pragma once

#include "Bindings.h"

namespace CesiumForUnity {

class Cesium3DTileset : public BaseCesium3DTileset {
public:
    CESIUM_FOR_UNITY_CESIUM3DTILESET_DEFAULT_CONTENTS_DECLARATION
    CESIUM_FOR_UNITY_CESIUM3DTILESET_DEFAULT_CONSTRUCTOR_DECLARATION
    void Start() override;
    void Update() override;
};

}
