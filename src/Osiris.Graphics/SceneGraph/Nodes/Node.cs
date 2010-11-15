using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using Osiris.Graphics.Effects;
using Osiris.Graphics.SceneGraph.Culling;
using Osiris.Graphics.SceneGraph.Geometries;
using Microsoft.Xna.Framework;

namespace Osiris.Graphics.SceneGraph.Nodes
{
	public class Node : Spatial
	{
		private NodeChildCollection _children;

		public NodeChildCollection Children
		{
			get { return _children; }
		}

		public Node(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
			_children = new NodeChildCollection(this);
		}

		protected override void UpdateWorldData(GameTime gameTime)
		{
			base.UpdateWorldData(gameTime);

			foreach (Spatial child in Children)
				child.UpdateGeometricState(gameTime, false);
		}

		protected override void UpdateWorldBound()
		{
			if (!WorldBoundIsCurrent)
			{
				bool foundFirstBound = false;
				foreach (Spatial child in Children)
				{
					if (child != null)
					{
						if (foundFirstBound)
						{
							// merge current world bound with child world bound
							WorldBound = BoundingSphere.CreateMerged(WorldBound, child.WorldBound);
						}
						else
						{
							// set world bound to first non-null child world bound
							foundFirstBound = true;
							WorldBound = child.WorldBound;
						}
					}
				}
			}
		}

		protected override void UpdateState(GlobalStateStackCollection globalStateStacks)
		{
			foreach (Spatial child in Children)
				if (child != null)
					child.UpdateRenderState(globalStateStacks);
		}

		private IEnumerable<Geometry> GetChildGeometries()
		{
			return Children.OfType<Geometry>().Union(GetChildGeometries()); ;
		}

		protected override void GetVisibleSet(Culler culler, bool noCull)
		{
			base.GetVisibleSet(culler, noCull);

			// All Geometry objects in the subtree are added to the visible set.  If
			// a global effect is active, the Geometry objects in the subtree will be
			// drawn using it.
			foreach (Spatial child in Children)
				if (child != null)
					child.OnGetVisibleSet(culler, noCull);
		}

		public override void BuildShader(Stack<ShaderEffect> effects)
		{
			PushEffects(effects);

			// recursively add children to stack - if a child is a Geometry,
			// then it will actually build the shader.
			foreach (Spatial child in Children)
				child.BuildShader(effects);

			PopEffects(effects);
		}

		#region Nested classes

		public class NodeChildCollection : IEnumerable<Spatial>
		{
			private Node _parent;
			private List<Spatial> _storage;

			public Spatial this[int index]
			{
				get { return _storage[index]; }
				set
				{
					if (value != null)
						Debug.Assert(value.Parent == null);

					// detach child currently in slot
					Spatial previousChild = _storage[index];
					if (previousChild != null)
						previousChild.Parent = null;

					// attach new child
					if (value != null)
						value.Parent = _parent;

					_storage[index] = value;
				}
			}

			public int Count
			{
				get { return _storage.Count; }
			}

			public NodeChildCollection(Node parent)
			{
				_parent = parent;
				_storage = new List<Spatial>();
			}

			public void Add(Spatial child)
			{
				Debug.Assert(child != null && child.Parent == null);
				child.Parent = _parent;
				_storage.Add(child);
			}

			public void Remove(Spatial child)
			{
				if (_storage.Contains(child))
				{
					child.Parent = null;
					_storage.Remove(child);
				}
			}

			public void RemoveAt(int index)
			{
				_storage[index].Parent = null;
				_storage.RemoveAt(index);
			}

			public IEnumerator<Spatial> GetEnumerator()
			{
				return _storage.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _storage.GetEnumerator();
			}
		}

		#endregion
	}
}
