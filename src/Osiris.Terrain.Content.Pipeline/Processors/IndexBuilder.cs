using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Osiris.Terrain.Content.Pipeline.Processors
{
	/// <summary>
	/// Summary description for IndexBuilder.
	/// </summary>
	public class IndexBuilder
	{
		#region Fields

		private readonly LevelContentBuilder _levelBuilder;
		private int _lastIndexAdded;

		#endregion

		#region Properties

		public IndexCollection Indices
		{
			get;
			private set;
		}

		#endregion

		#region Constructor

		public IndexBuilder(LevelContentBuilder levelBuilder)
		{
			_levelBuilder = levelBuilder;
			Indices = new IndexCollection();
		}

		#endregion

		#region Methods

		public void AddIndex(int nX, int nY)
		{
			// get index
			_lastIndexAdded = _levelBuilder.GetIndex(nX, nY);

			// store
			Indices.Add(_lastIndexAdded);
		}

		public void AddLastIndexAgain()
		{
			Indices.Add(_lastIndexAdded);
		}

		#endregion
	}
}
