#include "CesiumIonSessionImpl.h"

#include "UnityAssetAccessor.h"
#include "UnityExternals.h"
#include "UnityTaskProcessor.h"

#include <CesiumUtility/Uri.h>

#include <DotNet/CesiumForUnity/CesiumIonServer.h>
#include <DotNet/CesiumForUnity/CesiumIonServerManager.h>
#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/CesiumForUnity/QuickAddItem.h>
#include <DotNet/CesiumForUnity/QuickAddItemType.h>
#include <DotNet/System/Object.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Debug.h>

using namespace DotNet;

namespace {

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

void logResponseErrors(const std::exception& exception) {
  UnityEngine::Debug::LogError(
      System::String(fmt::format("Exception: {}", exception.what())));
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
      broadcastConnectionUpdate(std::bind(
          &DotNet::CesiumForUnity::CesiumIonSession::BroadcastConnectionUpdate,
          session)),
      broadcastAssetsUpdate(std::bind(
          &DotNet::CesiumForUnity::CesiumIonSession::BroadcastAssetsUpdate,
          session)),
      broadcastProfileUpdate(std::bind(
          &DotNet::CesiumForUnity::CesiumIonSession::BroadcastProfileUpdate,
          session)),
      broadcastTokensUpdate(std::bind(
          &DotNet::CesiumForUnity::CesiumIonSession::BroadcastTokensUpdate,
          session)),
      broadcastDefaultsUpdate(std::bind(
          &DotNet::CesiumForUnity::CesiumIonSession::BroadcastDefaultsUpdate,
          session)),
      _quickAddItems() {}

CesiumIonSessionImpl::~CesiumIonSessionImpl() {}

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

  CesiumAsync::Future<std::optional<std::string>> futureApiUrl =
      !ionApiUrl.empty()
          ? this->_asyncSystem.createResolvedFuture<std::optional<std::string>>(
                ionApiUrl)
          : CesiumIonClient::Connection::getApiUrl(
                this->_asyncSystem,
                this->_pAssetAccessor,
                ionServerUrl);

  std::move(futureApiUrl)
      .thenInMainThread([ionServerUrl, server, session, this](
                            std::optional<std::string>&& ionApiUrl) {
        CesiumAsync::Promise<
            CesiumIonClient::Response<CesiumIonClient::ApplicationData>>
            promise = this->_asyncSystem.createPromise<
                CesiumIonClient::Response<CesiumIonClient::ApplicationData>>();

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
        }

        // Make request to /appData to learn the server's authentication mode
        return CesiumIonClient::Connection::appData(
            this->_asyncSystem,
            this->_pAssetAccessor,
            server.apiUrl().ToStlString());
      })
      .thenInMainThread(
          [ionServerUrl, server, session, this](
              CesiumIonClient::Response<CesiumIonClient::ApplicationData>&&
                  appData) {
            CesiumAsync::Promise<void> promise =
                this->_asyncSystem.createPromise<void>();

            this->_appData = appData.value;
            if (!appData.value.has_value()) {
              promise.reject(std::runtime_error(fmt::format(
                  "Failed to obtain ion server application data: {}",
                  appData.errorMessage)));
            } else {
              promise.resolve();
            }

            return promise.getFuture();
          })
      .catchInMainThread(
          [ionServerUrl, server, session, this](std::exception&& e) {
            DotNet::UnityEngine::Debug::Log(System::String::String(e.what()));
            this->_isConnecting = false;
            this->_connection = std::nullopt;
            this->broadcastConnectionUpdate();
          })
      .thenInMainThread([ionServerUrl, server, session, this]() {
        if (this->_appData->needsOauthAuthentication()) {
          int64_t clientID = server.oauth2ApplicationID();
          return CesiumIonClient::Connection::authorize(
              this->_asyncSystem,
              this->_pAssetAccessor,
              "Cesium for Unity",
              clientID,
              "/cesium-for-unity/oauth2/callback",
              {"assets:list",
               "assets:read",
               "profile:read",
               "tokens:read",
               "tokens:write",
               "geocode"},
              [this](const std::string& url) {
                this->_authorizeUrl = url;
                this->_redirectUrl =
                    CesiumUtility::Uri::getQueryValue(url, "redirect_uri");
                UnityEngine::Application::OpenURL(url);
              },
              this->_appData.value(),
              server.apiUrl().ToStlString(),
              CesiumUtility::Uri::resolve(ionServerUrl, "oauth"));
        }

        return this->_asyncSystem
            .createResolvedFuture<CesiumIonClient::Connection>(
                CesiumIonClient::Connection(
                    this->_asyncSystem,
                    this->_pAssetAccessor,
                    "",
                    this->_appData.value(),
                    server.apiUrl().ToStlString()));
      })
      .thenInMainThread([this,
                         session](CesiumIonClient::Connection&& connection) {
        this->_isConnecting = false;
        this->_connection = std::move(connection);

        CesiumForUnity::CesiumIonServer server = session.server();
        CesiumForUnity::CesiumIonServerManager::instance().SetUserAccessToken(
            server,
            this->_connection.value().getAccessToken());
        this->_quickAddItems = nullptr;
        this->broadcastConnectionUpdate();
      })
      .catchInMainThread([this](std::exception&& e) {
        this->_isConnecting = false;
        this->_connection = std::nullopt;
        this->_quickAddItems = nullptr;
        this->broadcastConnectionUpdate();
      });
}

void CesiumIonSessionImpl::Resume(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  CesiumForUnity::CesiumIonServer server = session.server();
  if (server == nullptr || this->IsConnecting(session) ||
      this->IsConnected(session) || this->IsResuming(session)) {
    return;
  }

  System::String userAccessToken =
      CesiumForUnity::CesiumIonServerManager::instance().GetUserAccessToken(
          server);

  if (System::String::IsNullOrEmpty(userAccessToken)) {
    // No user access token was stored, so there's no existing session to
    // resume.
    return;
  }

  if (!this->_appData.has_value()) {
    // _appData is filled in by Connect - if we're missing it, we don't have a
    // valid session to resume.
    return;
  }

  this->_isResuming = true;

  std::shared_ptr<CesiumIonClient::Connection> pConnection =
      std::make_shared<CesiumIonClient::Connection>(
          this->_asyncSystem,
          this->_pAssetAccessor,
          userAccessToken.ToStlString(),
          this->_appData.value(),
          server.apiUrl().ToStlString());

  // Verify that the connection actually works.
  pConnection->me()
      .thenInMainThread(
          [this, pConnection](
              CesiumIonClient::Response<CesiumIonClient::Profile>&& response) {
            logResponseErrors(response);
            if (response.value.has_value()) {
              this->_connection = std::move(*pConnection);
            }
            this->_isResuming = false;
            this->_quickAddItems = nullptr;
            this->broadcastConnectionUpdate();

            this->startQueuedLoads();
          })
      .catchInMainThread([this](std::exception&& e) {
        logResponseErrors(e);
        this->_isResuming = false;
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

  CesiumForUnity::CesiumIonServer server = session.server();
  CesiumForUnity::CesiumIonServerManager::instance().SetUserAccessToken(
      server,
      nullptr);

  this->_quickAddItems = nullptr;

  this->broadcastConnectionUpdate();
  this->broadcastAssetsUpdate();
  this->broadcastProfileUpdate();
  this->broadcastTokensUpdate();
  this->broadcastDefaultsUpdate();
}

void CesiumIonSessionImpl::Tick(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  this->_asyncSystem.dispatchMainThreadTasks();
}

System::String CesiumIonSessionImpl::GetProfileUsername(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  return System::String(this->getProfile().username);
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

  const CesiumIonClient::Defaults& defaults = this->getDefaults();
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
  this->refreshProfile();
}

void CesiumIonSessionImpl::refreshProfile() {
  if (!this->_connection || this->_isLoadingProfile) {
    this->_loadProfileQueued = true;
    return;
  }

  this->_isLoadingProfile = true;
  this->_loadProfileQueued = false;

  this->_connection->me()
      .thenInMainThread(
          [this](
              CesiumIonClient::Response<CesiumIonClient::Profile>&& profile) {
            this->_isLoadingProfile = false;
            this->_profile = std::move(profile.value);
            this->broadcastProfileUpdate();
            if (this->_loadProfileQueued)
              this->refreshProfile();
          })
      .catchInMainThread([this](std::exception&& e) {
        this->_isLoadingProfile = false;
        this->_profile = std::nullopt;
        this->broadcastProfileUpdate();
        if (this->_loadProfileQueued)
          this->refreshProfile();
      });
}

void CesiumIonSessionImpl::RefreshAssets(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  this->refreshAssets();
}

void CesiumIonSessionImpl::refreshAssets() {
  if (!this->_connection || this->_isLoadingAssets) {
    this->_loadAssetsQueued = true;
    return;
  }

  this->_isLoadingAssets = true;
  this->_loadAssetsQueued = false;

  this->_connection->assets()
      .thenInMainThread(
          [this](CesiumIonClient::Response<CesiumIonClient::Assets>&& assets) {
            this->_isLoadingAssets = false;
            this->_assets = std::move(assets.value);
            this->broadcastAssetsUpdate();
            if (this->_loadAssetsQueued)
              this->refreshAssets();
          })
      .catchInMainThread([this](std::exception&& e) {
        this->_isLoadingAssets = false;
        this->_assets = std::nullopt;
        this->broadcastAssetsUpdate();
        if (this->_loadAssetsQueued)
          this->refreshAssets();
      });
}

void CesiumIonSessionImpl::RefreshTokens(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  this->refreshTokens();
}

void CesiumIonSessionImpl::RefreshDefaults(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  this->refreshDefaults();
}

void CesiumIonSessionImpl::refreshTokens() {
  if (!this->_connection || this->_isLoadingTokens) {
    this->_loadTokensQueued = true;
    return;
  }

  this->_isLoadingTokens = true;
  this->_loadTokensQueued = false;

  this->_connection->tokens()
      .thenInMainThread(
          [this](
              CesiumIonClient::Response<CesiumIonClient::TokenList>&& tokens) {
            this->_isLoadingTokens = false;
            this->_tokens =
                tokens.value
                    ? std::make_optional(std::move(tokens.value->items))
                    : std::nullopt;
            this->broadcastTokensUpdate();
            if (this->_loadTokensQueued)
              this->refreshTokens();
          })
      .catchInMainThread([this](std::exception&& e) {
        this->_isLoadingTokens = false;
        this->_tokens = std::nullopt;
        this->broadcastTokensUpdate();
        if (this->_loadTokensQueued)
          this->refreshTokens();
      });
}

void CesiumIonSessionImpl::refreshDefaults() {
  if (!this->_connection || this->_isLoadingDefaults) {
    this->_loadDefaultsQueued = true;
    return;
  }

  this->_isLoadingDefaults = true;
  this->_loadDefaultsQueued = false;

  this->_connection->defaults()
      .thenInMainThread(
          [this](
              CesiumIonClient::Response<CesiumIonClient::Defaults>&& defaults) {
            logResponseErrors(defaults);
            this->_isLoadingDefaults = false;
            this->_defaults = std::move(defaults.value);
            this->_quickAddItems = nullptr;
            this->broadcastDefaultsUpdate();
            if (this->_loadDefaultsQueued)
              this->refreshDefaults();
          })
      .catchInMainThread([this](std::exception&& e) {
        logResponseErrors(e);
        this->_isLoadingDefaults = false;
        this->_defaults = std::nullopt;
        this->_quickAddItems = nullptr;
        this->broadcastDefaultsUpdate();
        if (this->_loadDefaultsQueued)
          this->refreshDefaults();
      });
}

bool CesiumIonSessionImpl::refreshProfileIfNeeded() {
  if (this->_loadProfileQueued || !this->_profile.has_value()) {
    this->refreshProfile();
  }
  return this->_profile.has_value();
}

bool CesiumIonSessionImpl::refreshAssetsIfNeeded() {
  if (this->_loadAssetsQueued || !this->_assets.has_value()) {
    this->refreshAssets();
  }
  return this->_assets.has_value();
}

bool CesiumIonSessionImpl::refreshTokensIfNeeded() {
  if (this->_loadTokensQueued || !this->_tokens.has_value()) {
    this->refreshTokens();
  }
  return this->_tokens.has_value();
}

bool CesiumIonSessionImpl::refreshDefaultsIfNeeded() {
  if (this->_loadDefaultsQueued || !this->_defaults.has_value()) {
    this->refreshDefaults();
  }
  return this->_defaults.has_value();
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

const CesiumIonClient::Profile& CesiumIonSessionImpl::getProfile() {
  static const CesiumIonClient::Profile empty{};
  if (this->_profile) {
    return *this->_profile;
  } else {
    this->refreshProfile();
    return empty;
  }
}

const CesiumIonClient::Assets& CesiumIonSessionImpl::getAssets() {
  static const CesiumIonClient::Assets empty;
  if (this->_assets) {
    return *this->_assets;
  } else {
    this->refreshAssets();
    return empty;
  }
}

const std::vector<CesiumIonClient::Token>& CesiumIonSessionImpl::getTokens() {
  static const std::vector<CesiumIonClient::Token> empty;
  if (this->_tokens) {
    return *this->_tokens;
  } else {
    this->refreshTokens();
    return empty;
  }
}

const CesiumIonClient::ApplicationData& CesiumIonSessionImpl::getAppData() {
  static const CesiumIonClient::ApplicationData empty;
  return this->_appData.value_or(empty);
}

const CesiumIonClient::Defaults& CesiumIonSessionImpl::getDefaults() {
  static const CesiumIonClient::Defaults empty;
  if (this->_defaults) {
    return *this->_defaults;
  } else {
    this->refreshDefaults();
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

void CesiumIonSessionImpl::startQueuedLoads() {
  if (this->_loadProfileQueued)
    this->refreshProfile();
  if (this->_loadAssetsQueued)
    this->refreshAssets();
  if (this->_loadTokensQueued)
    this->refreshTokens();
  if (this->_loadDefaultsQueued)
    this->refreshDefaults();
}

} // namespace CesiumForUnityNative
