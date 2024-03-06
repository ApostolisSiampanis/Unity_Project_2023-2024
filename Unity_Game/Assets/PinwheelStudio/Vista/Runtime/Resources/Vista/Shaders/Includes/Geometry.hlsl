#ifndef GEOMETRY_INCLUDED
	#define GEOMETRY_INCLUDED

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

		float leftHeight = saturate(tex2Dlod(heightMap, float4(uvLeft, 0, 0)).r) * terrainSize.y;
		float upHeight = saturate(tex2Dlod(heightMap, float4(uvUp, 0, 0)).r) * terrainSize.y;
		float rightHeight = saturate(tex2Dlod(heightMap, float4(uvRight, 0, 0)).r) * terrainSize.y;
		float downHeight = saturate(tex2Dlod(heightMap, float4(uvDown, 0, 0)).r) * terrainSize.y;
		float centerHeight = saturate(tex2Dlod(heightMap, float4(uvCenter, 0, 0)).r) * terrainSize.y;
		float leftUpHeight = saturate(tex2Dlod(heightMap, float4(uvLeftUp, 0, 0)).r) * terrainSize.y;
		float upRightHeight = saturate(tex2Dlod(heightMap, float4(uvUpRight, 0, 0)).r) * terrainSize.y;
		float rightDownHeight = saturate(tex2Dlod(heightMap, float4(uvRightDown, 0, 0)).r) * terrainSize.y;
		float downLeftHeight = saturate(tex2Dlod(heightMap, float4(uvDownLeft, 0, 0)).r) * terrainSize.y;

		float3 left = float3(uvLeft.x * terrainSize.x, leftHeight, uvLeft.y * terrainSize.z);
		float3 up = float3(uvUp.x * terrainSize.x, upHeight, uvUp.y * terrainSize.z);
		float3 right = float3(uvRight.x * terrainSize.x, rightHeight, uvRight.y * terrainSize.z);
		float3 down = float3(uvDown.x * terrainSize.x, downHeight, uvDown.y * terrainSize.z);
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

	float3 normalFromHeightMap(Texture2D heightMap, float4 heightMap_texelSize, SamplerState heightMapSampler, float3 terrainSize, float2 uv)
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
		
		float leftHeight = saturate(heightMap.SampleLevel(heightMapSampler, uvLeft, 0).r) * terrainSize.y;
		float upHeight = saturate(heightMap.SampleLevel(heightMapSampler, uvUp, 0).r) * terrainSize.y;
		float rightHeight = saturate(heightMap.SampleLevel(heightMapSampler, uvRight, 0).r) * terrainSize.y;
		float downHeight = saturate(heightMap.SampleLevel(heightMapSampler, uvDown, 0).r) * terrainSize.y;
		float centerHeight = saturate(heightMap.SampleLevel(heightMapSampler, uvCenter, 0).r) * terrainSize.y;
		float leftUpHeight = saturate(heightMap.SampleLevel(heightMapSampler, uvLeftUp, 0).r) * terrainSize.y;
		float upRightHeight = saturate(heightMap.SampleLevel(heightMapSampler, uvUpRight, 0).r) * terrainSize.y;
		float rightDownHeight = saturate(heightMap.SampleLevel(heightMapSampler, uvRightDown, 0).r) * terrainSize.y;
		float downLeftHeight = saturate(heightMap.SampleLevel(heightMapSampler, uvDownLeft, 0).r) * terrainSize.y;

		float3 left = float3(uvLeft.x * terrainSize.x, leftHeight, uvLeft.y * terrainSize.z);
		float3 up = float3(uvUp.x * terrainSize.x, upHeight, uvUp.y * terrainSize.z);
		float3 right = float3(uvRight.x * terrainSize.x, rightHeight, uvRight.y * terrainSize.z);
		float3 down = float3(uvDown.x * terrainSize.x, downHeight, uvDown.y * terrainSize.z);
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

	float3 calculateNormal(float3 center, float3 left, float3 leftUp, float3 up, float3 upRight, float3 right, float3 rightDown, float3 down, float3 downLeft)
	{
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

#endif