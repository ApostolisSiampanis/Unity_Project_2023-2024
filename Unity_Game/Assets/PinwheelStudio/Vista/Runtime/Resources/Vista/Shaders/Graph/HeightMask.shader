Shader "Hidden/Vista/Graph/HeightMask"
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

			sampler2D _MainTex;
			float _MinHeight;
			float _MaxHeight;
			sampler2D _Transition;
			float3 _TerrainSize;

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
				float value = tex2D(_MainTex, input.localPos).r;
				float fHeightMin = _MinHeight / _TerrainSize.y;
				float fHeightMax = _MaxHeight / _TerrainSize.y;
				float fHeight = saturate(inverseLerp(value, fHeightMin, fHeightMax));
				fHeight = tex2D(_Transition, float2(fHeight, 0.5)).r;
				return fHeight;
			}
			ENDCG
		}
	}
}
