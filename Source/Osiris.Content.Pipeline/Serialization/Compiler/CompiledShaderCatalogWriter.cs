using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Osiris.Content.Pipeline.Graphics.Shaders;
using Osiris.Graphics.Shaders;

namespace Osiris.Content.Pipeline.Serialization.Compiler
{
	[ContentTypeWriter]
	public class CompiledShaderCatalogWriter : ContentTypeWriter<CompiledShaderCatalogContent>
	{
		protected override void Write(ContentWriter output, CompiledShaderCatalogContent value)
		{
			output.Write(value.Count);
			foreach (CompiledShaderContent item in value)
			{
				output.WriteObject(item.CompiledEffect);
				output.WriteObject(item.RendererConstants);
				output.Write(item.CompiledShaderFragments.Count);
				foreach (CompiledShaderFragmentContent compiledFragment in item.CompiledShaderFragments)
				{
					output.WriteObject(compiledFragment.EffectParameters);
					output.Write(compiledFragment.MangledNamePrefix);
					output.Write(compiledFragment.Name);
				}
				output.WriteObject(item.VertexElements);
			}
		}

		public override string GetRuntimeType(TargetPlatform targetPlatform)
		{
			return typeof(ShaderCatalog).AssemblyQualifiedName;
		}

		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return typeof(ShaderCatalogReader).AssemblyQualifiedName;
		}
	}
}