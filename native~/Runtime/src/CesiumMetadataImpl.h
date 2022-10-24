#pragma once

#include <CesiumGltf/AccessorView.h>
#include <CesiumGltf/Model.h>
#include <DotNet/CesiumForUnity/MetadataProperty.h>
#include <DotNet/System/String.h>

#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumMetadata;
}

namespace DotNet::UnityEngine {
class Transform;
}

namespace CesiumForUnityNative {

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
  void loadMetadata(const CesiumGltf::Model* pModel);

  void loadMetadata(
      const DotNet::CesiumForUnity::CesiumMetadata& metadata,
      const DotNet::UnityEngine::Transform& transform,
      int triangleIndex);

  int getNumberOfProperties(
      const DotNet::CesiumForUnity::CesiumMetadata& metadata) {
    return _currentMetadataValues.size();
  }

  void getProperty(const DotNet::CesiumForUnity::CesiumMetadata& metadata, const DotNet::CesiumForUnity::MetadataProperty& property, int index) {
    if (index >= 0 && index < _currentMetadataValues.size()) {
      const MetadataProperty& propertyInfo = _currentMetadataValues[index];
      property.NativeImplementation().SetProperty(propertyInfo.propertyName, propertyInfo.propertyView, propertyInfo.propertyValue);
    }
  }

private:
  void loadMetadata();
  const CesiumGltf::Model* _pModel = nullptr;

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

  struct MetadataProperty {
    std::string propertyName;
    PropertyType propertyView;
    ValueType propertyValue;
  };

  std::vector<MetadataProperty> _currentMetadataValues;
};
} // namespace CesiumForUnityNative
