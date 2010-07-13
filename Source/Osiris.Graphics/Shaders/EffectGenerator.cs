using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Osiris.Graphics.Shaders
{
	public class EffectGenerator
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fragments"></param>
		/// <returns>Source code for effect file; could be saved as a .fx file and compiled.</returns>
		public static void CreateFromFragments(GraphicsDevice graphicsDevice, ShaderFragment[] fragments, VertexElement[] vertexElements, out Shader shader, out CompiledShaderFragment[] compiledFragments)
		{
			// First sort fragments according to class.
			ShaderFragment[] sortedFragments = fragments.OrderBy(f => (int) f.Class).ToArray();

			StringBuilder shaderCode = new StringBuilder();

			CreateParameterDeclarations(sortedFragments, shaderCode);
			CreateTextureDeclarations(sortedFragments, shaderCode);

			CreateVertexInputStructure(shaderCode, vertexElements);
			CreateShaderInputStructures(sortedFragments, shaderCode, "VertexOutput");
			CreateShaderInputStructures(sortedFragments, shaderCode, "PixelInput");
			CreatePixelOutputStructure(shaderCode);

			GetProgram getVertexProgramHandler = delegate(ShaderFragment fragment) { return fragment.VertexProgram; };
			GetProgram getPixelProgramHandler = delegate(ShaderFragment fragment) { return fragment.PixelProgram; };

			// core of the shader program
			CreateShaderMethods(sortedFragments, shaderCode, "Vertex", getVertexProgramHandler);
			CreateShaderMethods(sortedFragments, shaderCode, "Pixel", getPixelProgramHandler);

			CreateShaderMainBody(sortedFragments, shaderCode, "Vertex", getVertexProgramHandler);
			CreateShaderMainBody(sortedFragments, shaderCode, "Pixel", getPixelProgramHandler);

			CreateTechnique(shaderCode);

			// compile the effect
			string shaderCodeString = shaderCode.ToString();
			CompiledEffect compiledEffect = Effect.CompileEffectFromSource(shaderCodeString,
				null, null, CompilerOptions.Debug, TargetPlatform.Windows);

			// Check for errors.
			if (!compiledEffect.Success)
				throw new Exception(compiledEffect.ErrorsAndWarnings);

			// Map original fragment parameters to the mangled compiled versions.
			// Also extract parameters which have semantics attached
			shader = new Shader(graphicsDevice, compiledEffect);

			Dictionary<string, EffectParameter> rendererConstants = new Dictionary<string, EffectParameter>();

			Shader tempShader = shader;
			List<CompiledShaderFragment> tempCompiledFragments = new List<CompiledShaderFragment>();
			CreateFragmentCode(sortedFragments,
				delegate(ShaderFragment fragment, string uniqueName)
				{
					CompiledShaderFragment compiledShaderFragment = new CompiledShaderFragment(tempShader, fragment, uniqueName);
					tempCompiledFragments.Add(compiledShaderFragment);

					foreach (ShaderParameter parameter in fragment.Parameters)
						if (!string.IsNullOrEmpty(parameter.Semantic))
							rendererConstants.Add(parameter.Semantic, compiledShaderFragment.GetParameter(parameter.Name));
				});

			shader.RendererConstants = rendererConstants;

			compiledFragments = tempCompiledFragments.ToArray();
		}

		private static void CreateParameterDeclarations(ShaderFragment[] fragments, StringBuilder shaderCode)
		{
			BeginSection(shaderCode, "Parameters");

			// parameters
			CreateFragmentCode(fragments,
				delegate(ShaderFragment fragment, string uniqueName)
				{
					if (fragment.Parameters.Length > 0)
					{
						shaderCode.AppendLine(string.Format("// {0} parameters", uniqueName));
						foreach (ShaderParameter parameter in fragment.Parameters)
							shaderCode.AppendLine(string.Format("const {0} {1}{2};", parameter.DataType, uniqueName, parameter.Name));
						shaderCode.AppendLine();
					}
				});

			EndSection(shaderCode, "Parameters");
		}

		private static void CreateTextureDeclarations(ShaderFragment[] fragments, StringBuilder shaderCode)
		{
			BeginSection(shaderCode, "Textures");

			// textures
			CreateFragmentCode(fragments,
				delegate(ShaderFragment fragment, string uniqueName)
				{
					if (fragment.Textures.Length > 0)
					{
						shaderCode.AppendLine(string.Format("// {0} textures", uniqueName));
						foreach (ShaderTexture texture in fragment.Textures)
							shaderCode.AppendLine(string.Format("sampler {0}{1}", uniqueName, texture.Name));
						shaderCode.AppendLine();
					}
				});

			EndSection(shaderCode, "Textures");
		}

		private static void CreateTechnique(StringBuilder shaderCode)
		{
			BeginSection(shaderCode, "Technique");

			shaderCode.AppendLine("technique");
			shaderCode.AppendLine("{");
			shaderCode.AppendLine("\tpass");
			shaderCode.AppendLine("\t{");
			shaderCode.AppendLine("\t\tVertexShader = compile vs_3_0 VertexShader();");
			shaderCode.AppendLine("\t\tPixelShader = compile ps_3_0 PixelShader();");
			shaderCode.AppendLine("\t}");
			shaderCode.AppendLine("}");
			shaderCode.AppendLine();

			EndSection(shaderCode, "Technique");
		}

		private static void CreateShaderMethods(ShaderFragment[] fragments, StringBuilder shaderCode, string pixelOrVertex, GetProgram getProgramHandler)
		{
			// "metafunctions" - imports and exports
			// exports will be in the form:
			// export(TYPE, NAME, VALUE);
			// imports will be in the form:
			// import(NAME, OPERATION NAME OPERATION);

			BeginSection(shaderCode, pixelOrVertex + " Shader Functions");

			Dictionary<string, string> exports = new Dictionary<string, string>();
			CreateFragmentCode(fragments,
				delegate(ShaderFragment fragment, string uniqueName)
				{
					string program = getProgramHandler(fragment);
					if (!string.IsNullOrEmpty(program))
					{
						// Replace parameters which are used in the code with the mangled names.
						foreach (ShaderParameter parameter in fragment.Parameters)
							program = Regex.Replace(program, @"(\W)(" + parameter.Name + @")(\W)", "$1" + uniqueName + "$2$3");

						shaderCode.AppendLine(string.Format("// {0} shader {1}", pixelOrVertex.ToLower(), uniqueName));

						// check program for exports
						string exportPattern = @"export\((?<TYPE>[\w]+), (?<NAME>[\w]+), (?<VALUE>[\s\S]+?)\);";
						string importPattern = @"import\((?<NAME>[\w]+), (?<OPERATION>[\s\S]+?)\);";
						MatchCollection exportMatches = Regex.Matches(program, exportPattern);
						foreach (Match match in exportMatches)
						{
							string variableName = string.Format("{0}_Export_{1}", uniqueName, match.Groups["NAME"].Value);
							shaderCode.AppendLine(string.Format("{0} {1}; // exported value", match.Groups["TYPE"].Value, variableName));
							exports.Add(match.Groups["NAME"].Value, variableName);
						}

						program = program.Replace("void main(", string.Format("void {0}_{1}ShaderFragment(", uniqueName, pixelOrVertex));
						program = program.Replace(" main()", string.Format(" {0}_{1}ShaderFragment()", uniqueName, pixelOrVertex));
						program = program.Replace(pixelOrVertex.ToUpper() + "_INPUT", string.Format("{0}{1}Input", uniqueName, pixelOrVertex));
						program = program.Replace(pixelOrVertex.ToUpper() + "_OUTPUT", string.Format("{0}{1}Output", uniqueName, pixelOrVertex));
						program = Regex.Replace(program, exportPattern, string.Format("// metafunction: $0\n\t{0}_Export_${{NAME}} = ${{VALUE}};", uniqueName));

						program = Regex.Replace(program, importPattern,
							delegate(Match match)
							{
								// look up full variable name from matched name
								string variableName = exports[match.Groups["NAME"].Value];

								return match.Result(
									string.Format("// metafunction: $0\n\t{0};",
										Regex.Replace(match.Groups["OPERATION"].Value, @"(\W)(" + match.Groups["NAME"].Value + @")(\W|$)", "$1" + variableName + "$3")
									)
								);
							});

						shaderCode.AppendLine(program);
						shaderCode.AppendLine();
					}
				});
		}

		private static void CreateShaderMainBody(ShaderFragment[] fragments, StringBuilder shaderCode, string pixelOrVertex, GetProgram getProgramHandler)
		{
			BeginSection(shaderCode, pixelOrVertex + " Shader Entry Point");

			// main body of the pixel shader
			shaderCode.AppendLine(string.Format("{0}Output {0}Shader(const {0}Input input)", pixelOrVertex));
			shaderCode.AppendLine("{");
			shaderCode.AppendLine(string.Format("\tg{0}Input = input;", pixelOrVertex));
			shaderCode.AppendLine();
			shaderCode.AppendLine(string.Format("\t{0}Output output = ({0}Output) 0;", pixelOrVertex));
			shaderCode.AppendLine();
			CreateFragmentCode(fragments,
				delegate(ShaderFragment fragment, string uniqueName)
				{
					string program = getProgramHandler(fragment);
					if (!string.IsNullOrEmpty(program))
					{
						if (pixelOrVertex == "Pixel")
							if (fragment.Interpolators.Length > 0)
								shaderCode.AppendLine(string.Format("\t{0}_{1}ShaderFragment(g{1}Input.{0}, output);", uniqueName, pixelOrVertex));
							else
								shaderCode.AppendLine(string.Format("\t{0}_{1}ShaderFragment(output);", uniqueName, pixelOrVertex));
						else
							shaderCode.AppendLine(string.Format("\toutput.{0} = {0}_{1}ShaderFragment();", uniqueName, pixelOrVertex));
					}
				});
			shaderCode.AppendLine();
			shaderCode.AppendLine("\treturn output;");
			shaderCode.AppendLine("}");

			EndSection(shaderCode, pixelOrVertex + " Shader Entry Point");
		}

		private static void CreateVertexInputStructure(StringBuilder shaderCode, VertexElement[] vertexElements)
		{
			BeginSection(shaderCode, "Vertex Input Structure");

			shaderCode.AppendLine("struct VertexInput");
			shaderCode.AppendLine("{");
			foreach (VertexElement vertexElement in vertexElements)
			{
				shaderCode.AppendLine(
					string.Format("\t{0} {1} : {2};",
						GetHlslType(vertexElement.VertexElementFormat),
						vertexElement.VertexElementUsage,
						GetHlslSemantic(vertexElement.VertexElementUsage) + vertexElement.UsageIndex.ToString()));
			}
			shaderCode.AppendLine("};");
			shaderCode.AppendLine();
			shaderCode.AppendLine("VertexInput gVertexInput;");
			shaderCode.AppendLine();

			EndSection(shaderCode, "Vertex Input Structure");
		}

		private static void CreateShaderInputStructures(ShaderFragment[] fragments, StringBuilder shaderCode, string pixelOrVertexInputOrOutput)
		{
			BeginSection(shaderCode, pixelOrVertexInputOrOutput + " Structures");

			int texCoordIndex = 0;
			CreateFragmentCode(fragments,
				delegate(ShaderFragment fragment, string uniqueName)
				{
					if (fragment.Interpolators.Count(p => pixelOrVertexInputOrOutput == "VertexOutput" || p.Semantic != "POSITION") > 0)
					{
						shaderCode.AppendLine(string.Format("struct {0}{1}", uniqueName, pixelOrVertexInputOrOutput));
						shaderCode.AppendLine("{");
						foreach (ShaderParameter interpolator in fragment.Interpolators)
						{
							string semantic = (!string.IsNullOrEmpty(interpolator.Semantic)) ? interpolator.Semantic : "TEXCOORD" + (texCoordIndex++).ToString();
							if (pixelOrVertexInputOrOutput == "VertexOutput" || semantic != "POSITION")
								shaderCode.AppendLine("\t" + string.Format("{0} {1} : {2};", interpolator.DataType, interpolator.Name, semantic));
						}
						shaderCode.AppendLine("};");
						shaderCode.AppendLine();
					}
				});
			shaderCode.AppendLine(string.Format("struct {0}", pixelOrVertexInputOrOutput));
			shaderCode.AppendLine("{");
			CreateFragmentCode(fragments,
				delegate(ShaderFragment fragment, string uniqueName)
				{
					if (fragment.Interpolators.Count(p => pixelOrVertexInputOrOutput == "VertexOutput" || p.Semantic != "POSITION") > 0)
						shaderCode.AppendLine(string.Format("\t{0}{1} {0};", uniqueName, pixelOrVertexInputOrOutput));
				});
			shaderCode.AppendLine("};");
			shaderCode.AppendLine();
			if (pixelOrVertexInputOrOutput == "PixelInput")
			{
				shaderCode.AppendLine(string.Format("{0} g{0};", pixelOrVertexInputOrOutput));
				shaderCode.AppendLine();
			}

			EndSection(shaderCode, pixelOrVertexInputOrOutput + " Structures");
		}

		private static void CreatePixelOutputStructure(StringBuilder shaderCode)
		{
			BeginSection(shaderCode, "Pixel Output Structure");

			// output structure
			shaderCode.AppendLine("struct PixelOutput");
			shaderCode.AppendLine("{");
			shaderCode.AppendLine("\tfloat4 Colour : COLOR0;");
			shaderCode.AppendLine("};");
			shaderCode.AppendLine();

			EndSection(shaderCode, "Pixel Output Structure");
		}

		private static void BeginSection(StringBuilder shaderCode, string name)
		{
			shaderCode.AppendLine("//**************************************************************");
			shaderCode.AppendLine(string.Format("// Begin {0}", name));
			shaderCode.AppendLine("//**************************************************************");
			shaderCode.AppendLine();
		}

		private static void EndSection(StringBuilder shaderCode, string name)
		{
			shaderCode.AppendLine("//**************************************************************");
			shaderCode.AppendLine(string.Format("// End {0}", name));
			shaderCode.AppendLine("//**************************************************************");
			shaderCode.AppendLine();
		}

		private static void CreateFragmentCode(ShaderFragment[] fragments, ProcessFragment fragmentProcessor)
		{
			int index = 0;
			foreach (ShaderFragment fragment in fragments)
			{
				string uniqueName = fragment.Name + index.ToString();
				fragmentProcessor(fragment, uniqueName);
				++index;
			}
		}

		private static string GetHlslType(VertexElementFormat format)
		{
			switch (format)
			{
				case VertexElementFormat.Color:
				case VertexElementFormat.Vector4:
					return "float4";
				case VertexElementFormat.HalfVector2:
					return "half2";
				case VertexElementFormat.HalfVector4:
					return "half4";
				case VertexElementFormat.Vector2:
					return "float2";
				case VertexElementFormat.Vector3:
					return "float3";
				default:
					throw new NotImplementedException();
			}
		}

		private static string GetHlslSemantic(VertexElementUsage usage)
		{
			switch (usage)
			{
				case VertexElementUsage.TextureCoordinate:
					return "TEXCOORD";
				default:
					return usage.ToString().ToUpper();
			}
		}

		private delegate void ProcessFragment(ShaderFragment fragment, string uniqueName);
		private delegate string GetProgram(ShaderFragment fragment);
	}
}