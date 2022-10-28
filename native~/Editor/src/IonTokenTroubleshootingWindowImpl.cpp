#include "IonTokenTroubleshootingWindowImpl.h"

#include "CesiumIonSessionImpl.h"

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
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window) {}

IonTokenTroubleshootingWindowImpl::~IonTokenTroubleshootingWindowImpl() {}

void IonTokenTroubleshootingWindowImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window) {}

namespace {

void getTokenTroubleShootingDetails(
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window,
    std::string token,
    CesiumForUnity::TokenTroubleshootingDetails details) {
  CesiumForUnity::CesiumIonAsset ionAsset = window.ionAsset();
  CesiumIonSessionImpl ionSession = CesiumIonSessionImpl::ion();

  auto pConnection = std::make_shared<CesiumIonClient::Connection>(
      ionSession.getAsyncSystem(),
      ionSession.getAssetAccessor(),
      token);

  pConnection->me()
      .thenInMainThread([pConnection, ionAsset, details](
                            CesiumIonClient::Response<
                                CesiumIonClient::Profile>&& profile) {
        // If the window was closed before this was reached, don't attempt to
        // write values to the details.
        if (!CesiumForUnity::IonTokenTroubleshootingWindow::HasExistingWindow(
                ionAsset)) {
          return pConnection->getAsyncSystem().createResolvedFuture(
              CesiumIonClient::Response<CesiumIonClient::Asset>{});
        }
        details.isValid(profile.value.has_value());
        return pConnection->asset(ionAsset.ionAssetID());
      })
      .thenInMainThread([pConnection, ionAsset, details](
                            CesiumIonClient::Response<CesiumIonClient::Asset>&&
                                asset) {
        if (!CesiumForUnity::IonTokenTroubleshootingWindow::HasExistingWindow(
                ionAsset)) {
          return pConnection->getAsyncSystem().createResolvedFuture(
              CesiumIonClient::Response<CesiumIonClient::TokenList>{});
        }

        details.allowsAccessToAsset(asset.value.has_value());

        // Query the tokens using the user's connection (_not_ the token
        // connection created above).
        CesiumIonSessionImpl ionSession = CesiumIonSessionImpl::ion();
        ionSession.Resume(CesiumForUnity::CesiumIonSession::Ion());

        const std::optional<CesiumIonClient::Connection>& userConnection =
            ionSession.getConnection();
        if (!userConnection) {
          CesiumIonClient::Response<CesiumIonClient::TokenList> result{};
          return ionSession.getAsyncSystem().createResolvedFuture(
              std::move(result));
        }

        return userConnection->tokens();
      })
      .thenInMainThread([pConnection, ionAsset, details](
                            CesiumIonClient::Response<
                                CesiumIonClient::TokenList>&& tokens) {
        if (!CesiumForUnity::IonTokenTroubleshootingWindow::HasExistingWindow(
                ionAsset)) {
          return;
        }
        if (tokens.value.has_value()) {
          auto it = std::find_if(
              tokens.value->items.begin(),
              tokens.value->items.end(),
              [&pConnection](const CesiumIonClient::Token& token) {
                return token.token == pConnection->getAccessToken();
              });
          details.associatedWithUserAccount(it != tokens.value->items.end());
        }

        details.loaded(true);
      });
}

void getAssetTroubleshootingDetails(
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window,
    long assetID,
    CesiumForUnity::AssetTroubleshootingDetails details) {
  CesiumForUnity::CesiumIonAsset ionAsset = window.ionAsset();
  CesiumIonSessionImpl::ion().getConnection()->asset(assetID).thenInMainThread(
      [window, ionAsset, details](
          CesiumIonClient::Response<CesiumIonClient::Asset>&& asset) {
        if (!CesiumForUnity::IonTokenTroubleshootingWindow::HasExistingWindow(
                ionAsset)) {
          return;
        }
        details.assetExistsInUserAccount(asset.value.has_value());
        details.loaded(true);
      });
}
} // namespace

void IonTokenTroubleshootingWindowImpl::GetTroubleshootingDetails(
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window) {
  System::String assetToken = window.ionAsset().ionAccessToken();
  if (!System::String::IsNullOrEmpty(assetToken)) {
    getTokenTroubleShootingDetails(
        window,
        assetToken.ToStlString(),
        window.assetTokenDetails());
  }

  System::String defaultToken =
      CesiumForUnity::CesiumRuntimeSettings::defaultIonAccessToken();
  getTokenTroubleShootingDetails(
      window,
      defaultToken.ToStlString(),
      window.defaultTokenDetails());

  if (CesiumIonSessionImpl::ion().IsConnected(
          CesiumForUnity::CesiumIonSession::Ion())) {
    getAssetTroubleshootingDetails(
        window,
        window.ionAsset().ionAssetID(),
        window.assetDetails());
  }
}

} // namespace CesiumForUnityNative
