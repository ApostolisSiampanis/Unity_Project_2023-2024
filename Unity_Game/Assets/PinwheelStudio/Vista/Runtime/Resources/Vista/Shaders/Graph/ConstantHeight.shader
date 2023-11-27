Shader "Hidden/Vista/Graph/ConstantHeight"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		ColorMask R

		Pass
		{
			CGPROGRAM

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

			float _Value;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.localPos = v.vertex;
				return o;
			}

			float frag(v2f input): SV_Target
			{
				return _Value;
			}
			ENDCG

		}
	}
}
