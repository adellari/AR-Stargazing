Shader "Unlit/Wireframe Dome"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Frequency ("Frequency", Range(0, 200)) = 10
        _Opacity ("Line Opacity", Range(0, 1)) = 0.2
        _LineWidth ("Line Width", Range(0, 1)) = 0.02
    }
    SubShader
    {
        Blend One One
        Tags { "RenderQueue"= "Transparent" "RenderType"="Transaprent" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _tanFOV;
            float _aspectRatio;
            float4x4 InverseViewMatrix;
            float _Frequency;
            float _Opacity;
            float _LineWidth;
            
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenUV = (i.uv * 2) - 1;
                screenUV.x *= _aspectRatio * _tanFOV;
                screenUV.y *= _tanFOV;

                float3 camRayWorld = normalize(mul(InverseViewMatrix, float4(screenUV, -1.,  0.)).xyz);
                float theta = acos(camRayWorld.y);
                float phi = atan2(camRayWorld.z, camRayWorld.x);

                float Line = 0;
                
                // sample the texture
                //fixed4 col = Line > 0.5 ? tex2D(_MainTex, i.uv) : float4(0, 1, 0, 1);
                Line = smoothstep(1-_LineWidth, 1, max(cos(phi * _Frequency), sin(theta * _Frequency)));
                fixed4 col = float4(Line, Line, Line, 1) * _Opacity;
                return col;
            }
            ENDCG
        }
    }
}
