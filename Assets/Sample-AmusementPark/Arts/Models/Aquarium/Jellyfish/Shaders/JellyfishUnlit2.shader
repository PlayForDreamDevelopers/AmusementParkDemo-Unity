Shader "JellyfishUnlit2"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [Toggle(_ALPHA_CLIP_ON)] _AlphaClipOn ("Alpha Clip On/Off", Float) = 0
        _Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5
        [NoScaleOffset]_EmissionMap("Emissive Map",2D) = "black" {}
        [HDR]_EmissionColor("Emissive Color",Color) = (1,1,1,1)
        _Velocity("Velocity",Vector) = (0,0,0,0)
        [Toggle(_UV_ANIMATION_ON)] _UVAnimationOn ("UV Animation On/Off", Float) = 0
        [Toggle(_RIM_LIT_ON)] _RimLitOn ("Rim Lit On/Off", Float) = 0
        _RimPower("Rim Power",Float) = 5
        _RimColor("Rim Color",Color) = (1,1,1)
        _WaveSize("Wave Size",Float) = 10
        _WaveSpeed("Wave Speed",Float) = 0.5
        _WaveScale("Wave Scale",Float) = 1

        _PosY("Pos Y",Float) = 0
    }
        SubShader
        {
            Tags {"RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" "ShaderModel" = "4.5"}
            LOD 100

            HLSLINCLUDE
                
            #pragma multi_compile_instancing
            #pragma shader_feature_local_vertex _UV_ANIMATION_ON
            #pragma shader_feature_local_fragment _ALPHA_CLIP_ON
            #pragma shader_feature_local _RIM_LIT_ON
            #pragma shader_feature_local_vertex _OPTIMIZED_ANIMATION_ON

            //#pragma enable_d3d11_debug_symbols

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            //#define TEST_MODE

 
    #if UNITY_ANY_INSTANCING_ENABLED

            // StructuredBuffer<float4> _BaseColors;

            //UNITY_INSTANCING_BUFFER_START(Material0)
            //UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
            //UNITY_INSTANCING_BUFFER_END(Instance)

            float4 _BaseColorBuffers[200];
            float4 _VelocityBuffers[200];
            float4  _AnimSpeedBuffers[200];

            //#define _BaseColor  UNITY_ACCESS_INSTANCED_PROP(Instance, _BaseColor)
            #define _BaseColorX  _BaseColorBuffers[unity_InstanceID]
            #define _Velocity  _VelocityBuffers[unity_InstanceID]
            #define _AnimSpeed _AnimSpeedBuffers[unity_InstanceID]

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _EmissionColor;
                half _Cutoff;
                half _WaveSize;
                half _WaveSpeed;
                half _RimPower;
                half3 _RimColor;
                half  _WaveScale;
                half  _PosY;
            CBUFFER_END
    #else

            #define _BaseColorX (1.0).xxxx
            #define _AnimSpeed (1.0).xxxx
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _EmissionColor;
                half _Cutoff;
                float4 _Velocity;
                half _WaveSize;
                half _WaveSpeed;
                half _RimPower;
                half3 _RimColor;
                half  _WaveScale;
                half  _PosY;
            CBUFFER_END
    #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
#if _RIM_LIT_ON
               float3 normalOS : NORMAL; 
#endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
            #ifdef TEST_MODE    
                float3 positionOS : TEXCOORD3;
            #endif
                float4 positionCS : SV_POSITION;
#if _RIM_LIT_ON
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
#endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
          

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_TexelSize;
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);
            
            Varyings Vert (Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionOS = input.positionOS.xyz;
                float3 directionOS = float3(positionOS.x, 0.0, positionOS.z) * 0.3;
#ifdef TEST_MODE
                output.positionOS = positionOS;
#endif
                float delta = _Time.y * _WaveSpeed + positionOS.y * _WaveSize ;
                delta *= max(_AnimSpeed.x,1.0);

    #if UNITY_ANY_INSTANCING_ENABLED
                delta += unity_InstanceID * 5;
    #endif
                
                float3 vel = TransformWorldToObjectDir(_Velocity.xyz, false) * 0.3;

#if _OPTIMIZED_ANIMATION_ON
                if(_WaveScale > 0)
                {

                    float y = positionOS.y - _PosY;
                    float2 w = cos(float2(pow(_AnimSpeed.z, 5),1)*(_Time.y * 2 * _AnimSpeed.y + float2(0, y * 10)));
                    positionOS.y = _PosY + y * (w.x * 0.2 + 0.8);
                    positionOS.xz = positionOS.xz + ((positionOS.xz) * 2)  * saturate(-y) * (w.y * 0.7 + 0.3) * _AnimSpeed.z;
                }
                
                positionOS = positionOS + (smoothstep(0.0, 1.0, abs(frac(delta) * 2.0 - 1.0)) - 0.5) * (directionOS - vel);

#else
                positionOS = positionOS + (smoothstep(0.0, 1.0, abs(frac(delta) * 2.0 - 1.0)) - 0.5) * (directionOS - vel);
                
                if(_WaveScale > 0)
                {
                    positionOS.y = 0.175 + (positionOS.y - 0.175) * lerp(1.0,(cos(_Time.y * 2 * _AnimSpeed.y) * 0.2 + 0.8),_AnimSpeed.z);
                }

#endif

#ifdef TEST_MODE
                positionOS = output.positionOS;
#endif
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);

                output.positionCS = vertexInput.positionCS;
                
    #if _RIM_LIT_ON
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS); 
    #endif

    #if _UV_ANIMATION_ON
                output.uv.xy = input.uv.xy * _BaseMap_ST.xy + float2(_BaseMap_ST.x,frac(_Time.y * _BaseMap_ST.w));
    #else
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    #endif
                
                return output;
            }

            half3 Pal( half t, half3 a, half3 b, half3 c, half3 d )
            {
                return a + b * cos( 6.28318*(c*t+d) );
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
#ifdef TEST_MODE
                if(_WaveScale < 1) discard;

                return abs(input.positionOS.y - _PosY) < 0.001 ? half4(1,0,0,1):half4(0,0,0,1);
#endif
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor * _BaseColorX;
    #if _ALPHA_CLIP_ON
                clip(col.a - _Cutoff);
    #endif
                half4 em = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv) * _EmissionColor;

    #if _RIM_LIT_ON
                half ndv = dot(NormalizeNormalPerPixel(input.normalWS),GetWorldSpaceNormalizeViewDir(input.positionWS));
                half absNdv = abs(ndv);
                half3 rim = pow(saturate(1.0 - absNdv), _RimPower) * _RimColor;
                
                half3 palCol = Pal(ndv * 2, half3(0.5,0.5,0.5), half3(0.5,0.5,0.5), half3(1.0,1.0,1.0), half3(0.0,0.33,0.67));
                em.rgb += rim + 0.4 * palCol * (1.0 - absNdv); 
    #endif

                return half4(col.rgb + em.rgb, col.a);
            }
            ENDHLSL

            Pass
            {
                Name "Unlit"
                
                Blend One Zero
                ZWrite On
                Cull Back

                HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
                ENDHLSL
            }

            Pass
            {
                Name "UnlitTransparent"
                
                Blend One OneMinusSrcAlpha
                ZWrite Off
                Cull Off

                HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
                ENDHLSL
            }
        }
}
