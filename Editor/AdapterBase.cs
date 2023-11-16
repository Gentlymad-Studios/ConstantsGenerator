using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConstantsGenerator {

	using static Settings;

	public abstract class AdapterBase : ScriptableObject, IAdapter {
		public abstract Dictionary<string, Action<GeneratorItem>> GetConversionActions();
	}
}
