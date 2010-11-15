using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Osiris.Graphics.Shaders;
using System.Text.RegularExpressions;

namespace Osiris.Content.Pipeline.Processors
{
	[ContentProcessor(DisplayName = "Shader Fragment - Osiris Framework")]
	public class ShaderFragmentProcessor : ContentProcessor<ShaderFragment, ShaderFragment>
	{
		public override ShaderFragment Process(ShaderFragment input, ContentProcessorContext context)
		{
			// Verify that we can compile vertex and pixel shaders. The compiled code is not used
			// for anything other than verification.
			//VerifyShaderProgram(input.VertexProgram, ShaderProfile.VS_3_0, input, context);
			//VerifyShaderProgram(input.PixelProgram, ShaderProfile.PS_3_0, input, context);

			return input;
		}

		private static void VerifyShaderProgram(string program, ShaderProfile profile, ShaderFragment input, ContentProcessorContext context)
		{
			if (!string.IsNullOrEmpty(program))
			{
				CompiledShader compiledShader = ShaderCompiler.CompileFromSource(program, null, null, CompilerOptions.None, "main", profile, TargetPlatform.Windows);
				if (!compiledShader.Success)
					DoSomethingAwesomeWithErrorsAndWarnings(compiledShader.Success, compiledShader.ErrorsAndWarnings, input, context);
			}
		}

		private static void DoSomethingAwesomeWithErrorsAndWarnings(bool success, string errorsAndWarnings, ShaderFragment input, ContentProcessorContext context)
		{
			if (!success || !string.IsNullOrEmpty(errorsAndWarnings))
			{
				if (success)
				{
					foreach (string errorAndWarning in errorsAndWarnings.Split(new string[] { "\n", "\r", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
					{
						string formattedErrorAndWarning = errorAndWarning;
						ContentIdentity contentIdentity = CreateContentIdentityFromErrorString(ref formattedErrorAndWarning, input.Identity);
						string message = string.Format("Warning compiling {0}: {1}", new object[] { input.Identity.SourceFilename, formattedErrorAndWarning });
						context.Logger.LogWarning("ShaderFragmentProcessor_WarningsOrErrorsWhenCompilingFragment", contentIdentity, message, new object[0]);
					}
				}
				else
				{
					string errorString = errorsAndWarnings;
					ContentIdentity identity = CreateContentIdentityFromErrorString(ref errorString, input.Identity);
					errorsAndWarnings = errorsAndWarnings.Trim();
					InvalidContentException exception = new InvalidContentException(string.Format("Errors compiling {0}:{1}{2}",
						new object[] { input.Identity.SourceFilename, Environment.NewLine, errorsAndWarnings }), identity);
					exception.HelpLink = "ShaderFragmentProcessor_WarningsOrErrorsWhenCompilingFragment";
					throw exception;
				}
			}
		}

		private static ContentIdentity CreateContentIdentityFromErrorString(ref string errorString, ContentIdentity oldContentId)
		{
			MatchCollection matchs = new Regex(@"(.*)\((\d*)\)\s*: ?(.*)").Matches(errorString);
			if (((matchs.Count == 0) || !matchs[0].Success) || (matchs[0].Groups.Count != 4))
			{
				return oldContentId;
			}
			ContentIdentity identity = new ContentIdentity();
			identity.SourceTool = oldContentId.SourceTool;
			string str2 = matchs[0].Groups[1].Value;
			if (string.IsNullOrEmpty(str2))
			{
				identity.SourceFilename = oldContentId.SourceFilename;
			}
			else
			{
				identity.SourceFilename = str2;
			}
			string str = matchs[0].Groups[2].Value;
			if (!string.IsNullOrEmpty(str))
			{
				identity.FragmentIdentifier = str;
			}
			errorString = matchs[0].Groups[3].Value;
			return identity;
		}
	}
}