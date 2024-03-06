Shader "Hidden/Vista/Graph/ErosionHelper"
{
	CGINCLUDE
	//#include "ErosionToolCommon.cginc"
	#include "../Includes/Math.hlsl"
	#include "../Includes/Sampling.hlsl"

	struct appdata
	{
		float4 vertex: POSITION;
		float2 uv: TEXCOORD0;
	};

	struct v2f
	{
		float2 uv: TEXCOORD0;
		float4 vertex: SV_POSITION;
		float3 localPos: TEXCOORD1;
	};

	float4 _Bounds;
	float2 _TextureSize;
	float _HeightScale;
	float _ErosionBoost;
	float _DepositionBoost;

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
			Name "Init World Data"
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _HeightMap;
			sampler2D _DetailHeightMap;
			float _DetailHeightScale;
			sampler2D _BedRockDepthMap;
			float _BedRockDepth;


			float4 frag(v2f i): SV_Target
			{
				float2 uv = i.localPos;
				float h = tex2D(_HeightMap, uv).r * _Bounds.y;
				float n = tex2D(_DetailHeightMap, uv).r * _DetailHeightScale;
				float height = (h + n) * _HeightScale;

				float br = saturate(tex2D(_BedRockDepthMap, uv).r) * _BedRockDepth;

				float4 color = float4(height, 0, br, 0);

				return color;
			}
			ENDCG

		}
		Pass
		{
			Name "Copy Mask"
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _WaterSourceMap;
			sampler2D _HardnessMap;

			float2 frag(v2f i): SV_Target
			{
				float waterSource = tex2D(_WaterSourceMap, i.localPos).r;
				float hardness = tex2D(_HardnessMap, i.localPos).r;
				float2 color = float2(waterSource, 1 - hardness); //water source, erosion strength

				return color;
			}
			ENDCG

		}
		Pass
		{
			Name "Output Height"
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _HeightMap;
			float4 _HeightMap_TexelSize;
			sampler2D _HeightChangeData;
			float4 _HeightChangeData_TexelSize;

			float frag(v2f i): SV_Target
			{
				float originalHeight = tex2DBicubic(_HeightMap, _HeightMap_TexelSize.zw, i.localPos).r;
				float2 heightChangeWS = tex2DBicubic(_HeightChangeData, _HeightChangeData_TexelSize.zw, i.localPos).rg * float2(_ErosionBoost, _DepositionBoost);

				float delta = -heightChangeWS.r / _Bounds.y + heightChangeWS.y / _Bounds.y;
				float h = originalHeight + delta;
				return h;
			}
			ENDCG

		}
		Pass
		{
			Name "Output Erosion"
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _HeightChangeData;
			float4 _HeightChangeData_TexelSize;

			float frag(v2f i): SV_Target
			{
				float erosionWS = tex2DBicubic(_HeightChangeData, _HeightChangeData_TexelSize.zw, i.localPos).r * _ErosionBoost;
				float h = erosionWS / _Bounds.y;
				return h;
			}
			ENDCG

		}
		Pass
		{
			Name "Output Deposition"
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _HeightChangeData;
			float4 _HeightChangeData_TexelSize;

			float frag(v2f i): SV_Target
			{
				float2 depositionWS = tex2DBicubic(_HeightChangeData, _HeightChangeData_TexelSize.zw, i.localPos).g * _DepositionBoost;
				float h = depositionWS / _Bounds.y;
				return h;
			}
			ENDCG

		}
		Pass
		{
			Name "Output Water"
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _WorldData;
			float4 _WorldData_TexelSize;

			float frag(v2f i): SV_Target
			{
				float waterWS = tex2DBicubic(_WorldData, _WorldData_TexelSize.zw, i.localPos).a;
				float w = waterWS / _Bounds.y;
				return w;
			}
			ENDCG

		}
	}
}