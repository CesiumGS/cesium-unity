#include "IonTokenTroubleshootingWindowImpl.h"

#include <CesiumIonClient/Connection.h>

#include <DotNet/CesiumForUnity/AssetTroubleshootingDetails.h>
#include <DotNet/CesiumForUnity/CesiumIonAsset.h>
#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/CesiumForUnity/CesiumRuntimeSettings.h>
#include <DotNet/CesiumForUnity/IonTokenTroubleshootingWindow.h>
#include <DotNet/CesiumForUnity/TokenTroubleshootingDetails.h>
#include <DotNet/System/String.h>

#include <memory>

using namespace DotNet;

namespace CesiumForUnityNative {

IonTokenTroubleshootingWindowImpl::IonTokenTroubleshootingWindowImpl(
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window)
    : _assetTokenState{window.ionAsset().ionAccessToken().ToStlString(), false, false, false},
      _defaultTokenState{
          CesiumForUnity::CesiumRuntimeSettings::defaultIonAccessToken()
              .ToStlString(),
          false,
          false,
          false},
      _assetExistsInUserAccount(false) {}

IonTokenTroubleshootingWindowImpl::~IonTokenTroubleshootingWindowImpl() {}

void IonTokenTroubleshootingWindowImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window) {}

namespace {

void getTokenTroubleShootingDetails(
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window,
    std::string token,
  bool isAssetToken) {

  CesiumIonSessionImpl ionSession = CesiumIonSessionImpl::ion();

  auto pConnection = std::make_shared<CesiumIonClient::Connection>(
      ionSession.getAsyncSystem(),
      ionSession.getAssetAccessor(),
      token);
  /*
  pConnection->me()
      .thenInMainThread(
          [pPanel, pConnection, assetID, &state](Response<Profile>&& profile) {
            state.isValid = profile.value.has_value();
            if (pPanel->IsVisible()) {
              return pConnection->asset(assetID);
            } else {
              return pConnection->getAsyncSystem().createResolvedFuture(
                  Response<Asset>{});
            }
          })
      .thenInMainThread([pPanel, pConnection, &state](Response<Asset>&& asset) {
        state.allowsAccessToAsset = asset.value.has_value();

        if (pPanel->IsVisible()) {
          // Query the tokens using the user's connection (_not_ the token
          // connection created above).
          CesiumIonSession& ionSession = FCesiumEditorModule::ion();
          ionSession.resume();

          const std::optional<Connection>& userConnection =
              ionSession.getConnection();
          if (!userConnection) {
            Response<TokenList> result{};
            return ionSession.getAsyncSystem().createResolvedFuture(
                std::move(result));
          }
          return userConnection->tokens();
        } else {
          return pConnection->getAsyncSystem().createResolvedFuture(
              Response<TokenList>{});
        }
      })
      .thenInMainThread(
          [pPanel, pConnection, &state](Response<TokenList>&& tokens) {
            state.associatedWithUserAccount = false;
            if (tokens.value.has_value()) {
              auto it = std::find_if(
                  tokens.value->items.begin(),
                  tokens.value->items.end(),
                  [&pConnection](const Token& token) {
                    return token.token == pConnection->getAccessToken();
                  });
              state.associatedWithUserAccount = it != tokens.value->items.end();
            }
          });*/
}
} // namespace

void IonTokenTroubleshootingWindowImpl::GetTroubleshootingDetails(
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window) {
  System::String assetToken = window.ionAsset().ionAccessToken();
  if (!System::String::IsNullOrEmpty(assetToken)) {
    getTokenTroubleShootingDetails(window, assetToken.ToStlString(), true);
  }

  System::String defaultToken =
      CesiumForUnity::CesiumRuntimeSettings::defaultIonAccessToken();
  getTokenTroubleShootingDetails(window, defaultToken.ToStlString(), false);
}

} // namespace CesiumForUnityNative
