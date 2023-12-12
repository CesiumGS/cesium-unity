#include "CesiumIonServerHelper.h"

#include "UnityTilesetExternals.h"

#include <CesiumIonClient/Connection.h>

#include <DotNet/CesiumForUnity/CesiumIonServer.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEditor/EditorUtility.h>
#include <DotNet/UnityEngine/Object.h>

using namespace DotNet;

namespace CesiumForUnityNative {

void resolveCesiumIonApiUrl(
    const DotNet::CesiumForUnity::CesiumIonServer& server) {
#if UNITY_EDITOR
  getAsyncSystem().dispatchMainThreadTasks();

  if (server.serverUrlThatIsLoadingApiUrl() != server.serverUrl()) {
    System::String serverUrl = server.serverUrl();
    server.serverUrlThatIsLoadingApiUrl(serverUrl);
    CesiumIonClient::Connection::getApiUrl(
        getAsyncSystem(),
        getAssetAccessor(),
        server.serverUrl().ToStlString())
        .thenInMainThread(
            [server, serverUrl](std::optional<std::string>&& maybeApiUrl) {
              if (!maybeApiUrl)
                return;

              if (server != nullptr) {
                if (serverUrl == server.serverUrlThatIsLoadingApiUrl()) {
                  server.serverUrlThatIsLoadingApiUrl(nullptr);
                  server.apiUrl(System::String(*maybeApiUrl));
                  UnityEditor::EditorUtility::SetDirty(server);
                }
              }
            });
  }
#endif
}

} // namespace CesiumForUnityNative
