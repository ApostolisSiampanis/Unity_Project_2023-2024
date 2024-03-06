Shader "Hidden/Vista/AlphaMapsCombine"
{
	CGINCLUDE
	#pragma vertex vert
	#pragma fragment frag

	#include "UnityCG.cginc"
	#include "./Includes/Math.hlsl"

	struct appdata
	{
		float4 vertex: POSITION;
		float2 uv: TEXCOORD0;
	};

	struct v2f
	{
		float2 uv: TEXCOORD0;
		float4 vertex: SV_POSITION;
		float4 localPos: TEXCOORD1;
	};


	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.localPos = v.vertex;
		return o;
	}
	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Name "Combine"

			CGPROGRAM			
			sampler2D _Layer0;
			sampler2D _Layer1;
			sampler2D _Layer2;
			sampler2D _Layer3;

			float4 frag(v2f input): SV_Target
			{
				float r = tex2D(_Layer0, input.localPos).r;
				float g = tex2D(_Layer1, input.localPos).r;
				float b = tex2D(_Layer2, input.localPos).r;
				float a = tex2D(_Layer3, input.localPos).r;
				return float4(r, g, b, a);
			}
			ENDCG
		}
		Pass
		{
			Name "Combine & Merge"
			Blend One One
			BlendOp Add		

			CGPROGRAM	
			sampler2D _LayerWeight;
			float4 _ChannelMask;

			float4 frag(v2f input): SV_Target
			{
				float value = tex2D(_LayerWeight, input.localPos).r;
				return value * _ChannelMask;
			}
			ENDCG
		}
	}
}
