Shader "Hidden/Vista/BiomeMaskCombine"
{
	Properties
	{
		_SrcBlend ("Src Blend", Int) = 1
		_DstBlend ("Dst Blend", Int) = 0
		_BlendOp ("Blend Op", Int) = 0
	}
	CGINCLUDE
	#pragma vertex vert
	#pragma fragment frag

	#include "UnityCG.cginc"

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

	sampler2D _BaseBiomeMask;
	sampler2D _BiomeMaskAdjustments;

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
		Tags { "RenderType" = "Transparent" }
		LOD 100
		ZWrite Off
		ZTest Always
		Blend [_SrcBlend] [_DstBlend]
		BlendOp [_BlendOp]
		

		Pass
		{
			Name "Write to new RT"
			CGPROGRAM

			float4 frag(v2f input): SV_Target
			{
				float adjustment = tex2D(_BiomeMaskAdjustments, input.uv).r;
				float baseMask = tex2D(_BaseBiomeMask, input.uv).r;
				float value = saturate(baseMask + adjustment);

				return float4(value, 0, 0, value);
			}
			ENDCG

		}
	}
}
