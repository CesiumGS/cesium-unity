#include "UnityAssetAccessor.h"

#include "Cesium.h"

#include <CesiumAsync/AsyncSystem.h>
#include <CesiumAsync/IAssetResponse.h>
#include <CesiumUtility/ScopeGuard.h>

#include <DotNet/CesiumForUnity/Helpers.h>
#include <DotNet/CesiumForUnity/NativeDownloadHandler.h>
#include <DotNet/System/Action1.h>
#include <DotNet/System/Collections/Generic/Dictionary2.h>
#include <DotNet/System/Collections/Generic/Enumerator0.h>
#include <DotNet/System/Collections/Generic/KeyValuePair2.h>
#include <DotNet/System/Environment.h>
#include <DotNet/System/OperatingSystem.h>
#include <DotNet/System/String.h>
#include <DotNet/Unity/Collections/Allocator.h>
#include <DotNet/Unity/Collections/LowLevel/Unsafe/NativeArrayUnsafeUtility.h>
#include <DotNet/Unity/Collections/NativeArray1.h>
#include <DotNet/Unity/Collections/NativeArrayOptions.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Networking/DownloadHandler.h>
#include <DotNet/UnityEngine/Networking/Result.h>
#include <DotNet/UnityEngine/Networking/UnityWebRequest.h>
#include <DotNet/UnityEngine/Networking/UnityWebRequestAsyncOperation.h>
#include <DotNet/UnityEngine/Networking/UploadHandler.h>
#include <DotNet/UnityEngine/Networking/UploadHandlerRaw.h>

#include <algorithm>
#include <fmt/format.h>

using namespace CesiumAsync;
using namespace CesiumUtility;
using namespace DotNet;

#if __EMSCRIPTEN__
struct RequestMetaDataLengths
{
    RequestMetaDataLengths()
        : headerLength(0u)
        , responseUrlLength(0u)
    {
    }

    uint32_t headerLength;
    uint32_t responseUrlLength;
};
typedef void (*OnResponseCallback)(void* instance, int statusCode, void* data, uint32_t size, char* error, int webError);
typedef void (*OnProgressCallback)(void* instance, int statusCode, uint32_t bytes, uint32_t total, void* data, uint32_t size);

extern "C" {
extern uint32_t JS_WebRequest_Create(const char* url, const char* method);
extern void JS_WebRequest_SetRequestHeader(uint32_t request, const char* header, const char* value);
extern void JS_WebRequest_Send(uint32_t request, void *ptr, uint32_t length, void *ref, OnResponseCallback onresponse, OnProgressCallback onprogress);
extern void JS_WebRequest_GetResponseMetaDataLengths(uint32_t request, RequestMetaDataLengths* buffer);
extern void JS_WebRequest_GetResponseMetaData(uint32_t request, char* headerBuffer, uint32_t headerSize, char* responseUrlBuffer, uint32_t responseUrlSize);
extern void JS_WebRequest_Abort(uint32_t request);
extern void JS_WebRequest_Release(uint32_t request);
} // extern "C"
#endif // __EMSCRIPTEN__

namespace {

class UnityAssetResponse : public IAssetResponse {
public:
  UnityAssetResponse(
      const UnityEngine::Networking::UnityWebRequest& request,
      const DotNet::CesiumForUnity::NativeDownloadHandler& handler)
      : _statusCode(uint16_t(request.responseCode())),
        _contentType(),
        _data(std::move(handler.NativeImplementation().getData())) {
    System::Collections::Generic::Dictionary2<System::String, System::String>
        responseHeaders = request.GetResponseHeaders();
    if (responseHeaders != nullptr) {
      System::Collections::Generic::Enumerator0 enumerator =
          responseHeaders.GetEnumerator();
      while (enumerator.MoveNext()) {
        this->_headers.emplace(
            enumerator.Current().Key().ToStlString(),
            enumerator.Current().Value().ToStlString());
      }
      auto find = this->_headers.find("content-type");
      if (find != this->_headers.end()) {
        this->_contentType = find->second;
      }
    }
  }

  virtual uint16_t statusCode() const override { return _statusCode; }

  virtual std::string contentType() const override { return _contentType; }

  virtual const HttpHeaders& headers() const override { return _headers; }

  virtual std::span<const std::byte> data() const override {
    return this->_data;
  }

private:
  uint16_t _statusCode;
  std::string _contentType;
  HttpHeaders _headers;
  std::vector<std::byte> _data;
};

class UnityAssetRequest : public IAssetRequest {
public:
  UnityAssetRequest(
      const DotNet::UnityEngine::Networking::UnityWebRequest& request,
      const HttpHeaders& headers,
      const DotNet::CesiumForUnity::NativeDownloadHandler& handler)
      : _method(request.method().ToStlString()),
        _url(request.url().ToStlString()),
        _headers(headers),
        _response(request, handler) {}

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

std::string replaceInvalidChars(const std::string& input) {
  std::string result(input.size(), '?');
  std::transform(
      input.cbegin(),
      input.cend(),
      result.begin(),
      [](unsigned char c) { return (c >= 32 && c <= 126) ? c : '?'; });
  return result;
}

#if __EMSCRIPTEN__
class JSAssetResponse : public IAssetResponse {
public:
  JSAssetResponse(const HttpHeaders& headers, int statusCode, void* data, uint32_t size)
      : _headers(headers)
      , _statusCode(statusCode)
      , _data(size) {
      memcpy(_data.data(), data, size);
  }

  virtual uint16_t statusCode() const override { return _statusCode; }

  virtual std::string contentType() const override {
    auto find = this->_headers.find("content-type");
    if (find != this->_headers.end()) {
      return find->second;
    }
    return "";
  }

  virtual const HttpHeaders& headers() const override { return _headers; }

  virtual std::span<const std::byte> data() const override { return _data; }

private:
  HttpHeaders _headers;
  int _statusCode;
  std::vector<std::byte> _data;
  std::vector<char> _headerBuffer;
  std::vector<char> _responseUrlBuffer;
};

class JSAssetRequest : public IAssetRequest {
public:
  JSAssetRequest(const std::string& method,
                 const std::string& url,
                 const HttpHeaders& headers,
                 IAssetResponse* response)
      : _method(method)
      , _url(url)
      , _headers(headers)
      , _response(response) {}

  virtual ~JSAssetRequest() {
    delete this->_response;
  }

  virtual const std::string& method() const override { return _method; }
  virtual const std::string& url() const override { return _url; }
  virtual const HttpHeaders& headers() const override { return _headers; }
  virtual const IAssetResponse* response() const override { return _response; }

private:
  std::string _method;
  std::string _url;
  HttpHeaders _headers;
  IAssetResponse* _response;
};
#endif // __EMSCRIPTEN__

struct ResponseData {
  uint32_t request;
  std::string url;
  HttpHeaders requestHeaders;
  HttpHeaders responseHeaders;
  Promise<std::shared_ptr<CesiumAsync::IAssetRequest>> promise;
  std::vector<uint8_t> data;
};

} // namespace

namespace CesiumForUnityNative {

UnityAssetAccessor::UnityAssetAccessor() : _cesiumRequestHeaders() {
  std::string version = CesiumForUnityNative::Cesium::version + " " +
                        CesiumForUnityNative::Cesium::commit;
  std::string projectName = replaceInvalidChars(
      UnityEngine::Application::productName().ToStlString());
  std::string engine =
      "Unity " + UnityEngine::Application::unityVersion().ToStlString() + " " +
      CesiumForUnity::Helpers::ToString(UnityEngine::Application::platform())
          .ToStlString();
  std::string osVersion =
      System::Environment::OSVersion().VersionString().ToStlString();

  this->_cesiumRequestHeaders.insert({"X-Cesium-Client", "Cesium For Unity"});
  this->_cesiumRequestHeaders.insert({"X-Cesium-Client-Version", version});
  this->_cesiumRequestHeaders.insert({"X-Cesium-Client-Project", projectName});
  this->_cesiumRequestHeaders.insert({"X-Cesium-Client-Engine", engine});
  this->_cesiumRequestHeaders.insert({"X-Cesium-Client-OS", osVersion});
}

static bool IsSpace(char c) {
  return c == ' ' || c == '\t' || c == '\r' || c == '\n';
}

static void SetUnvalidated(std::string_view name, std::string_view value, bool replace, HttpHeaders& headers)
{
    // Insert or replace header
    std::string nameStr(name.data(), name.size());
    HttpHeaders::iterator it = headers.find(nameStr);
    if (it != headers.end()) {
      if (replace) {
        it->second = value;
      } else {
        // According HTTP spec header with the same name can appear more than-once if and only if it's entire value is comma separated values.
        // Multiple such headers always can be combined into one. (case 791722)
        it->second.reserve(it->second.size() + 1 + value.size());
        it->second.append(",");
        it->second.append(value);
      }
    } else {
      headers.insert({std::string(name), std::string(value)});
    }
}

static void ParseAndSetAllHeaders(const char* buf, size_t length, HttpHeaders& headers) {
  while (length > 0) {
    // Find : character delimiting the key from the value
    const char* delim = buf;
    while ((delim - buf) < length && *delim != ':') {
      ++delim;

      // If we hit a new line and did not find a delimiter character, skip this line.
      if (*delim == '\r' || *delim == '\n') {
        length -= (delim  - buf);
        buf = delim;
      }
    }

    // skip leading newline characters
    while (*buf == '\r' || *buf == '\n') {
      length--;
      buf++;
    }

    if ((delim - buf) >= length)
        break;

    const char* end = delim;
    while ((end - buf) < length && *end != '\r' && *end != '\n') {
      ++end;
    }

    // If we found a delimiter, parse it into headers
    size_t keyLen = delim - buf;
    // skip ':'
    delim++;

    // Skip starting spaces in value
    while (delim < end && IsSpace(*delim)) {
      delim++;
    }

    std::string_view key(buf, keyLen);

    if (delim < end) {
      // Value was not all spaces
      std::string_view value(delim, end - delim);
      SetUnvalidated(key, value, true, headers);
    } else {
      SetUnvalidated(key, "", true, headers);
    }

    while ((end - buf) < length && (*end == '\r' || *end == '\n')) {
      ++end;
    }

    length -= (end - buf);
    buf = end;
  }
}

static void _OnProgress(void* _instance, int statusCode, uint32_t bytes, uint32_t total, void* data, uint32_t size) {
    ResponseData* responseData = static_cast<ResponseData*>(_instance);
    if (size > 0) {
      size_t endPosition = responseData->data.size();
      responseData->data.resize(responseData->data.size() + size);
      memcpy(responseData->data.data() + endPosition, data, size);
    }

    RequestMetaDataLengths lengths;
    JS_WebRequest_GetResponseMetaDataLengths(responseData->request, &lengths);
    std::string headers((size_t)lengths.headerLength + 1, 0);
    std::string responseUrl((size_t)lengths.responseUrlLength + 1, 0);
    JS_WebRequest_GetResponseMetaData(responseData->request, (char *)headers.data(), headers.size(), (char *)responseUrl.data(), responseUrl.size());
    headers.resize(headers.size() - 1); // strip null terminator
    responseUrl.resize(responseUrl.size() - 1); // strip null terminator
    ParseAndSetAllHeaders(headers.c_str(), headers.size(), responseData->responseHeaders);
}

static void _OnResponse(void* _instance, int statusCode, void* data, uint32_t size, char* error, int webError) {
    ResponseData* responseData = static_cast<ResponseData*>(_instance);
    printf("#### completed request: url:%s status:%d size:%d data:%p error:%s webError:%d\n", responseData->url.c_str(), statusCode, size, data, error, webError);
    auto& promise = responseData->promise;
    if (webError == 0) {
      // Success
      //printf("data: %.*s\n", responseData->data.size(), responseData->data.data());
      for (const auto& header : responseData->responseHeaders) {
        printf("    &&&& ResponseHeader: '%s' = '%s'\n", header.first.c_str(), header.second.c_str());
      }
      promise.resolve(std::make_shared<JSAssetRequest>("GET", responseData->url, responseData->requestHeaders,
        new JSAssetResponse(responseData->responseHeaders, statusCode, responseData->data.data(), responseData->data.size())));
    } else {
      // Error
      promise.reject(std::runtime_error(fmt::format(
          "Request for `{}` failed: {}",
          responseData->url,
          error)));
    }
    delete responseData;
}

CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
UnityAssetAccessor::get(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const std::string& url,
    const std::vector<THeader>& headers) {

  // Sadly, Unity requires us to call this from the main thread.
  return asyncSystem.runInMainThread([asyncSystem,
                                      url,
                                      headers,
                                      &cesiumRequestHeaders = this->_cesiumRequestHeaders]() {

    #if __EMSCRIPTEN__
    auto promise = asyncSystem.createPromise<std::shared_ptr<CesiumAsync::IAssetRequest>>();

    auto future = promise.getFuture();

    uint32_t request = JS_WebRequest_Create(url.c_str(), "GET");
    HttpHeaders requestHeaders = cesiumRequestHeaders;
    for (const auto& header : headers) {
      requestHeaders.insert(header);
    }
    for (const auto& header : requestHeaders) {
      JS_WebRequest_SetRequestHeader(request, header.first.c_str(), header.second.c_str());
    }

    ResponseData* responseData = new ResponseData{request, url, std::move(requestHeaders), {}, std::move(promise)};
    JS_WebRequest_Send(request, nullptr, 0, responseData, _OnResponse, _OnProgress);

    return future;
    #else
    UnityEngine::Networking::UnityWebRequest request =
        UnityEngine::Networking::UnityWebRequest::Get(System::String(url));

    DotNet::CesiumForUnity::NativeDownloadHandler handler{};
    request.downloadHandler(handler);

    HttpHeaders requestHeaders = cesiumRequestHeaders;
    for (const auto& header : headers) {
      requestHeaders.insert(header);
    }
    for (const auto& header : requestHeaders) {
      request.SetRequestHeader(
          System::String(header.first),
          System::String(header.second));
    }

    auto promise = asyncSystem.createPromise<std::shared_ptr<CesiumAsync::IAssetRequest>>();

    auto future = promise.getFuture();

    UnityEngine::Networking::UnityWebRequestAsyncOperation op =
        request.SendWebRequest();
    op.add_completed(System::Action1<UnityEngine::AsyncOperation>(
        [request,
         headers = std::move(requestHeaders),
         promise = std::move(promise),
         handler = std::move(handler)](
            const UnityEngine::AsyncOperation& operation) mutable {
          ScopeGuard disposeHandler{[&handler]() { handler.Dispose(); }};
          if (request.isDone() &&
              request.result() !=
                  UnityEngine::Networking::Result::ConnectionError) {
            promise.resolve(
                std::make_shared<UnityAssetRequest>(request, headers, handler));
          } else {
            promise.reject(std::runtime_error(fmt::format(
                "Request for `{}` failed: {}",
                request.url().ToStlString(),
                request.error().ToStlString())));
          }
        }));
    return future;
    #endif // __EMSCRIPTEN__
  });
}

CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
UnityAssetAccessor::request(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const std::string& verb,
    const std::string& url,
    const std::vector<THeader>& headers,
    const std::span<const std::byte>& contentPayload) {
  if (contentPayload.size() >
      size_t(std::numeric_limits<std::int32_t>::max())) {
    // This implementation cannot be used to send more than 2 gigabytes - just
    // fail.
    return asyncSystem
        .createResolvedFuture<std::shared_ptr<CesiumAsync::IAssetRequest>>(
            nullptr);
  }

  Unity::Collections::NativeArray1<std::uint8_t> payloadBytes(
      std::int32_t(contentPayload.size()),
      Unity::Collections::Allocator::Persistent,
      Unity::Collections::NativeArrayOptions::UninitializedMemory);
  std::byte* pDest = static_cast<std::byte*>(
      Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
          GetUnsafeBufferPointerWithoutChecks(payloadBytes));
  std::memcpy(pDest, contentPayload.data(), contentPayload.size());

  // Sadly, Unity requires us to call this from the main thread.
  return asyncSystem.runInMainThread([asyncSystem,
                                      url,
                                      verb,
                                      headers,
                                      payloadBytes,
                                      &cesiumRequestHeaders =
                                          this->_cesiumRequestHeaders]() {
    DotNet::CesiumForUnity::NativeDownloadHandler downloadHandler{};
    UnityEngine::Networking::UploadHandlerRaw uploadHandler(payloadBytes, true);
    UnityEngine::Networking::UnityWebRequest request(
        System::String(url),
        System::String(verb),
        downloadHandler,
        uploadHandler);

    HttpHeaders requestHeaders = cesiumRequestHeaders;
    for (const auto& header : headers) {
      requestHeaders.insert(header);
    }

    for (const auto& header : requestHeaders) {
      request.SetRequestHeader(
          System::String(header.first),
          System::String(header.second));
    }

    auto promise =
        asyncSystem
            .createPromise<std::shared_ptr<CesiumAsync::IAssetRequest>>();

    auto future = promise.getFuture();

    UnityEngine::Networking::UnityWebRequestAsyncOperation op =
        request.SendWebRequest();
    op.add_completed(System::Action1<UnityEngine::AsyncOperation>(
        [request,
         headers = std::move(requestHeaders),
         promise = std::move(promise),
         handler = std::move(downloadHandler)](
            const UnityEngine::AsyncOperation& operation) mutable {
          ScopeGuard disposeHandler{[&handler]() { handler.Dispose(); }};
          if (request.isDone() &&
              request.result() !=
                  UnityEngine::Networking::Result::ConnectionError) {
            promise.resolve(
                std::make_shared<UnityAssetRequest>(request, headers, handler));
          } else {
            promise.reject(std::runtime_error(fmt::format(
                "Request for `{}` failed: {}",
                request.url().ToStlString(),
                request.error().ToStlString())));
          }
        }));

    return future;
  });
}

void UnityAssetAccessor::tick() noexcept {}

} // namespace CesiumForUnityNative
