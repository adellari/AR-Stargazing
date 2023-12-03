Shader "Unlit/DisplayDepth"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "white" {}
        _DepthTex ("_DepthTex", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
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
                float2 texcoord : TEXCOORD1;
                float4 vertex : SV_POSITION;

            };

            float4x4 _displayMat;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                #if !UNITY_UV_STARTS_AT_TOP
                o.uv.y = 1-o.uv.y;
                #endif

                o.texcoord = mul(float3(v.uv.xy, 1.0f), _displayMat).xy;
                //o.texcoord = v.uv;
                return o;
            }
            
            sampler2D _MainTex;
            sampler2D _DepthTex;

            fixed4 frag (v2f i) : SV_Target
            {
                float depth = tex2D(_DepthTex, i.texcoord).r;

                const float MAX_VIEW_DISP = 4.0f;
                const float scaledDisparity = 1.0f / depth;
                const float normDisparity = scaledDisparity / MAX_VIEW_DISP;

                return float4(normDisparity,normDisparity,normDisparity,0.8);
            }
            ENDCG
        }
    }
}