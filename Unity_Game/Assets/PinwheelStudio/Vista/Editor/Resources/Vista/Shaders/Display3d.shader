Shader "Hidden/Vista/TerrainGraphEditor/TerrainVisualize"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		Cull Off
		Offset -1, -1

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma shader_feature_local MASK_IS_COLOR

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv: TEXCOORD0;
				float4 vertex: SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _HeightMap;
			float4 _HeightMap_TexelSize;
			sampler2D _MaskMap;
			sampler2D _GradientMap;

			#if RENDER_SPLATS
				sampler2D _Splat0;
				sampler2D _Weight0;
				sampler2D _Splat1;
				sampler2D _Weight1;
				sampler2D _Splat2;
				sampler2D _Weight2;
				sampler2D _Splat3;
				sampler2D _Weight3;
			#endif

			float3 _TerrainSize;
			
			UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _UvRemap)
            UNITY_INSTANCING_BUFFER_END(Props)

			float3 normalFromHeightMap(sampler2D heightMap, float4 heightMap_texelSize, float3 terrainSize, float2 uv)
			{
				float inverseSqrt2 = 1.0 / sqrt(2.0);
				float2 texel = heightMap_texelSize.xy;
				float2 uvLeft = uv - float2(texel.x, 0);
				float2 uvUp = uv + float2(0, texel.y);
				float2 uvRight = uv + float2(texel.x, 0);
				float2 uvDown = uv - float2(0, texel.y);
				float2 uvCenter = uv;
				float2 uvLeftUp = uv + float2(-texel.x, texel.y) * inverseSqrt2;
				float2 uvUpRight = uv + float2(texel.x, texel.y) * inverseSqrt2;
				float2 uvRightDown = uv + float2(texel.x, -texel.y) * inverseSqrt2;
				float2 uvDownLeft = uv + float2(-texel.x, -texel.y) * inverseSqrt2;

				float leftHeight = clamp(tex2Dlod(heightMap, float4(uvLeft, 0, 0)).r, -1, 1) * terrainSize.y;
				float upHeight = clamp(tex2Dlod(heightMap, float4(uvUp, 0, 0)).r, -1, 1) * terrainSize.y;
				float rightHeight = clamp(tex2Dlod(heightMap, float4(uvRight, 0, 0)).r, -1, 1) * terrainSize.y;
				float downHeight = clamp(tex2Dlod(heightMap, float4(uvDown, 0, 0)).r, -1, 1) * terrainSize.y;
				float centerHeight = clamp(tex2Dlod(heightMap, float4(uvCenter, 0, 0)).r, -1, 1) * terrainSize.y;
				float leftUpHeight = clamp(tex2Dlod(heightMap, float4(uvLeftUp, 0, 0)).r, -1, 1) * terrainSize.y;
				float upRightHeight = clamp(tex2Dlod(heightMap, float4(uvUpRight, 0, 0)).r, -1, 1) * terrainSize.y;
				float rightDownHeight = clamp(tex2Dlod(heightMap, float4(uvRightDown, 0, 0)).r, -1, 1) * terrainSize.y;
				float downLeftHeight = clamp(tex2Dlod(heightMap, float4(uvDownLeft, 0, 0)).r, -1, 1) * terrainSize.y;

				float3 left = float3(uvLeft.x * terrainSize.x, leftHeight, uvLeft.y * terrainSize.z);
				float3 up = float3(uvUp.x * terrainSize.x, leftHeight, uvUp.y * terrainSize.z);
				float3 right = float3(uvRight.x * terrainSize.x, leftHeight, uvRight.y * terrainSize.z);
				float3 down = float3(uvDown.x * terrainSize.x, leftHeight, uvDown.y * terrainSize.z);
				float3 center = float3(uvCenter.x * terrainSize.x, centerHeight, uvCenter.y * terrainSize.z);
				float3 leftUp = float3(uvLeftUp.x * terrainSize.x, leftUpHeight, uvLeftUp.y * terrainSize.z);
				float3 upRight = float3(uvUpRight.x * terrainSize.x, upRightHeight, uvUpRight.y * terrainSize.z);
				float3 rightDown = float3(uvRightDown.x * terrainSize.x, rightDownHeight, uvRightDown.y * terrainSize.z);
				float3 downLeft = float3(uvDownLeft.x * terrainSize.x, downLeftHeight, uvDownLeft.y * terrainSize.z);

				float3 n0 = cross(left - center, leftUp - center);
				float3 n1 = cross(up - center, upRight - center);
				float3 n2 = cross(right - center, rightDown - center);
				float3 n3 = cross(down - center, downLeft - center);

				float3 n4 = cross(leftUp - center, up - center);
				float3 n5 = cross(upRight - center, right - center);
				float3 n6 = cross(rightDown - center, down - center);
				float3 n7 = cross(downLeft - center, left - center);

				float3 nc = (n0 + n1 + n2 + n3 + n4 + n5 + n6 + n7) / 8;

				float3 n = float3(nc.x, nc.y, nc.z);
				float3 normal = normalize(n);

				return normal;
			}

			v2f vert(appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				float4 uvRemap = UNITY_ACCESS_INSTANCED_PROP(Props, _UvRemap);
				float2 uvStart = uvRemap.xy;
				float2 uvEnd = uvRemap.xy + uvRemap.zw;
				float2 uv = float2(lerp(uvStart.x, uvEnd.x, v.uv.x), lerp(uvStart.y, uvEnd.y, v.uv.y));
				v.uv = uv;
				float h = tex2Dlod(_HeightMap, float4(v.uv, 0, 0)).r;
				v.vertex.y = clamp(h, -1, 1);

				v2f o;
                UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				return o;
			}

			float3 calculateDiffuseLighting(float3 normalWS, float3 lightDir, float3 lightColor, float lightIntensity)
			{
				float nDotL = dot(normalWS, -lightDir);
				float atten = max(0.15, nDotL);
				float3 light = atten * lightColor * lightIntensity;
				return light;
			}

			fixed4 frag(v2f i): SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 normalWS = UnityObjectToWorldNormal(normalFromHeightMap(_HeightMap, _HeightMap_TexelSize, _TerrainSize, i.uv));
				float3 diffuseLighting = calculateDiffuseLighting(normalWS, -float3(1, 1, -1), float3(1, 1, 1), 0.7) + calculateDiffuseLighting(normalWS, -float3(-1, 1, 1), float3(0.85, 0.85, 1), 0.1);

				float4 albedo = tex2D(_MaskMap, i.uv);
				#if !MASK_IS_COLOR
					float f = albedo.r;
					albedo = tex2D(_GradientMap, f.xx);
				#endif

				float3 color = albedo * diffuseLighting * 0.9;

				return float4(color, 1);
			}
			ENDCG

		}
	}
}
