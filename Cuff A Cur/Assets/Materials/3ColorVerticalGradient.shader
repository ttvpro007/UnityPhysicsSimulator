Shader "Custom/3 Colors Vertical Gradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_ColorTop("Top Color", Color) = (1,1,1,1)
		_ColorMid("Middle Color", Color) = (1,1,1,1)
		_ColorBot("Bottom Color", Color) = (1,1,1,1)
		_ValueMid("Middle Value", Range(0.001, 0.999)) = .5

		// Required properties (6) for UI
		// Details can be found in UI-Default.shader
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
    }
    SubShader
    {
		Tags {"Queue"="Background" "IgnoreProjector"="True"}
		LOD 100
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			fixed4 _ColorTop;
			fixed4 _ColorMid;
			fixed4 _ColorBot;
			float  _ValueMid;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 texcoord : TEXTCOORD0;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 c = lerp(_ColorBot, _ColorMid, i.texcoord.y / _ValueMid) * step(i.texcoord.y, _ValueMid);
				c += lerp(_ColorMid, _ColorTop, (i.texcoord.y - _ValueMid) / (1 - _ValueMid)) * step(_ValueMid, i.texcoord.y);
				c.a = 1;
				return c;
            }
            ENDCG
        }
    }
}
