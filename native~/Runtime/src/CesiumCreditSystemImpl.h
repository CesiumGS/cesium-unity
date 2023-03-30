#pragma once

#include <Cesium3DTilesSelection/CreditSystem.h>

#include <DotNet/System/Collections/Generic/List1.h>

#include <memory>
#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumCredit;
class CesiumCreditComponent;
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

private:
  // The underlying cesium-native credit system.
  std::shared_ptr<Cesium3DTilesSelection::CreditSystem> _pCreditSystem;

  const DotNet::CesiumForUnity::CesiumCredit convertHtmlToUnityCredit(
      const std::string& html,
      const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);

  std::unordered_map<std::string, DotNet::CesiumForUnity::CesiumCredit>
      _htmlToUnityCredit;

  std::vector<Cesium3DTilesSelection::Credit> _onScreenCredits;
  std::vector<Cesium3DTilesSelection::Credit> _popupCredits;

  bool _creditsUpdated;
};

} // namespace CesiumForUnityNative
