//#define DEEP_COPY

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Shaders
{
	/// <summary>
	/// Entry point for loading compiled shader fragments.
	/// </summary>
	public class ShaderCatalog : List<Shader>, IShaderCatalogService
	{
		public Shader GetShader(GraphicsDevice graphicsDevice, List<ShaderFragmentRequest> fragmentRequests, VertexElement[] vertexElements)
		{
			// Try to find a shader that matches the incoming criteria.
			Shader shader = this.SingleOrDefault(s =>
				CompareFragments(s.CompiledFragments, fragmentRequests) && CompareVertexElements(s.VertexElements, vertexElements)
			);

			// Error handling.
			if (shader == null)
				throw new Exception("A shader matching these fragments and vertex elements could not be found");

			// Clone shader.
			Shader shaderCopy = CloneShader(shader, fragmentRequests, graphicsDevice);
			return shaderCopy;
		}

		/// <summary>
		/// Compares the names in the lists of fragments.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		private static bool CompareFragments(CompiledShaderFragment[] left, List<ShaderFragmentRequest> right)
		{
			if (left.Length != right.Count)
				return false;

			// The list in "right" may not be ordered in the same way as "left".
			for (int i = 0; i < left.Length; i++)
				if (!right.Any(r => r.Name == left[i].Name))
					return false;

			return true;
		}

		private static bool CompareVertexElements(VertexElement[] left, VertexElement[] right)
		{
			if (left.Length != right.Length)
				return false;

			for (int i = 0; i < left.Length; i++)
				if (left[i].UsageIndex != right[i].UsageIndex || left[i].VertexElementFormat != right[i].VertexElementFormat || left[i].VertexElementUsage != right[i].VertexElementUsage)
					return false;

			return true;
		}

		private Shader CloneShader(Shader shader, List<ShaderFragmentRequest> fragmentRequests, GraphicsDevice graphicsDevice)
		{
			// Clone each part of the shader.
#if DEEP_COPY
			Effect effectCopy = shader.Effect.Clone(graphicsDevice);
#else
			Effect effectCopy = shader.Effect;
#endif

			Dictionary<string, EffectParameter> rendererConstantsCopy = shader.RendererConstants.Select(
				rc => effectCopy.Parameters[rc.Value.Name])
				.ToDictionary(p => p.Semantic);

			List<ShaderFragmentRequest> fragmentRequestsCopy = new List<ShaderFragmentRequest>(fragmentRequests);
			List<CompiledShaderFragment> compiledFragments = new List<CompiledShaderFragment>();
			foreach (CompiledShaderFragment originalFragment in shader.CompiledFragments)
			{
				// We might have more than request with the same name,
				// for example if we've got multiple directional lights.
				ShaderFragmentRequest request = fragmentRequestsCopy.First(r => r.Name == originalFragment.Name);
				List<EffectParameter> effectParametersCopy = originalFragment.EffectParameters.Values.Select(p => effectCopy.Parameters[p.Name]).ToList();
				compiledFragments.Add(new CompiledShaderFragment(request.ShaderEffect, effectParametersCopy, originalFragment.MangledNamePrefix, originalFragment.Name));

				fragmentRequestsCopy.Remove(request);
			}

			Shader shaderCopy = new Shader(effectCopy, rendererConstantsCopy, compiledFragments.ToArray(), shader.VertexElements);
			return shaderCopy;
		}
	}
}
