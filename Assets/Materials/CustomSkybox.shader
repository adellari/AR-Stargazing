Shader "Skybox/CustomCubemap" {
Properties {
    _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    _Rotation ("Rotation", Range(0, 360)) = 0
    [NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}
    _MainTex("Semantic Texture", 2D) = "white" {}
}

SubShader {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    Pass {

        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0

        #include "UnityCG.cginc"

        samplerCUBE _Tex;
        half4 _Tex_HDR;
        half4 _Tint;
        half _Exposure;
        float _Rotation;

        sampler2D _MainTex;
        float4x4 DisplayMatrix;
        float4 _MainTex_ST;
        

        float3 RotateAroundYInDegrees (float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }

        struct appdata_t {
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
            float3 maskUV : TEXCOORD1;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert (appdata_t v)
        {
            v2f o;

            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);


            o.vertex = UnityObjectToClipPos(rotated);
            o.texcoord = v.vertex.xyz;

            float4 clipPos = o.vertex;
            float3 screenPos = clipPos.xyz / clipPos.w;
            o.maskUV = mul(DisplayMatrix, float4(screenPos.xy, 1.0f, 1.0f)).xyz + float3(-0.3f, -0.1f, 0.f);

            float2 pivot = float2(0.5, 0.5);
            o.maskUV.xy = (o.maskUV.xy - pivot) * float2(1.8, 1.8) + pivot;

            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            half4 tex = texCUBE (_Tex, i.texcoord);
            half3 c = DecodeHDR (tex, _Tex_HDR);

            float confidence = tex2D(_MainTex, float2(i.maskUV.xy/i.maskUV.z)).r;

            confidence = step(0.03, confidence);


            c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
            c *= _Exposure;
            //half4 o = half4(c, max(i.texcoord.y, 0)) * confidence;
            half4 o = half4(c, confidence);
            return o;

        }
        ENDCG
    }
}


Fallback Off

}