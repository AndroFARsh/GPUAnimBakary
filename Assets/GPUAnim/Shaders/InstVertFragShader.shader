 Shader "GPUAnimationSkinning/InstancedVertFragShader" 
 {
    Properties {
        mainTex ("Albedo (RGB)", 2D) = "red" {}
    }
    SubShader {

        Pass {
            Tags {"LightMode"="ForwardBase"}
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma multi_compile_instancing
            #pragma target 3.0

            #include "UnityStandardCore.cginc"
    #if SHADER_TARGET >= 30
            #include "AnimationCore.cginc"
    #endif
        
            sampler2D mainTex;

            struct v2f
            {
                float4 pos        : SV_POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float3 ambient    : TEXCOORD1;
                float3 diffuse    : TEXCOORD2;
                float3 color      : TEXCOORD3;
                SHADOW_COORDS(4)
            };

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
            {  
            #if SHADER_TARGET >= 30
                float4x4 transformMatrix = TransformMatrix (instanceID);
                float4x4 animationMatrix = AnimationMatrix (v.texcoord1, v.texcoord2, instanceID);
                 
                float4 posWorld = mul(transformMatrix, mul(animationMatrix, v.vertex));
            #else
                float4 posWorld = v.vertex;    
            #endif
          
                float3 color         = v.color;
                float3 worldPosition = posWorld.xyz;
                float3 worldNormal   = v.normal;

                float3 ndotl   = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
                float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
                float3 diffuse = (ndotl * _LightColor0.rgb);
                
                v2f o;
                o.pos          = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                o.uv_MainTex   = v.texcoord;
                o.ambient      = ambient;
                o.diffuse      = diffuse;
                o.color        = color;
                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed shadow = SHADOW_ATTENUATION(i);
                fixed4 albedo = tex2D(mainTex, i.uv_MainTex);
                float3 lighting = i.diffuse * shadow + i.ambient;
                fixed4 output = fixed4(albedo.rgb * i.color * lighting, albedo.w);
                UNITY_APPLY_FOG(i.fogCoord, output);
                return output;
            }

            ENDCG
        }
    }
}