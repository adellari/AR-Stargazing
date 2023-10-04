Shader "Unlit/MapBlending"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OverlayTex  ("Overlay", 2D) = "white" {}
        _OverlayCoeff ("Overaly Coefficient", Range(0.0, 1.0)) = 1.0
        _MaskTex ("Segmentation Mask", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _OverlayTex;
            float4 _Overlay_ST;

            sampler2D _MaskTex;
            float4 _MaskTex_ST;

            float _OverlayCoeff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * (1- _OverlayCoeff) + tex2D(_OverlayTex, float2(1 - i.uv.x, i.uv.y)) * (_OverlayCoeff);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
