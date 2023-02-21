#include "IonTokenTroubleshootingWindowImpl.h"

#include "CesiumIonSessionImpl.h"
#include "SelectIonTokenWindowImpl.h"

#include <DotNet/CesiumForUnity/AssetTroubleshootingDetails.h>
#include <DotNet/CesiumForUnity/CesiumIonAsset.h>
#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/CesiumForUnity/CesiumRuntimeSettings.h>
#include <DotNet/CesiumForUnity/IonTokenTroubleshootingWindow.h>
#include <DotNet/CesiumForUnity/TokenTroubleshootingDetails.h>
#include <DotNet/System/Object.h>
#include <DotNet/UnityEngine/Debug.h>

#include <memory>
#include <optional>

using namespace DotNet;

namespace CesiumForUnityNative {

namespace {

void getTokenTroubleShootingDetails(
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window,
    std::string token,
    CesiumForUnity::TokenTroubleshootingDetails details) {
  CesiumIonSessionImpl ionSession = CesiumIonSessionImpl::ion();

  auto pConnection = std::make_shared<CesiumIonClient::Connection>(
      ionSession.getAsyncSystem(),
      ionSession.getAssetAccessor(),
      token);

  CesiumForUnity::CesiumIonAsset ionAsset = window.ionAsset();

  pConnection->me()
      .thenInMainThread(
          [pConnection, ionAsset, details](
              CesiumIonClient::Response<CesiumIonClient::Profile>&& profile) {
            // If the asset was destroyed before this was reached, don't attempt
            // to write values to the details.
            if (ionAsset.IsNull()) {
              return pConnection->getAsyncSystem().createResolvedFuture(
                  CesiumIonClient::Response<CesiumIonClient::Asset>{});
            }
            details.isValid(profile.value.has_value());
            return pConnection->asset(ionAsset.ionAssetID());
          })
      .thenInMainThread(
          [pConnection, ionAsset, details](
              CesiumIonClient::Response<CesiumIonClient::Asset>&& asset) {
            if (ionAsset.IsNull()) {
              return pConnection->getAsyncSystem().createResolvedFuture(
                  CesiumIonClient::Response<CesiumIonClient::TokenList>{});
            }

            details.allowsAccessToAsset(asset.value.has_value());

            // Query the tokens using the user's connection (_not_ the token
            // connection created above).
            CesiumIonSessionImpl& ionSession = CesiumIonSessionImpl::ion();
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
      .thenInMainThread(
          [pConnection, ionAsset, details](
              CesiumIonClient::Response<CesiumIonClient::TokenList>&& tokens) {
            if (ionAsset.IsNull()) {
              return;
            }
            if (tokens.value.has_value()) {
              auto it = std::find_if(
                  tokens.value->items.begin(),
                  tokens.value->items.end(),
                  [&pConnection](const CesiumIonClient::Token& token) {
                    return token.token == pConnection->getAccessToken();
                  });
              details.associatedWithUserAccount(
                  it != tokens.value->items.end());
            }

            details.loaded(true);
          });
}

void getAssetTroubleshootingDetails(
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window,
    long assetID,
    CesiumForUnity::AssetTroubleshootingDetails details) {
  CesiumIonSessionImpl::ion().getConnection()->asset(assetID).thenInMainThread(
      [ionAsset = window.ionAsset(),
       details](CesiumIonClient::Response<CesiumIonClient::Asset>&& asset) {
        if (ionAsset.IsNull()) {
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

  if (CesiumForUnity::CesiumIonSession::Ion().IsConnected()) {
    getAssetTroubleshootingDetails(
        window,
        window.ionAsset().ionAssetID(),
        window.assetDetails());
  }
}

void IonTokenTroubleshootingWindowImpl::AuthorizeToken(
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window,
    DotNet::System::String token,
    bool isDefaultToken) {
  CesiumForUnity::CesiumIonSession session =
      CesiumForUnity::CesiumIonSession::Ion();
  CesiumIonSessionImpl& sessionImpl = session.NativeImplementation();
  const std::optional<CesiumIonClient::Connection>& maybeConnection =
      sessionImpl.getConnection();
  if (!session.IsConnected() || !maybeConnection) {
    UnityEngine::Debug::LogError(System::String(
        "Cannot grant a token access to an asset because you are not signed in "
        "to Cesium ion."));
    return;
  }

  sessionImpl.findToken(token.ToStlString())
      .thenInMainThread(
          [connection = *maybeConnection,
           ionAsset = window.ionAsset(),
           isDefaultToken](
              CesiumIonClient::Response<CesiumIonClient::Token>&& response) {
            if (ionAsset.IsNull()) {
              return connection.getAsyncSystem().createResolvedFuture();
            }

            if (!response.value) {
              UnityEngine::Debug::LogError(System::String(
                  "Cannot grant a token access to an asset because the token "
                  "was not found in the signed-in Cesium ion "
                  "account."));
              return connection.getAsyncSystem().createResolvedFuture();
            }

            if (!response.value->assetIds) {
              UnityEngine::Debug::LogWarning(System::String(
                  "Cannot grant a token access to an asset because the token "
                  "appears to already have access to all assets."));
              return connection.getAsyncSystem().createResolvedFuture();
            }

            int64_t ionAssetID = ionAsset.ionAssetID();

            auto it = std::find(
                response.value->assetIds->begin(),
                response.value->assetIds->end(),
                ionAssetID);
            if (it != response.value->assetIds->end()) {
              UnityEngine::Debug::LogWarning(System::String(
                  "Cannot grant a token access to an asset because the token "
                  "appears to already have access to the asset."));
              return connection.getAsyncSystem().createResolvedFuture();
            }

            response.value->assetIds->emplace_back(ionAssetID);

            return connection
                .modifyToken(
                    response.value->id,
                    response.value->name,
                    response.value->assetIds,
                    response.value->scopes,
                    response.value->allowedUrls)
                .thenInMainThread([ionAsset, isDefaultToken](
                                      CesiumIonClient::Response<
                                          CesiumIonClient::NoValue>&& result) {
                  if (result.value) {
                    // Refresh the object now that the token is valid
                    // (hopefully).
                    if (!ionAsset.IsNull()) {
                      if (isDefaultToken) {
                        CesiumForUnity::IonTokenTroubleshootingWindow::
                            UseDefaultToken(ionAsset);
                      } else {
                        // Set the token to the same value to force a refresh.
                        ionAsset.ionAccessToken(ionAsset.ionAccessToken());
                      }
                    }
                  } else {
                    UnityEngine::Debug::LogError(System::String(
                        "An error occurred while attempting to modify a token "
                        "to grant it access to an asset. Please visit "
                        "https://cesium.com/ion/tokens to modify the token "
                        "manually."));
                  }
                });
          });
}

void IonTokenTroubleshootingWindowImpl::SelectNewDefaultToken(
    const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window) {
  CesiumForUnity::CesiumIonSession session =
      CesiumForUnity::CesiumIonSession::Ion();
  const std::optional<CesiumIonClient::Connection>& maybeConnection =
      session.NativeImplementation().getConnection();
  if (!session.IsConnected() || !maybeConnection) {
    UnityEngine::Debug::LogError(
        System::String("Cannot create a new project default token "
                       "because you are not signed in to Cesium ion."));
    return;
  }

  SelectIonTokenWindowImpl::SelectNewToken().thenInMainThread(
      [ionAsset = window.ionAsset()](
          const std::optional<CesiumIonClient::Token>& newToken) {
        if (!newToken) {
          return;
        }

        CesiumForUnity::IonTokenTroubleshootingWindow::UseDefaultToken(
            ionAsset);
      });
}

} // namespace CesiumForUnityNative
