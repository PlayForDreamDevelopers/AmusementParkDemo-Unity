Shader "Custom/ShadowOnly"
{
    Properties
    {
        _ShadowIntensity("Shadow Intensity", Range(0, 1)) = 0.6
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline" 
        }

        Pass
        {
            Name "ShadowOnly"
            Tags{"LightMode" = "UniversalForward"}
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            //ColorMask RGB

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // 确保包含所有需要的shadow keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            float _ShadowIntensity;

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                
                // 获取主光源阴影
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                float shadowAtten = mainLight.shadowAttenuation;
                
                // 计算最终阴影值
                float shadow = (1.0 - shadowAtten) * _ShadowIntensity;
                
                return float4(0, 0, 0, shadow);
            }
            ENDHLSL
        }

        //Pass
        //{
        //    Name "ShadowCaster"
        //    Tags{"LightMode" = "ShadowCaster"}

        //    ZWrite On
        //    ZTest LEqual
        //    ColorMask 0

        //    HLSLPROGRAM
        //    #pragma vertex ShadowPassVertex
        //    #pragma fragment ShadowPassFragment

        //    // 这些是必需的shadow casting变体
        //    #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

        //    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        //    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        //    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
        //    #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
        
        //    ENDHLSL
        //}
    }
}