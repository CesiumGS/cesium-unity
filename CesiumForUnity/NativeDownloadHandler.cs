using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Networking;

namespace CesiumForUnity
{

    public struct RawDownloadedData
    {
        public IntPtr pointer;
        public int length;
    }

    public abstract class AbstractBaseNativeDownloadHandler : DownloadHandlerScript
    {
        public AbstractBaseNativeDownloadHandler()
         : base(new byte[16384])
        {
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

        public abstract bool ReceiveDataNative(IntPtr data, int dataLength);
    }

}
