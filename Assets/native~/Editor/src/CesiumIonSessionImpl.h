#pragma once

#include <CesiumAsync/AsyncSystem.h>
#include <CesiumAsync/IAssetAccessor.h>
#include <CesiumAsync/SharedFuture.h>

#include <CesiumIonClient/Assets.h>
#include <CesiumIonClient/Connection.h>
#include <CesiumIonClient/Profile.h>
#include <CesiumIonClient/Token.h>

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
}

namespace CesiumForUnityNative {
class CesiumIonSessionImpl {
public:
  CesiumIonSessionImpl(const DotNet::CesiumForUnity::CesiumIonSession& session);
  ~CesiumIonSessionImpl();

  void
  JustBeforeDelete(const DotNet::CesiumForUnity::CesiumIonSession& session);

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

  void Connect(const DotNet::CesiumForUnity::CesiumIonSession& session);
  void Resume(const DotNet::CesiumForUnity::CesiumIonSession& session);
  void Disconnect(const DotNet::CesiumForUnity::CesiumIonSession& session);

private:
  CesiumAsync::AsyncSystem _asyncSystem;
  std::shared_ptr<CesiumAsync::IAssetAccessor> _pAssetAccessor;

  std::optional<CesiumIonClient::Connection> _connection;
  std::optional<CesiumIonClient::Profile> _profile;
  std::optional<CesiumIonClient::Assets> _assets;
  std::optional<std::vector<CesiumIonClient::Token>> _tokens;

  bool _isConnecting;
  bool _isResuming;
  bool _isLoadingProfile;
  bool _isLoadingAssets;
  bool _isLoadingTokens;

  std::string _authorizeUrl;

  const static std::string userAccessTokenKey;
};
} // namespace CesiumForUnityNative
