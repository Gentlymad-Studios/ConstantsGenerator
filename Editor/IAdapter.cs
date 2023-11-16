using System;
using System.Collections.Generic;

namespace ConstantsGenerator {

	using static Settings;

	public interface IAdapter {
		Dictionary<string, Action<GeneratorItem>> GetConversionActions();
	}
}
