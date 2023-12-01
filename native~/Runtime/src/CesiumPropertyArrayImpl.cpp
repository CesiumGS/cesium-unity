#include "CesiumPropertyArrayImpl.h"

#include <DotNet/CesiumForUnity/CesiumMetadataValue.h>
#include <DotNet/System/Object.h>

namespace CesiumForUnityNative {
DotNet::CesiumForUnity::CesiumMetadataValue CesiumPropertyArrayImpl::GetValue(
    const DotNet::CesiumForUnity::CesiumPropertyArray& array,
    std::int64_t index) {
  return std::visit(
      [index](const auto& v) -> DotNet::CesiumForUnity::CesiumMetadataValue {
        if (index < 0 || index >= v.size()) {
          // TODO: warn?
          return DotNet::CesiumForUnity::CesiumMetadataValue();
        }

        // TODO: convert C++ value to closest C# representation
        //DotNet::System::Object object(v[index]);
        return DotNet::CesiumForUnity::CesiumMetadataValue();
      },
      this->_value);
}
} // namespace CesiumForUnityNative
