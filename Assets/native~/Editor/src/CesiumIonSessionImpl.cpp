#include "CesiumIonSessionImpl.h"

#include "UnityAssetAccessor.h"
#include "UnityTaskProcessor.h"

#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/CesiumForUnity/CesiumRuntimeSettings.h>
#include <DotNet/UnityEditor/EditorPrefs.h>
#include <DotNet/UnityEngine/Application.h>

using namespace DotNet;

namespace CesiumForUnityNative {

const std::string CesiumIonSessionImpl::_userAccessTokenEditorKey =
    "CesiumUserAccessToken";

CesiumIonSessionImpl& CesiumIonSessionImpl::ion() {
  return CesiumForUnity::CesiumIonSession::Ion().NativeImplementation();
}

CesiumIonSessionImpl::CesiumIonSessionImpl(
    const DotNet::CesiumForUnity::CesiumIonSession& session)
    : _asyncSystem(
          CesiumAsync::AsyncSystem(std::make_shared<UnityTaskProcessor>())),
      _pAssetAccessor(std::make_shared<UnityAssetAccessor>()),
      _connection(std::nullopt),
      _profile(std::nullopt),
      _assets(std::nullopt),
      _tokens(std::nullopt),
      _isConnecting(false),
      _isResuming(false),
      _isLoadingProfile(false),
      _isLoadingAssets(false),
      _isLoadingTokens(false),
      _loadProfileQueued(false),
      _loadAssetsQueued(false),
      _loadTokensQueued(false),
      triggerConnectionUpdate(std::bind(
          &DotNet::CesiumForUnity::CesiumIonSession::TriggerConnectionUpdate,
          session)),
      triggerAssetsUpdate(std::bind(
          &DotNet::CesiumForUnity::CesiumIonSession::TriggerAssetsUpdate,
          session)),
      triggerProfileUpdate(std::bind(
          &DotNet::CesiumForUnity::CesiumIonSession::TriggerProfileUpdate,
          session)),
      triggerTokensUpdate(std::bind(
          &DotNet::CesiumForUnity::CesiumIonSession::TriggerTokensUpdate,
          session)) {}

CesiumIonSessionImpl::~CesiumIonSessionImpl() {}

void CesiumIonSessionImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {}

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

void CesiumIonSessionImpl::Connect(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  if (this->IsConnecting(session) || this->IsConnected(session) ||
      this->IsResuming(session)) {
    return;
  }

  this->_isConnecting = true;

  CesiumIonClient::Connection::authorize(
      this->_asyncSystem,
      this->_pAssetAccessor,
      "Cesium for Unity",
      381,
      "/cesium-for-unity/oauth2/callback",
      {"assets:list",
       "assets:read",
       "profile:read",
       "tokens:read",
       "tokens:write",
       "geocode"},
      [this](const std::string& url) {
        this->_authorizeUrl = url;
        UnityEngine::Application::OpenURL(url);
      })
      .thenInMainThread([this](CesiumIonClient::Connection&& connection) {
        this->_isConnecting = false;
        this->_connection = std::move(connection);

        UnityEditor::EditorPrefs::SetString(
            CesiumIonSessionImpl::_userAccessTokenEditorKey,
            this->_connection.value().getAccessToken());
        this->triggerConnectionUpdate();

        this->refreshProfile();
        this->triggerProfileUpdate();
      })
      .catchInMainThread([this](std::exception&& e) {
        this->_isConnecting = false;
        this->_connection = std::nullopt;
        this->triggerConnectionUpdate();
      });
}

void CesiumIonSessionImpl::Resume(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  if (this->IsConnecting(session) || this->IsConnected(session) ||
      this->IsResuming(session)) {
    return;
  }

  System::String userAccessToken = UnityEditor::EditorPrefs::GetString(
      CesiumIonSessionImpl::_userAccessTokenEditorKey);

  if (System::String::Equals(userAccessToken, System::String(""))) {
    // No user access token was stored, so there's no existing session to
    // resume.
    return;
  }

  this->_isResuming = true;

  this->_connection = CesiumIonClient::Connection(
      this->_asyncSystem,
      this->_pAssetAccessor,
      userAccessToken.ToStlString());

  // Verify that the connection actually works.
  this->_connection.value()
      .me()
      .thenInMainThread(
          [this](
              CesiumIonClient::Response<CesiumIonClient::Profile>&& response) {
            if (!response.value.has_value()) {
              this->_connection.reset();
            }
            this->_isResuming = false;
            this->triggerConnectionUpdate();

            this->refreshProfile();
            this->triggerProfileUpdate();
          })
      .catchInMainThread([this](std::exception&& e) {
        this->_isResuming = false;
        this->_connection.reset();
      });
}

void CesiumIonSessionImpl::Disconnect(
    const DotNet::CesiumForUnity::CesiumIonSession& session) {
  this->_connection.reset();
  this->_profile.reset();
  this->_assets.reset();
  this->_tokens.reset();

  UnityEditor::EditorPrefs::DeleteKey(
      CesiumIonSessionImpl::_userAccessTokenEditorKey);

  this->triggerConnectionUpdate();
  this->triggerAssetsUpdate();
  this->triggerProfileUpdate();
  this->triggerTokensUpdate();
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
            this->triggerProfileUpdate();
            this->refreshProfileIfNeeded();
          })
      .catchInMainThread([this](std::exception&& e) {
        this->_isLoadingProfile = false;
        this->_profile = std::nullopt;
        this->triggerProfileUpdate();
        this->refreshProfileIfNeeded();
      });
}

void CesiumIonSessionImpl::refreshAssets() {
  if (!this->_connection || this->_isLoadingAssets) {
    return;
  }

  this->_isLoadingAssets = true;
  this->_loadAssetsQueued = false;

  this->_connection->assets()
      .thenInMainThread(
          [this](CesiumIonClient::Response<CesiumIonClient::Assets>&& assets) {
            this->_isLoadingAssets = false;
            this->_assets = std::move(assets.value);
            this->triggerAssetsUpdate();
            this->refreshAssetsIfNeeded();
          })
      .catchInMainThread([this](std::exception&& e) {
        this->_isLoadingAssets = false;
        this->_assets = std::nullopt;
        this->triggerAssetsUpdate();
        this->refreshAssetsIfNeeded();
      });
}

void CesiumIonSessionImpl::refreshTokens() {
  if (!this->_connection || this->_isLoadingTokens) {
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
            this->triggerTokensUpdate();
            this->refreshTokensIfNeeded();
          })
      .catchInMainThread([this](std::exception&& e) {
        this->_isLoadingTokens = false;
        this->_tokens = std::nullopt;
        this->triggerTokensUpdate();
        this->refreshTokensIfNeeded();
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

CesiumIonClient::Token defaultTokenFromSettings() {
  CesiumIonClient::Token result;

  const System::String& defaultToken =
      CesiumForUnity::CesiumRuntimeSettings::GetDefaultIonAccessToken();
  result.token = defaultToken.ToStlString();

  return result;
}

CesiumAsync::Future<CesiumIonClient::Token>
getDefaultTokenFuture(const CesiumIonSessionImpl& session) {
  if (CesiumForUnity::CesiumRuntimeSettings::HasDefaultIonAccessTokenId()) {
    return session.getConnection()
        ->token(
            CesiumForUnity::CesiumRuntimeSettings::GetDefaultIonAccessTokenId()
                    .ToStlString())
        .thenImmediately([](CesiumIonClient::Response<CesiumIonClient::Token>&&
                                tokenResponse) {
          if (tokenResponse.value) {
            return *tokenResponse.value;
          } else {
            return defaultTokenFromSettings();
          }
        });
  } else if (CesiumForUnity::CesiumRuntimeSettings::
                 HasDefaultIonAccessToken()) {
    return session
        .findToken(
            CesiumForUnity::CesiumRuntimeSettings::GetDefaultIonAccessToken()
                       .ToStlString())
        .thenImmediately(
            [](CesiumIonClient::Response<CesiumIonClient::Token>&& response) {
              if (response.value) {
                return *response.value;
              } else {
                return defaultTokenFromSettings();
              }
            });
  } else {
    return session.getAsyncSystem().createResolvedFuture(
        defaultTokenFromSettings());
  }
}
} // namespace

CesiumAsync::SharedFuture<CesiumIonClient::Token>
CesiumIonSessionImpl::getProjectDefaultTokenDetails() {
  if (this->_projectDefaultTokenDetailsFuture) {
    // If the future is resolved but its token doesn't match the designated
    // default token, do the request again because the user probably specified a
    // new token.
    const System::String& defaultToken =
        CesiumForUnity::CesiumRuntimeSettings::GetDefaultIonAccessToken();
    if (this->_projectDefaultTokenDetailsFuture->isReady() &&
        this->_projectDefaultTokenDetailsFuture->wait().token !=
            defaultToken.ToStlString()) {
      this->_projectDefaultTokenDetailsFuture.reset();
    } else {
      return *this->_projectDefaultTokenDetailsFuture;
    }
  }

  if (!this->IsConnected(CesiumForUnity::CesiumIonSession::Ion())) {
    return this->getAsyncSystem()
        .createResolvedFuture(defaultTokenFromSettings())
        .share();
  }

  this->_projectDefaultTokenDetailsFuture =
      getDefaultTokenFuture(*this).share();
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

} // namespace CesiumForUnityNative
