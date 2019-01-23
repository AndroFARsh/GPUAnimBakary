Shader "GPUAnimationSkinning/InstancedSurfaceShader"
{
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
    }
    SubShader { 
        Tags { "RenderType"="Opaque" }
        LOD 400
            
        CGPROGRAM
        #pragma surface surf BlinnPhong addshadow
        #pragma vertex vert 
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
        #pragma target 3.0
    
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            #include "AnimationCore.cginc"
        #endif    
        
        void setup()
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            unity_WorldToObject = unity_ObjectToWorld = TransformMatrix(unity_InstanceID);
            unity_WorldToObject._14_24_34 *= -1;
            unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
        #endif    
        }
                    
        void vert (inout appdata_full v) 
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            float4x4 animationMatrix = AnimationMatrix (v.texcoord1, v.texcoord2, unity_InstanceID);
            v.vertex = mul(animationMatrix, v.vertex);
        #endif
        }
    
        sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _Color;
        half _Shininess;
            
        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
        };
            
        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = tex.rgb * _Color.rgb;
            o.Gloss = tex.a;
            o.Alpha = tex.a * _Color.a;
            o.Specular = _Shininess;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
        }
    
        ENDCG            
    }
}