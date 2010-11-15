// taken from Sean O'Neil, 2004

// the number of sample points taken along the ray
static int nSamples = 2;
const float fSamples = 2.0;

// the scale depth (the altitude at which the average atmospheric density is found)
const float fScaleDepth;
const float fInvScaleDepth;

// the scale equation calculated by Vernier's Graphical Analysis
float scale(float cos)
{
	float x = 1.0 - cos;
	return fScaleDepth * exp(-0.00287 + x * (0.459 + x * (3.83 + x * (-6.80 + x * 5.25))));
}

// calculates the Mie phase function
float getMiePhase(float fCos, float fCos2, float g, float g2)
{
	return 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos2) / pow(1.0 + g2 - 2.0 * g * fCos, 1.5);
}

// calculates the Rayleigh phase function
float getRayleighPhase(float fCos2)
{
	//return 1.0;
	//return 0.75 + 0.75 * fCos2;
	return 0.75 * (2.0 + 0.5 * fCos2);
}

// vertex shader constants
const float3 v3CameraPos;		// The camera's current position
//const float3 v3LightPos;			// The direction vector to the light source
const float3 v3InvWavelength;	// 1 / pow(wavelength, 4) for the red, green, and blue channels
const float fCameraHeight;		// The camera's current height
const float fCameraHeight2;		// fCameraHeight^2
const float fOuterRadius;		// The outer (atmosphere) radius
const float fOuterRadius2;		// fOuterRadius^2
const float fInnerRadius;		// The inner (planetary) radius
const float fInnerRadius2;		// fInnerRadius^2
const float fKrESun;				// Kr * ESun
const float fKmESun;				// Km * ESun
const float fKr4PI;				// Kr * 4 * PI
const float fKm4PI;				// Km * 4 * PI
const float fScale;				// 1 / (fOuterRadius - fInnerRadius)
const float fScaleOverScaleDepth;// fScale / fScaleDepth
const float fSkydomeRadius; // skydome radius (allows us to normalise skydome distances etc.)

// pixel shader constants
const float g;
const float g2;

void GetAtmosphericSkyDataVS(
	in float3 lightDirection,
	in float3 position,
	out float3 direction,
	out float4 rayleigh,
	out float4 mie)
{
	//lightDirection.y *= -1;
		
	// Get the ray from the camera to the vertex, and its length (which is the far point of the ray passing through the atmosphere)
	//float3 v3Pos = input.Position.xyz;
	float3 v3Pos = position;
	
	//v3Pos *= fOuterRadius;
	//v3Pos.y += fInnerRadius;
	
	float3 v3Ray = v3Pos - v3CameraPos;
	float fFar = length(v3Ray);
	v3Ray /= fFar;

	// Calculate the ray's starting position, then calculate its scattering offset
	float3 v3Start = v3CameraPos;
	float fHeight = length(v3Start);	
	float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fCameraHeight));
	float fStartAngle = dot(v3Ray, v3Start) / fHeight;
	float fStartOffset = fDepth*scale(fStartAngle);

	// Initialize the scattering loop variables
	float fSampleLength = fFar / fSamples;
	float fScaledLength = fSampleLength * fScale;
	float3 v3SampleRay = v3Ray * fSampleLength;
	float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;

	// Now loop through the sample rays
	float3 v3FrontColor = float3(0.0, 0.0, 0.0);
	for(int i=0; i<nSamples; i++)
	{
		float fHeight = length(v3SamplePoint);
		float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
		
		float fLightAngle = dot(lightDirection, v3SamplePoint) / fHeight;
		float fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;
		float fScatter = (fStartOffset + fDepth*(scale(fLightAngle) - scale(fCameraAngle)));
		float3 v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
		v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
		v3SamplePoint += v3SampleRay;
	}

	// Finally, scale the Mie and Rayleigh colors and set up the varying variables for the pixel shader
	direction = v3CameraPos - v3Pos;
	rayleigh = float4(v3FrontColor * (v3InvWavelength * fKrESun), 1);
	mie = float4(v3FrontColor * fKmESun, 1);
}

void GetAtmosphericGroundDataVS(
	in float3 lightDirection,
	in float3 position,
	out float4 rayleigh,
	out float4 mie)
{
	//lightDirection.y *= -1;
		
	// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
	float3 v3Pos = position;
	v3Pos.y += fInnerRadius;
	
	float3 v3Ray = v3Pos - v3CameraPos;
	v3Pos = normalize(v3Pos);
	float fFar = length(v3Ray);
	v3Ray /= fFar;
	
	// Calculate the ray's starting position, then calculate its scattering offset
	float3 v3Start = v3CameraPos;
	float fDepth = exp((fInnerRadius - fCameraHeight) * fInvScaleDepth);
	float fCameraAngle = dot(-v3Ray, v3Pos);
	float fLightAngle = dot(lightDirection, v3Pos);
	float fCameraScale = scale(fCameraAngle);
	float fLightScale = scale(fLightAngle);
	float fCameraOffset = fDepth*fCameraScale;
	float fTemp = (fLightScale + fCameraScale);

	// Initialize the scattering loop variables
	float fSampleLength = fFar / fSamples;
	float fScaledLength = fSampleLength * fScale;
	float3 v3SampleRay = v3Ray * fSampleLength;
	float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;

	// Now loop through the sample rays
	float3 v3FrontColor = float3(0.0, 0.0, 0.0);
	float3 v3Attenuate;
	for(int i=0; i<nSamples; i++)
	{
		float fHeight = length(v3SamplePoint);
		float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
		float fScatter = fDepth*fTemp - fCameraOffset;
		v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
		v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
		v3SamplePoint += v3SampleRay;
	}

	rayleigh = float4(v3FrontColor * (v3InvWavelength * fKrESun + fKmESun), 1);
	mie = float4(v3Attenuate, 1);
}

float4 GetAtmosphericSkyColourPS(
	in float3 lightDirection,
	in float3 direction,
	in float4 rayleigh,
	in float4 mie)
{
	lightDirection.y *= -1;
		
	float fCos = dot(lightDirection, direction) / length(direction);
	float fCos2 = fCos*fCos;
	float4 color = float4(getRayleighPhase(fCos2) * rayleigh + getMiePhase(fCos, fCos2, g, g2) * mie);
	return color;
}

float4 GetAtmosphericGroundColourPS(
	in float4 diffuse,
	in float4 rayleigh,
	in float4 mie) : COLOR
{
	return rayleigh + (diffuse * mie);
}