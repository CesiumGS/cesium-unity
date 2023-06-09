using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace CesiumForUnity
{
    public sealed class CesiumTextureUtility : MonoBehaviour
    {
        private CesiumTextureUtility() { }

        private static CesiumTextureUtility _instance = null;

        public static CesiumTextureUtility Instance
        {
            get
            {
                if (_instance == null) _instance = new CesiumTextureUtility();
                return _instance;
            }
        }

        private bool _initialized = false;

        public bool ETC1_RGB { get; set; } = false;

        public bool ETC2_RGBA { get; set; } = false;

        public bool BC1_RGB { get; set; } = false;

        public bool BC3_RGBA { get; set; } = false;

        public bool BC4_R { get; set; } = false;

        public bool BC5_RG { get; set; } = false;

        public bool BC7_RGBA { get; set; } = false;

        public bool PVRTC1_4_RGB { get; set; } = false;

        public bool PVRTC1_4_RGBA { get; set; } = false;

        public bool ASTC_4x4_RGBA { get; set; } = false;

        public bool PVRTC2_4_RGB { get; set; } = false;

        public bool PVRTC2_4_RGBA { get; set; } = false;

        public bool ETC2_EAC_R11 { get; set; } = false;

        public bool ETC2_EAC_RG11 { get; set; } = false;

        public void CheckSupportedGpuCompressedPixelFormats()
        {
            if (_initialized)
                return;

            ETC1_RGB = SystemInfo.IsFormatSupported(GraphicsFormat.RGB_ETC_UNorm, FormatUsage.Sample);
            ETC2_RGBA = SystemInfo.IsFormatSupported(GraphicsFormat.RGBA_ETC2_SRGB, FormatUsage.Sample);
            BC1_RGB = SystemInfo.IsFormatSupported(GraphicsFormat.RGBA_DXT1_SRGB, FormatUsage.Sample);
            BC3_RGBA = SystemInfo.IsFormatSupported(GraphicsFormat.RGBA_DXT5_SRGB, FormatUsage.Sample);
            BC4_R = SystemInfo.IsFormatSupported(GraphicsFormat.R_BC4_SNorm, FormatUsage.Sample);
            BC5_RG = SystemInfo.IsFormatSupported(GraphicsFormat.RG_BC5_SNorm, FormatUsage.Sample);
            BC7_RGBA = SystemInfo.IsFormatSupported(GraphicsFormat.RGBA_BC7_SRGB, FormatUsage.Sample);
            ASTC_4x4_RGBA = SystemInfo.IsFormatSupported(GraphicsFormat.RGBA_ASTC4X4_SRGB, FormatUsage.Sample);

            //not ready in TextureLoader::loadTexture() (path: native~/Runtime/src/TextureLoader.cpp)
            PVRTC1_4_RGB = SystemInfo.IsFormatSupported(GraphicsFormat.RGB_PVRTC_4Bpp_SRGB, FormatUsage.Sample);
            PVRTC1_4_RGBA = SystemInfo.IsFormatSupported(GraphicsFormat.RGBA_PVRTC_4Bpp_SRGB, FormatUsage.Sample);

            //these formats don't exist in UnityEngine.Experimental.Rendering.GraphicsFormat enum
            //PVRTC2_4_RGB = false;
            //PVRTC2_4_RGBA = false;

            ETC2_EAC_R11 = SystemInfo.IsFormatSupported(GraphicsFormat.R_EAC_UNorm, FormatUsage.Sample);
            ETC2_EAC_RG11 = SystemInfo.IsFormatSupported(GraphicsFormat.RG_EAC_UNorm, FormatUsage.Sample);

            _initialized = true;
        }
    }
}
