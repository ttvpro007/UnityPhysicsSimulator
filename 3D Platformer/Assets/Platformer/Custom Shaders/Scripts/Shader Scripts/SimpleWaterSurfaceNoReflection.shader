Shader "Unlit/Simple Water Surface (No Reflection)"
{
    Properties
    {
		_NoiseTex("Noise Texture", 2D) = "white" {}
		_FoamDepth ("Foam Depth", Range(0, 10)) = 0.5
		_FoamColor ("Foam Color", Color) = (0,1,1,0)
		_WaterColor ("Water Color", Color) = (0,1,1,0)
		_WaveTurbulence ("Wave Turbulence", Float) = 1
		_DepthBand ("Depth Band", Range(1, 256)) = 1
		_NoiseScale("Noise Scale", Float) = 1
		_Amplitude("Amplitude", Range(-1, 1)) = 0
		_Frequency("Frequency", Float) = 0
		_PhaseMultiplier("Phase Multiplier", Float) = 0
    }
    SubShader
    {
		Tags { "Queue" = "Transparent" }
		// Grab the screen behind the object into _BackgroundTexture
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
				float4 grabPos : TEXCOORD2;
            };

            sampler2D _CameraDepthTexture;
			sampler2D _BackgroundTexture;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
			float _NoiseScale;
			float _FoamDepth;
			float4 _FoamColor;
			float4 _WaterColor;
			float _WaveTurbulence;
			float _DepthBand;
			float _Amplitude;
			float _Frequency;
			float _PhaseMultiplier;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.screenPos = ComputeScreenPos(o.vertex);
				o.grabPos = ComputeGrabScreenPos(o.vertex);
				float4 noise = tex2Dlod(_NoiseTex, float4(o.uv.xy, 0, 0) / _NoiseScale);
				o.vertex.y += (1 + 0.5 * cos(noise.x * _PhaseMultiplier + _Time * _Frequency)) * _Amplitude * _WaveTurbulence;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos);
                float depth = LinearEyeDepth(depthSample).r;
				depth -= i.screenPos.w;
				depth = _FoamDepth - depth;
				depth = depth < 0 ? 0 : depth;
				depth /= _FoamDepth;
				depth = ceil(depth * _DepthBand) / _DepthBand;
				fixed4 col = depth * _FoamColor;

				fixed4 noise = tex2D(_NoiseTex, i.uv / _NoiseScale);
				// cos || sin (noise.x || noise.y * phase multiplier + frequency) * amplitude
				i.grabPos.x += sin(noise.x * _PhaseMultiplier + _Time * _Frequency) * _Amplitude;
				i.grabPos.y += cos(noise.y * _PhaseMultiplier + _Time * _Frequency) * _Amplitude;
				fixed4 bgCol = tex2Dproj(_BackgroundTexture, i.grabPos) * _WaterColor;
                return col + bgCol;
            }
            ENDCG
        }
    }
}
