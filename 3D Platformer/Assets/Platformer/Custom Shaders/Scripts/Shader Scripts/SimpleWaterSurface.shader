// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Simple Water Surface"
{
    Properties
    {
        _NoiseTex("Noise Tex", 2D) = "white" {}
        _CubeMap("CubeMap", CUBE) = ""{}
        _FoamDepth("Foam Depth", Range(0, 10)) = 0.5
        _FoamColor("Foam Color", Color) = (0, 1, 1, 0)
        _WaterColor("Water Color", Color) = (0, 1, 1, 0)
        _Strength("Strength", Range(0, 10)) = 0.1
        _Frequency("Frequency", Range(0, 500)) = 100
        _PhaseMultiplier("Phase Multiplier", Range(0, 100)) = 10
    }
    SubShader
    {
        GrabPass
        {
            "_BackgroundTexture"
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

             struct appdata
             {
                 float4 vertex : POSITION;
                 float3 normal : NORMAL;
                 float2 uv : TEXCOORD0;
             };

             struct v2f
             {
                 float2 uv : TEXCOORD0;
                 float4 vertex : SV_POSITION;
                 float4 screenPos : TEXCOORD1;
                 float4 grabPos : TEXCOORD2;
                 float3 worldViewDir : TEXCOORD3;
                 float3 worldNormal : TEXCOORD4;
             };

             sampler2D _NoiseTex;
             float4 _NoiseTex_ST;
             sampler2D _BackgroundTexture;
             float _Strength;
             float _Frequency;
             float _PhaseMultiplier;
             sampler2D _CameraDepthTexture;
             float _FoamDepth;
             float4 _FoamColor;
             float4 _WaterColor;
             samplerCUBE _CubeMap;

             v2f vert(appdata v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 
                 o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                 float4 noise = tex2Dlod(_NoiseTex, float4(o.uv.xy, 0, 0) / 10);
                 o.vertex.y += (cos(noise.y * _PhaseMultiplier + _Time * _Frequency))
                     * _Strength * 10;
                 o.screenPos = ComputeScreenPos(o.vertex);
                 o.grabPos = ComputeGrabScreenPos(o.vertex);

                 // Calculate reflection vector
                 float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                 worldPos.y += (cos(noise.y * _PhaseMultiplier + _Time * _Frequency))
                     * _Strength * 5;
                 o.worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                 o.worldNormal = UnityObjectToWorldNormal(v.normal);     
                 return o;
             }

             fixed4 frag(v2f i) : SV_Target
             {
                 fixed4 noise = tex2D(_NoiseTex, i.uv / 10);
                 i.screenPos.x += cos(noise.x * _PhaseMultiplier + _Time * _Frequency) * _Strength;
                 i.screenPos.y += sin(noise.y * _PhaseMultiplier + _Time * _Frequency) * _Strength;
                 float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos);
                 float depth = LinearEyeDepth(depthSample).r;
                 depth -= i.screenPos.w;
                 if (depth > 0)
                 {
                     i.grabPos.x += cos(noise.x * _PhaseMultiplier + _Time * _Frequency) * _Strength;
                     i.grabPos.y += sin(noise.y * _PhaseMultiplier + _Time * _Frequency) * _Strength;
                 }
                 depth = _FoamDepth - depth;
                 if (depth < 0)
                 {
                     depth = 0;
                 }
                 if (depth > _FoamDepth)
                 {
                     depth = 0;
                 }
                 depth /= _FoamDepth;
                 depth = ceil(depth * 3) / 3;
                 float4 col = depth * _FoamColor;
               
                 fixed4 bgCol = tex2Dproj(_BackgroundTexture, i.grabPos);

                 float3 worldRefl = reflect(-i.worldViewDir, i.worldNormal);
                 float4 cubeCol = texCUBE(_CubeMap, worldRefl);
                 //half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, -worldRefl);
                 //// decode cubemap data into actual color
                 //half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
                 return col + bgCol * _WaterColor * cubeCol;
             }
             ENDCG
         }
    }
}
