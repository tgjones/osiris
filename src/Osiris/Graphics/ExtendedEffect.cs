using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Content;

namespace Osiris.Graphics
{
	/// <summary>
	/// Summary description for EffectInstance.
	/// </summary>
	public class ExtendedEffect
	{
		#region Variables

		private Effect _effect;
		private Hashtable _effectHandles;

		#endregion

		#region Properties

		public EffectTechnique CurrentTechnique
		{
			get { return _effect.CurrentTechnique; }
		}

		public Effect InnerEffect
		{
			get { return _effect; }
		}

		#endregion

		#region Constructor

		public ExtendedEffect(Effect effect)
		{
			_effect = effect;

			// set technique
			//_effect.CurrentTechnique = _effect.Techniques[0];

			// cache effect handles
			_effectHandles = new Hashtable();
		}

		#endregion

		#region Methods

		#region Set value methods

		private EffectParameter GetEffectHandle(string parameterName)
		{
			if (_effectHandles.Contains(parameterName))
			{
				return (EffectParameter) _effectHandles[parameterName];
			}
			else
			{
				EffectParameter effectHandle = _effect.Parameters[parameterName];
				_effectHandles.Add(parameterName, effectHandle);
				return effectHandle;
			}
		}

		public void SetValue(string sParameterName, Matrix tMatrix)
		{
			GetEffectHandle(sParameterName).SetValue(tMatrix);
		}

		public void SetValue(string sParameterName, Vector2 tVector)
		{
			GetEffectHandle(sParameterName).SetValue(tVector);
		}

		public void SetValue(string sParameterName, Vector3 tVector)
		{
			GetEffectHandle(sParameterName).SetValue(tVector);
		}

		public void SetValue(string sParameterName, Vector4 tVector)
		{
			GetEffectHandle(sParameterName).SetValue(tVector);
		}

		public void SetValue(string sParameterName, Texture pTexture)
		{
			GetEffectHandle(sParameterName).SetValue(pTexture);
		}

		public void SetValue(string sParameterName, float fValue)
		{
			GetEffectHandle(sParameterName).SetValue(fValue);
		}

		public void SetValue(string sParameterName, bool value)
		{
			GetEffectHandle(sParameterName).SetValue(value);
		}

		public void SetValue(string sParameterName, Color value)
		{
			GetEffectHandle(sParameterName).SetValue(value.ToVector4());
		}

		#endregion

		public void ChangeTechnique(string name)
		{
			_effect.CurrentTechnique = _effect.Techniques[name];
		}

		public void Begin()
		{
			_effect.Begin(SaveStateMode.SaveState);
		}

		public void End()
		{
			_effect.End();
		}

		public void CommitChanges()
		{
			_effect.CommitChanges();
		}

		#endregion
	}
}