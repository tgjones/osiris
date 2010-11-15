/*const float EncodeMaxDiff = 4096;
const float EncodeMaxDiffTimes2 = 8192;

const float SamplesPerHeightUnit = 1.0f;

float encodeHeights(float zCurrent, float zCoarser)
{
	// pack both fine and coarser heights into a single float
	float zd = zCoarser - zCurrent;
	return zCurrent + saturate((zd + EncodeMaxDiff) / EncodeMaxDiffTimes2);
}

void decodeHeights(float zIn, out float zf, out float zd)
{
	zf = floor(zIn);
	zd = (frac(zIn) * EncodeMaxDiffTimes2) - EncodeMaxDiff; // zd = zc - zf
}*/

float decodeHeight(float zIn)
{
	return zIn;
}

float scaleHeight(float zIn)
{
	//return zIn / SamplesPerHeightUnit;
	return zIn;
}