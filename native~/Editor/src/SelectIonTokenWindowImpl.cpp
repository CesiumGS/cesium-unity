#include "SelectIonTokenWindowImpl.h"

#include "CesiumIonSessionImpl.h"
#include "UnityExternals.h"

#include <CesiumIonClient/Response.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumDataSource.h>
#include <DotNet/CesiumForUnity/CesiumIonRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumIonServer.h>
#include <DotNet/CesiumForUnity/CesiumIonServerManager.h>
#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/CesiumForUnity/SelectIonTokenWindow.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Collections/Generic/List1.h>
#include <DotNet/System/Object.h>
#include <DotNet/UnityEditor/EditorUtility.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Object.h>

#include <algorithm>

using namespace DotNet;

namespace CesiumForUnityNative {

namespace {

CesiumForUnity::CesiumIonServer
getServer(const DotNet::CesiumForUnity::SelectIonTokenWindow& window) {
  return window.server();
}

CesiumForUnity::CesiumIonSession
getSession(const DotNet::CesiumForUnity::SelectIonTokenWindow& window) {
  return CesiumForUnity::CesiumIonServerManager::instance().GetSession(
      getServer(window));
}

SelectIonTokenWindowImpl& currentWindow() {
  return CesiumForUnity::SelectIonTokenWindow::currentWindow()
      .NativeImplementation();
}

} // namespace

/*static*/ CesiumAsync::SharedFuture<std::optional<CesiumIonClient::Token>>
SelectIonTokenWindowImpl::SelectNewToken(
    const DotNet::CesiumForUnity::CesiumIonServer& server) {
  CesiumForUnity::CesiumIonSession session =
      CesiumForUnity::CesiumIonServerManager::instance().GetSession(server);
  // If the current server doesn't require tokens, don't bother opening the
  // window to create one.
  if (!session.NativeImplementation().IsAuthenticationRequired(session)) {
    return session.NativeImplementation()
        .getAsyncSystem()
        .createResolvedFuture<std::optional<CesiumIonClient::Token>>(
            std::make_optional(CesiumIonClient::Token()))
        .share();
  }

  CesiumForUnity::SelectIonTokenWindow::ShowWindow(server);
  SelectIonTokenWindowImpl& window = currentWindow();

  return *window._future;
}

/*static*/ CesiumAsync::Future<std::optional<CesiumIonClient::Token>>
SelectIonTokenWindowImpl::SelectTokenIfNecessary(
    const DotNet::CesiumForUnity::CesiumIonServer& server) {
  CesiumForUnity::CesiumIonSession session =
      CesiumForUnity::CesiumIonServerManager::instance().GetSession(server);

  return session.NativeImplementation()
      .getProjectDefaultTokenDetails(session)
      .thenInMainThread([server, session](const CesiumIonClient::Token& token) {
        if (token.token.empty() &&
            session.NativeImplementation().IsAuthenticationRequired(session)) {
          return SelectNewToken(server).thenImmediately(
              [](const std::optional<CesiumIonClient::Token>& maybeToken) {
                return maybeToken;
              });
        } else {
          return getAsyncSystem().createResolvedFuture(
              std::make_optional(token));
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
    const DotNet::CesiumForUnity::CesiumIonServer& server,
    const std::vector<int64_t>& assetIDs) {
  CesiumForUnity::CesiumIonSession session =
      CesiumForUnity::CesiumIonServerManager::instance().GetSession(server);
  // If the current server doesn't require tokens, don't try to create or
  // authorize one.
  if (!session.NativeImplementation().IsAuthenticationRequired(session)) {
    return session.NativeImplementation()
        .getAsyncSystem()
        .createResolvedFuture<std::optional<CesiumIonClient::Token>>(
            std::make_optional(CesiumIonClient::Token()));
  }

  return SelectTokenIfNecessary(server).thenInMainThread(
      [assetIDs,
       session](const std::optional<CesiumIonClient::Token>& maybeToken) {
        const std::optional<CesiumIonClient::Connection>& maybeConnection =
            session.NativeImplementation().getConnection();
        if (maybeConnection && maybeToken && !maybeToken->id.empty() &&
            maybeToken->assetIds) {
          std::vector<int64_t> missingAssets =
              findUnauthorizedAssets(*maybeToken->assetIds, assetIDs);
          if (!missingAssets.empty()) {
            // Refresh the token details. We don't want to update the token
            // based on stale information.
            return maybeConnection->token(maybeToken->id)
                .thenInMainThread([maybeToken, assetIDs, session](
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
                          "Authorizing the project's default Cesium ion "
                          "token "
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

                      return session.NativeImplementation()
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

                  return getAsyncSystem().createResolvedFuture(
                      std::optional<CesiumIonClient::Token>(maybeToken));
                });
          }
        }

        return getAsyncSystem().createResolvedFuture(
            std::optional<CesiumIonClient::Token>(maybeToken));
      });
}

SelectIonTokenWindowImpl::SelectIonTokenWindowImpl(
    const DotNet::CesiumForUnity::SelectIonTokenWindow& window)
    : _promise(getAsyncSystem()
                   .createPromise<std::optional<CesiumIonClient::Token>>()),
      _future(this->_promise->getFuture().share()) {}

SelectIonTokenWindowImpl::~SelectIonTokenWindowImpl() {
  if (_promise) {
    this->_promise->resolve(std::nullopt);
  }
}

void SelectIonTokenWindowImpl::RefreshTokens(
    const DotNet::CesiumForUnity::SelectIonTokenWindow& window) {
  CesiumForUnity::CesiumIonSession session = getSession(window);
  const std::vector<CesiumIonClient::Token>& tokens =
      session.NativeImplementation().getTokens(session);
  this->_tokens.resize(tokens.size());

  System::Collections::Generic::List1<System::String> tokenNames =
      window.GetExistingTokenList();

  CesiumForUnity::CesiumIonServer server = getServer(window);

  const std::string createName =
      CesiumForUnity::SelectIonTokenWindow::GetDefaultNewTokenName()
          .ToStlString();
  const std::string defaultTokenId =
      server.defaultIonAccessTokenId().ToStlString();
  const std::string specifiedToken = window.specifiedToken().ToStlString();

  std::optional<size_t> selectedTokenById;
  std::optional<size_t> selectedTokenByValue;
  std::optional<size_t> selectedTokenByCreateName;

  for (size_t i = 0; i < tokens.size(); ++i) {
    if (this->_tokens[i]) {
      *this->_tokens[i] = tokens[i];
    } else {
      this->_tokens[i] = std::make_shared<CesiumIonClient::Token>(tokens[i]);
    }

    if (this->_tokens[i]->id == defaultTokenId) {
      selectedTokenById = i;
    }

    if (this->_tokens[i]->token == specifiedToken) {
      selectedTokenByValue = i;
    }

    if (this->_tokens[i]->name == createName) {
      selectedTokenByCreateName = i;
    }

    tokenNames.Add(System::String(this->_tokens[i]->name));
  }

  // If there is already a token selected, the panel should reflect that
  // accurately. But if there is no token selected, the panel should reflect a
  // _recommendation_: either create a new one or select the existing one with
  // the name we would create.
  if (!specifiedToken.empty()) {
    if (selectedTokenByValue) {
      window.selectedExistingTokenIndex(*selectedTokenByValue);
      window.tokenSource(CesiumForUnity::IonTokenSource::UseExisting);
    } else if (selectedTokenById) {
      window.selectedExistingTokenIndex(*selectedTokenById);
      window.tokenSource(CesiumForUnity::IonTokenSource::UseExisting);
    } else {
      // The specified token does not exist in the user's account.
      window.tokenSource(CesiumForUnity::IonTokenSource::Specify);

      // If the user's account does have a token that matches the default name
      // we would use if we created one for this project, then select that token
      // in the "Use Existing" dropdown, for convenience.
      if (selectedTokenByCreateName) {
        window.selectedExistingTokenIndex(*selectedTokenByCreateName);
      }
    }
  } else {
    if (selectedTokenByCreateName) {
      // A token already exists that has the name we recommend for a new token.
      // So recommend selecting that existing one.
      window.selectedExistingTokenIndex(*selectedTokenByCreateName);
      window.tokenSource(CesiumForUnity::IonTokenSource::UseExisting);
    } else {
      window.tokenSource(CesiumForUnity::IonTokenSource::Create);
    }
  }

  window.RefreshExistingTokenList();
}

namespace {
void updateDefaultToken(
    const CesiumForUnity::CesiumIonServer& server,
    CesiumIonClient::Response<CesiumIonClient::Token> response) {
  if (response.value) {
    CesiumForUnity::CesiumIonSession session =
        CesiumForUnity::CesiumIonServerManager::instance().GetSession(server);
    session.NativeImplementation().invalidateProjectDefaultTokenDetails();

    server.defaultIonAccessToken(System::String(response.value->token));
    server.defaultIonAccessTokenId(System::String(response.value->id));
    UnityEditor::EditorUtility::SetDirty(server);

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

  auto getToken = [this, name, session = getSession(window)]() {
    if (name.empty()) {
      return getAsyncSystem().createResolvedFuture(
          CesiumIonClient::Response<CesiumIonClient::Token>());
    }

    // Create a new token, initially only with access to asset ID 1
    // (Cesium World Terrain).
    return session.NativeImplementation().getConnection()->createToken(
        name,
        {"assets:read"},
        std::vector<int64_t>{1},
        std::nullopt);
  };

  getToken().thenInMainThread(
      [window, promise = std::move(promise), server = getServer(window)](
          CesiumIonClient::Response<CesiumIonClient::Token>&& response) {
        updateDefaultToken(server, response);
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
    return getAsyncSystem().createResolvedFuture(
        CesiumIonClient::Response<CesiumIonClient::Token>(
            CesiumIonClient::Token(token),
            200,
            "",
            ""));
  };

  getToken().thenInMainThread(
      [window, promise = std::move(promise), server = getServer(window)](
          CesiumIonClient::Response<CesiumIonClient::Token>&& response) {
        updateDefaultToken(server, response);
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

  auto getToken = [this, token, session = getSession(window)]() {
    // Check if this is a known token, and use it if so.
    return session.NativeImplementation().findToken(token).thenInMainThread(
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
      [window, promise = std::move(promise), server = getServer(window)](
          CesiumIonClient::Response<CesiumIonClient::Token>&& response) {
        updateDefaultToken(server, response);
        promise.resolve(std::move(response.value));
        window.Close();
      });
}

} // namespace CesiumForUnityNative
