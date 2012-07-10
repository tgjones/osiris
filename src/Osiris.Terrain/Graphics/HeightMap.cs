namespace Osiris.Terrain.Graphics
{
	public class HeightMap
	{
		private readonly float[,] _values;
		private readonly int _horizontalScale;

		public float this[int x, int z]
		{
			get
			{
				if (x < 0 || x >= Width || z < 0 || z >= Height)
					return 0;
				return _values[x, z];
			}
		}

		public float this[float x, float z]
		{
			get
			{
				// convert coordinates to heightmap scale
				x /= _horizontalScale;
				z /= _horizontalScale;

				// get integer and fractional parts of coordinates
				int nIntX0 = MathUtility.FloorToInt(x);
				int nIntY0 = MathUtility.FloorToInt(z);
				float fFractionalX = x - nIntX0;
				float fFractionalY = z - nIntY0;

				// get coordinates for "other" side of quad
				int nIntX1 = MathUtility.Clamp(nIntX0 + 1, 0, Width - 1);
				int nIntY1 = MathUtility.Clamp(nIntY0 + 1, 0, Height - 1);

				// read 4 map values
				float f0 = this[nIntX0, nIntY0];
				float f1 = this[nIntX1, nIntY0];
				float f2 = this[nIntX0, nIntY1];
				float f3 = this[nIntX1, nIntY1];

				// calculate averages
				float fAverageLo = (f1 * fFractionalX) + (f0 * (1.0f - fFractionalX));
				float fAverageHi = (f3 * fFractionalX) + (f2 * (1.0f - fFractionalX));

				return (fAverageHi * fFractionalY) + (fAverageLo * (1.0f - fFractionalY));
			}
		}

		public int Width { get; private set; }
		public int Height { get; private set; }

		public HeightMap(int width, int height, float[,] values, int horizontalScale)
		{
			Width = width;
			Height = height;
			_values = values;
			_horizontalScale = horizontalScale;
		}
	}
}