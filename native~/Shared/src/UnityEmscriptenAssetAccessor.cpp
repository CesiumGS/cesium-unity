#ifdef __EMSCRIPTEN__

#include "UnityEmscriptenAssetAccessor.h"

#include "Cesium.h"

#include <CesiumAsync/AsyncSystem.h>
#include <CesiumAsync/IAssetResponse.h>
#include <CesiumUtility/ScopeGuard.h>
#include <CesiumUtility/StringHelpers.h>

#include <fmt/format.h>

#include <algorithm>

using namespace CesiumAsync;
using namespace CesiumUtility;

namespace {

struct RequestMetaDataLengths {
  RequestMetaDataLengths() : headerLength(0u), responseUrlLength(0u) {}

  uint32_t headerLength;
  uint32_t responseUrlLength;
};

typedef void (*OnResponseCallback)(
    void* instance,
    int statusCode,
    void* data,
    uint32_t size,
    char* error,
    int webError);
typedef void (*OnProgressCallback)(
    void* instance,
    int statusCode,
    uint32_t bytes,
    uint32_t total,
    void* data,
    uint32_t size);

extern "C" {
extern uint32_t JS_WebRequest_Create(const char* url, const char* method);
extern void JS_WebRequest_SetRequestHeader(
    uint32_t request,
    const char* header,
    const char* value);
extern void JS_WebRequest_Send(
    uint32_t request,
    void* ptr,
    uint32_t length,
    void* ref,
    OnResponseCallback onresponse,
    OnProgressCallback onprogress);
extern void JS_WebRequest_GetResponseMetaDataLengths(
    uint32_t request,
    RequestMetaDataLengths* buffer);
extern void JS_WebRequest_GetResponseMetaData(
    uint32_t request,
    char* headerBuffer,
    uint32_t headerSize,
    char* responseUrlBuffer,
    uint32_t responseUrlSize);
extern void JS_WebRequest_Abort(uint32_t request);
extern void JS_WebRequest_Release(uint32_t request);
} // extern "C"

class JSAssetResponse : public IAssetResponse {
public:
  JSAssetResponse(
      HttpHeaders&& headers,
      int statusCode,
      std::vector<std::byte>&& data)
      : _headers(std::move(headers)),
        _statusCode(statusCode),
        _data(std::move(data)) {}

  virtual uint16_t statusCode() const override { return _statusCode; }

  virtual std::string contentType() const override {
    auto find = this->_headers.find("content-type");
    if (find != this->_headers.end()) {
      return find->second;
    }
    return "";
  }

  virtual const HttpHeaders& headers() const override { return this->_headers; }

  virtual std::span<const std::byte> data() const override {
    return this->_data;
  }

private:
  HttpHeaders _headers;
  int _statusCode;
  std::vector<std::byte> _data;
};

class JSAssetRequest : public IAssetRequest {
public:
  JSAssetRequest(
      std::string&& method,
      std::string&& url,
      HttpHeaders&& headers,
      JSAssetResponse&& response)
      : _method(std::move(method)),
        _url(std::move(url)),
        _headers(std::move(headers)),
        _response(std::move(response)) {}

  virtual const std::string& method() const override { return this->_method; }
  virtual const std::string& url() const override { return this->_url; }
  virtual const HttpHeaders& headers() const override { return this->_headers; }
  virtual const IAssetResponse* response() const override {
    return &this->_response;
  }

private:
  std::string _method;
  std::string _url;
  HttpHeaders _headers;
  JSAssetResponse _response;
};

struct ResponseData {
  uint32_t request;
  std::string url;
  HttpHeaders requestHeaders;
  HttpHeaders responseHeaders;
  Promise<std::shared_ptr<CesiumAsync::IAssetRequest>> promise;
  std::vector<std::byte> data;
};

void setHeaderUnvalidated(
    std::string_view name,
    std::string_view value,
    bool replace,
    HttpHeaders& headers) {
  // Insert or replace header
  std::string nameStr(name.data(), name.size());
  HttpHeaders::iterator it = headers.find(nameStr);
  if (it != headers.end()) {
    if (replace) {
      it->second = value;
    } else {
      it->second.reserve(it->second.size() + 1 + value.size());
      it->second.append(",");
      it->second.append(value);
    }
  } else {
    headers.insert({std::move(nameStr), std::string(value)});
  }
}

void parseAndSetAllHeaders(const std::string& s, HttpHeaders& headers) {
  std::vector<std::string_view> lines =
      StringHelpers::splitOnCharacter(s, '\n');

  for (const std::string_view& line : lines) {
    size_t colonPos = line.find(':');
    if (colonPos == std::string_view::npos) {
      continue; // Malformed header line
    }

    std::string_view key = line.substr(0, colonPos);
    std::string_view value = line.substr(colonPos + 1);

    // Trim whitespace
    key = StringHelpers::trimWhitespace(key);
    value = StringHelpers::trimWhitespace(value);

    setHeaderUnvalidated(key, value, true, headers);
  }
}

void onProgress(
    void* _instance,
    int statusCode,
    uint32_t bytes,
    uint32_t total,
    void* data,
    uint32_t size) {
  if (size > 0) {
    ResponseData* pResponseData = static_cast<ResponseData*>(_instance);
    size_t endPosition = pResponseData->data.size();
    pResponseData->data.resize(pResponseData->data.size() + size);
    memcpy(pResponseData->data.data() + endPosition, data, size);
  }
}

void onResponse(
    void* _instance,
    int statusCode,
    void* data,
    uint32_t size,
    char* error,
    int webError) {
  std::unique_ptr<ResponseData> pResponseData =
      std::unique_ptr<ResponseData>(static_cast<ResponseData*>(_instance));

  RequestMetaDataLengths lengths;
  JS_WebRequest_GetResponseMetaDataLengths(pResponseData->request, &lengths);
  std::string headers((size_t)lengths.headerLength + 1, 0);
  JS_WebRequest_GetResponseMetaData(
      pResponseData->request,
      headers.data(),
      headers.size(),
      nullptr,
      0);
  headers.resize(headers.size() - 1); // strip null terminator
  parseAndSetAllHeaders(headers, pResponseData->responseHeaders);

  Promise<std::shared_ptr<IAssetRequest>>& promise = pResponseData->promise;
  if (webError == 0) {
    // Success
    promise.resolve(std::make_shared<JSAssetRequest>(
        "GET",
        std::move(pResponseData->url),
        std::move(pResponseData->requestHeaders),
        JSAssetResponse(
            std::move(pResponseData->responseHeaders),
            statusCode,
            std::move(pResponseData->data))));
  } else {
    // Error
    promise.reject(std::runtime_error(
        fmt::format("Request for `{}` failed: {}", pResponseData->url, error)));
  }
}

} // namespace

namespace CesiumForUnityNative {

UnityEmscriptenAssetAccessor::UnityEmscriptenAssetAccessor() {}

CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
UnityEmscriptenAssetAccessor::get(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const std::string& url,
    const std::vector<THeader>& headers) {

  // Sadly, Unity requires us to call this from the main thread.
  return asyncSystem.runInMainThread([asyncSystem, url, headers]() {
    auto promise =
        asyncSystem
            .createPromise<std::shared_ptr<CesiumAsync::IAssetRequest>>();

    auto future = promise.getFuture();

    uint32_t request = JS_WebRequest_Create(url.c_str(), "GET");

    for (const auto& header : headers) {
      JS_WebRequest_SetRequestHeader(
          request,
          header.first.c_str(),
          header.second.c_str());
    }

    ResponseData* responseData = new ResponseData{
        request,
        url,
        HttpHeaders(headers.begin(), headers.end()),
        {},
        std::move(promise)};
    JS_WebRequest_Send(
        request,
        nullptr,
        0,
        responseData,
        onResponse,
        onProgress);

    return future;
  });
}

CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
UnityEmscriptenAssetAccessor::request(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const std::string& verb,
    const std::string& url,
    const std::vector<THeader>& headers,
    const std::span<const std::byte>& contentPayload) {
  // Only GET requests are currently supported.
  return asyncSystem
      .createResolvedFuture<std::shared_ptr<CesiumAsync::IAssetRequest>>(
          nullptr);
}

void UnityEmscriptenAssetAccessor::tick() noexcept {}

} // namespace CesiumForUnityNative

#endif // __EMSCRIPTEN__
