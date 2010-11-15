using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Osiris.Graphics.SceneGraph.Geometries;
using Osiris.Graphics.Effects;
using Osiris.Graphics.Effects.Lights;
using Osiris.Graphics.Effects.LightingModels;

namespace Osiris.Graphics.SceneGraph.Nodes
{
	public class ModelNode : Node
	{
		public Vector3[] Vertices
		{
			get;
			private set;
		}

		public ModelNode(IServiceProvider serviceProvider, Model model)
			: base(serviceProvider)
		{
			// Look up our custom collision data from the Tag property of the model.
			Dictionary<string, object> tagData = (Dictionary<string, object>) model.Tag;
			Vertices = (Vector3[]) tagData["Vertices"];
			BoundingSphere boundingSphere = (BoundingSphere) tagData["BoundingSphere"];

			Matrix[] transforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(transforms);

			foreach (ModelMesh modelMesh in model.Meshes)
			{
				foreach (ModelMeshPart modelMeshPart in modelMesh.MeshParts)
				{
					GeometryContainer geometryContainer = new GeometryContainer(
						modelMeshPart.VertexBuffer, modelMeshPart.IndexBuffer);

					TriangleMesh triangleMesh = new TriangleMesh(serviceProvider, geometryContainer,
						modelMeshPart.VertexOffset, modelMeshPart.NumVertices,
						modelMeshPart.StartIndex, modelMeshPart.PrimitiveCount,
						boundingSphere);

					triangleMesh.Local = transforms[modelMesh.ParentBone.Index];

					BasicEffect basicEffect = (BasicEffect) modelMeshPart.Effect;

					// Attach material effect.
					MaterialEffect materialEffect = new MaterialEffect(serviceProvider);
					materialEffect.Alpha = basicEffect.Alpha;
					materialEffect.DiffuseColour = new Color(basicEffect.DiffuseColor);
					materialEffect.SpecularColour = new Color(basicEffect.SpecularColor);
					materialEffect.SpecularPower = basicEffect.SpecularPower;
					materialEffect.Roughness = 1f;
					materialEffect.TextureEnabled = basicEffect.TextureEnabled;
					if (basicEffect.TextureEnabled)
						materialEffect.DiffuseTexture = basicEffect.Texture;
					triangleMesh.Effects.Attach(materialEffect);

					// Attach lighting effect.
					DirectionalLightEffect directionalLightEffect = new DirectionalLightEffect(serviceProvider);
					directionalLightEffect.DiffuseColour = Color.White;
					directionalLightEffect.Direction = Vector3.Down;
					triangleMesh.Effects.Attach(directionalLightEffect);

					// Attach lighting effect.
					DirectionalLightEffect directionalLightEffect2 = new DirectionalLightEffect(serviceProvider);
					directionalLightEffect2.DiffuseColour = Color.Red;
					directionalLightEffect2.Direction = Vector3.Right;
					triangleMesh.Effects.Attach(directionalLightEffect2);

					OrenNayarEffect lightingModelEffect = new OrenNayarEffect(serviceProvider);
					lightingModelEffect.AmbientLightDiffuseColour = new Color(basicEffect.AmbientLightColor);
					triangleMesh.Effects.Attach(lightingModelEffect);

					Children.Add(triangleMesh);
				}
			}
		}
	}
}
