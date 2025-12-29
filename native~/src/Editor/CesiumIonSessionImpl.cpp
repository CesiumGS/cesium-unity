#include "CesiumIonSessionImpl.h"

#include "UnityExternals.h"
#include "UnityTaskProcessor.h"

#include <CesiumAsync/AsyncSystem.h>
#include <CesiumUtility/IntrusivePointer.h>
#include <CesiumUtility/Uri.h>

#include <DotNet/CesiumForUnity/CesiumIonServer.h>
#include <DotNet/CesiumForUnity/CesiumIonServerManager.h>
#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/CesiumForUnity/QuickAddItem.h>
#include <DotNet/CesiumForUnity/QuickAddItemType.h>
#include <DotNet/System/Object.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/Object.h>
#include <fmt/format.h>

#if UNITY_EDITOR
#include <DotNet/UnityEditor/EditorUtility.h>
#endif

using namespace CesiumAsync;
using namespace CesiumIonClient;
using namespace CesiumUtility;
using namespace DotNet;

namespace {

const char* OAUTH2_REDIRECT_URI = "/cesium-for-unity/oauth2/callback";

template <typename T>
void logResponseErrors(const CesiumIonClient::Response<T>& response) {
  if (!response.errorCode.empty() && !response.errorMessage.empty()) {
    UnityEngine::Debug::LogError(System::String(fmt::format(
        "{} (Code {})",
        response.errorMessage,
        response.errorCode)));
  } else if (!response.errorCode.empty()) {
    UnityEngine::Debug::LogError(
        System::String(fmt::format("Code {}", response.errorCode)));
  } else if (!response.errorMessage.empty()) {
    UnityEngine::Debug::LogError(System::String(response.errorMessage));
  }
}

template <typename T>
bool disconnectOnNoValidToken(
    const DotNet::CesiumForUnity::CesiumIonSession& session,
    const Response<T>& response) {
  if (response.errorCode == "NoValidToken") {
    session.NativeImplementation().Disconnect(session);
    return true;
  } else {
    return false;
  }
}

} // namespace

namespace CesiumForUnityNative {

CesiumIonSessionImpl::CesiumIonSessionImpl(
    const DotNet::CesiumForUnity::CesiumIonSession& session)
    : _asyncSystem(CesiumForUnityNative::getAsyncSystem()),
      _pAssetAccessor(CesiumForUnityNative::getAssetAccessor()),
      _connection(std::nullopt),
      _profile(std::nullopt),
      _assets(std::nullopt),
      _tokens(std::nullopt),
      _defaults(std::nullopt),
      _isConnecting(false),
      _isResuming(false),
      _isLoadingProfile(false),
      _isLoadingAssets(false),
      _isLoadingTokens(false),
      _isLoadingDefaults(false),
      _loadProfileQueued(false),
      _loadAssetsQueued(false),
      _loadTokensQueued(false),
      _loadDefaultsQueued(false),
      _quickAddItems() {}

CesiumIonSessionImpl::~CesiumIonSessionImpl() {}

bool CesiumIonSessionImpl::IsBusy(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->IsConnecting(session) || this->IsResuming(session) ||
         this->IsLoadingProfile(session) || this->IsLoadingAssetList(session) ||
         this->IsLoadingTokenList(session) || this->IsLoadingDefaults(session);
}

bool CesiumIonSessionImpl::IsConnected(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->_connection.has_value();
}

bool CesiumIonSessionImpl::IsConnecting(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->_isConnecting;
}

bool CesiumIonSessionImpl::IsResuming(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->_isResuming;
}

bool CesiumIonSessionImpl::IsProfileLoaded(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->_profile.has_value();
}

bool CesiumIonSessionImpl::IsLoadingProfile(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->_isLoadingProfile;
}

bool CesiumIonSessionImpl::IsAssetListLoaded(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->_assets.has_value();
}

bool CesiumIonSessionImpl::IsLoadingAssetList(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->_isLoadingAssets;
}

bool CesiumIonSessionImpl::IsTokenListLoaded(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->_tokens.has_value();
}

bool CesiumIonSessionImpl::IsLoadingTokenList(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->_isLoadingTokens;
}

bool CesiumIonSessionImpl::IsDefaultsLoaded(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->_defaults.has_value();
}

bool CesiumIonSessionImpl::IsLoadingDefaults(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->_isLoadingDefaults;
}

bool CesiumIonSessionImpl::IsAuthenticationRequired(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return this->_appData.has_value() ? this->_appData->needsOauthAuthentication()
                                    : true;
}

void CesiumIonSessionImpl::Connect(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  CesiumForUnity::CesiumIonServer server = session.server();
  if (server == nullptr || this->IsConnecting(session) ||
      this->IsConnected(session) || this->IsResuming(session)) {
    return;
  }

  this->_isConnecting = true;

  std::string ionServerUrl = server.serverUrl().ToStlString();
  std::string ionApiUrl = server.apiUrl().ToStlString();

  CesiumUtility::IntrusivePointer<CesiumIonSessionImpl> pThis = this;

  CesiumAsync::Future<std::optional<std::string>> futureApiUrl =
      !ionApiUrl.empty()
          ? this->_asyncSystem.createResolvedFuture<std::optional<std::string>>(
                ionApiUrl)
          : CesiumIonClient::Connection::getApiUrl(
                this->_asyncSystem,
                this->_pAssetAccessor,
                ionServerUrl);

  std::move(futureApiUrl)
      .thenInMainThread([ionServerUrl,
                         server,
                         session,
                         pThis,
                         asyncSystem = this->_asyncSystem](
                            std::optional<std::string>&& ionApiUrl) {
        CesiumAsync::Promise<bool> promise = asyncSystem.createPromise<bool>();

        if (session == nullptr) {
          promise.reject(
              std::runtime_error("CesiumIonSession unexpectedly nullptr"));
          return promise.getFuture();
        }
        if (server == nullptr) {
          promise.reject(
              std::runtime_error("CesiumIonServer unexpectedly nullptr"));
          return promise.getFuture();
        }

        if (!ionApiUrl) {
          promise.reject(std::runtime_error(fmt::format(
              "Failed to retrieve API URL from the config.json file at the "
              "specified Ion server URL: {}",
              ionServerUrl)));
          return promise.getFuture();
        }

        if (System::String::IsNullOrEmpty(server.apiUrl())) {
          server.apiUrl(System::String(*ionApiUrl));
#if UNITY_EDITOR
          UnityEditor::EditorUtility::SetDirty(server);
#endif
        }

        // Make request to /appData to learn the server's authentication mode
        return pThis->ensureAppDataLoaded(session);
      })
      .thenInMainThread([ionServerUrl, server, session, pThis](
                            bool loadedAppData) {
        if (!loadedAppData || !pThis->_appData.has_value()) {
          CesiumAsync::Promise<CesiumIonClient::Connection> promise =
              pThis->_asyncSystem.createPromise<CesiumIonClient::Connection>();

          promise.reject(std::runtime_error(
              "Failed to load _appData, can't create connection"));
          return promise.getFuture();
        }

        if (pThis->_appData->needsOauthAuthentication()) {
          int64_t clientID = server.oauth2ApplicationID();
          return CesiumIonClient::Connection::authorize(
                     pThis->_asyncSystem,
                     pThis->_pAssetAccessor,
                     "Cesium for Unity",
                     clientID,
                     OAUTH2_REDIRECT_URI,
                     {"assets:list",
                      "assets:read",
                      "profile:read",
                      "tokens:read",
                      "tokens:write",
                      "geocode"},
                     [pThis](const std::string& url) {
                       pThis->_authorizeUrl = url;
                       pThis->_redirectUrl = CesiumUtility::Uri::getQueryValue(
                           url,
                           "redirect_uri");
                       UnityEngine::Application::OpenURL(url);
                     },
                     pThis->_appData.value(),
                     server.apiUrl().ToStlString(),
                     CesiumUtility::Uri::resolve(ionServerUrl, "oauth"))
              .thenInMainThread(
                  [pThis](CesiumUtility::Result<Connection>&& result) {
                    Promise<Connection> promise =
                        pThis->_asyncSystem.createPromise<Connection>();
                    if (!result.value) {
                      promise.reject(std::runtime_error(result.errors.format(
                          "Errors connecting to Cesium ion:")));
                    } else {
                      promise.resolve(std::move(*result.value));
                    }

                    return promise.getFuture();
                  });
        }

        return pThis->_asyncSystem
            .createResolvedFuture<CesiumIonClient::Connection>(
                CesiumIonClient::Connection(
                    pThis->_asyncSystem,
                    pThis->_pAssetAccessor,
                    pThis->_appData.value(),
                    server.apiUrl().ToStlString()));
      })
      .thenInMainThread([pThis, session](Connection&& connection) {
        pThis->_isConnecting = false;
        pThis->_connection = std::move(connection);

        CesiumForUnity::CesiumIonServer server = session.server();
        CesiumForUnity::CesiumIonServerManager::instance()
            .SetUserAccessTokenAndRefreshToken(
                server,
                pThis->_connection->getAccessToken(),
                pThis->_connection->getRefreshToken());
        pThis->_quickAddItems = nullptr;
        session.BroadcastConnectionUpdate();
      })
      .catchInMainThread([pThis, session](std::exception&& e) {
        DotNet::UnityEngine::Debug::Log(System::String(e.what()));
        pThis->_isConnecting = false;
        pThis->_connection = std::nullopt;
        pThis->_quickAddItems = nullptr;
        session.BroadcastConnectionUpdate();
      });
}

void CesiumIonSessionImpl::Resume(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  CesiumForUnity::CesiumIonServer server = session.server();
  if (server == nullptr || this->IsConnecting(session) ||
      this->IsConnected(session) || this->IsResuming(session)) {
    return;
  }

  if (System::String::IsNullOrEmpty(server.apiUrl())) {
    // Don't try to resume a connection if we don't have an API URL.
    return;
  }

  System::String userAccessToken =
      CesiumForUnity::CesiumIonServerManager::instance().GetUserAccessToken(
          server);
  System::String refreshToken =
      CesiumForUnity::CesiumIonServerManager::instance().GetUserRefreshToken(
          server);

  if (userAccessToken == nullptr)
    userAccessToken = System::String("");
  if (refreshToken == nullptr)
    refreshToken = System::String("");

  this->_isResuming = true;

  CesiumUtility::IntrusivePointer<CesiumIonSessionImpl> pThis = this;

  // Verify that the connection actually works.
  this->ensureAppDataLoaded(session)
      .thenInMainThread([pThis,
                         session,
                         userAccessToken,
                         refreshToken,
                         server,
                         asyncSystem = this->_asyncSystem](bool loadedAppData) {
        CesiumAsync::Promise<void> promise = asyncSystem.createPromise<void>();

        if (session == nullptr || !loadedAppData ||
            !pThis->_appData.has_value()) {
          promise.reject(std::runtime_error(
              "Failed to obtain _appData, can't resume connection"));
          return promise.getFuture();
        }

        CesiumUtility::Result<LoginToken> tokenResult =
            System::String::IsNullOrEmpty(userAccessToken)
                ? LoginToken("", std::nullopt)
                : LoginToken::parse(userAccessToken.ToStlString());
        if (!tokenResult.value) {
          Promise<void> promise = pThis->_asyncSystem.createPromise<void>();
          promise.reject(std::runtime_error(
              tokenResult.errors.format("Failed to parse access token:")));
          return promise.getFuture();
        }

        // We can resume a session with an access token or with a refresh token,
        // or both.
        if (pThis->_appData->needsOauthAuthentication() &&
            System::String::IsNullOrEmpty(userAccessToken) &&
            System::String::IsNullOrEmpty(refreshToken)) {
          // No user access token was stored, so there's no existing session
          // to resume.
          promise.resolve();
          pThis->_isResuming = false;
          return promise.getFuture();
        }

        std::shared_ptr<CesiumIonClient::Connection> pConnection =
            std::make_shared<CesiumIonClient::Connection>(
                pThis->_asyncSystem,
                pThis->_pAssetAccessor,
                *tokenResult.value,
                refreshToken.ToStlString(),
                server.oauth2ApplicationID(),
                OAUTH2_REDIRECT_URI,
                pThis->_appData.value(),
                server.apiUrl().ToStlString());

        return pConnection->me().thenInMainThread(
            [pThis, session, pConnection](
                CesiumIonClient::Response<CesiumIonClient::Profile>&&
                    response) {
              if (session == nullptr)
                return;

              logResponseErrors(response);
              if (response.value.has_value()) {
                pThis->_connection = std::move(*pConnection);
              }
              pThis->_isResuming = false;
              pThis->_quickAddItems = nullptr;
              session.BroadcastConnectionUpdate();

              pThis->startQueuedLoads(session);
            });
      })
      .catchInMainThread([pThis, session](std::exception&& e) {
        if (session == nullptr)
          return;

        UnityEngine::Debug::LogError(System::String(fmt::format(
            "Exception while resuming Cesium ion connection: {}",
            e.what())));

        pThis->_isResuming = false;
      });
}

void CesiumIonSessionImpl::Disconnect(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  this->_connection.reset();
  this->_profile.reset();
  this->_assets.reset();
  this->_tokens.reset();
  this->_defaults.reset();
  this->_appData.reset();

  this->_loadProfileQueued = false;
  this->_loadAssetsQueued = false;
  this->_loadTokensQueued = false;
  this->_loadDefaultsQueued = false;

  CesiumForUnity::CesiumIonServer server = session.server();
  CesiumForUnity::CesiumIonServerManager::instance()
      .SetUserAccessTokenAndRefreshToken(server, nullptr, nullptr);

  this->_quickAddItems = nullptr;

  session.BroadcastConnectionUpdate();
  session.BroadcastAssetsUpdate();
  session.BroadcastProfileUpdate();
  session.BroadcastTokensUpdate();
  session.BroadcastDefaultsUpdate();
}

void CesiumIonSessionImpl::Tick(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  this->_asyncSystem.dispatchMainThreadTasks();
}

System::String CesiumIonSessionImpl::GetProfileUsername(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return System::String(this->getProfile(session).username);
}

System::String CesiumIonSessionImpl::GetAuthorizeUrl(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return System::String(this->_authorizeUrl);
}

System::String CesiumIonSessionImpl::GetRedirectUrl(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return System::String(this->_redirectUrl);
}

DotNet::System::Collections::Generic::List1<
    DotNet::CesiumForUnity::QuickAddItem>
CesiumIonSessionImpl::GetQuickAddItems(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  if (this->_quickAddItems != nullptr) {
    return this->_quickAddItems;
  }

  DotNet::System::Collections::Generic::List1<
      DotNet::CesiumForUnity::QuickAddItem>
      result{};

  const CesiumIonClient::Defaults& defaults = this->getDefaults(session);
  for (const CesiumIonClient::QuickAddAsset& asset : defaults.quickAddAssets) {
    if (asset.type == "3DTILES" ||
        (asset.type == "TERRAIN" && !asset.rasterOverlays.empty())) {
      CesiumForUnity::QuickAddItem item(
          CesiumForUnity::QuickAddItemType::IonTileset,
          System::String(asset.name),
          System::String(asset.description),
          System::String(asset.objectName),
          asset.assetId,
          asset.rasterOverlays.empty()
              ? nullptr
              : System::String(asset.rasterOverlays[0].name),
          asset.rasterOverlays.empty() ? -1 : asset.rasterOverlays[0].assetId);
      result.Add(item);
    }
  }

  this->_quickAddItems = result;

  return result;
}

void CesiumIonSessionImpl::RefreshProfile(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  this->refreshProfile(session);
}

void CesiumIonSessionImpl::refreshProfile(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  if (!this->_connection || this->_isLoadingProfile) {
    this->_loadProfileQueued = true;
    return;
  }

  this->_isLoadingProfile = true;
  this->_loadProfileQueued = false;

  CesiumUtility::IntrusivePointer<CesiumIonSessionImpl> pThis = this;

  this->_connection->me()
      .thenInMainThread(
          [pThis, session](
              CesiumIonClient::Response<CesiumIonClient::Profile>&& profile) {
            if (session == nullptr)
              return;

            logResponseErrors(profile);
            pThis->_isLoadingProfile = false;
            if (disconnectOnNoValidToken(session, profile))
              return;

            pThis->_profile = std::move(profile.value);
            session.BroadcastProfileUpdate();
            if (pThis->_loadProfileQueued)
              pThis->refreshProfile(session);
          })
      .catchInMainThread([pThis, session](std::exception&& e) {
        if (session == nullptr)
          return;

        UnityEngine::Debug::LogError(System::String(fmt::format(
            "Exception while loading Cesium ion profile: {}",
            e.what())));

        pThis->_isLoadingProfile = false;
        pThis->_profile.emplace();
        session.BroadcastProfileUpdate();
        if (pThis->_loadProfileQueued)
          pThis->refreshProfile(session);
      });
}

void CesiumIonSessionImpl::RefreshAssets(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  this->refreshAssets(session);
}

void CesiumIonSessionImpl::refreshAssets(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  if (!this->_connection || this->_isLoadingAssets) {
    this->_loadAssetsQueued = true;
    return;
  }

  this->_isLoadingAssets = true;
  this->_loadAssetsQueued = false;

  CesiumUtility::IntrusivePointer<CesiumIonSessionImpl> pThis = this;

  this->_connection->assets()
      .thenInMainThread(
          [pThis, session](
              CesiumIonClient::Response<CesiumIonClient::Assets>&& assets) {
            if (session == nullptr)
              return;

            logResponseErrors(assets);
            pThis->_isLoadingAssets = false;
            if (disconnectOnNoValidToken(session, assets))
              return;

            pThis->_assets = std::move(assets.value);
            session.BroadcastAssetsUpdate();
            if (pThis->_loadAssetsQueued)
              pThis->refreshAssets(session);
          })
      .catchInMainThread([pThis, session](std::exception&& e) {
        if (session == nullptr)
          return;

        UnityEngine::Debug::LogError(System::String(fmt::format(
            "Exception while loading Cesium ion assets: {}",
            e.what())));

        pThis->_isLoadingAssets = false;
        pThis->_assets.emplace();
        session.BroadcastAssetsUpdate();
        if (pThis->_loadAssetsQueued)
          pThis->refreshAssets(session);
      });
}

void CesiumIonSessionImpl::RefreshTokens(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  this->refreshTokens(session);
}

void CesiumIonSessionImpl::RefreshDefaults(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  this->refreshDefaults(session);
}

void CesiumIonSessionImpl::refreshTokens(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  if (!this->_connection || this->_isLoadingTokens) {
    this->_loadTokensQueued = true;
    return;
  }

  this->_isLoadingTokens = true;
  this->_loadTokensQueued = false;

  CesiumUtility::IntrusivePointer<CesiumIonSessionImpl> pThis = this;

  this->_connection->tokens()
      .thenInMainThread(
          [pThis, session](
              CesiumIonClient::Response<CesiumIonClient::TokenList>&& tokens) {
            if (session == nullptr)
              return;

            logResponseErrors(tokens);
            pThis->_isLoadingTokens = false;
            if (disconnectOnNoValidToken(session, tokens))
              return;

            pThis->_tokens =
                tokens.value
                    ? std::make_optional(std::move(tokens.value->items))
                    : std::nullopt;
            session.BroadcastTokensUpdate();
            if (pThis->_loadTokensQueued)
              pThis->refreshTokens(session);
          })
      .catchInMainThread([pThis, session](std::exception&& e) {
        if (session == nullptr)
          return;

        UnityEngine::Debug::LogError(System::String(fmt::format(
            "Exception while loading Cesium ion tokens: {}",
            e.what())));

        pThis->_isLoadingTokens = false;
        pThis->_tokens.emplace();
        session.BroadcastTokensUpdate();
        if (pThis->_loadTokensQueued)
          pThis->refreshTokens(session);
      });
}

void CesiumIonSessionImpl::refreshDefaults(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  if (!this->_connection || this->_isLoadingDefaults) {
    this->_loadDefaultsQueued = true;
    return;
  }

  this->_isLoadingDefaults = true;
  this->_loadDefaultsQueued = false;

  CesiumUtility::IntrusivePointer<CesiumIonSessionImpl> pThis = this;

  this->_connection->defaults()
      .thenInMainThread(
          [pThis, session](
              CesiumIonClient::Response<CesiumIonClient::Defaults>&& defaults) {
            if (session == nullptr)
              return;

            logResponseErrors(defaults);
            pThis->_isLoadingDefaults = false;
            if (disconnectOnNoValidToken(session, defaults))
              return;

            pThis->_defaults = std::move(defaults.value);
            pThis->_quickAddItems = nullptr;
            session.BroadcastDefaultsUpdate();
            if (pThis->_loadDefaultsQueued)
              pThis->refreshDefaults(session);
          })
      .catchInMainThread([pThis, session](std::exception&& e) {
        if (session == nullptr)
          return;

        UnityEngine::Debug::LogError(System::String(fmt::format(
            "Exception while loading Cesium ion defaults: {}",
            e.what())));

        pThis->_isLoadingDefaults = false;
        pThis->_defaults.emplace();
        pThis->_quickAddItems = nullptr;
        session.BroadcastDefaultsUpdate();
        if (pThis->_loadDefaultsQueued)
          pThis->refreshDefaults(session);
      });
}

CesiumAsync::Future<CesiumIonClient::Response<CesiumIonClient::Token>>
CesiumIonSessionImpl::findToken(const std::string& token) const {
  if (!this->_connection) {
    return this->getAsyncSystem().createResolvedFuture(
        CesiumIonClient::Response<CesiumIonClient::Token>(
            0,
            "NOTCONNECTED",
            "Not connected to Cesium ion."));
  }

  std::optional<std::string> maybeTokenID =
      CesiumIonClient::Connection::getIdFromToken(token);

  if (!maybeTokenID) {
    return this->getAsyncSystem().createResolvedFuture(
        CesiumIonClient::Response<CesiumIonClient::Token>(
            0,
            "INVALIDTOKEN",
            "The token is not valid."));
  }

  return this->_connection->token(*maybeTokenID);
}

namespace {

CesiumIonClient::Token createDefaultToken(const std::string& token) {
  CesiumIonClient::Token result;
  result.token = token;
  return result;
}

CesiumAsync::Future<CesiumIonClient::Token>
getDefaultTokenFuture(const CesiumForUnity::CesiumIonSession& session) {
  System::String defaultTokenID = session.server().defaultIonAccessTokenId();
  System::String defaultToken = session.server().defaultIonAccessToken();
  if (!System::String::IsNullOrEmpty(defaultTokenID)) {
    return session.NativeImplementation()
        .getConnection()
        ->token(defaultTokenID.ToStlString())
        .thenImmediately([defaultToken = defaultToken.ToStlString()](
                             CesiumIonClient::Response<CesiumIonClient::Token>&&
                                 tokenResponse) {
          if (tokenResponse.value) {
            return *tokenResponse.value;
          } else {
            return createDefaultToken(defaultToken);
          }
        });
  } else if (!System::String::IsNullOrEmpty(defaultToken)) {
    return session.NativeImplementation()
        .findToken(defaultToken.ToStlString())
        .thenImmediately(
            [defaultToken = defaultToken.ToStlString()](
                CesiumIonClient::Response<CesiumIonClient::Token>&& response) {
              if (response.value) {
                return *response.value;
              } else {
                return createDefaultToken(defaultToken);
              }
            });
  } else {
    return getAsyncSystem().createResolvedFuture(
        createDefaultToken(defaultToken.ToStlString()));
  }
}
} // namespace

CesiumAsync::SharedFuture<CesiumIonClient::Token>
CesiumIonSessionImpl::getProjectDefaultTokenDetails(
    const CesiumForUnity::CesiumIonSession& session) {
  if (this->_projectDefaultTokenDetailsFuture) {
    // If the future is resolved but its token doesn't match the designated
    // default token, do the request again because the user probably specified a
    // new token.
    const System::String& defaultToken =
        session.server().defaultIonAccessToken();
    if (this->_projectDefaultTokenDetailsFuture->isReady() &&
        this->_projectDefaultTokenDetailsFuture->wait().token !=
            defaultToken.ToStlString()) {
      this->_projectDefaultTokenDetailsFuture.reset();
    } else {
      return *this->_projectDefaultTokenDetailsFuture;
    }
  }

  if (!session.IsConnected()) {
    return this->getAsyncSystem()
        .createResolvedFuture(createDefaultToken(
            session.server().defaultIonAccessToken().ToStlString()))
        .share();
  }

  this->_projectDefaultTokenDetailsFuture =
      getDefaultTokenFuture(session).share();
  return *this->_projectDefaultTokenDetailsFuture;
}

void CesiumIonSessionImpl::invalidateProjectDefaultTokenDetails() {
  this->_projectDefaultTokenDetailsFuture.reset();
}

const std::optional<CesiumIonClient::Connection>&
CesiumIonSessionImpl::getConnection() const {
  return this->_connection;
}

const CesiumIonClient::Profile& CesiumIonSessionImpl::getProfile(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  static const CesiumIonClient::Profile empty{};
  if (this->_profile) {
    return *this->_profile;
  } else {
    this->refreshProfile(session);
    return empty;
  }
}

const CesiumIonClient::Assets& CesiumIonSessionImpl::getAssets(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  static const CesiumIonClient::Assets empty;
  if (this->_assets) {
    return *this->_assets;
  } else {
    this->refreshAssets(session);
    return empty;
  }
}

const std::vector<CesiumIonClient::Token>& CesiumIonSessionImpl::getTokens(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  static const std::vector<CesiumIonClient::Token> empty;
  if (this->_tokens) {
    return *this->_tokens;
  } else {
    this->refreshTokens(session);
    return empty;
  }
}

const CesiumIonClient::ApplicationData& CesiumIonSessionImpl::getAppData() {
  static const CesiumIonClient::ApplicationData empty{};
  if (this->_appData) {
    return *this->_appData;
  }
  return empty;
}

const CesiumIonClient::Defaults& CesiumIonSessionImpl::getDefaults(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  static const CesiumIonClient::Defaults empty;
  if (this->_defaults) {
    return *this->_defaults;
  } else {
    this->refreshDefaults(session);
    return empty;
  }
}

const std::shared_ptr<CesiumAsync::IAssetAccessor>&
CesiumIonSessionImpl::getAssetAccessor() const {
  return this->_pAssetAccessor;
}

const CesiumAsync::AsyncSystem& CesiumIonSessionImpl::getAsyncSystem() const {
  return this->_asyncSystem;
}

CesiumAsync::AsyncSystem& CesiumIonSessionImpl::getAsyncSystem() {
  return this->_asyncSystem;
}

void CesiumIonSessionImpl::startQueuedLoads(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  if (this->_loadProfileQueued)
    this->refreshProfile(session);
  if (this->_loadAssetsQueued)
    this->refreshAssets(session);
  if (this->_loadTokensQueued)
    this->refreshTokens(session);
  if (this->_loadDefaultsQueued)
    this->refreshDefaults(session);
}

CesiumAsync::Future<bool> CesiumIonSessionImpl::ensureAppDataLoaded(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  CesiumForUnity::CesiumIonServer server = session.server();

  CesiumUtility::IntrusivePointer<CesiumIonSessionImpl> pThis = this;

  return CesiumIonClient::Connection::appData(
             this->_asyncSystem,
             this->_pAssetAccessor,
             server.apiUrl().ToStlString())
      .thenInMainThread(
          [pThis, session, asyncSystem = this->_asyncSystem](
              CesiumIonClient::Response<CesiumIonClient::ApplicationData>&&
                  appData) {
            CesiumAsync::Promise<bool> promise =
                asyncSystem.createPromise<bool>();

            if (session == nullptr) {
              UnityEngine::Debug::LogError(
                  System::String("CesiumIonSession unexpectedly nullptr"));
              promise.resolve(false);
              return promise.getFuture();
            }

            pThis->_appData = appData.value;
            if (!appData.value.has_value()) {
              UnityEngine::Debug::LogError(System::String(fmt::format(
                  "Failed to obtain ion server application data: {}",
                  appData.errorMessage)));
              promise.resolve(false);
            } else {
              promise.resolve(true);
            }

            return promise.getFuture();
          })
      .catchInMainThread([pThis, session, asyncSystem = this->_asyncSystem](
                             std::exception&& e) {
        UnityEngine::Debug::LogError(System::String(fmt::format(
            "Exception while loading Cesium ion app data: {}",
            e.what())));

        return asyncSystem.createResolvedFuture(false);
      });
}

} // namespace CesiumForUnityNative
