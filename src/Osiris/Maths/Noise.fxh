const texture PermutationTexture;
const texture GradientTexture;


sampler PermutationSampler = sampler_state
{
	Texture = <PermutationTexture>;
	MipFilter = NONE;
	MinFilter = POINT;
	MagFilter = POINT;
	AddressU = WRAP;
	AddressV = CLAMP;
};

sampler GradientSampler = sampler_state
{
	Texture = <GradientTexture>;
	MipFilter = NONE;
	MinFilter = POINT;
	MagFilter = POINT;
	AddressU = WRAP;
	AddressV = CLAMP;
};


float2 fade(float2 t)
{
	return t * t * t * (t * (t * 6 - 15) + 10); // new curve: t^3(t(6t-15)+10) = t^3(6t^2-15t+10) = 6t^5 - 15t^4 + 10t^3
	// return t * t * (3 - 2 * t); // old curve: t^2(3-2t) = 3t^2 - 2t^3
}

float perm(float x)
{
	x = floor(x);
	return tex1D(PermutationSampler, (x + 0.5f) / 256.0f).x * 256.0f;
}

float grad(float x, float2 p)
{
	x = floor(x);
	return dot(tex1D(GradientSampler, (x + 0.5f) / 16.0f), p);
}

float noise(float2 p)
{
	// get integer and fractional parts of p
	float2 pi = floor(p);
	float2 pf = p - pi;
	
	// calculate interpolation factor (fade curve)
	float2 f = fade(pf);
	
	// hash coordinates for two of the four square corners
	float A = perm(pi.x)     + pi.y;
	float B = perm(pi.x + 1) + pi.y;
	
	// add blended results from 4 corners of square
	float result = lerp(lerp(grad(perm(A), pf),
													 grad(perm(B), pf + float2(-1, 0)),
													 f.x),
											lerp(grad(perm(A + 1), pf + float2(0, -1)),
													 grad(perm(B + 1), pf + float2(-1, -1)),
													 f.x),
											f.y);
											
	return result;
}


// fractal sum
float fBm(float2 p, int octaves, float lacunarity = 2.0, float gain = 0.5)
{
	float freq = 1.0, amp = 0.5;
	float sum = 0;	
	for(int i=0; i<octaves; i++) {
		sum += noise(p*freq)*amp;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}

// ridged multifractal

float ridge(float h, float offset)
{
	h = abs(h);
	h = offset - h;
	h = h * h;
	return h;
}

float ridgedmf(float2 p, int octaves, float lacunarity = 2.0f, float gain = 0.5f, float offset = 1.0f)
{
	float sum = 0;
	float freq = 1.0f, amp = 0.5f;
	float prev = 1.0f;
	for (int i = 0; i < octaves; i++)
	{
		float n = ridge(noise(p * freq), offset);
		sum += n * amp * prev;
		prev = n;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}

float hetero(float2 p, int octaves, float lacunarity = 2.0f, float gain = 0.5f)
{
	float sum = 1.0f;
	float freq = 1.0f, amp = 0.5f;
	for (int i = 0; i < octaves; i++)
	{
		float n = noise(p * freq) * amp * sum;
		sum += n;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}