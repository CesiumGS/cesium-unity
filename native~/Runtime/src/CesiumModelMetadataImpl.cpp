#include "CesiumModelMetadataImpl.h"

#include <CesiumGltf/AccessorView.h>
#include <CesiumGltf/Model.h>
#include <CesiumGltf/ExtensionModelExtStructuralMetadata.h>
#include <CesiumGltf/PropertyTablePropertyView.h>
#include <CesiumGltf/PropertyTableView.h>


#include <DotNet/System/Array1.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Mesh.h>
#include <DotNet/UnityEngine/Transform.h>

using namespace CesiumForUnityNative;
using namespace CesiumGltf;

void CesiumModelMetadataImpl::initializeMetadata(
    const CesiumGltf::Model* pModel,
    const CesiumGltf::ExtensionModelExtStructuralMetadata* pExtension) {
  /*DotNet::System::Array1<DotNet::CesiumForUnity::PropertyTable> features =
      DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature>(
          pMetadata->featureIdAttributes.size());*/
}
