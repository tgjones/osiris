using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Shaders
{
	public interface IShaderCatalogService
	{
		Shader GetShader(GraphicsDevice graphicsDevice, List<ShaderFragmentRequest> fragmentRequests, VertexElement[] vertexElements);
	}
}
