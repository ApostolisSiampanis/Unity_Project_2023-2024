Shader "Hidden/Vista/PolarisHeightMapOutput"
{
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

	sampler2D _HeightMap;
	sampler2D _NewHeightData;
	sampler2D _NewHoleData;
	sampler2D _NewDensityData;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.localPos = v.vertex;
		return o;
	}

	inline float2 GriffinEncodeFloatRG(float v)
	{
		float2 kEncodeMul = float2(1.0, 255.0);
		float kEncodeBit = 1.0 / 255.0;
		float2 enc = kEncodeMul * v;
		enc = frac(enc);
		enc.x -= enc.y * kEncodeBit;
		return enc;
	}

	inline float GriffinDecodeFloatRG(float2 enc)
	{
		float2 kDecodeDot = float2(1.0, 1 / 255.0);
		return dot(enc, kDecodeDot);
	}
	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Name "Output Height"
			CGPROGRAM
			
			float4 frag(v2f input): SV_Target
			{
				float4 data = tex2D(_HeightMap, input.localPos);
				float h = tex2D(_NewHeightData, input.localPos).r;
				data.rg = GriffinEncodeFloatRG(clamp(h, 0, 0.99999));
				data.b = 0;
				data.a = 0;
				return data;
			}
			ENDCG

		}
		Pass
		{
			Name "Output Hole"
			CGPROGRAM
			
			float4 frag(v2f input): SV_Target
			{
				float4 data = tex2D(_HeightMap, input.localPos);
				float h = tex2D(_NewHoleData, input.localPos).r;
				data.a = saturate(h);
				return data;
			}
			ENDCG

		}
		Pass
		{
			Name "Output Mesh Density"
			CGPROGRAM
			
			float4 frag(v2f input): SV_Target
			{
				float4 data = tex2D(_HeightMap, input.localPos);
				float h = tex2D(_NewDensityData, input.localPos).r;
				data.b = saturate(h);
				return data;
			}
			ENDCG

		}
		Pass
		{
			Name "Collect Scene Height"
			CGPROGRAM
			
			float4 frag(v2f input): SV_Target
			{
				float4 data = tex2D(_HeightMap, input.uv); //uv, not localPos
				float h = GriffinDecodeFloatRG(data.rg);
				return h;
			}
			ENDCG

		}
	}
}
