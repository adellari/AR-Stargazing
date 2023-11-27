Shader "Unlit/SkyboxQuad"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Cubemap Skybox", Cube) = "white" {}
        _SemanticMask ("Semantic Segmenation Mask", 2D) = "grey" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "PreviewType"="Skybox"}
        Cull Off ZWrite Off
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD1;
            };

            samplerCUBE _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            sampler2D _SemanticMask;
            float4 _SemanticMask_ST;
            float4 _SemanticMask_TexelSize;
            
            float4x4 _InverseViewMatrix;
            float4x4 _DisplayMatrix;

            float _AspectRatio;
            float _TanFov;
            
            fixed4 Lerp(fixed4 a, fixed4 b, float t)
            {
                return a * (1 - t) + b * t;
            }

            fixed3 bilinearSample(fixed2 texcoord, fixed2 dim)
            {
                //we sample the low res texture with uvs of the higher res one + offset based on texel of the low res
                fixed3 result;
                fixed2 pix = texcoord * dim.x + 0.5;
                fixed2 fract = frac(pix);
                
                fixed2 texSize = _SemanticMask_TexelSize.xy / 2;
                
                fixed4 tl = tex2D(_SemanticMask, fixed2(texcoord) + fixed2(-texSize.x, -texSize.y));
                fixed4 tr = tex2D(_SemanticMask, fixed2(texcoord) + fixed2(texSize.x, -texSize.y));
                fixed4 bl = tex2D(_SemanticMask, fixed2(texcoord) + fixed2(-texSize.x, texSize.y));
                fixed4 br = tex2D(_SemanticMask, fixed2(texcoord) + fixed2(texSize.x, texSize.y));

                fixed4 top = Lerp(tl, tr, fract.x);
                fixed4 bot = Lerp(bl, br, fract.x);

                result = Lerp(top, bot, fract.y).xyz;
                
                return result;
            }
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.texcoord = mul(_DisplayMatrix, float4(v.texcoord, 1.0f, 1.0f)).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float2 s_uv = float2(i.texcoord.xy / i.texcoord.z);
                fixed3 confidence = bilinearSample(s_uv, _MainTex_TexelSize.zw); //pass the current uv and size of the main texture
                //fixed4 confidence = tex2D(_SemanticMask, s_uv);
                float mask = confidence.r;
                mask = step(0.1, mask);

                i.uv = i.uv * 2.0 - 1.0;
                i.uv.x *= _AspectRatio * _TanFov;
                i.uv.y *= _TanFov;
                float4 posOnCam = float4(i.uv, -1, 0);

                float3 camRayWorld = mul(_InverseViewMatrix, posOnCam);
                camRayWorld = float3(camRayWorld.x, camRayWorld.y, camRayWorld.z);
                camRayWorld = normalize(camRayWorld);
                    
                // sample the texture
                fixed4 col = fixed4(texCUBE(_MainTex, camRayWorld).rgb, 1);
                //fixed4 col = fixed4(abs(i.uv), 0, 1);
                //col = fixed4(confidence.rgb, 0.4f);
                return col;
            }
            ENDCG
        }
    }
}
