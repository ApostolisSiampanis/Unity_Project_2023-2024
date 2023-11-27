#ifndef SAMPLING_INCLUDED
	#define SAMPLING_INCLUDED
	
	SamplerState _SamplerLinearClamp;
	SamplerState _SamplerPointClamp;
		
	float4 SampleTextureBilinear(RWTexture2D < float4 > buffer, float width, float height, float2 pixelCoord)
	{
		float4 value = 0;
		pixelCoord.x = clamp(pixelCoord.x, 0, width - 1);
		pixelCoord.y = clamp(pixelCoord.y, 0, height - 1);
		//apply a bilinear filter
		int xFloor = floor(pixelCoord.x);
		int xCeil = ceil(pixelCoord.x);
		int yFloor = floor(pixelCoord.y);
		int yCeil = ceil(pixelCoord.y);

		float4 f00 = buffer[uint2(xFloor, yFloor)];
		float4 f01 = buffer[uint2(xFloor, yCeil)];

		float4 f10 = buffer[uint2(xCeil, yFloor)];
		float4 f11 = buffer[uint2(xCeil, yCeil)];

		float2 unitCoord = float2(pixelCoord.x - xFloor, pixelCoord.y - yFloor);

		value = f00 * (1 - unitCoord.x) * (1 - unitCoord.y) + f01 * (1 - unitCoord.x) * unitCoord.y + f10 * unitCoord.x * (1 - unitCoord.y) + f11 * unitCoord.x * unitCoord.y;

		return value;
	}

	float SampleTextureBilinear(Texture2D < float > buffer, float width, float height, float2 uv)
	{
		float value = 0;
		float2 pixelCoord = float2(lerp(0, width - 1, uv.x), lerp(0, height - 1, uv.y));
		//apply a bilinear filter
		int xFloor = floor(pixelCoord.x);
		int xCeil = ceil(pixelCoord.x);
		int yFloor = floor(pixelCoord.y);
		int yCeil = ceil(pixelCoord.y);

		float f00 = buffer[uint2(xFloor, yFloor)];
		float f01 = buffer[uint2(xFloor, yCeil)];

		float f10 = buffer[uint2(xCeil, yFloor)];
		float f11 = buffer[uint2(xCeil, yCeil)];

		float2 unitCoord = float2(pixelCoord.x - xFloor, pixelCoord.y - yFloor);

		value = f00 * (1 - unitCoord.x) * (1 - unitCoord.y) + f01 * (1 - unitCoord.x) * unitCoord.y + f10 * unitCoord.x * (1 - unitCoord.y) + f11 * unitCoord.x * unitCoord.y;

		return value;
	}

	float SampleTextureBilinear(RWTexture2D < float > buffer, float width, float height, float2 pixelCoord)
	{
		float value = 0;
		pixelCoord.x = clamp(pixelCoord.x, 0, width - 1);
		pixelCoord.y = clamp(pixelCoord.y, 0, height - 1);
		//apply a bilinear filter
		int xFloor = floor(pixelCoord.x);
		int xCeil = ceil(pixelCoord.x);
		int yFloor = floor(pixelCoord.y);
		int yCeil = ceil(pixelCoord.y);

		float f00 = buffer[uint2(xFloor, yFloor)];
		float f01 = buffer[uint2(xFloor, yCeil)];

		float f10 = buffer[uint2(xCeil, yFloor)];
		float f11 = buffer[uint2(xCeil, yCeil)];

		float2 unitCoord = float2(pixelCoord.x - xFloor, pixelCoord.y - yFloor);

		value = f00 * (1 - unitCoord.x) * (1 - unitCoord.y) + f01 * (1 - unitCoord.x) * unitCoord.y + f10 * unitCoord.x * (1 - unitCoord.y) + f11 * unitCoord.x * unitCoord.y;

		return value;
	}

	//Source: http://www.mate.tue.nl/mate/pdfs/10318.pdf
	float4 tex2DBicubic(sampler2D tex, float2 texSize, float2 uv)
	{
		float2 maxXY = texSize - 1;
		float x = uv.x * maxXY.x;
		float y = uv.y * maxXY.y;
		// transform the coordinate from [0,extent] to [-0.5, extent-0.5]
		float2 coord_grid = float2(x - 0.5, y - 0.5);
		float2 index = floor(coord_grid);
		float2 fraction = coord_grid - index;
		float2 one_frac = 1.0 - fraction;
		float2 one_frac2 = one_frac * one_frac;
		float2 fraction2 = fraction * fraction;

		float2 w0 = 1.0 / 6.0 * one_frac2 * one_frac;
		float2 w1 = 2.0 / 3.0 - 0.5 * fraction2 * (2.0 - fraction);
		float2 w2 = 2.0 / 3.0 - 0.5 * one_frac2 * (2.0 - one_frac);
		float2 w3 = 1.0 / 6.0 * fraction2 * fraction;
		float2 g0 = w0 + w1;
		float2 g1 = w2 + w3;
		// h0 = w1/g0 - 1, move from [-0.5, extent-0.5] to [0, extent]
		float2 h0 = (w1 / g0) - 0.5 + index;
		float2 h1 = (w3 / g1) + 1.5 + index;
		// fetch the four linear interpolations
		float4 tex00 = tex2D(tex, float2(h0.x, h0.y) / maxXY);
		float4 tex10 = tex2D(tex, float2(h1.x, h0.y) / maxXY);
		float4 tex01 = tex2D(tex, float2(h0.x, h1.y) / maxXY);
		float4 tex11 = tex2D(tex, float2(h1.x, h1.y) / maxXY);
		// weigh along the y-direction
		tex00 = lerp(tex01, tex00, g0.y);
		tex10 = lerp(tex11, tex10, g0.y);
		// weigh along the x-direction
		return lerp(tex10, tex00, g0.x);
	}

	float4 tex2DBicubic(RWTexture2D<float4> tex, float2 texSize, float x, float y)
	{
		float2 maxXY = texSize - 1;

		// transform the coordinate from [0,extent] to [-0.5, extent-0.5]
		float2 coord_grid = float2(x - 0.5, y - 0.5);
		float2 index = floor(coord_grid);
		float2 fraction = coord_grid - index;
		float2 one_frac = 1.0 - fraction;
		float2 one_frac2 = one_frac * one_frac;
		float2 fraction2 = fraction * fraction;

		float2 w0 = 1.0 / 6.0 * one_frac2 * one_frac;
		float2 w1 = 2.0 / 3.0 - 0.5 * fraction2 * (2.0 - fraction);
		float2 w2 = 2.0 / 3.0 - 0.5 * one_frac2 * (2.0 - one_frac);
		float2 w3 = 1.0 / 6.0 * fraction2 * fraction;
		float2 g0 = w0 + w1;
		float2 g1 = w2 + w3;
		// h0 = w1/g0 - 1, move from [-0.5, extent-0.5] to [0, extent]
		float2 h0 = (w1 / g0) - 0.5 + index;
		float2 h1 = (w3 / g1) + 1.5 + index;
		// fetch the four linear interpolations
		float4 tex00 = SampleTextureBilinear(tex, texSize.x, texSize.y, float2(h0.x, h0.y));
		float4 tex10 = SampleTextureBilinear(tex, texSize.x, texSize.y, float2(h1.x, h0.y));
		float4 tex01 = SampleTextureBilinear(tex, texSize.x, texSize.y, float2(h0.x, h1.y));
		float4 tex11 = SampleTextureBilinear(tex, texSize.x, texSize.y, float2(h1.x, h1.y));
		// weigh along the y-direction
		tex00 = lerp(tex01, tex00, g0.y);
		tex10 = lerp(tex11, tex10, g0.y);
		// weigh along the x-direction
		return lerp(tex10, tex00, g0.x);
	}


#endif // SAMPLING_INCLUDED