using System;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Osiris.Graphics.SceneGraph.Culling;
using Osiris.Graphics.Shaders;
using System.Collections.Generic;
using Osiris.Graphics.Effects;
using Osiris.Graphics.Rendering.GlobalStates;
using Osiris.Graphics.SceneGraph.Nodes;

namespace Osiris.Graphics.SceneGraph.Geometries
{
	public abstract class Geometry : Spatial
	{
		private Shader _shader;

		public int StreamOffset { get; private set; }
		public int BaseVertex { get; private set; }
		public int NumVertices { get; private set; }
		public int StartIndex { get; private set; }
		public int PrimitiveCount { get; private set; }
		public int VertexStride { get; private set; }
		public GeometryContainer GeometryContainer { get; private set; }
		public VertexDeclaration VertexDeclaration { get; private set; }
		public VertexBuffer VertexBuffer;
		public IndexBuffer IndexBuffer;
		public BoundingSphere ModelBound;

		internal abstract PrimitiveType Type
		{
			get;
		}

		internal Rendering.GlobalStates.GlobalStateCollection States;

		public Geometry(IServiceProvider serviceProvider, GeometryContainer geometryContainer,
			int streamOffset, int baseVertex, int numVertices, int startIndex, int primitiveCount,
			VertexDeclaration vertexDeclaration)
			: base(serviceProvider)
		{
			GeometryContainer = geometryContainer;
			StreamOffset = streamOffset;
			BaseVertex = baseVertex;
			NumVertices = numVertices;
			StartIndex = startIndex;
			PrimitiveCount = primitiveCount;
			VertexDeclaration = vertexDeclaration;
			VertexStride = VertexDeclaration.GetVertexStrideSize(0);

			ModelBound = new BoundingSphere();

			States = new Rendering.GlobalStates.GlobalStateCollection();
			UpdateModelBound();
		}

		public virtual void UpdateModelState()
		{
			UpdateModelBound();
		}

		#region Geometric updates

		protected abstract void UpdateModelBound();

		protected override void UpdateWorldBound()
		{
			WorldBound = ModelBound.Transform(World);
		}

		#endregion

		#region Render state updates

		protected override void UpdateState(GlobalStateStackCollection globalStateStacks)
		{
			// update global state
			for (int i = 0; i < (int) GlobalState.StateType.MAX; i++)
				States[(GlobalState.StateType) i] = globalStateStacks[i].Peek();
		}

		#endregion

		#region Culling

		protected override void GetVisibleSet(Culler culler, bool noCull)
		{
			base.GetVisibleSet(culler, noCull);
			culler.VisibleSet.VisibleGeometries.Add(this);
		}

		#endregion

		protected override void UpdateWorldData(GameTime gameTime)
		{
			base.UpdateWorldData(gameTime);
			
			// no need to do anything, until we support non-uniform scaling.
			// in that case, we'll need to store Transformations in the base
			// Spatial class, instead of Matrix's, and then here we'll do
			// the conversion into a Matrix.
		}

		#region Shader management

		/// <summary>
		/// Stores both the ShaderEffect objects, as well as compiled shader fragments.
		/// </summary>
		/// <param name="effects"></param>
		public override void BuildShader(Stack<ShaderEffect> effects)
		{
			PushEffects(effects);

			// get fragments from each effect, and combine them with fragments from
			// this geometry which are determined by vertex type. then put it
			// all together.
			Dictionary<ShaderFragment, ShaderEffect> effectFragmentLookup = new Dictionary<ShaderFragment, ShaderEffect>();
			List<ShaderFragment> shaderFragments = new List<ShaderFragment>();
			foreach (ShaderEffect effect in effects)
			{
				ShaderFragment fragment = effect.GetShaderFragment();
				effectFragmentLookup.Add(fragment, effect);
				shaderFragments.Add(fragment);
			}

			// compile
			CompiledShaderFragment[] compiledFragments;
			EffectGenerator.CreateFromFragments(
				((GraphicsDeviceManager) ServiceProvider.GetService(typeof(GraphicsDeviceManager))).GraphicsDevice,
				shaderFragments.ToArray(), VertexDeclaration.GetVertexElements(),
				out _shader, out compiledFragments);

			// Associate compiled fragments with shader effects that provided
			// the original fragments.
			foreach (CompiledShaderFragment compiledFragment in compiledFragments)
				effectFragmentLookup[compiledFragment.ShaderFragment].SetCompiledShaderFragment(compiledFragment);

			PopEffects(effects);
		}

		public Shader GetShader()
		{
			return _shader;
		}

		#endregion
	}
}
