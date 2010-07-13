using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Osiris.Graphics.Effects;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Reflection;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Osiris.Content.Pipeline.Graphics.Shaders
{
	public static class ShaderGenerator
	{
		public static CompiledShaderContent CreateFromFragments(List<ShaderFragmentContent> fragments, VertexElementContent[] vertexElements, ContentProcessorContext context)
		{
			//System.Diagnostics.Debugger.Launch();

			// TODO: Verify that vertex elements are compatible with vertex input requirements of all fragments.

			// First sort fragments according to class.
			List<ShaderFragmentContent> sortedFragments = fragments.OrderBy(f => (int) f.Class).ToList();

			// Give each fragment a unique name.
			AssignUniqueNames(sortedFragments);

			// Populate shader code.
			string shaderCode = GetShaderCode(vertexElements, sortedFragments);

			// Write out shader code for debugging purposes.
			string debugFile = Path.Combine(Path.GetTempPath(), "C2E3EE3B-8792-4564-9531-DA976141D80D.fx");
			File.WriteAllText(debugFile, shaderCode);

			// Compile the shader.
			CompiledEffect compiledEffect = CompileShader(context, shaderCode);

			// Check for errors.
			DoSomethingAwesomeWithErrorsAndWarnings(compiledEffect.Success, compiledEffect.ErrorsAndWarnings, debugFile, context);

			// Get a collection of renderer constants and other shader parameters.
			Dictionary<string, string> rendererConstants;
			List<CompiledShaderFragmentContent> compiledShaderFragments;
			GetShaderParameters(sortedFragments, out rendererConstants, out compiledShaderFragments);

			// Create result object.
			CompiledShaderContent result = new CompiledShaderContent();
			result.CompiledShaderFragments = compiledShaderFragments;
			result.CompiledEffect = compiledEffect;
			result.RendererConstants = rendererConstants;

			result.VertexElements = vertexElements.Select(e => new VertexElement
				{
					VertexElementFormat = e.VertexElementFormat,
					VertexElementUsage = e.VertexElementUsage,
					UsageIndex = e.UsageIndex
				}
			).ToArray();

			return result;
		}

		#region Helper methods

		private static void AssignUniqueNames(List<ShaderFragmentContent> fragments)
		{
			int index = 0;
			fragments.ForEach(f => f.UniqueName = f.Name.Replace(".", string.Empty) + (index++).ToString());
		}

		#region Shader code building methods

		private static string GetShaderCode(VertexElementContent[] vertexElements, List<ShaderFragmentContent> sortedFragments)
		{
			string shaderCode = GetTemplate();

			// Populate the data types used in the shader.
			PopulateParameterDeclarations(sortedFragments, ref shaderCode);
			PopulateTextureDeclarations(sortedFragments, ref shaderCode);
			PopulateVertexInputStructure(vertexElements, ref shaderCode);
			PopulateVertexOutputStructure(sortedFragments, ref shaderCode);
			PopulatePixelInputStructure(sortedFragments, ref shaderCode);

			// Populate helper functions.
			PopulateHelperFunctions(sortedFragments, ref shaderCode);

			// Populate the functions, as defined in each fragment.
			PopulateShaderFunctions(sortedFragments, ShaderType.Vertex, "VERTEX_SHADER_FUNCTIONS", ref shaderCode);
			PopulateShaderFunctions(sortedFragments, ShaderType.Pixel, "PIXEL_SHADER_FUNCTIONS", ref shaderCode);

			// Populate the entry points into the vertex and pixel shaders.
			PopulateVertexShaderMainBody(sortedFragments, ref shaderCode);
			PopulatePixelShaderMainBody(sortedFragments, ref shaderCode);

			return shaderCode;
		}

		private static string GetTemplate()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream templateStream = assembly.GetManifestResourceStream(typeof(ShaderGenerator), "ShaderTemplate.txt");
			StreamReader textStreamReader = new StreamReader(templateStream);
			string templateContent = textStreamReader.ReadToEnd();
			textStreamReader.Close();
			return templateContent;
		}

		private static void PopulateParameterDeclarations(List<ShaderFragmentContent> fragments, ref string shaderCode)
		{
			StringBuilder result = new StringBuilder();

			fragments.Where(f => f.Parameters.Length > 0).ToList().ForEach(f =>
				{
					result.AppendLine(string.Format("// {0} parameters", f.UniqueName));
					foreach (ShaderParameterContent parameter in f.Parameters)
						if (!string.IsNullOrEmpty(parameter.Semantic))
							result.AppendLine(string.Format("const {0} {1}{2} : {3};", parameter.DataType, f.UniqueName, parameter.Name, parameter.Semantic));
						else
							result.AppendLine(string.Format("const {0} {1}{2};", parameter.DataType, f.UniqueName, parameter.Name));
					result.AppendLine();
				}
			);

			shaderCode = shaderCode.Replace("[[*** PARAMETERS ***]]", result.ToString());
		}

		private static void PopulateTextureDeclarations(List<ShaderFragmentContent> fragments, ref string shaderCode)
		{
			StringBuilder result = new StringBuilder();

			fragments.Where(f => f.Textures.Length > 0).ToList().ForEach(f =>
				{
					result.AppendLine(string.Format("// {0} textures", f.UniqueName));
					foreach (ShaderTextureContent texture in f.Textures)
					{
						result.AppendLine(string.Format("texture {0}{1};", f.UniqueName, texture.Name));
						result.AppendLine(string.Format("{0} {1}{2}=", texture.SamplerDataType, f.UniqueName, texture.SamplerName));
						result.AppendLine("sampler_state");
						result.AppendLine("{");
						result.AppendLine(string.Format("\tTexture = <{0}{1}>;", f.UniqueName, texture.Name));
						result.AppendLine(string.Format("\tMipFilter = {0};", texture.MipFilter));
						result.AppendLine(string.Format("\tMinFilter = {0};", texture.MinFilter));
						result.AppendLine(string.Format("\tMagFilter = {0};", texture.MagFilter));
						result.AppendLine(string.Format("\tAddressU = {0};", texture.AddressU));
						result.AppendLine(string.Format("\tAddressV = {0};", texture.AddressV));
						result.AppendLine("};");
						result.AppendLine();
					}
					result.AppendLine();
				}
			);

			shaderCode = shaderCode.Replace("[[*** TEXTURES ***]]", result.ToString());
		}

		private static void PopulateVertexInputStructure(VertexElementContent[] vertexElements, ref string shaderCode)
		{
			StringBuilder result = new StringBuilder();

			foreach (VertexElementContent vertexElement in vertexElements)
				result.AppendLine(
					string.Format("\t{0} {1} : {2}{3};",
						GetHlslType(vertexElement.VertexElementFormat),
						vertexElement.VertexElementUsage,
						GetHlslSemantic(vertexElement.VertexElementUsage),
						vertexElement.UsageIndex));

			shaderCode = shaderCode.Replace("[[*** VERTEX_INPUT ***]]", result.ToString());
		}

		private static void PopulateVertexOutputStructure(List<ShaderFragmentContent> fragments, ref string shaderCode)
		{
			StringBuilder result = new StringBuilder();

			// Create VertexOutput structs for each fragment.
			int texCoordIndex = 0;
			fragments.Where(f => f.Interpolators.Length > 0).ToList().ForEach(f =>
			{
				result.AppendLine(string.Format("struct {0}VertexOutput", f.UniqueName));
				result.AppendLine("{");
				foreach (ShaderParameterContent interpolator in f.Interpolators)
				{
					string semantic = (!string.IsNullOrEmpty(interpolator.Semantic)) ? interpolator.Semantic : "TEXCOORD" + (texCoordIndex++).ToString();
					result.AppendLine("\t" + string.Format("{0} {1} : {2};", interpolator.DataType, interpolator.Name, semantic));
				}
				result.AppendLine("};");
				result.AppendLine();
			}
			);

			// Create VertexOutput struct which contains the individual fragment structs.
			result.AppendLine("struct VertexOutput");
			result.AppendLine("{");
			fragments.Where(f => f.Interpolators.Length > 0).ToList().ForEach(f => result.AppendLine(string.Format("\t{0}VertexOutput {0};", f.UniqueName)));
			result.AppendLine("};");
			result.AppendLine();

			shaderCode = shaderCode.Replace("[[*** VERTEX_OUTPUT ***]]", result.ToString());
		}

		/// <summary>
		/// This method is very similar to PopulateVertexOutputStructure, but when I tried combining
		/// them the code was quite tortuous, so to make things clearer I've left them as two
		/// separate methods.
		/// </summary>
		/// <param name="fragments"></param>
		/// <param name="shaderCode"></param>
		private static void PopulatePixelInputStructure(List<ShaderFragmentContent> fragments, ref string shaderCode)
		{
			StringBuilder result = new StringBuilder();

			// Create PixelInput structs for each fragment.
			int texCoordIndex = 0;
			fragments.Where(f => f.Interpolators.Count(p => p.Semantic != "POSITION") > 0).ToList().ForEach(f =>
				{
					result.AppendLine(string.Format("struct {0}PixelInput", f.UniqueName));
					result.AppendLine("{");
					foreach (ShaderParameterContent interpolator in f.Interpolators)
					{
						string semantic = (!string.IsNullOrEmpty(interpolator.Semantic)) ? interpolator.Semantic : "TEXCOORD" + (texCoordIndex++).ToString();
						if (semantic != "POSITION")
							result.AppendLine("\t" + string.Format("{0} {1} : {2};", interpolator.DataType, interpolator.Name, semantic));
					}
					result.AppendLine("};");
					result.AppendLine();
				}
			);

			// Create PixelOutput struct which contains the individual fragment structs.
			result.AppendLine("struct PixelInput");
			result.AppendLine("{");
			fragments.Where(f => f.Interpolators.Any(p => p.Semantic != "POSITION")).ToList().ForEach(f =>
				result.AppendLine(string.Format("\t{0}PixelInput {0};", f.UniqueName))
			);
			result.AppendLine("};");
			result.AppendLine();

			// Declare global instance of PixelInput which can be referenced by any fragment's pixel program.
			result.AppendLine("PixelInput gPixelInput;");
			result.AppendLine();

			shaderCode = shaderCode.Replace("[[*** PIXEL_INPUT ***]]", result.ToString());
		}

		private static void PopulateHelperFunctions(List<ShaderFragmentContent> fragments, ref string shaderCode)
		{
			StringBuilder result = new StringBuilder();

			fragments.Where(f => f.Parameters.Length > 0).ToList().ForEach(f =>
				{
					if (!string.IsNullOrEmpty(f.Functions))
					{
						result.AppendLine(string.Format("// {0} helper functions", f.UniqueName));
						result.AppendLine(f.Functions);
						result.AppendLine();
					}
				}
			);

			shaderCode = shaderCode.Replace("[[*** HELPER_FUNCTIONS ***]]", result.ToString());
		}

		/// <summary>
		/// Vertex or pixel programs can contain special metafunction calls. These metafunctions
		/// handle passing of variables to other fragments. There are two metafunctions:
		/// 
		/// export(TYPE, NAME, VALUE);
		/// import(NAME, OPERATION NAME OPERATION);
		/// </summary>
		/// <param name="fragments"></param>
		/// <param name="shaderType"></param>
		/// <param name="shaderCode"></param>
		private static void PopulateShaderFunctions(List<ShaderFragmentContent> fragments, ShaderType shaderType, string templateReplacementName, ref string shaderCode)
		{
			StringBuilder result = new StringBuilder();

			// We keep track of what has been exported using this dictionary.
			Dictionary<string, List<string>> exports = new Dictionary<string, List<string>>();

			fragments.ForEach(f =>
			{
				// Not all fragments have both vertex and pixel programs.
				string program = f.GetShaderProgram(shaderType);
				if (!string.IsNullOrEmpty(program))
				{
					// Replace parameters and sampler names which are used in the code with the mangled names.
					f.Parameters.ToList().ForEach(p =>
						program = Regex.Replace(program, @"([^\w\.])(" + p.Name + @")(\W)", "$1" + f.UniqueName + "$2$3"));
					f.Textures.ToList().ForEach(t =>
						program = Regex.Replace(program, @"(\W)(" + t.SamplerName + @")(\W)", "$1" + f.UniqueName + "$2$3"));

					result.AppendLine(string.Format("// {0} shader {1}", shaderType.ToString().ToLower(), f.UniqueName));

					// Check program for exports.
					const string exportPattern = @"export\((?<TYPE>[\w]+), (?<NAME>[\w]+), (?<VALUE>[\s\S]+?)\);";
					MatchCollection exportMatches = Regex.Matches(program, exportPattern);
					foreach (Match match in exportMatches)
					{
						// Values might be exporting used the same name by multiple fragments; in this case, we should declare
						// a variable for each fragment that exports the value. But if the same fragment exports a value multiple
						// times, we should only declare the variable once.
						string exportName = match.Groups["NAME"].Value;
						if (!exports.ContainsKey(exportName))
							exports[exportName] = new List<string>();

						string variableName = string.Format("{0}_Export_{1}", f.UniqueName, exportName);
						if (!exports[exportName].Contains(variableName))
						{
							result.AppendLine(string.Format("{0} {1}; // exported value", match.Groups["TYPE"].Value, variableName));
							exports[exportName].Add(variableName);
						}
					}

					program = program.Replace("void main(", string.Format("void {0}_{1}ShaderFragment(", f.UniqueName, shaderType));
					program = program.Replace(" main()", string.Format(" {0}_{1}ShaderFragment()", f.UniqueName, shaderType));
					program = program.Replace(shaderType.ToString().ToUpper() + "_INPUT", string.Format("{0}{1}Input", f.UniqueName, shaderType));
					program = program.Replace(shaderType.ToString().ToUpper() + "_OUTPUT", string.Format("{0}{1}Output", f.UniqueName, shaderType));
					program = Regex.Replace(program, exportPattern, string.Format("// metafunction: $0\n\t{0}_Export_${{NAME}} = ${{VALUE}};", f.UniqueName));

					// Check program for imports.
					const string importPattern = @"import\((?<NAME>[\w]+), (?<OPERATION>[\s\S]+?)\);";
					program = Regex.Replace(program, importPattern,
						m =>
						{
							// Look up full variable names from matched name
							List<string> variableNames = exports[m.Groups["NAME"].Value];

							string replacement = "// metafunction: $0";
							foreach (string variableName in variableNames)
								replacement += string.Format("\n\t{0};",
									Regex.Replace(m.Groups["OPERATION"].Value, @"(\W)(" + m.Groups["NAME"].Value + @")(\W|$)", "$1" + variableName + "$3")
								);

							return m.Result(replacement);
						}
					);

					result.AppendLine(program);
					result.AppendLine();
				}
			});

			shaderCode = shaderCode.Replace("[[*** " + templateReplacementName + " ***]]", result.ToString());
		}

		private static void PopulateVertexShaderMainBody(List<ShaderFragmentContent> fragments, ref string shaderCode)
		{
			StringBuilder result = new StringBuilder();

			fragments.ForEach(f =>
			{
				if (!string.IsNullOrEmpty(f.GetShaderProgram(ShaderType.Vertex)))
					result.AppendLine(string.Format("\toutput.{0} = {0}_VertexShaderFragment();", f.UniqueName));
			});

			shaderCode = shaderCode.Replace("[[*** VERTEX_SHADER_MAIN_BODY ***]]", result.ToString());
		}

		private static void PopulatePixelShaderMainBody(List<ShaderFragmentContent> fragments, ref string shaderCode)
		{
			StringBuilder result = new StringBuilder();

			fragments.ForEach(f =>
			{
				if (!string.IsNullOrEmpty(f.GetShaderProgram(ShaderType.Pixel)))
					if (f.Interpolators.Length > 0)
						result.AppendLine(string.Format("\t{0}_PixelShaderFragment(gPixelInput.{0}, output);", f.UniqueName));
					else
						result.AppendLine(string.Format("\t{0}_PixelShaderFragment(output);", f.UniqueName));
			});

			shaderCode = shaderCode.Replace("[[*** PIXEL_SHADER_MAIN_BODY ***]]", result.ToString());
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

		#endregion

		private static CompiledEffect CompileShader(ContentProcessorContext context, string shaderCode)
		{
			CompiledEffect compiledEffect = Effect.CompileEffectFromSource(shaderCode, null, null, CompilerOptions.None, context.TargetPlatform);
			return compiledEffect;
		}

		private static void DoSomethingAwesomeWithErrorsAndWarnings(bool success, string errorsAndWarnings, string debugFile, ContentProcessorContext context)
		{
			if (!success || !string.IsNullOrEmpty(errorsAndWarnings))
			{
				if (success)
				{
					/*foreach (string errorAndWarning in errorsAndWarnings.Split(new string[] { "\n", "\r", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
					{
						string error = errorAndWarning;
						ContentIdentity contentIdentity = CreateContentIdentityFromErrorString(ref error, input.Identity);
						string message = string.Format(CultureInfo.CurrentCulture, Resources.EffectProcessorWarningCompilingEffect, new object[] { debugFile, error });
						context.Logger.LogWarning(null, contentIdentity, message, new object[0]);
					}*/
				}
				else
				{
					string errorString = errorsAndWarnings;
					ContentIdentity identity = CreateContentIdentityFromErrorString(ref errorString, debugFile);
					errorsAndWarnings = errorsAndWarnings.Trim();
					InvalidContentException exception = new InvalidContentException(
						string.Format("Errors compiling auto-generated shader:{0}{1}", new object[] { Environment.NewLine, errorsAndWarnings }),
						identity);
					throw exception;
				}
			}
		}

		private static ContentIdentity CreateContentIdentityFromErrorString(ref string errorString, string debugFile)
		{
			MatchCollection matchs = new Regex(@"(.*)\((\d*)\)\s*: ?(.*)").Matches(errorString);
			if (((matchs.Count == 0) || !matchs[0].Success) || (matchs[0].Groups.Count != 4))
				throw new Exception(errorString);

			ContentIdentity identity = new ContentIdentity();
			identity.SourceFilename = debugFile;
			string str = matchs[0].Groups[2].Value;
			if (!string.IsNullOrEmpty(str))
				identity.FragmentIdentifier = str;
			errorString = matchs[0].Groups[3].Value;
			return identity;
		}

		private static void GetShaderParameters(List<ShaderFragmentContent> sortedFragments,
			out Dictionary<string, string> rendererConstants,
			out List<CompiledShaderFragmentContent> compiledShaderFragments)
		{
			// Store renderer constants (parameters which have a semantic).
			rendererConstants = new Dictionary<string, string>();
			Dictionary<string, string> tempRendererConstants = rendererConstants;
			sortedFragments.ForEach(f =>
			{
				foreach (ShaderParameterContent parameter in f.Parameters)
					if (!string.IsNullOrEmpty(parameter.Semantic))
						tempRendererConstants.Add(parameter.Semantic, f.UniqueName + parameter.Name);
			});

			// Store a link between the original fragment and the compiled version. When we load
			// the shader at runtime, this will allow us to hook up the effect parameters properly.
			compiledShaderFragments = new List<CompiledShaderFragmentContent>();
			List<CompiledShaderFragmentContent> tempCompiledShaderFragments = compiledShaderFragments;
			sortedFragments.ForEach(f =>
			{
				// Get compiled effect parameters for this fragment.
				List<string> effectParameters = f.Parameters
					.Where(p => string.IsNullOrEmpty(p.Semantic))
					.Select(p => f.UniqueName + p.Name)
					.Concat(f.Textures.Select(t => f.UniqueName + t.Name))
					.ToList();

				// Create a compiled shader fragment object, which stores the original fragment name
				// as well as the mangled name prefix which this fragment has in the compiled version.
				CompiledShaderFragmentContent compiledShaderFragment = new CompiledShaderFragmentContent();
				compiledShaderFragment.EffectParameters = effectParameters;
				compiledShaderFragment.MangledNamePrefix = f.UniqueName;
				compiledShaderFragment.Name = f.Name;

				tempCompiledShaderFragments.Add(compiledShaderFragment);
			});
		}

		#endregion
	}
}