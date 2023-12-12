// clang-format off
//#pragma once
//
// namespace CesiumForUnityNative {
// class CesiumPropertyArrayImpl;
//}
//
//#include "CesiumFeaturesMetadataUtility.h"
//
//#include <CesiumGltf/PropertyArrayView.h>
//
//#include <DotNet/CesiumForUnity/CesiumPropertyArray.h>
//
//#include <any>
//#include <unordered_map>
//
// namespace DotNet::CesiumForUnity {
// class CesiumPropertyArray;
// class CesiumMetadataValue;
//} // namespace DotNet::CesiumForUnity
//
// namespace DotNet::System {
// class String;
//}
//
// namespace CesiumForUnityNative {
//
// class CesiumPropertyArrayImpl {
//#pragma region ArrayType
//  template <typename T>
//  using ArrayPropertyView = CesiumGltf::PropertyArrayView<T>;
//
//  using ArrayType = std::variant<
//      ArrayPropertyView<int8_t>,
//      ArrayPropertyView<uint8_t>,
//      ArrayPropertyView<int16_t>,
//      ArrayPropertyView<uint16_t>,
//      ArrayPropertyView<int32_t>,
//      ArrayPropertyView<uint32_t>,
//      ArrayPropertyView<int64_t>,
//      ArrayPropertyView<uint64_t>,
//      ArrayPropertyView<float>,
//      ArrayPropertyView<double>,
//      ArrayPropertyView<bool>,
//      ArrayPropertyView<std::string_view>,
//      ArrayPropertyView<glm::vec<2, int8_t>>,
//      ArrayPropertyView<glm::vec<2, uint8_t>>,
//      ArrayPropertyView<glm::vec<2, int16_t>>,
//      ArrayPropertyView<glm::vec<2, uint16_t>>,
//      ArrayPropertyView<glm::vec<2, int32_t>>,
//      ArrayPropertyView<glm::vec<2, uint32_t>>,
//      ArrayPropertyView<glm::vec<2, int64_t>>,
//      ArrayPropertyView<glm::vec<2, uint64_t>>,
//      ArrayPropertyView<glm::vec<2, float>>,
//      ArrayPropertyView<glm::vec<2, double>>,
//      ArrayPropertyView<glm::vec<3, int8_t>>,
//      ArrayPropertyView<glm::vec<3, uint8_t>>,
//      ArrayPropertyView<glm::vec<3, int16_t>>,
//      ArrayPropertyView<glm::vec<3, uint16_t>>,
//      ArrayPropertyView<glm::vec<3, int32_t>>,
//      ArrayPropertyView<glm::vec<3, uint32_t>>,
//      ArrayPropertyView<glm::vec<3, int64_t>>,
//      ArrayPropertyView<glm::vec<3, uint64_t>>,
//      ArrayPropertyView<glm::vec<3, float>>,
//      ArrayPropertyView<glm::vec<3, double>>,
//      ArrayPropertyView<glm::vec<4, int8_t>>,
//      ArrayPropertyView<glm::vec<4, uint8_t>>,
//      ArrayPropertyView<glm::vec<4, int16_t>>,
//      ArrayPropertyView<glm::vec<4, uint16_t>>,
//      ArrayPropertyView<glm::vec<4, int32_t>>,
//      ArrayPropertyView<glm::vec<4, uint32_t>>,
//      ArrayPropertyView<glm::vec<4, int64_t>>,
//      ArrayPropertyView<glm::vec<4, uint64_t>>,
//      ArrayPropertyView<glm::vec<4, float>>,
//      ArrayPropertyView<glm::vec<4, double>>,
//      ArrayPropertyView<glm::mat<2, 2, int8_t>>,
//      ArrayPropertyView<glm::mat<2, 2, uint8_t>>,
//      ArrayPropertyView<glm::mat<2, 2, int16_t>>,
//      ArrayPropertyView<glm::mat<2, 2, uint16_t>>,
//      ArrayPropertyView<glm::mat<2, 2, int32_t>>,
//      ArrayPropertyView<glm::mat<2, 2, uint32_t>>,
//      ArrayPropertyView<glm::mat<2, 2, int64_t>>,
//      ArrayPropertyView<glm::mat<2, 2, uint64_t>>,
//      ArrayPropertyView<glm::mat<2, 2, float>>,
//      ArrayPropertyView<glm::mat<2, 2, double>>,
//      ArrayPropertyView<glm::mat<3, 3, int8_t>>,
//      ArrayPropertyView<glm::mat<3, 3, uint8_t>>,
//      ArrayPropertyView<glm::mat<3, 3, int16_t>>,
//      ArrayPropertyView<glm::mat<3, 3, uint16_t>>,
//      ArrayPropertyView<glm::mat<3, 3, int32_t>>,
//      ArrayPropertyView<glm::mat<3, 3, uint32_t>>,
//      ArrayPropertyView<glm::mat<3, 3, int64_t>>,
//      ArrayPropertyView<glm::mat<3, 3, uint64_t>>,
//      ArrayPropertyView<glm::mat<3, 3, float>>,
//      ArrayPropertyView<glm::mat<3, 3, double>>,
//      ArrayPropertyView<glm::mat<4, 4, int8_t>>,
//      ArrayPropertyView<glm::mat<4, 4, uint8_t>>,
//      ArrayPropertyView<glm::mat<4, 4, int16_t>>,
//      ArrayPropertyView<glm::mat<4, 4, uint16_t>>,
//      ArrayPropertyView<glm::mat<4, 4, int32_t>>,
//      ArrayPropertyView<glm::mat<4, 4, uint32_t>>,
//      ArrayPropertyView<glm::mat<4, 4, int64_t>>,
//      ArrayPropertyView<glm::mat<4, 4, uint64_t>>,
//      ArrayPropertyView<glm::mat<4, 4, float>>,
//      ArrayPropertyView<glm::mat<4, 4, double>>>;
//#pragma endregion
//
// public:
//  ~CesiumPropertyArrayImpl(){};
//  CesiumPropertyArrayImpl(
//      const DotNet::CesiumForUnity::CesiumPropertyArray& array){};
//  void
//  JustBeforeDelete(const DotNet::CesiumForUnity::CesiumPropertyArray& array){};
//
//  template <typename T>
//  static DotNet::CesiumForUnity::CesiumPropertyArray
//  CreateArray(const CesiumGltf::PropertyArrayView<T>& arrayView);
// private:
//  ArrayType _value;
//};
//
// template <typename T>
///*static*/ DotNet::CesiumForUnity::CesiumPropertyArray
// CesiumPropertyArrayImpl::CreateArray(
//    const CesiumGltf::PropertyArrayView<T>& arrayView) {
//  DotNet::CesiumForUnity::CesiumPropertyArray array =
//      DotNet::CesiumForUnity::CesiumPropertyArray();
//  array.length(arrayView.size());
//  array.valueType(CesiumFeaturesMetadataUtility::TypeToMetadataValueType<T>());
//
//  CesiumPropertyArrayImpl& arrayImpl = array.NativeImplementation();
//  arrayImpl._value = arrayView;
//
//  return array;
//}
//} // namespace CesiumForUnityNative
// clang-format on
