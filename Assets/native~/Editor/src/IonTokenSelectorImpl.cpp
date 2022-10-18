#include "IonTokenSelectorImpl.h"

#include <DotNet/CesiumForUnity/IonTokenSelector.h>


using namespace DotNet;

namespace CesiumForUnityNative {

IonTokenSelectorImpl::IonTokenSelectorImpl(
    const DotNet::CesiumForUnity::IonTokenSelector& tokenSelector) {}

IonTokenSelectorImpl::~IonTokenSelectorImpl() {}

void IonTokenSelectorImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::IonTokenSelector& tokenSelector) {}

void IonTokenSelectorImpl::RefreshTokens(
    const DotNet::CesiumForUnity::IonTokenSelector& tokenSelector) {}

void IonTokenSelectorImpl::CreateToken(
    const DotNet::CesiumForUnity::IonTokenSelector& tokenSelector) {}

void IonTokenSelectorImpl::UseExistingToken(
    const DotNet::CesiumForUnity::IonTokenSelector& tokenSelector) {}

void IonTokenSelectorImpl::SpecifyToken(
    const DotNet::CesiumForUnity::IonTokenSelector& tokenSelector,
    DotNet::System::String token) {
  CesiumAsync::Promise<std::optional<CesiumIonClient::Token>> promise = std::move(*this->_promise);
  this->_promise.reset();
  /*
  ion()
      .findToken(this->_specifyToken.token)
      .thenInMainThread([this](Response<Token>&& response) {
        if (response.value) {
          return std::move(response);
        } else {
          Token t;
          t.token = TCHAR_TO_UTF8(*this->_specifyToken.token);
          return Response(std::move(t), 200, "", "");
        }
      });*/
}

} // namespace CesiumForUnityNative
