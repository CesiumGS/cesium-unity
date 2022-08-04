using Oxidize;
using System;
using UnityEngine.Networking;

namespace CesiumForUnity
{

    public struct RawDownloadedData
    {
        public IntPtr pointer;
        public int length;
    }

    [OxidizeNativeImplementation("CesiumForUnity::NativeDownloadHandlerImpl", "NativeDownloadHandlerImpl.h")]
    public partial class NativeDownloadHandler : DownloadHandlerScript
    {
        public NativeDownloadHandler()
         : base(new byte[16384])
        {
            CreateImplementation();
        }

        protected override bool ReceiveData(byte[] data, int dataLength) {
            unsafe
            {
                fixed(byte* p = data)
                {
                    bool result = this.ReceiveDataNative((IntPtr)p, dataLength);
                    return result;
                }
            }
        }

        private partial bool ReceiveDataNative(IntPtr data, int dataLength);
    }
}
