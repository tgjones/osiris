using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Osiris.Content.Pipeline.Processors
{
	[ContentProcessor(DisplayName = "Model - Osiris Framework")]
	public class ModelNodeProcessor : ModelProcessor
	{
		private List<Vector3> _vertices = new List<Vector3>();

		public override ModelContent Process(NodeContent input, ContentProcessorContext context)
		{
			ModelContent modelContent = base.Process(input, context);

			// Look up the input vertex positions.
			FindVertices(input);

			Dictionary<string, object> tagData = new Dictionary<string, object>();

			modelContent.Tag = tagData;

			// Store vertex information in the tag data, as an array of Vector3.
			tagData.Add("Vertices", _vertices.ToArray());

			// Also store a custom bounding sphere.
			tagData.Add("BoundingSphere", BoundingSphere.CreateFromPoints(_vertices));

			return modelContent;
		}

		/// <summary>
		/// Helper for extracting a list of all the vertex positions in a model.
		/// Taken from the TrianglePicking sample.
		/// </summary>
		private void FindVertices(NodeContent node)
		{
			// Is this node a mesh?
			MeshContent mesh = node as MeshContent;

			if (mesh != null)
			{
				// Look up the absolute transform of the mesh.
				Matrix absoluteTransform = mesh.AbsoluteTransform;

				// Loop over all the pieces of geometry in the mesh.
				foreach (GeometryContent geometry in mesh.Geometry)
				{
					// Loop over all the indices in this piece of geometry.
					// Every group of three indices represents one triangle.
					foreach (int index in geometry.Indices)
					{
						// Look up the position of this vertex.
						Vector3 vertex = geometry.Vertices.Positions[index];

						// Transform from local into world space.
						vertex = Vector3.Transform(vertex, absoluteTransform);

						// Store this vertex.
						_vertices.Add(vertex);
					}
				}
			}

			// Recursively scan over the children of this node.
			foreach (NodeContent child in node.Children)
				FindVertices(child);
		}
	}
}