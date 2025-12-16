#pragma once

namespace DotNet::CesiumForUnity {
class CesiumIonServer;
}

namespace CesiumForUnityNative {

void resolveCesiumIonApiUrl(
    const DotNet::CesiumForUnity::CesiumIonServer& server);

}
