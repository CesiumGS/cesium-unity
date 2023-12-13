#pragma once

#include <CesiumAsync/AsyncSystem.h>
#include <CesiumAsync/IAssetAccessor.h>
#include <CesiumAsync/SharedFuture.h>
#include <CesiumIonClient/Assets.h>
#include <CesiumIonClient/Connection.h>
#include <CesiumIonClient/Defaults.h>
#include <CesiumIonClient/Profile.h>
#include <CesiumIonClient/Token.h>

#include <DotNet/System/Collections/Generic/List1.h>
#include <DotNet/System/String.h>

#include <functional>
#include <memory>
#include <optional>
#include <vector>

namespace DotNet::CesiumForUnity {
class CesiumIonSession;
}

namespace CesiumAsync {
class AsyncSystem;
}

namespace CesiumIonClient {
class Assets;
class Connection;
class Profile;
class Token;
} // namespace CesiumIonClient

namespace CesiumForUnityNative {
class CesiumIonSessionImpl {
public:
  CesiumIonSessionImpl(const DotNet::CesiumForUnity::CesiumIonSession& session);
  ~CesiumIonSessionImpl();

  bool IsConnected(const DotNet::CesiumForUnity::CesiumIonSession& session);
  bool IsConnecting(const DotNet::CesiumForUnity::CesiumIonSession& session);
  bool IsResuming(const DotNet::CesiumForUnity::CesiumIonSession& session);

  bool IsProfileLoaded(const DotNet::CesiumForUnity::CesiumIonSession& session);
  bool
  IsLoadingProfile(const DotNet::CesiumForUnity::CesiumIonSession& session);

  bool
  IsAssetListLoaded(const DotNet::CesiumForUnity::CesiumIonSession& session);
  bool
  IsLoadingAssetList(const DotNet::CesiumForUnity::CesiumIonSession& session);

  bool
  IsTokenListLoaded(const DotNet::CesiumForUnity::CesiumIonSession& session);
  bool
  IsLoadingTokenList(const DotNet::CesiumForUnity::CesiumIonSession& session);

  bool
  IsDefaultsLoaded(const DotNet::CesiumForUnity::CesiumIonSession& session);
  bool
  IsLoadingDefaults(const DotNet::CesiumForUnity::CesiumIonSession& session);

  void Connect(const DotNet::CesiumForUnity::CesiumIonSession& session);
  void Resume(const DotNet::CesiumForUnity::CesiumIonSession& session);
  void Disconnect(const DotNet::CesiumForUnity::CesiumIonSession& session);

  void Tick(const DotNet::CesiumForUnity::CesiumIonSession& session);

  DotNet::System::String
  GetProfileUsername(const DotNet::CesiumForUnity::CesiumIonSession& session);
  DotNet::System::String
  GetAuthorizeUrl(const DotNet::CesiumForUnity::CesiumIonSession& session);
  DotNet::System::String
  GetRedirectUrl(const DotNet::CesiumForUnity::CesiumIonSession& session);
  DotNet::System::Collections::Generic::List1<
      DotNet::CesiumForUnity::QuickAddItem>
  GetQuickAddItems(const DotNet::CesiumForUnity::CesiumIonSession& session);

  void RefreshProfile(const DotNet::CesiumForUnity::CesiumIonSession& session);
  void RefreshAssets(const DotNet::CesiumForUnity::CesiumIonSession& session);
  void RefreshTokens(const DotNet::CesiumForUnity::CesiumIonSession& session);
  void RefreshDefaults(const DotNet::CesiumForUnity::CesiumIonSession& session);

  void refreshProfile();
  void refreshAssets();
  void refreshTokens();
  void refreshDefaults();

  bool refreshProfileIfNeeded();
  bool refreshAssetsIfNeeded();
  bool refreshTokensIfNeeded();
  bool refreshDefaultsIfNeeded();

  CesiumAsync::Future<CesiumIonClient::Response<CesiumIonClient::Token>>
  findToken(const std::string& token) const;

  CesiumAsync::SharedFuture<CesiumIonClient::Token>
  getProjectDefaultTokenDetails(
      const DotNet::CesiumForUnity::CesiumIonSession& session);

  void invalidateProjectDefaultTokenDetails();

  const std::optional<CesiumIonClient::Connection>& getConnection() const;
  const CesiumIonClient::Profile& getProfile();
  const CesiumIonClient::Assets& getAssets();
  const std::vector<CesiumIonClient::Token>& getTokens();
  const CesiumIonClient::Defaults& getDefaults();

  const std::shared_ptr<CesiumAsync::IAssetAccessor>& getAssetAccessor() const;
  const CesiumAsync::AsyncSystem& getAsyncSystem() const;
  CesiumAsync::AsyncSystem& getAsyncSystem();

private:
  void startQueuedLoads();

  CesiumAsync::AsyncSystem _asyncSystem;
  std::shared_ptr<CesiumAsync::IAssetAccessor> _pAssetAccessor;

  std::optional<CesiumIonClient::Connection> _connection;
  std::optional<CesiumIonClient::Profile> _profile;
  std::optional<CesiumIonClient::Assets> _assets;
  std::optional<std::vector<CesiumIonClient::Token>> _tokens;
  std::optional<CesiumIonClient::Defaults> _defaults;

  std::optional<CesiumAsync::SharedFuture<CesiumIonClient::Token>>
      _projectDefaultTokenDetailsFuture;

  bool _isConnecting;
  bool _isResuming;
  bool _isLoadingProfile;
  bool _isLoadingAssets;
  bool _isLoadingTokens;
  bool _isLoadingDefaults;

  bool _loadProfileQueued;
  bool _loadAssetsQueued;
  bool _loadTokensQueued;
  bool _loadDefaultsQueued;

  std::function<void()> broadcastConnectionUpdate;
  std::function<void()> broadcastAssetsUpdate;
  std::function<void()> broadcastProfileUpdate;
  std::function<void()> broadcastTokensUpdate;
  std::function<void()> broadcastDefaultsUpdate;

  std::string _authorizeUrl;
  std::string _redirectUrl;

  DotNet::System::Collections::Generic::List1<
      DotNet::CesiumForUnity::QuickAddItem>
      _quickAddItems;

  const static std::string _userAccessTokenEditorKey;
};
} // namespace CesiumForUnityNative
