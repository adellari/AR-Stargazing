Shader "Unlit/ShootingStars"
{
    Properties
    {
        _MainTex ("Mandatory Main Texture", 2D) = "white" {}
        _CubeTex ("Cube Star Texture", Cube) = "white" {}
        _Opacity("Starmap opacity", Range(0, 100)) = 1
        //_XRange("X Slider", Range(-1, 1)) = 1
        //_YRange("Y Slider", Range(-1, 1)) = 1
    }
    SubShader
    {
        Blend One One
        Tags { "RenderType"="Transparent" }
        //LOD 100

        Pass
        {
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define PI 3.14159265358979323846

            
            //#include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 screenCoord : TEXCOORD1;
            };

            float _tanFOV;
            float _aspectRatio;
            float4x4 _InverseViewMatrix;
            float4x4 _RotationMatrix;
            
            samplerCUBE _CubeTex;
            float4 _CubeTex_ST;
            float _Opacity;
            
            
            

            float hash1(float2 p)
            {
                float2 v = float2(PI*1453.0,exp(1.)*3054.0);
                return frac(sin(dot(p,v)*0.1)*4323.0);
            }

            void drawMeteor(inout fixed3 col, in fixed2 uv, fixed2 startP, fixed2 endP, fixed linWidth){
 
               //uv*=3.0;
               float2 lineDir=endP-startP;
               float2 fragDir=uv-startP;
               
               // keep the line coefficient bewteen [0,1] so that the projective dir on the 
               // lineDir will not exceed or we couldn't get a line segment but a line.
               fixed lineCoe=clamp(dot(lineDir,fragDir)/dot(lineDir,lineDir),0.,1.0);
                                   
               fixed2 projDir=lineCoe*lineDir;
                
               fixed2 fragToLineDir= fragDir- projDir;
                
               fixed dis=length(fragToLineDir);
               fixed disToTail = length(projDir);
               dis=linWidth/dis;
                 
               col += dis * fixed3(0.0, 0.8, 0.9) * pow(disToTail,3.0);
                
            }
 
            void drawMeteors(inout fixed3 col, fixed2 uv){

                fixed2 dir = normalize(float2(-1.0,-0.5));
                
                //use mod to make this occur every 3.14s
                //specifically mod pi to get 1 -> -1 region of cos
                fixed2 mv  = -dir*cos(_Time.y*2.0 % PI)*100.0;
                fixed f = floor(_Time.y*2./PI);
                fixed r = hash1(fixed2(f, f));
                fixed2 sp  = fixed2(50.0*lerp(-4, 4, r), 10.0);
                //this controls how wide the area is around the meteor
                fixed2 ep  = sp+dir*5.0;

                drawMeteor(col,uv,sp+mv,ep+mv,0.001);

                //drawMeteor(col,uv,sp+mv,ep+mv,0.0005);

            }

            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenCoord = (v.uv * 2) - 1;
                o.texcoord = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 screenRay = fixed4((i.texcoord * 2) - 1, -1, 0);
                screenRay.x *= _aspectRatio * _tanFOV;
                screenRay.y *= _tanFOV;

                fixed2 cuv = i.texcoord - fixed2(0.5, 0.5);
                cuv.x *= _aspectRatio; 

                cuv *= 100.;
                fixed3 col = fixed3(0, 0, 0);
                fixed3 worldRay = normalize(mul(_InverseViewMatrix, screenRay).xyz);
                worldRay = mul(_RotationMatrix, worldRay);
                fixed3 colStars = texCUBE(_CubeTex, float3(-worldRay.x, worldRay.y, worldRay.z)).rgb;
                col =  colStars;
                //col = tex2D(_MainTex, i.texcoord).rgb;

                drawMeteors(col, cuv);
                //return fixed4(cuv, 0, 1);
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
