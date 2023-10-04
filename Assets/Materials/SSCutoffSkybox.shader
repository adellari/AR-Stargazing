Shader "Skybox/SSCutoffSkybox" {
Properties {
    _Rotation ("Rotation", Range(0, 360)) = 0
    [NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}
    _SemanticTex("Semantic Texture", 2D) = "white" {}
    //_MainTex("Screen Texture", 2D) = "white" {}
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
        float _Rotation;

        //sampler2D _MainTex;
        sampler2D _SemanticTex;
        float4x4 DisplayMatrix;
        //float4 _MainTex_ST;
        float4 _SemanticTex_ST;
        

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
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 maskUV : TEXCOORD1;
        };

        v2f vert (appdata_t v)
        {
            v2f o;

            float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);


            o.vertex = UnityObjectToClipPos(rotated);
            //o.texcoord = v.vertex.xyz;

            float4 clipPos = o.vertex;
            float3 screenPos = clipPos.xyz / clipPos.w;
            o.maskUV = mul(DisplayMatrix, float4(screenPos.xy, 1.0f, 1.0f)).xyz;
            o.uv = v.texcoord;

            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            float2 ndc = i.uv * 2.0 - 1.0;

            float3 camForward = normalize(_WorldSpaceCameraPos - i.vertex.xyz);
            float3 camRight = normalize(cross(camForward, float3(0,1,0)));
            float3 camUp = cross(camRight, camForward);

            float3 rayDir = normalize(camForward + camRight * ndc.x + camUp * ndc.y);

           // float confidence = tex2D(_SemanticTex, float2(i.maskUV.xy/i.maskUV.z)).r;
            half4 tex = texCUBE(_Tex, rayDir);
            half3 c = tex.rgb;


            half4 o = half4(c, 1.0); // or any alpha computation you'd like
            return o;

        }
        ENDCG
    }
}


Fallback Off

}