using System;
using System.Linq;
using Osiris.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Shaders;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Osiris.Graphics.SceneGraph.Culling;
using Osiris.Graphics.SceneGraph.Geometries;
using Osiris.Graphics.SceneGraph.Nodes;
using Osiris.Graphics.Rendering.GlobalStates;
using Osiris.Graphics.Effects;
using System.Collections.Generic;
using Osiris.Graphics.Rendering.Cameras;

namespace Osiris.Graphics.Rendering
{
	public class Renderer
	{
		private GraphicsDevice _graphicsDevice;

		// The camera for establishing the view frustum.
		private Camera _camera;

		// Global render states.
    private GlobalStateCollection _globalStates;

		// The projector for various effects such as projected textures and
		// shadow maps.
		public Camera Projector
		{
			get;
			set;
		}

		public Camera Camera
		{
			get { return _camera; }
			set
			{
				if (_camera != null)
					_camera.Moved -= new EventHandler<CameraEventArgs>(OnCameraMoved);

				if (value != null)
					value.Moved += new EventHandler<CameraEventArgs>(OnCameraMoved);

				_camera = value;

				if (_camera != null)
					OnCameraMoved(this, new CameraEventArgs { Camera = _camera });
			}
		}

		// Transformations used in the geometric pipeline.
		private Matrix _worldMatrix, _saveWorldMatrix;
		private Matrix _projectionMatrix, _saveProjectionMatrix;
		private Matrix _viewMatrix, _saveViewMatrix;

		public bool ReverseCullFace
		{
			get;
			set;
		}

		public Renderer(GraphicsDevice graphicsDevice)
		{
			_graphicsDevice = graphicsDevice;

			_worldMatrix = Matrix.Identity;
			_saveWorldMatrix = Matrix.Identity;
			_viewMatrix = Matrix.Identity;
			_saveViewMatrix = Matrix.Identity;
			_projectionMatrix = Matrix.Identity;
			_saveProjectionMatrix = Matrix.Identity;

			_globalStates = new GlobalStateCollection();
		}

		private void OnCameraMoved(object sender, CameraEventArgs e)
		{
			_viewMatrix = _camera.ViewMatrix;
			_projectionMatrix = _camera.ProjectionMatrix;
			//_graphicsDevice.Viewport = _camera.Viewport; TODO
		}

		#region Global render state management

		private void SetGlobalState(GlobalStateCollection globalStates)
		{
			GlobalState globalState = globalStates[GlobalState.StateType.Alpha];
			if (globalState != null)
				SetAlphaState((AlphaState) globalState);

			globalState = globalStates[GlobalState.StateType.Cull];
			if (globalState != null)
				SetCullState((CullState) globalState);

			globalState = globalStates[GlobalState.StateType.PolygonOffset];
			if (globalState != null)
				SetPolygonOffsetState((PolygonOffsetState) globalState);

			globalState = globalStates[GlobalState.StateType.Stencil];
			if (globalState != null)
				SetStencilState((StencilState) globalState);

			globalState = globalStates[GlobalState.StateType.Wireframe];
			if (globalState != null)
				SetWireframeState((WireframeState) globalState);

			globalState = globalStates[GlobalState.StateType.DepthBuffer];
			if (globalState != null)
				SetDepthBufferState((DepthBufferState) globalState);
		}

		private void RestoreGlobalState(GlobalStateCollection globalStates)
		{
			if (globalStates[GlobalState.StateType.Alpha] != null)
				SetAlphaState((AlphaState) GlobalStateCollection.Default[GlobalState.StateType.Alpha]);

			if (globalStates[GlobalState.StateType.Cull] != null)
				SetCullState((CullState) GlobalStateCollection.Default[GlobalState.StateType.Cull]);

			if (globalStates[GlobalState.StateType.PolygonOffset] != null)
				SetPolygonOffsetState((PolygonOffsetState) GlobalStateCollection.Default[GlobalState.StateType.PolygonOffset]);

			if (globalStates[GlobalState.StateType.Stencil] != null)
				SetStencilState((StencilState) GlobalStateCollection.Default[GlobalState.StateType.Stencil]);

			if (globalStates[GlobalState.StateType.Wireframe] != null)
				SetWireframeState((WireframeState) GlobalStateCollection.Default[GlobalState.StateType.Wireframe]);

			if (globalStates[GlobalState.StateType.DepthBuffer] != null)
				SetDepthBufferState((DepthBufferState) GlobalStateCollection.Default[GlobalState.StateType.DepthBuffer]);
		}

		private void SetAlphaState(AlphaState state)
		{
			_globalStates[GlobalState.StateType.Alpha] = state;

			if (state.BlendEnabled)
			{
				_graphicsDevice.RenderState.AlphaBlendEnable = true;
				_graphicsDevice.RenderState.SourceBlend = state.SourceBlend;
				_graphicsDevice.RenderState.DestinationBlend = state.DestinationBlend;

				// TODO: support RenderState.BlendFactor
			}
			else
			{
				_graphicsDevice.RenderState.AlphaBlendEnable = false;
			}

			if (state.TestEnabled)
			{
				_graphicsDevice.RenderState.AlphaTestEnable = true;
				_graphicsDevice.RenderState.AlphaFunction = state.Test;
				_graphicsDevice.RenderState.ReferenceAlpha = state.Reference;
			}
			else
			{
				_graphicsDevice.RenderState.AlphaTestEnable = false;
			}
		}

		private void SetCullState(CullState state)
		{
			_globalStates[GlobalState.StateType.Cull] = state;

			if (state.Enabled)
				if (ReverseCullFace)
					if (state.CullFace == CullMode.CullClockwiseFace)
						_graphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
					else
						_graphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
				else
					_graphicsDevice.RenderState.CullMode = state.CullFace;
			else
				_graphicsDevice.RenderState.CullMode = CullMode.None;
		}

		private void SetPolygonOffsetState(PolygonOffsetState state)
		{
			_globalStates[GlobalState.StateType.PolygonOffset] = state;

			if (state.FillEnabled)
			{
				_graphicsDevice.RenderState.SlopeScaleDepthBias = state.Scale;

				// TO DO.  The renderer currently always creates a 24-bit depth
				// buffer.  If the precision changes, the adjustment to depth bias
				// must depend on the bits of precision.  The divisor is 2^n for n
				// bits of precision.
				float bias = state.Bias / 16777216.0f;
				_graphicsDevice.RenderState.DepthBias = bias;
			}
			else
			{
				_graphicsDevice.RenderState.SlopeScaleDepthBias = 0;
				_graphicsDevice.RenderState.DepthBias = 0;
			}
		}

		private void SetStencilState(StencilState state)
		{
			_globalStates[GlobalState.StateType.Stencil] = state;

			if (state.Enabled)
			{
				_graphicsDevice.RenderState.StencilEnable = true;
				_graphicsDevice.RenderState.StencilFunction = state.Compare;
				_graphicsDevice.RenderState.ReferenceStencil = state.Reference;
				_graphicsDevice.RenderState.StencilMask = state.Mask;
				_graphicsDevice.RenderState.StencilWriteMask = state.WriteMask;
				_graphicsDevice.RenderState.StencilFail = state.OnFail;
				_graphicsDevice.RenderState.StencilDepthBufferFail = state.OnDepthBufferFail;
				_graphicsDevice.RenderState.StencilPass = state.OnDepthBufferPass;
			}
			else
			{
				_graphicsDevice.RenderState.StencilEnable = false;
			}
		}

		private void SetWireframeState(WireframeState state)
		{
			_globalStates[GlobalState.StateType.Wireframe] = state;

			if (state.Enabled)
				_graphicsDevice.RenderState.FillMode = FillMode.WireFrame;
			else
				_graphicsDevice.RenderState.FillMode = FillMode.Solid;
		}

		private void SetDepthBufferState(DepthBufferState state)
		{
			_globalStates[GlobalState.StateType.DepthBuffer] = state;

			if (state.Enabled)
				_graphicsDevice.RenderState.DepthBufferFunction = state.Compare;
			else
				_graphicsDevice.RenderState.DepthBufferFunction = CompareFunction.Always;

			_graphicsDevice.RenderState.DepthBufferWriteEnable = state.Writable;
		}

		#endregion

		private void SetPostWorldTransformation(Matrix matrix)
		{
			_saveViewMatrix = _viewMatrix;
			_viewMatrix = matrix * _saveViewMatrix;
		}

		private void RestorePostWorldTransformation()
		{
			_viewMatrix = _saveViewMatrix;
		}

		#region Drawing functions

		public void DrawScene(VisibleSet visibleSet)
		{
			// By the time we come in here all shaders have been compiled,
			// and associated with the leaf Geometry objects.

			// Realisation:
			// During PreProcess, only effects are drawn (and they draw their child geometries).
			// During Main, only geometries are drawn.

			// A geometry may have many different PreProcess pass effects, one for each ShaderEffect.
			// However, it will only ever have one Main pass effect.

			// We make an explicit design decision that shader effects will never influence each other;
			// for example, if you have both an environment map and a shadow map, then shadows will
			// never be visible in the environment map. This avoids recursion within shader effects.
			// This is a bit limiting, but for the moment it makes the code a lot simpler.

			// pre-process
			foreach (VisibleShaderEffect visibleEffect in visibleSet.VisibleShaderEffects)
				visibleEffect.ShaderEffect.DrawPreProcess(visibleEffect.Spatial);

			// main pass - group by geometry container
			var geometryGroups = visibleSet.VisibleGeometries.GroupBy(g => g.GeometryContainer).Select(g => new { GeometryContainer = g.Key, Geometries = g });
			foreach (var geometryGroup in geometryGroups)
			{
				_graphicsDevice.Indices = geometryGroup.GeometryContainer.IndexBuffer;
				foreach (var geometry in geometryGroup.Geometries)
					Draw(geometry);
			}
		}

		private void Draw(Geometry geometry)
		{
			SetGlobalState(geometry.States);
			_worldMatrix = geometry.World;

			_graphicsDevice.Vertices[0].SetSource(geometry.GeometryContainer.VertexBuffer, 0, geometry.VertexStride);
			_graphicsDevice.VertexDeclaration = geometry.VertexDeclaration;

			Shader shader = geometry.GetShader();
			SetRendererConstants(shader);

			shader.Effect.Begin();

			foreach (EffectPass pass in shader.Effect.CurrentTechnique.Passes)
			{
				pass.Begin();

				_graphicsDevice.DrawIndexedPrimitives(geometry.Type, geometry.BaseVertex, 0,
					geometry.NumVertices, geometry.StartIndex, geometry.PrimitiveCount);

				pass.End();
			}

			shader.Effect.End();

			RestoreGlobalState(geometry.States);
		}

		private void SetRendererConstants(Shader shader)
		{
			foreach (KeyValuePair<string, EffectParameter> pair in shader.RendererConstants)
			{
				switch (pair.Key)
				{
					case "WORLD" :
						pair.Value.SetValue(_worldMatrix);
						break;
					case "WORLDVIEWPROJECTION" :
						pair.Value.SetValue(_worldMatrix * _viewMatrix * _projectionMatrix);
						break;
					case "CAMERAPOSITION" :
						pair.Value.SetValue(_camera.Position);
						break;
					default :
						throw new NotImplementedException();
				}
			}
		}

		#endregion
	}
}
