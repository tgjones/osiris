using System;
using System.Collections.Generic;
using System.IO;
using Meshellator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Nexus;
using Nexus.Graphics.Colors;
using Nexus.Graphics.Transforms;

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
					MeshContent meshContent = new MeshContent
					{
						Name = mesh.Name
					};
					foreach (Point3D position in mesh.Positions)
						meshContent.Positions.Add(ConvertPoint3D(position));

					MaterialContent material = (mesh.Material != null)
					                           	? materials[mesh.Material]
					                           	: new BasicMaterialContent
					                           	{
					                           		DiffuseColor = new Vector3(0.5f),
					                           		VertexColorEnabled = false
					                           	};
					GeometryContent geometryContent = new GeometryContent
					{
						Material = material
					};
					meshContent.Geometry.Add(geometryContent);

					geometryContent.Indices.AddRange(mesh.Indices);

					for (int i = 0; i < mesh.Positions.Count; ++i)
						geometryContent.Vertices.Add(i);

					List<Vector2> textureCoordinates = new List<Vector2>();
					for (int i = 0; i < mesh.Positions.Count; ++i)
					{
						Vector2 textureCoordinate = (i < mesh.TextureCoordinates.Count)
							? ConvertTextureCoordinate(mesh.TextureCoordinates[i])
							: Vector2.Zero;
						textureCoordinates.Add(textureCoordinate);
					}
					geometryContent.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(0), textureCoordinates);

					List<Vector3> normals = new List<Vector3>();
					foreach (Vector3D normal in mesh.Normals)
						normals.Add(ConvertVector3D(normal));
					geometryContent.Vertices.Channels.Add(VertexChannelNames.Normal(), normals);

					// Finish the mesh and set the transform.
					if (SwapWindingOrder)
						MeshHelper.SwapWindingOrder(meshContent);
					meshContent.Transform = ConvertTransform(mesh.Transform);

					// Add the mesh to the model
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

		private static Matrix ConvertTransform(Transform3D transform)
		{
			Matrix3D transformMatrix = transform.Value;

			return new Matrix
			{
				M11 = transformMatrix.M11,
				M12 = transformMatrix.M12,
				M13 = transformMatrix.M13,
				M14 = transformMatrix.M14,
				M21 = transformMatrix.M21,
				M22 = transformMatrix.M22,
				M23 = transformMatrix.M23,
				M24 = transformMatrix.M24,
				M31 = transformMatrix.M31,
				M32 = transformMatrix.M32,
				M33 = transformMatrix.M33,
				M34 = transformMatrix.M34,
				M41 = transformMatrix.M41,
				M42 = transformMatrix.M42,
				M43 = transformMatrix.M43,
				M44 = transformMatrix.M44,
			};
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