Shader "Hidden/Vista/Graph/Math"
{
	CGINCLUDE
	#pragma vertex vert
	#pragma fragment frag

	#include "UnityCG.cginc"
	#include "../Includes/Math.hlsl"

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

	sampler2D _MainTex;
	float4 _Configs[4];

	#define ENABLE 1
	#define DISABLE 0
	#define ADD 0
	#define MUL 1
	#define POW 2
	#define ABS 3
	#define ONE_MINUS 4
	#define SIN 5
	#define COS 6

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
		ColorMask R

		Pass
		{
			Name "Heavy"
			CGPROGRAM

			float calculateValue(float value, int i)
			{
				float4 c = _Configs[i];
				float v = (value + c.y) * (c.z == ADD) + (value * c.y) * (c.z == MUL) + pow(value, c.y) * (c.z == POW) + abs(value) * (c.z == ABS) + (1 - value) * (c.z == ONE_MINUS) + sin(value * UNITY_TWO_PI) * (c.z == SIN) + cos(value * UNITY_TWO_PI) * (c.z == COS);
				v = value * (c.x == DISABLE) + v * (c.x == ENABLE);
				return v;
			}

			float frag(v2f input): SV_Target
			{
				float value = tex2D(_MainTex, input.localPos).r;
				for (int i = 0; i < 4; ++i)
				{
					value = calculateValue(value, i);
				}
				return value;
			}
			ENDCG

		}
		
		Pass
		{
			Name "Simple"
			CGPROGRAM

			float calculateValue(float value, int i)
			{
				float4 c = _Configs[i];
				float v = (value + c.y) * (c.z == ADD) + (value * c.y) * (c.z == MUL) + abs(value) * (c.z == ABS) + (1 - value) * (c.z == ONE_MINUS);
				v = value * (c.x == DISABLE) + v * (c.x == ENABLE);
				return v;
			}

			float frag(v2f input): SV_Target
			{
				float value = tex2D(_MainTex, input.localPos).r;
				for (int i = 0; i < 4; ++i)
				{
					value = calculateValue(value, i);
				}
				return value;
			}
			ENDCG

		}
	}
}
