using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Terrain.Graphics
{
	/// <summary>
	/// Summary description for Map.
	/// </summary>
	public class TerrainComponent : DrawableGameComponent
	{
		#region Variables

		private readonly string _terrainModelAssetName;
		private ICameraService _camera;
		private TerrainModel _terrainModel;

		#endregion

		#region Properties

		public TerrainModel Model
		{
			get { return _terrainModel; }
		}

		#endregion

		#region Constructors

		public TerrainComponent(Game game, string terrainModelAssetName)
			: base(game)
		{
			_terrainModelAssetName = terrainModelAssetName;
		}

		#endregion

		#region Methods

		public override void Initialize()
		{
			_camera = (ICameraService) Game.Services.GetService(typeof (ICameraService));
			base.Initialize();
		}

		protected override void LoadContent()
		{
			_terrainModel = Game.Content.Load<TerrainModel>(_terrainModelAssetName);
			_terrainModel.Initialize(_camera, GraphicsDevice);

			if (_terrainModel.Effect is BasicEffect)
			{
				BasicEffect effect = (BasicEffect) _terrainModel.Effect;
				effect.Texture = Game.Content.Load<Texture2D>("terrain");
				effect.TextureEnabled = true;
			}

			base.LoadContent();
		}

		public override void Update(GameTime gameTime)
		{
			_terrainModel.Update(_camera);
		}

		public override void Draw(GameTime gameTime)
		{
			if (_terrainModel.Effect is IEffectMatrices)
			{
				IEffectMatrices effectMatrices = (IEffectMatrices) _terrainModel.Effect;
				effectMatrices.World = Matrix.Identity;
				effectMatrices.View = _camera.View;
				effectMatrices.Projection = _camera.Projection;
			}

			/*GraphicsDevice.RasterizerState = new RasterizerState
			{
				FillMode = FillMode.WireFrame
			};*/


			_terrainModel.Draw();
		}

		#endregion
	}
}
