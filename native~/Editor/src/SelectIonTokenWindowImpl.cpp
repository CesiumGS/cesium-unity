#include "SelectIonTokenWindowImpl.h"

#include "CesiumIonSessionImpl.h"

#include <CesiumIonClient/Response.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumDataSource.h>
#include <DotNet/CesiumForUnity/CesiumIonRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/CesiumForUnity/CesiumRuntimeSettings.h>
#include <DotNet/CesiumForUnity/SelectIonTokenWindow.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Collections/Generic/List1.h>
#include <DotNet/System/Object.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Object.h>

#include <algorithm>

using namespace DotNet;

namespace CesiumForUnityNative {

namespace {

SelectIonTokenWindowImpl& currentWindow() {
  return CesiumForUnity::SelectIonTokenWindow::currentWindow()
      .NativeImplementation();
}

} // namespace

/*static*/ CesiumAsync::SharedFuture<std::optional<CesiumIonClient::Token>>
SelectIonTokenWindowImpl::SelectNewToken() {
  CesiumForUnity::SelectIonTokenWindow::ShowWindow();
  SelectIonTokenWindowImpl& window = currentWindow();

  return *window._future;
}

/*static*/ CesiumAsync::Future<std::optional<CesiumIonClient::Token>>
SelectIonTokenWindowImpl::SelectTokenIfNecessary() {
  return CesiumIonSessionImpl::ion()
      .getProjectDefaultTokenDetails()
      .thenInMainThread([](const CesiumIonClient::Token& token) {
        if (token.token.empty()) {
          return SelectNewToken().thenImmediately(
              [](const std::optional<CesiumIonClient::Token>& maybeToken) {
                return maybeToken;
              });
        } else {
          return CesiumIonSessionImpl::ion()
              .getAsyncSystem()
              .createResolvedFuture(std::make_optional(token));
        }
      });
}

namespace {

std::vector<int64_t> findUnauthorizedAssets(
    const std::vector<int64_t>& authorizedAssets,
    const std::vector<int64_t>& requiredAssets) {
  std::vector<int64_t> missingAssets;

  for (int64_t assetID : requiredAssets) {
    auto it =
        std::find(authorizedAssets.begin(), authorizedAssets.end(), assetID);
    if (it == authorizedAssets.end()) {
      missingAssets.emplace_back(assetID);
    }
  }

  return missingAssets;
}

} // namespace

/*static*/ CesiumAsync::Future<std::optional<CesiumIonClient::Token>>
SelectIonTokenWindowImpl::SelectAndAuthorizeToken(
    const std::vector<int64_t>& assetIDs) {
  return SelectTokenIfNecessary().thenInMainThread(
      [assetIDs](const std::optional<CesiumIonClient::Token>& maybeToken) {
        const std::optional<CesiumIonClient::Connection>& maybeConnection =
            CesiumIonSessionImpl::ion().getConnection();
        if (maybeConnection && maybeToken && !maybeToken->id.empty() &&
            maybeToken->assetIds) {
          std::vector<int64_t> missingAssets =
              findUnauthorizedAssets(*maybeToken->assetIds, assetIDs);
          if (!missingAssets.empty()) {
            // Refresh the token details. We don't want to update the token
            // based on stale information.
            return maybeConnection->token(maybeToken->id)
                .thenInMainThread([maybeToken, assetIDs](
                                      CesiumIonClient::Response<
                                          CesiumIonClient::Token>&& response) {
                  if (response.value) {
                    std::vector<int64_t> missingAssets =
                        findUnauthorizedAssets(*maybeToken->assetIds, assetIDs);
                    if (!missingAssets.empty()) {
                      std::vector<std::string> idStrings(missingAssets.size());
                      std::transform(
                          missingAssets.begin(),
                          missingAssets.end(),
                          idStrings.begin(),
                          [](int64_t id) { return std::to_string(id); });

                      std::string assetIDsString = "";
                      std::for_each(
                          idStrings.begin(),
                          idStrings.end(),
                          [&assetIDsString](std::string& id) {
                            assetIDsString += id;
                          });
                      UnityEngine::Debug::Log(System::String(
                          "Authorizing the project's default Cesium ion token "
                          "to access the following asset IDs: " +
                          assetIDsString));

                      CesiumIonClient::Token newToken = *maybeToken;
                      size_t destinationIndex = newToken.assetIds->size();
                      newToken.assetIds->resize(
                          newToken.assetIds->size() + missingAssets.size());
                      std::copy(
                          missingAssets.begin(),
                          missingAssets.end(),
                          newToken.assetIds->begin() + destinationIndex);

                      return CesiumIonSessionImpl::ion()
                          .getConnection()
                          ->modifyToken(
                              newToken.id,
                              newToken.name,
                              newToken.assetIds,
                              newToken.scopes,
                              newToken.allowedUrls)
                          .thenImmediately(
                              [maybeToken](CesiumIonClient::Response<
                                           CesiumIonClient::NoValue>&&) {
                                return maybeToken;
                              });
                    }
                  }

                  return CesiumIonSessionImpl::ion()
                      .getAsyncSystem()
                      .createResolvedFuture(
                          std::optional<CesiumIonClient::Token>(maybeToken));
                });
          }
        }

        return CesiumIonSessionImpl::ion()
            .getAsyncSystem()
            .createResolvedFuture(
                std::optional<CesiumIonClient::Token>(maybeToken));
      });
}

SelectIonTokenWindowImpl::SelectIonTokenWindowImpl(
    const DotNet::CesiumForUnity::SelectIonTokenWindow& window)
    : _promise(CesiumIonSessionImpl::ion()
                   .getAsyncSystem()
                   .createPromise<std::optional<CesiumIonClient::Token>>()),
      _future(this->_promise->getFuture().share()) {}

SelectIonTokenWindowImpl::~SelectIonTokenWindowImpl() {
  if (_promise) {
    this->_promise->resolve(std::nullopt);
  }
}

void SelectIonTokenWindowImpl::RefreshTokens(
    const DotNet::CesiumForUnity::SelectIonTokenWindow& window) {
  const std::vector<CesiumIonClient::Token>& tokens =
      CesiumIonSessionImpl::ion().getTokens();
  this->_tokens.resize(tokens.size());

  System::Collections::Generic::List1<System::String> tokenNames =
      window.GetExistingTokenList();

  const std::string& createName =
      CesiumForUnity::SelectIonTokenWindow::GetDefaultNewTokenName()
          .ToStlString();
  const std::string& defaultTokenId =
      CesiumForUnity::CesiumRuntimeSettings::defaultIonAccessTokenID()
          .ToStlString();
  const std::string& specifiedToken = window.specifiedToken().ToStlString();

  for (size_t i = 0; i < tokens.size(); ++i) {
    if (this->_tokens[i]) {
      *this->_tokens[i] = std::move(tokens[i]);
    } else {
      this->_tokens[i] =
          std::make_shared<CesiumIonClient::Token>(std::move(tokens[i]));
    }

    if (this->_tokens[i]->id == defaultTokenId) {
      window.selectedExistingTokenIndex(i);
      window.tokenSource(CesiumForUnity::IonTokenSource::UseExisting);
    }

    // If there's already a token with the default name we would use to create a
    // new one, default to selecting that rather than creating a new one.
    if (window.tokenSource() == CesiumForUnity::IonTokenSource::Create &&
        this->_tokens[i]->name == createName) {
      window.selectedExistingTokenIndex(i);
      window.tokenSource(CesiumForUnity::IonTokenSource::UseExisting);
    }

    // If this happens to be the specified token, select it.
    if (window.tokenSource() == CesiumForUnity::IonTokenSource::Specify &&
        this->_tokens[i]->token == specifiedToken) {
      window.selectedExistingTokenIndex(i);
      window.tokenSource(CesiumForUnity::IonTokenSource::UseExisting);
    }

    tokenNames.Add(System::String(this->_tokens[i]->name));
  }

  window.RefreshExistingTokenList();
}

namespace {
void updateDefaultToken(
    CesiumIonClient::Response<CesiumIonClient::Token> response) {
  if (response.value) {
    CesiumIonSessionImpl::ion().invalidateProjectDefaultTokenDetails();

    CesiumForUnity::CesiumRuntimeSettings::defaultIonAccessToken(
        System::String(response.value->token));
    CesiumForUnity::CesiumRuntimeSettings::defaultIonAccessTokenID(
        System::String(response.value->id));

    // TODO: source control

    // Refresh all tilesets and overlays that are using the project
    // default token.
    System::Array1 tilesets = UnityEngine::Object::FindObjectsOfType<
        CesiumForUnity::Cesium3DTileset>();
    for (int32_t i = 0; i < tilesets.Length(); i++) {
      CesiumForUnity::Cesium3DTileset tileset = tilesets[i];
      if (tileset.tilesetSource() ==
              CesiumForUnity::CesiumDataSource::FromCesiumIon &&
          System::String::IsNullOrEmpty(tileset.ionAccessToken())) {
        tileset.RecreateTileset();
      } else {
        CesiumForUnity::CesiumIonRasterOverlay rasterOverlay =
            tileset.gameObject()
                .GetComponent<CesiumForUnity::CesiumIonRasterOverlay>();
        if (rasterOverlay != nullptr &&
            System::String::IsNullOrEmpty(rasterOverlay.ionAccessToken())) {
          rasterOverlay.Refresh();
        }
      }
    }
  } else {
    UnityEngine::Debug::LogError(System::String(
        "An error occurred while selecting a token: " + response.errorMessage));
  }
}
} // namespace

void SelectIonTokenWindowImpl::CreateToken(
    const DotNet::CesiumForUnity::SelectIonTokenWindow& window) {
  if (!this->_promise) {
    return;
  }

  CesiumAsync::Promise<std::optional<CesiumIonClient::Token>> promise =
      std::move(*this->_promise);
  this->_promise.reset();

  const std::string& name = window.createdTokenName().ToStlString();

  auto getToken = [this, name]() {
    const CesiumAsync::AsyncSystem& asyncSystem =
        CesiumIonSessionImpl::ion().getAsyncSystem();

    if (name.empty()) {
      return asyncSystem.createResolvedFuture(
          CesiumIonClient::Response<CesiumIonClient::Token>());
    }

    // Create a new token, initially only with access to asset ID 1
    // (Cesium World Terrain).
    return CesiumIonSessionImpl::ion().getConnection()->createToken(
        name,
        {"assets:read"},
        std::vector<int64_t>{1},
        std::nullopt);
  };

  getToken().thenInMainThread(
      [window, promise = std::move(promise)](
          CesiumIonClient::Response<CesiumIonClient::Token>&& response) {
        updateDefaultToken(response);
        promise.resolve(std::move(response.value));
        window.Close();
      });
}

void SelectIonTokenWindowImpl::UseExistingToken(
    const DotNet::CesiumForUnity::SelectIonTokenWindow& window) {
  if (!this->_promise) {
    return;
  }

  CesiumAsync::Promise<std::optional<CesiumIonClient::Token>> promise =
      std::move(*this->_promise);
  this->_promise.reset();

  int tokenIndex = window.selectedExistingTokenIndex();
  const CesiumIonClient::Token& token = *this->_tokens[tokenIndex];
  auto getToken = [this, token]() {
    const CesiumAsync::AsyncSystem& asyncSystem =
        CesiumIonSessionImpl::ion().getAsyncSystem();

    return asyncSystem.createResolvedFuture(
        CesiumIonClient::Response<CesiumIonClient::Token>(
            CesiumIonClient::Token(token),
            200,
            "",
            ""));
  };

  getToken().thenInMainThread(
      [window, promise = std::move(promise)](
          CesiumIonClient::Response<CesiumIonClient::Token>&& response) {
        updateDefaultToken(response);
        promise.resolve(std::move(response.value));
        window.Close();
      });
}

void SelectIonTokenWindowImpl::SpecifyToken(
    const DotNet::CesiumForUnity::SelectIonTokenWindow& window) {
  if (!this->_promise) {
    return;
  }

  CesiumAsync::Promise<std::optional<CesiumIonClient::Token>> promise =
      std::move(*this->_promise);
  this->_promise.reset();
  const std::string& token = window.specifiedToken().ToStlString();

  auto getToken = [this, token]() {
    // Check if this is a known token, and use it if so.
    return CesiumIonSessionImpl::ion().findToken(token).thenInMainThread(
        [this,
         token](CesiumIonClient::Response<CesiumIonClient::Token>&& response) {
          if (response.value) {
            return std::move(response);
          } else {
            CesiumIonClient::Token t;
            t.token = token;
            return CesiumIonClient::Response(std::move(t), 200, "", "");
          }
        });
  };

  getToken().thenInMainThread(
      [window, promise = std::move(promise)](
          CesiumIonClient::Response<CesiumIonClient::Token>&& response) {
        updateDefaultToken(response);
        promise.resolve(std::move(response.value));
        window.Close();
      });
}

} // namespace CesiumForUnityNative
