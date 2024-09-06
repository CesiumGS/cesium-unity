#pragma once

#include <CesiumUtility/CreditSystem.h>
#include <CesiumUtility/ReferenceCounted.h>

#include <DotNet/CesiumForUnity/CesiumCredit.h>
#include <DotNet/System/Collections/Generic/List1.h>

#include <memory>
#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumCreditComponent;
class CesiumCreditSystem;
class GameObject;
} // namespace DotNet::CesiumForUnity

namespace CesiumUtility {
class CreditSystem;
struct Credit;
} // namespace CesiumUtility

namespace CesiumForUnityNative {

class CesiumCreditSystemImpl
    : public CesiumUtility::ReferenceCountedThreadSafe<CesiumCreditSystemImpl> {
public:
  CesiumCreditSystemImpl(
      const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);
  ~CesiumCreditSystemImpl();

  void UpdateCredits(
      const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem,
      bool forceUpdate);

  const std::shared_ptr<CesiumUtility::CreditSystem>&
  getNativeCreditSystem() const;

private:
  // The underlying cesium-native credit system.
  std::shared_ptr<CesiumUtility::CreditSystem> _pCreditSystem;

  const DotNet::CesiumForUnity::CesiumCredit convertHtmlToUnityCredit(
      const std::string& html,
      const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);

  std::unordered_map<std::string, DotNet::CesiumForUnity::CesiumCredit>
      _htmlToUnityCredit;

  size_t _lastCreditsCount;
  bool _creditsUpdated;
};

} // namespace CesiumForUnityNative
