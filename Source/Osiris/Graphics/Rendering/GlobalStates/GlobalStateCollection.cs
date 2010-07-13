using System;
using System.Collections.Generic;

namespace Osiris.Graphics.Rendering.GlobalStates
{
	public class GlobalStateCollection
	{
		private Dictionary<GlobalState.StateType, GlobalState> _storage;

		public GlobalState this[GlobalState.StateType type]
		{
			get { return _storage[type]; }
			set { _storage[type] = value; }
		}

		public static GlobalStateCollection Default
		{
			get;
			private set;
		}

		static GlobalStateCollection()
		{
			Default = new GlobalStateCollection();
		}

		public GlobalStateCollection()
		{
			_storage = new Dictionary<GlobalState.StateType, GlobalState>((int) GlobalState.StateType.MAX);
		}
	}
}
