Shader "Hidden/Vista/UnityHeightMapOutput"
{
	CGINCLUDE
	#pragma vertex vert
	#pragma fragment frag

	#include "UnityCG.cginc"
	#include "./Includes/Sampling.hlsl"
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

	sampler2D _MainTex; 
	float4 _MainTex_TexelSize;

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
		//Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Name "To Unity Height Map"
			CGPROGRAM

			float4 frag(v2f input): SV_Target
			{
				float value = tex2D(_MainTex, input.localPos.xy).r;
				if (value >= 0)
				{
					value = value * 0.5;
				}
				else
				{
					float f = 1 - value;
					value = lerp(0.5, 1, f);
				}
				return PackHeightmap(value);
			}
			ENDCG
		}

		Pass
		{
			Name "Collect Scene Height"

			CGPROGRAM

			float frag(v2f input): SV_Target
			{
				float4 value = tex2D(_MainTex, input.uv.xy); //uv, not localPos
				float height = UnpackHeightmap(value);
				if (height < 0.5)
				{
					height = height * 2;
				}
				else
				{
					float f = height - 0.5;
					height = -height*2;
				}
				return height;
			}
			ENDCG

		}
	}
}
