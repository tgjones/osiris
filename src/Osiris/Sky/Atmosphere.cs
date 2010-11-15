using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Cameras;
using System.ComponentModel;
using Osiris.Graphics;

namespace Osiris.Sky
{
	public class Atmosphere : GameComponent, IAtmosphereService
	{
		#region Fields

		private const float EARTH_RADIUS = 6.378e6f;
		//private const float EARTH_RADIUS = 20000;
		private const float ATMOSPHERE_DEPTH = EARTH_RADIUS * 0.025f;
		private const float SKYDOME_RADIUS = EARTH_RADIUS + ATMOSPHERE_DEPTH;

		/// <summary>
		/// Rayleigh scattering constant
		/// </summary>
		private float _kr;

		/// <summary>
		/// Mie scattering constant
		/// </summary>
		private float _km;

		/// <summary>
		/// Sun brightness constant
		/// </summary>
		private float _eSun;

		/// <summary>
		/// Mie phase asymmetry factor
		/// </summary>
		private float _g;

		//private float _exposure;

		private float _innerRadius;
		private float _outerRadius;
		private float _scale;
		private float[] _wavelength;
		private float[] _wavelength4;
		private float _rayleighScaleDepth;

		//private float _exposure;

		#endregion

		#region Properties

		/// <summary>
		/// Rayleigh scattering constant
		/// </summary>
		[Description("Rayleigh scattering constant")]
		public float Kr
		{
			get { return _kr; }
			set { _kr = value; }
		}

		private float Kr4PI
		{
			get { return _kr * 4.0f * MathHelper.Pi; }
		}

		/// <summary>
		/// Mie scattering constant
		/// </summary>
		[Description("Mie scattering constant")]
		public float Km
		{
			get { return _km; }
			set { _km = value; }
		}

		private float Km4PI
		{
			get { return _km * 4.0f * MathHelper.Pi; }
		}

		/// <summary>
		/// Mie phase asymmetry factor
		/// </summary>
		[Description("Mie phase asymmetry factor")]
		public float G
		{
			get { return _g; }
			set { _g = value; }
		}

		public float EarthRadius
		{
			get { return EARTH_RADIUS; }
		}

		public float AtmosphereDepth
		{
			get { return ATMOSPHERE_DEPTH; }
		}

		public float SkydomeRadius
		{
			get { return SKYDOME_RADIUS; }
		}

		#endregion

		#region Constructor

		public Atmosphere(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(IAtmosphereService), this);
			game.Components.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(Components_ComponentRemoved);

			_kr = 0.0025f;
			_km = 0.0010f;
			_eSun = 20.0f;
			_g = -0.990f;
			//_exposure = 2.0f;

			_innerRadius = EARTH_RADIUS;
			_outerRadius = SKYDOME_RADIUS;
			_scale = 1 / (_outerRadius - _innerRadius);

			_wavelength = new float[3];
			_wavelength[0] = 0.650f; // 650nm for red
			_wavelength[1] = 0.570f; // 570nm for green
			_wavelength[2] = 0.475f; // 475nm for blue

			_wavelength4 = new float[3];
			_wavelength4[0] = MathsHelper.Pow(_wavelength[0], 4.0f);
			_wavelength4[1] = MathsHelper.Pow(_wavelength[1], 4.0f);
			_wavelength4[2] = MathsHelper.Pow(_wavelength[2], 4.0f);

			_rayleighScaleDepth = 0.25f;
		}

		private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
		{
			if (e.GameComponent == this)
				Game.Services.RemoveService(typeof(IAtmosphereService));
		}

		#endregion

		#region Methods

		protected T GetService<T>()
		{
			return (T) this.Game.Services.GetService(typeof(T));
		}

		public void SetEffectParameters(ExtendedEffect effect, bool fudgePosition)
		{
			ICameraService camera = GetService<ICameraService>();
			Vector3 cameraPosition = camera.Position;
			cameraPosition.Y += EARTH_RADIUS;
			if (true || fudgePosition)
			{
				//cameraPosition.Y /= SKYDOME_RADIUS;
				//cameraPosition.X = cameraPosition.Z = 0;
				//cameraPosition.Y *= _innerRadius;
				//cameraPosition.Y += _innerRadius;
			}
			else
			{
				//cameraPosition.X /= SKYDOME_RADIUS;
				//cameraPosition.Y /= SKYDOME_RADIUS;
				//cameraPosition.Z /= SKYDOME_RADIUS;
			}

			effect.SetValue("v3CameraPos", cameraPosition);
			effect.SetValue("v3InvWavelength", new Vector3(1 / _wavelength4[0], 1 / _wavelength4[1], 1 / _wavelength4[2]));
			effect.SetValue("fCameraHeight", cameraPosition.Length());
			effect.SetValue("fCameraHeight2", cameraPosition.LengthSquared());
			effect.SetValue("fInnerRadius", _innerRadius);
			effect.SetValue("fInnerRadius2", _innerRadius * _innerRadius);
			effect.SetValue("fOuterRadius", _outerRadius);
			effect.SetValue("fOuterRadius2", _outerRadius * _outerRadius);
			effect.SetValue("fKrESun", _kr * _eSun);
			effect.SetValue("fKmESun", _km * _eSun);
			effect.SetValue("fKr4PI", Kr4PI);
			effect.SetValue("fKm4PI", Km4PI);
			effect.SetValue("fScale", 1.0f / (_outerRadius - _innerRadius));
			effect.SetValue("fScaleDepth", _rayleighScaleDepth);
			effect.SetValue("fInvScaleDepth", 1.0f / _rayleighScaleDepth);
			effect.SetValue("fScaleOverScaleDepth", (1.0f / (_outerRadius - _innerRadius)) / _rayleighScaleDepth);
			effect.SetValue("fSkydomeRadius", SKYDOME_RADIUS);
			effect.SetValue("g", _g);
			effect.SetValue("g2", _g * _g);
		}

		#endregion
	}
}
