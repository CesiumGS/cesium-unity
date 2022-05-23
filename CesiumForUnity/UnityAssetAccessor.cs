
using Cesium3DTilesSelection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CesiumForUnity
{

    public class UnityAssetAccessor : AssetAccessor
    {
        public override void Get(string url, string[] headers, SuccessCallback onSuccess, FailureCallback onFailure)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);

            if (headers != null)
            {
                for (int i = 0; i < headers.Length; i += 2)
                {
                    request.SetRequestHeader(headers[i], headers[i + 1]);
                }
            }

            UnityWebRequestAsyncOperation op = request.SendWebRequest();
            op.completed += (AsyncOperation op) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Dictionary<string, string> responseHeaders = request.GetResponseHeaders();
                    string[] flatHeaders = new string[responseHeaders.Count * 2];

                    int i = 0;
                    foreach (KeyValuePair<string, string> kvp in responseHeaders)
                    {
                        flatHeaders[i++] = kvp.Key;
                        flatHeaders[i++] = kvp.Value;
                    }

                    onSuccess((ushort)request.responseCode, request.GetResponseHeader("Content-Type"), flatHeaders, request.downloadHandler.data);
                }
                else
                {
                    onFailure("Request failed: " + request.result.ToString());
                }
            };
        }

        public override void Tick()
        {
        }
    }

}
