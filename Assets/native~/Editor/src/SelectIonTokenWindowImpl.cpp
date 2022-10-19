#include "SelectIonTokenWindowImpl.h"

#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/CesiumForUnity/SelectIonTokenWindow.h>
#include <DotNet/CesiumForUnity/IonTokenSelector.h>

#include <algorithm>

using namespace DotNet;

namespace CesiumForUnityNative {

namespace {

SelectIonTokenWindowImpl& currentWindow() {
  return CesiumForUnity::SelectIonTokenWindow::currentWindow()
      .NativeImplementation();
}

} // namespace

CesiumAsync::SharedFuture<std::optional<CesiumIonClient::Token>>
SelectIonTokenWindowImpl::SelectNewToken() {
  CesiumForUnity::SelectIonTokenWindow::ShowWindow();

  return *currentWindow()._future;
}

CesiumAsync::Future<std::optional<CesiumIonClient::Token>>
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

CesiumAsync::Future<std::optional<CesiumIonClient::Token>>
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
                      /* UE_LOG(
                          LogCesiumEditor,
                          Warning,
                          TEXT("Authorizing the project's default Cesium ion "
                               "token to access the following asset IDs: %s"),
                          UTF8_TO_TCHAR(joinToString(idStrings, ",
                         ").c_str()));*/

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
      _future(this->_promise->getFuture().share()) {
  CesiumIonSessionImpl::ion().refreshTokens();
}

SelectIonTokenWindowImpl::~SelectIonTokenWindowImpl() {}

void SelectIonTokenWindowImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::SelectIonTokenWindow& window) {
  if (_promise) {
    this->_promise->resolve(std::nullopt);
  }
}

void SelectIonTokenWindowImpl::RefreshTokens(
    const DotNet::CesiumForUnity::SelectIonTokenWindow& window) {
  const std::vector<CesiumIonClient::Token>& tokens =
      CesiumIonSessionImpl::ion().getTokens();
}

void SelectIonTokenWindowImpl::CreateToken(
    const DotNet::CesiumForUnity::SelectIonTokenWindow& window,
    DotNet::System::String name) {
  if (!this->_promise) {
    return;
  }
  CesiumAsync::Promise<std::optional<CesiumIonClient::Token>> promise =
      std::move(*this->_promise);
  this->_promise.reset();

  const std::string& nameStr = name.ToStlString();

  auto getToken = [this, nameStr]() {
    const CesiumAsync::AsyncSystem& asyncSystem =
        CesiumIonSessionImpl::ion().getAsyncSystem();

    if (nameStr.empty()) {
      return asyncSystem.createResolvedFuture(
          CesiumIonClient::Response<CesiumIonClient::Token>());
    }

    // Create a new token, initially only with access to asset ID 1
    // (Cesium World Terrain).
    return CesiumIonSessionImpl::ion().getConnection()->createToken(
        nameStr,
        {"assets:read"},
        std::vector<int64_t>{1},
        std::nullopt);
  };
}

void SelectIonTokenWindowImpl::UseExistingToken(
    const DotNet::CesiumForUnity::SelectIonTokenWindow& window,
    int tokenIndex) {
  if (!this->_promise) {
    return;
  }
  CesiumAsync::Promise<std::optional<CesiumIonClient::Token>> promise =
      std::move(*this->_promise);
  this->_promise.reset();

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
}

void SelectIonTokenWindowImpl::SpecifyToken(
    const DotNet::CesiumForUnity::SelectIonTokenWindow& window,
    DotNet::System::String token) {
  if (!this->_promise) {
    return;
  }
  CesiumAsync::Promise<std::optional<CesiumIonClient::Token>> promise =
      std::move(*this->_promise);
  this->_promise.reset();
  const std::string& tokenStr = token.ToStlString();

  auto getToken = [this, tokenStr]() {
    // Check if this is a known token, and use it if so.
    return CesiumIonSessionImpl::ion().findToken(tokenStr).thenInMainThread(
        [this, tokenStr](
            CesiumIonClient::Response<CesiumIonClient::Token>&& response) {
          if (response.value) {
            return std::move(response);
          } else {
            CesiumIonClient::Token t;
            t.token = tokenStr;
            return CesiumIonClient::Response(std::move(t), 200, "", "");
          }
        });
  };
}

} // namespace CesiumForUnityNative
