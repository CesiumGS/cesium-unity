#pragma once

#include <Cesium3DTilesSelection/CreditSystem.h>

#include <DotNet/CesiumForUnity/CesiumCreditSystem.h>
#include <DotNet/System/Collections/Generic/List1.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/GameObject.h>

#include <memory>
#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumCreditSystem;
class GameObject;
} // namespace DotNet::CesiumForUnity

namespace Cesium3DTilesSelection {
class CreditSystem;
class Credit;
} // namespace Cesium3DTilesSelection

namespace CesiumForUnityNative {

class CesiumCreditSystemImpl {
public:
  CesiumCreditSystemImpl(
      const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);
  ~CesiumCreditSystemImpl();

  void Update(const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);

  const std::shared_ptr<Cesium3DTilesSelection::CreditSystem>&
  getExternalCreditSystem() const;

  static DotNet::CesiumForUnity::CesiumCreditSystem getDefaultCreditSystem();

private:
  // The underlying cesium-native credit system.
  std::shared_ptr<Cesium3DTilesSelection::CreditSystem> _pCreditSystem;

  const std::string convertHtmlToRtf(
      const std::string& html,
      const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);

  std::unordered_map<std::string, DotNet::System::String> _htmlToRtf;
  DotNet::System::Collections::Generic::List1<DotNet::System::String>
      _popupCreditsList;
  DotNet::System::Collections::Generic::List1<DotNet::System::String>
      _onScreenCreditsList;
  size_t _lastCreditsCount;
};

} // namespace CesiumForUnityNative
