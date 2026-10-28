Shader "Custom/CesiumForUnityStencilShadowVolume"
{
    Properties
    {        
        _Color ("Color",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        //Using double-sided stencil, configured with 2 passes
        Pass
        {
            Name "StencilDepthRenderState"
            Tags{"LightMode" = "Step1"}
            Cull Off
            Stencil
            {
                Ref 0 
                //Comp always//equal

                CompFront Always
                FailFront Keep
                ZFailFront DecrWrap
                PassFront Keep

                CompBack Always
                FailBack Keep
                ZFailBack IncrWrap
                PassBack Keep
            }
            ZTest On
            ZWrite Off
            ColorMask 0
        }
        Pass
        {
            Name "StencilColorRenderState"
            Tags{"LightMode" = "Step2"}
            Cull Off
            Stencil
            {
                Ref 0

                CompFront NotEqual
                FailFront Zero
                ZFailFront Zero
                PassFront Zero

                CompBack NotEqual
                FailBack Zero
                ZFailBack Zero
                PassBack Zero
            }
            ZTest Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            # include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            //Using To UnityProperty
            half4 _Color;

            float4 vert(float4 positionOS:POSITION): SV_POSITION
            {
                //ObjectSpace To ClipSpace
                float4 OUT;
                OUT = TransformObjectToHClip(positionOS.xyz);
                return OUT;
            }
            half4 frag(): SV_Target
            {
                //Set to Color
                //half4 color = half4(0,1,0,1);
                half4 color = _Color;
                return color;
            }
            ENDHLSL
        }
    }
}