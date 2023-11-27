Shader "Hidden/Vista/Graph/Terrace"
{
	CGINCLUDE
	#pragma vertex vert
	#pragma fragment frag
	#pragma shader_feature_local HEIGHT_BLEND
	#pragma shader_feature_local SLOPE_BLEND
	#pragma shader_feature_local MASK_BLEND
	#include "UnityCG.cginc"
	#include "../Includes/Math.hlsl"
	#include "../Includes/Geometry.hlsl"

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
	float _StepHeight;
	float3 _TerrainSize;
	sampler2D _RemapTex;

	#if HEIGHT_BLEND
		float _MinHeight;
		float _MaxHeight;
		sampler2D _HeightBlendTexture;
	#endif
	
	#if SLOPE_BLEND
		float _MinSlope;
		float _MaxSlope;
		sampler2D _SlopeBlendTexture;
	#endif

	#if MASK_BLEND
		sampler2D _MaskTexture;
	#endif

	int _OutlinePosition;
	float _OutlineTolerance;

	#define OUTLINE_START 0
	#define OUTLINE_END 1

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.localPos = v.vertex;
		return o;
	}

	float GetMask(float2 uv)
	{
		float value = tex2D(_MainTex, uv).r;
		float floorValue = floor(value / _StepHeight) * _StepHeight;
		float ceilValue = ceil(value / _StepHeight) * _StepHeight;
		float fValue = (value - floorValue) / (ceilValue - floorValue);
		float maskValue = 1;

		float blendHeightValue = 1;
		#if HEIGHT_BLEND
			float fHeightMin = _MinHeight / _TerrainSize.y;
			float fHeightMax = _MaxHeight / _TerrainSize.y;
			float fHeight = saturate(inverseLerp(value, fHeightMin, fHeightMax));
			fHeight = (value >= fHeightMin) * (value <= fHeightMax) * tex2D(_HeightBlendTexture, float2(fHeight, 0.5)).r;
			blendHeightValue = fHeight;
		#endif

		float blendSlopeValue = 1;
		#if SLOPE_BLEND
			float3 normal = normalFromHeightMap(_MainTex, _MainTex_TexelSize, _TerrainSize, uv);
			float cosine = abs(normal.y);
			float slopeAngle = acos(cosine);
			float slopeTransitionFactor = (slopeAngle - _MinSlope) / (_MaxSlope - _MinSlope);
			float slopeTransition = tex2D(_SlopeBlendTexture, float2(slopeTransitionFactor, 0.5f)).r;
			blendSlopeValue = (slopeAngle >= _MinSlope) * (slopeAngle <= _MaxSlope) * slopeTransition;
		#endif

		float maskBlendValue = 1;
		#if MASK_BLEND
			maskBlendValue = saturate(tex2D(_MaskTexture, uv).r);
		#endif

		maskValue = blendHeightValue * blendSlopeValue * maskBlendValue;
		return maskValue;
	}
	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		ColorMask R

		Pass
		{
			Name "Height"
			CGPROGRAM

			float frag(v2f input): SV_Target
			{
				float value = tex2D(_MainTex, input.localPos).r;
				float floorValue = floor(value / _StepHeight) * _StepHeight;
				float ceilValue = ceil(value / _StepHeight) * _StepHeight;
				float fValue = (value - floorValue) / (ceilValue - floorValue);
				float fRemap = tex2D(_RemapTex, float2(fValue, 0.5)).r;
				float newValue = lerp(floorValue, ceilValue, fRemap);

				float maskValue = GetMask(input.localPos);
				float result = lerp(value, newValue, maskValue);
				return result;
			}
			ENDCG

		}
		
		Pass
		{
			Name "Mask"
			CGPROGRAM

			float frag(v2f input): SV_Target
			{
				float maskValue = GetMask(input.localPos);
				return maskValue;
			}
			ENDCG

		}

		Pass
		{
			Name "Outline"
			CGPROGRAM

			float frag(v2f input): SV_Target
			{
				float value = tex2D(_MainTex, input.localPos).r;
				float floorValue = floor(value / _StepHeight) * _StepHeight;
				float ceilValue = ceil(value / _StepHeight) * _StepHeight;
				float fValue = (value - floorValue) / (ceilValue - floorValue);
				float maskValue = GetMask(input.localPos);

				float isOutline = (fValue <= _OutlineTolerance) * (_OutlinePosition == OUTLINE_START) + (fValue >= (1 - _OutlineTolerance)) * (_OutlinePosition == OUTLINE_END);
				float result = isOutline * maskValue;
				return result;
			}
			ENDCG

		}
	}
}
