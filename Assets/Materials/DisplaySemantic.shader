Shader "Unlit/DisplaySemantic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {

            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float3 texcoord : TEXCOORD1;
                float4 position : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4x4 DisplayMatrix;

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.texcoord = mul(DisplayMatrix, float4(v.texcoord, 1.0f, 1.0f)).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float2 uv = float2(i.texcoord.x / i.texcoord.z, i.texcoord.y / i.texcoord.z);
                fixed c = tex2D(_MainTex, uv).r;
                c = saturate(c);
                float fac = step(0.03f, c);

                half hue = lerp(0.70f, -0.15f, c);
                
                half3 color = half3(hue, 1.0f, 1.0f);

                c = lerp(0.5, 1, c);
                c = fac;
                //c = min(c, 0.5);
                fixed4 col = tex2D(_MainTex, uv).rrra;
                col = fixed4(color, 0.4f);
                //fixed4 color = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
