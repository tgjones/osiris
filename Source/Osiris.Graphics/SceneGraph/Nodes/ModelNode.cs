using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Osiris.Graphics.SceneGraph.Geometries;
using Osiris.Graphics.Effects;
using Osiris.Graphics.Effects.VertexEffects;
using Osiris.Graphics.Effects.Lights;

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
				GeometryContainer geometryContainer = new GeometryContainer(
					modelMesh.VertexBuffer, modelMesh.IndexBuffer);

				foreach (ModelMeshPart modelMeshPart in modelMesh.MeshParts)
				{
					TriangleMesh triangleMesh = new TriangleMesh(serviceProvider, geometryContainer,
						modelMeshPart.StreamOffset, modelMeshPart.BaseVertex, modelMeshPart.NumVertices,
						modelMeshPart.StartIndex, modelMeshPart.PrimitiveCount, modelMeshPart.VertexDeclaration,
						boundingSphere);

					triangleMesh.Local = transforms[modelMesh.ParentBone.Index];

					BasicEffect basicEffect = (BasicEffect) modelMeshPart.Effect;

					// Attach material effect.
					MaterialEffect materialEffect = new MaterialEffect(serviceProvider);
					materialEffect.Alpha = basicEffect.Alpha;
					materialEffect.DiffuseColour = new Color(basicEffect.DiffuseColor);
					materialEffect.SpecularColour = new Color(basicEffect.SpecularColor);
					materialEffect.SpecularPower = basicEffect.SpecularPower;
					triangleMesh.Effects.Attach(materialEffect);

					// Attach lighting effect.
					DirectionalLightEffect lightingEffect = new DirectionalLightEffect(serviceProvider);
					lightingEffect.AmbientLightDiffuseColour = new Color(basicEffect.AmbientLightColor);
					//lightingEffect.LightDiffuseColour = new Color(basicEffect.DirectionalLight0.DiffuseColor);
					//lightingEffect.LightDirection = basicEffect.DirectionalLight0.Direction;
					lightingEffect.LightDiffuseColour = Color.Red;
					lightingEffect.LightDirection = Vector3.Down;
					triangleMesh.Effects.Attach(lightingEffect);

					PixelColourOutputEffect pixelColourOutputEffect = new PixelColourOutputEffect(serviceProvider);
					triangleMesh.Effects.Attach(pixelColourOutputEffect);

					// Attach geometry effects.
					PositionNormalTextureEffect vertexEffect = new PositionNormalTextureEffect(serviceProvider);
					triangleMesh.Effects.Attach(vertexEffect);

					VertexPassThruEffect passThruEffect = new VertexPassThruEffect(serviceProvider);
					triangleMesh.Effects.Attach(passThruEffect);

					Children.Add(triangleMesh);
				}
			}
		}
	}
}
