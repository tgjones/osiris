using System;
using Microsoft.Xna.Framework;
using Osiris.Graphics.Rendering;
using System.Collections.Generic;
using System.Diagnostics;
using Osiris.Graphics.SceneGraph.Culling;
using Osiris.Graphics.Effects;
using Osiris.Graphics.Rendering.GlobalStates;
using System.Collections;
using Microsoft.Xna.Framework.Content;

namespace Osiris.Graphics.SceneGraph
{
	public abstract class Spatial
	{
		#region Fields

		// Local and world transforms.  In some situations you might need to set
		// the world transform directly and bypass the Spatial::Update()
		// mechanism.  If World is set directly, the WorldIsCurrent flag should
		// be set to true.  For example, inverse kinematic controllers and skin
		// controllers need this capability.
		public Matrix Local;
		public Matrix World;
		public bool WorldIsCurrent;

		// World bound access.  In some situations you might want to set the
		// world bound directly and bypass the Spatial::UpdateGS() mechanism.  If
		// WorldBound is set directly, the WorldBoundIsCurrent flag should be set
		// to true.
		public BoundingSphere WorldBound;
		public bool WorldBoundIsCurrent;

		public CullingMode Culling;

		// support for hierarchical scene graph
		private Spatial _parent;

		// global render state
		protected GlobalStateCollection _globalStates;

		// Effect state.  If the effect is attached to a Geometry object, it
		// applies to that object alone.  If the effect is attached to a Node
		// object, it applies to all Geometry objects in the subtree rooted at
		// the Node.  The "mutable" tag allows this array to change temporarily
		// in Geometry::Save in order to prevent Spatial from saving the
		// LightingEffect m_spkLEffect to disk.
		protected ShaderEffectCollection _effects;

		#endregion

		#region Properties

		public IServiceProvider ServiceProvider
		{
			get;
			private set;
		}

		protected ContentManager Content
		{
			get
			{
				return (ContentManager) ServiceProvider.GetService(typeof(ContentManager));
			}
		}

		public Spatial Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		public ShaderEffectCollection Effects
		{
			get { return _effects; }
		}

		public GlobalStateCollection GlobalStates
		{
			get { return _globalStates; }
		}

		#endregion

		#region Constructor

		protected Spatial(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
			WorldBound = new BoundingSphere();
			Culling = CullingMode.Dynamic;
			Local = Matrix.Identity;
			WorldIsCurrent = false;
			WorldBoundIsCurrent = false;
			_parent = null;
			_globalStates = new GlobalStateCollection();
			_effects = new ShaderEffectCollection();
		}

		#endregion

		#region Methods

		public void UpdateGeometricState()
		{
			UpdateGeometricState(new GameTime(), true);
		}

		// Update of geometric state and controllers.  The UpdateGS function
		// computes world transformations on the downward pass and world bounding
		// volumes on the upward pass.  The UpdateBS function just computes the
		// world bounding volumes on an upward pass.  This is useful if model
		// data changes, causing the model and world bounds to change, but no
		// transformations need recomputing.
		public void UpdateGeometricState(GameTime gameTime, bool bInitiator)
		{
			UpdateWorldData(gameTime);
			UpdateWorldBound();
			if (bInitiator)
				PropagateBoundToRoot();
		}

		public void UpdateBoundState()
		{
			UpdateWorldBound();
			PropagateBoundToRoot();
		}

		// update of render state
		public void UpdateRenderState()
		{
			UpdateRenderState(null);
		}

		public void UpdateRenderState(GlobalStateStackCollection globalStateStacks)
		{
			bool bInitiator = (globalStateStacks == null);

			if (bInitiator)
			{
				// The order of preference is
				//   (1) Default global states are used.
				//   (2) Geometry can override them, but if global state FOOBAR
				//       has not been pushed to the Geometry leaf node, then
				//       the current FOOBAR remains in effect (rather than the
				//       default FOOBAR being used).
				//   (3) Effect can override default or Geometry render states.
				globalStateStacks = new GlobalStateStackCollection();

				// traverse to root and push states from root to this node
				PropagateStateFromRoot(globalStateStacks);
			}
			else
			{
				// push states at this node
				PushState(globalStateStacks);
			}

			// propagate the new state to the subtree rooted here
			UpdateState(globalStateStacks);

			if (!bInitiator)
			{
				// pop states at this node
				PopState(globalStateStacks);
			}
		}

		private void PropagateStateFromRoot(GlobalStateStackCollection globalStateStacks)
		{
			// traverse to root to allow downward state propagation
			if (_parent != null)
				_parent.PropagateStateFromRoot(globalStateStacks);

			// push states onto current render state stack
			PushState(globalStateStacks);
		}

		private void PushState(GlobalStateStackCollection globalStateStacks)
		{
			globalStateStacks.PushState(_globalStates);
		}

		private void PopState(GlobalStateStackCollection globalStateStacks)
		{
			globalStateStacks.PopState(_globalStates);
		}

		protected abstract void UpdateState(GlobalStateStackCollection globalStateStacks);

		// geometric updates
		protected virtual void UpdateWorldData(GameTime gameTime)
		{
			// update any controllers associated with this object
			//UpdateControllers(gameTime);

			//foreach (GlobalState globalState in _globalStates)
			//	globalState.UpdateControllers(gameTime);

			// update world transforms
			if (!WorldIsCurrent)
				if (_parent != null)
					World = _parent.World * Local;
				else
					World = Local;
		}

		protected abstract void UpdateWorldBound();

		protected void PropagateBoundToRoot()
		{
			if (_parent != null)
			{
				_parent.UpdateWorldBound();
				_parent.PropagateBoundToRoot();
			}
		}

		#region Culling

		internal void OnGetVisibleSet(Culler culler, bool noCull)
		{
			if (Culling == CullingMode.Always)
				return;

			if (Culling == CullingMode.Never)
				noCull = true;

			if (noCull || culler.IsVisible(WorldBound))
				GetVisibleSet(culler, noCull);
		}

		protected virtual void GetVisibleSet(Culler culler, bool noCull)
		{
			foreach (ShaderEffect effect in _effects)
				culler.VisibleSet.VisibleShaderEffects.Add(new VisibleShaderEffect { ShaderEffect = effect, Spatial = this });
		}

		#endregion

		/// <summary>
		/// Creates effect from fragments; nodes can add fragments to the stack,
		/// while geometries build the shader from all fragments in the stack.
		/// </summary>
		public abstract void BuildShader(Stack<ShaderEffect> effects);

		protected virtual void PushEffects(Stack<ShaderEffect> effects)
		{
			foreach (ShaderEffect effect in _effects)
				effects.Push(effect);
		}

		protected virtual void PopEffects(Stack<ShaderEffect> effects)
		{
			foreach (ShaderEffect effect in _effects)
				effects.Pop();
		}

		#endregion

		#region Enums and classes

		// Culling parameters.
		public enum CullingMode
		{
			// Determine visibility state by comparing the world bounding volume
			// to culling planes.
			Dynamic,

			// Force the object to be culled.  If a Node is culled, its entire
			// subtree is culled.
			Always,

			// Never cull the object.  If a Node is never culled, its entire
			// subtree is never culled.  To accomplish this, the first time such
			// a Node is encountered, the bNoCull parameter is set to 'true' in
			// the recursive chain GetVisibleSet/OnGetVisibleSet.
			Never
		}

		public class GlobalStateCollection : IEnumerable<GlobalState>
		{
			private List<GlobalState> _storage;

			public GlobalState this[GlobalState.StateType type]
			{
				get
				{
					foreach (GlobalState globalState in _storage)
						if (globalState.Type == type)
							return globalState;
					return null;
				}
			}

			public GlobalStateCollection()
			{
				_storage = new List<GlobalState>();
			}

			public void Attach(GlobalState globalState)
			{
				// Check if this type of state is already in the list.
				for (int i = 0; i < _storage.Count; i++)
					if (_storage[i].Type == globalState.Type)
					{
						// This type of state already exists, so replace it.
						_storage[i] = globalState;
						return;
					}

				// This type of state is not in the current list, so add it.
				_storage.Add(globalState);
			}

			public void Detach(GlobalState.StateType stateType)
			{
				int index = _storage.FindIndex(delegate(GlobalState globalState) { return globalState.Type == stateType; });
				if (index != -1)
					_storage.RemoveAt(index);
			}

			public void DetachAll()
			{
				_storage.Clear();
			}

			public IEnumerator<GlobalState> GetEnumerator()
			{
				return _storage.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _storage.GetEnumerator();
			}
		}

		public class ShaderEffectCollection : IEnumerable<ShaderEffect>
		{
			private List<ShaderEffect> _storage;

			public int Count
			{
				get { return _storage.Count; }
			}

			public ShaderEffectCollection()
			{
				_storage = new List<ShaderEffect>();
			}

			public void Attach(ShaderEffect effect)
			{
				// Check if the effect is already in the list.
				foreach (ShaderEffect forEachEffect in _storage)
					if (forEachEffect == effect)
					{
						// The effect already exists, so do nothing.
						return;
					}

				// The effect is not in the current list, so add it.
				_storage.Add(effect);
			}

			public void Detach(ShaderEffect effect)
			{
				_storage.Remove(effect);
			}

			public void DetachAll()
			{
				_storage.Clear();
			}

			public IEnumerator<ShaderEffect> GetEnumerator()
			{
				return _storage.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _storage.GetEnumerator();
			}
		}

		public class GlobalStateStackCollection
		{
			private Stack<GlobalState>[] _storage;

			public Stack<GlobalState> this[int index]
			{
				get { return _storage[index]; }
			}

			public GlobalStateStackCollection()
			{
				_storage = new Stack<GlobalState>[(int) GlobalState.StateType.MAX];
				for (int i = 0; i < _storage.Length; i++)
				{
					_storage[i] = new Stack<GlobalState>();
					_storage[i].Push(null);
				}
			}

			public void PushState(GlobalStateCollection globalStates)
			{
				foreach (GlobalState globalState in globalStates)
					_storage[(int) globalState.Type].Push(globalState);
			}

			public void PopState(GlobalStateCollection globalStates)
			{
				foreach (GlobalState globalState in globalStates)
					_storage[(int) globalState.Type].Pop();
			}
		}

		#endregion
	}
}
