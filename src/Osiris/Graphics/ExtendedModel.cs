using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Content;

namespace Osiris.Graphics
{
	public class ExtendedModel
	{
		private Model _model;
		private BoundingBox _boundingBox;
		private Matrix _worldMatrix;

		public BoundingBox BoundingBox
		{
			get
			{
				Vector3[] corners = _boundingBox.GetCorners();
				Vector3[] transformedCorners = new Vector3[BoundingBox.CornerCount];
				Vector3.Transform(corners, ref _worldMatrix, transformedCorners);
				return BoundingBox.CreateFromPoints(transformedCorners);
			}
		}

		public BoundingBox LocalBoundingBox
		{
			get
			{
				return _boundingBox;
			}
		}

		public Model InnerModel
		{
			get
			{
				return _model;
			}
		}

		public Matrix WorldMatrix
		{
			get { return _worldMatrix; }
			set { _worldMatrix = value; }
		}

		public ExtendedModel(Game game, string modelAssetName)
		{
			_model = AssetLoader.LoadAsset<Model>(modelAssetName, game);

			_boundingBox = new BoundingBox();
			foreach (ModelMesh mesh in _model.Meshes)
				_boundingBox = BoundingBox.CreateMerged(_boundingBox, BoundingBox.CreateFromSphere(mesh.BoundingSphere));
		}

		public void Draw(ExtendedEffect effect)
		{
			foreach (ModelMesh mesh in _model.Meshes)
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
					meshPart.Effect = effect.InnerEffect;

			foreach (ModelMesh mesh in _model.Meshes)
				mesh.Draw();
		}
	}
}
