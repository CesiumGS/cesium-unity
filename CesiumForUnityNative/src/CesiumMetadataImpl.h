#pragma once

#include <CesiumGltf/AccessorView.h>
#include <CesiumGltf/ExtensionMeshPrimitiveExtFeatureMetadata.h>
#include <CesiumGltf/ExtensionModelExtFeatureMetadata.h>
#include <CesiumGltf/MetadataPropertyView.h>
#include <CesiumGltf/Model.h>
#include <DotNet/CesiumForUnity/MetadataValue.h>
#include <DotNet/System/String.h>

#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumMetadata;
}

namespace DotNet::UnityEngine {
class Transform;
}

namespace CesiumForUnityNative {

using PropertyType = std::variant<
    CesiumGltf::MetadataPropertyView<int8_t>,
    CesiumGltf::MetadataPropertyView<uint8_t>,
    CesiumGltf::MetadataPropertyView<int16_t>,
    CesiumGltf::MetadataPropertyView<uint16_t>,
    CesiumGltf::MetadataPropertyView<int32_t>,
    CesiumGltf::MetadataPropertyView<uint32_t>,
    CesiumGltf::MetadataPropertyView<int64_t>,
    CesiumGltf::MetadataPropertyView<uint64_t>,
    CesiumGltf::MetadataPropertyView<float>,
    CesiumGltf::MetadataPropertyView<double>,
    CesiumGltf::MetadataPropertyView<bool>,
    CesiumGltf::MetadataPropertyView<std::string_view>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<int8_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<uint8_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<int16_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<uint16_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<int32_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<uint32_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<int64_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<uint64_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<float>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<double>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<bool>>,
    CesiumGltf::MetadataPropertyView<
        CesiumGltf::MetadataArrayView<std::string_view>>>;

using AccessorType = std::variant<
    CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<int8_t>>,
    CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<uint8_t>>,
    CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<int16_t>>,
    CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<uint16_t>>,
    CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<uint32_t>>,
    CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<float>>>;

class CesiumMetadataImpl {
public:
  ~CesiumMetadataImpl(){};
  CesiumMetadataImpl(const DotNet::CesiumForUnity::CesiumMetadata& metadata){};
  void JustBeforeDelete(const DotNet::CesiumForUnity::CesiumMetadata& metadata){};
  void loadMetadata(
      const CesiumGltf::Model& model,
      const CesiumGltf::ExtensionModelExtFeatureMetadata& modelMetadata);

  void loadMetadata(
      const DotNet::CesiumForUnity::CesiumMetadata& metadata,
      const DotNet::UnityEngine::Transform& transform,
      int triangleIndex);

  int getNumberOfProperties(
      const DotNet::CesiumForUnity::CesiumMetadata& metadata) {
    return _currentMetadataValues.size();
  }

  void getValue(const DotNet::CesiumForUnity::CesiumMetadata& metadata, DotNet::CesiumForUnity::MetadataValue& value, int index){
    if(index < _currentMetadataValues.size()){
      value.NativeImplementation().SetValue(_currentMetadataValues[index].second);
    }
  }

  DotNet::System::String
  getKey(const DotNet::CesiumForUnity::CesiumMetadata& metadata, int index) {
    if (index < _currentMetadataValues.size()) {
      return DotNet::System::String(
          _currentMetadataValues[index].first.c_str());
    }
    return DotNet::System::String("");
  }

private:
  void loadMetadata();
  const CesiumGltf::Model* _pModel = nullptr;
  const CesiumGltf::ExtensionModelExtFeatureMetadata* _pModelMetadata = nullptr;

  /**
   * @brief Feature tables are a map of property names to properties 
   */
  using FeatureTable = std::unordered_map<std::string, PropertyType>;
  /**
   * @brief Map of feature table name to feature tables
   */
  std::unordered_map<std::string, FeatureTable>
      _featureTables;

  /**
   * @brief FeatureIDAttributes is a pair of feature table name, and the
   * corresponding accessor view of the primitive's feature IDs.
   */
  using FeatureIDAttribute = std::pair<std::string, AccessorType>;

  /**
   * @brief Vector containing one element per primitive containing the
   * primitive's indices accessor view, and collection of Feature ID Attributes.
   */
  std::vector<std::pair<
      AccessorType,
      std::vector<FeatureIDAttribute>>>
      _featureIDs;

  /**
   * @brief Map of property names to property values of currently loaded metadata. 
   */
  std::vector<std::pair<std::string, ValueType>> _currentMetadataValues;
};
} // namespace CesiumForUnityNative
