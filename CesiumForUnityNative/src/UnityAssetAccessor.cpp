#include "UnityAssetAccessor.h"

#include "Bindings.h"
#include "Interop.h"
#include "NativeDownloadHandler.h"

#include <CesiumAsync/IAssetResponse.h>

using namespace CesiumAsync;
using namespace CesiumForUnity;
using namespace System;
using namespace UnityEngine;
using namespace UnityEngine::Networking;

namespace {

class UnityAssetResponse : public IAssetResponse {
public:
  UnityAssetResponse(
      UnityWebRequest& request,
      const std::shared_ptr<NativeDownloadHandler>& pHandler)
      : _statusCode(uint16_t(request.GetResponseCode())),
        _contentType(Interop::convert(
            request.GetResponseHeader(String("Content-Type")))),
        _pHandler(pHandler) {
    this->_headers.emplace("Content-Type", this->_contentType);
    // TODO: get all response headers
  }

  virtual uint16_t statusCode() const override { return _statusCode; }

  virtual std::string contentType() const override { return _contentType; }

  virtual const HttpHeaders& headers() const override { return _headers; }

  virtual gsl::span<const std::byte> data() const override {
    return this->_pHandler->getData();
  }

private:
  uint16_t _statusCode;
  std::string _contentType;
  HttpHeaders _headers;
  std::shared_ptr<NativeDownloadHandler> _pHandler;
};

class UnityAssetRequest : public IAssetRequest {
public:
  UnityAssetRequest(
      UnityWebRequest& request,
      const std::shared_ptr<NativeDownloadHandler>& pHandler)
      : _method(Interop::convert(request.GetMethod())),
        _url(Interop::convert(request.GetUrl())),
        _headers(),
        _response(request, pHandler) {
    // TODO: get request headers
  }

  virtual const std::string& method() const override { return _method; }

  virtual const std::string& url() const override { return _url; }

  virtual const HttpHeaders& headers() const override { return _headers; }

  virtual const IAssetResponse* response() const override { return &_response; }

private:
  std::string _method;
  std::string _url;
  HttpHeaders _headers;
  UnityAssetResponse _response;
};

struct WebRequestCompleted : public Action_1<AsyncOperation> {
  UnityWebRequest _request;
  std::shared_ptr<NativeDownloadHandler> _pHandler;
  Promise<std::shared_ptr<CesiumAsync::IAssetRequest>> _promise;

  WebRequestCompleted(
      const UnityWebRequest& request,
      const std::shared_ptr<NativeDownloadHandler>& pHandler,
      Promise<std::shared_ptr<CesiumAsync::IAssetRequest>>& promise)
      : _request(request), _pHandler(pHandler), _promise(promise) {}

  virtual void operator()(UnityEngine::AsyncOperation& obj) override {
    if (this->_request.GetIsDone() && this->_request.GetError() == nullptr) {
      this->_promise.resolve(
          std::make_shared<UnityAssetRequest>(this->_request, this->_pHandler));
    } else {
      this->_promise.reject(std::runtime_error(
          "Request failed: " + Interop::convert(this->_request.GetError())));
    }
  }
};

} // namespace

CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
UnityAssetAccessor::get(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const std::string& url,
    const std::vector<THeader>& headers) {
  // Sadly, Unity requires us to call this from the main thread.
  return asyncSystem.runInMainThread([asyncSystem, url, headers]() {
    UnityWebRequest request = UnityWebRequest::Get(String(url.c_str()));

    auto pHandler = std::make_shared<NativeDownloadHandler>();
    request.SetDownloadHandler(*pHandler);

    for (const auto& header : headers) {
      request.SetRequestHeader(
          String(header.first.c_str()),
          String(header.second.c_str()));
    }

    auto promise =
        asyncSystem
            .createPromise<std::shared_ptr<CesiumAsync::IAssetRequest>>();

    auto pCompleted =
        std::make_shared<WebRequestCompleted>(request, pHandler, promise);

    UnityWebRequestAsyncOperation op = request.SendWebRequest();
    op.AddCompleted(*pCompleted);

    return promise.getFuture().thenImmediately(
        [pCompleted](std::shared_ptr<IAssetRequest>&& pRequest) {
          // The lambda capture here keeps the managed WebRequestCompleted alive
          // until we're done with it.
          return std::move(pRequest);
        });
  });
}

CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
UnityAssetAccessor::request(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const std::string& verb,
    const std::string& url,
    const std::vector<THeader>& headers,
    const gsl::span<const std::byte>& contentPayload) {
  throw std::exception();
}

void UnityAssetAccessor::tick() noexcept {}
