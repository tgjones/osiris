namespace Osiris.Terrain.Content.Pipeline.Graphics
{
	public class HeightMapContent
	{
		private readonly float[,] _values;
		private readonly float _verticalScale;

		public float this[int x, int z]
		{
			get { return _values[x, z] * _verticalScale; }
		}

		public int Width { get; private set; }
		public int Height { get; private set; }

		public int HorizontalScale { get; private set; }

		public HeightMapContent(int width, int height, float[,] values, float verticalScale, int horizontalScale)
		{
			Width = width;
			Height = height;
			HorizontalScale = horizontalScale;
			_values = values;
			_verticalScale = verticalScale;
		}
	}
}