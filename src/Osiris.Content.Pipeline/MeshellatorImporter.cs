using System;
using System.Collections.Generic;
using System.IO;
using Meshellator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Nexus;

namespace Osiris.Content.Pipeline
{
	public abstract class MeshellatorImporter : ContentImporter<NodeContent>
	{
		protected virtual bool SwapWindingOrder
		{
			get { return false; }
		}

		/// <summary>
		/// The importer's entry point.
		/// Called by the framework when importing a game asset.
		/// </summary>
		/// <param name="filename">Name of a game asset file.</param>
		/// <param name="context">
		/// Contains information for importing a game asset, such as a logger interface.
		/// </param>
		/// <returns>Resulting game asset.</returns>
		public override NodeContent Import(string filename, ContentImporterContext context)
		{
			NodeContent rootNode = new NodeContent
			{
				Identity = new ContentIdentity(filename),
				Name = Path.GetFileNameWithoutExtension(filename)
			};

			try
			{
				// Import file using Meshellator.
				Scene scene = MeshellatorLoader.ImportFromFile(filename);

				// Create materials.
				//System.Diagnostics.Debugger.Launch();
				Dictionary<Material, MaterialContent> materials = GetMaterials(scene);

				// Convert Meshellator scene to XNA mesh.
				foreach (Mesh mesh in scene.Meshes)
				{
					MeshBuilder meshBuilder = MeshBuilder.StartMesh(mesh.Name);
					meshBuilder.SwapWindingOrder = SwapWindingOrder;

					// Set material.
					meshBuilder.SetMaterial(materials[mesh.Material]);

					// Add additional vertex channels for texture coordinates and normals
					int textureCoordinateDataIndex = meshBuilder.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(0));
					int normalDataIndex = meshBuilder.CreateVertexChannel<Vector3>(VertexChannelNames.Normal());

					int[] positionMap = new int[mesh.Indices.Count];
					for (int i = 0; i < mesh.Indices.Count; ++i)
					{
						int meshIndex = mesh.Indices[i];
						int index = meshBuilder.CreatePosition(ConvertPoint3D(mesh.Positions[meshIndex]));
						positionMap[i] = index;
					}

					for (int i = 0; i < mesh.Indices.Count; ++i)
					{
						int meshIndex = mesh.Indices[i];

						Vector2 texCoord = (mesh.TextureCoordinates.Count > meshIndex)
							? ConvertTextureCoordinate(mesh.TextureCoordinates[meshIndex])
							: Vector2.Zero;
						meshBuilder.SetVertexChannelData(textureCoordinateDataIndex, texCoord);
						meshBuilder.SetVertexChannelData(normalDataIndex, ConvertVector3D(mesh.Normals[meshIndex]));
						meshBuilder.AddTriangleVertex(positionMap[i]);
					}
					
					// Add the mesh to the model
					MeshContent meshContent = meshBuilder.FinishMesh();
					rootNode.Children.Add(meshContent);
				}

				return rootNode;
			}
			catch (InvalidContentException)
			{
				// InvalidContentExceptions do not need further processing
				throw;
			}
			catch (Exception e)
			{
				// Wrap exception with content identity (includes line number)
				throw new InvalidContentException(
					"Unable to parse file. Exception:\n" + e.ToString(),
					rootNode.Identity, e);
			}
		}

		private static Dictionary<Material, MaterialContent> GetMaterials(Scene scene)
		{
			Dictionary<Material, MaterialContent> materials = new Dictionary<Material, MaterialContent>();
			foreach (Material material in scene.Materials)
			{
				BasicMaterialContent materialContent = new BasicMaterialContent
				{
					Name = material.Name,
					Identity = new ContentIdentity(material.FileName),
					Alpha = material.Transparency,
					DiffuseColor = ConvertColorRgbF(material.DiffuseColor),
					SpecularColor = ConvertColorRgbF(material.SpecularColor),
					SpecularPower = material.Shininess,
					VertexColorEnabled = false,
				};
				if (!string.IsNullOrEmpty(material.DiffuseTextureName))
					materialContent.Texture = new ExternalReference<TextureContent>(material.DiffuseTextureName);

				materials.Add(material, materialContent);
			}
			return materials;
		}

		private static Vector3 ConvertColorRgbF(ColorRgbF color)
		{
			return new Vector3(color.R, color.G, color.B);
		}

		private static Vector3 ConvertPoint3D(Point3D point)
		{
			return new Vector3(point.X, point.Y, point.Z);
		}

		private static Vector2 ConvertTextureCoordinate(Point3D point)
		{
			return new Vector2(point.X, point.Y);
		}

		private static Vector3 ConvertVector3D(Vector3D vector)
		{
			return new Vector3(vector.X, vector.Y, vector.Z);
		}
	}
}