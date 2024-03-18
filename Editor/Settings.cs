using System;
using UnityEditor;
using UnityEngine;

namespace ConstantsGenerator {
	/// <summary>
	/// Settings file for component tool settings.
	/// Add your individual settings class here if you want to have it automatically show up in the project settings.
	/// </summary>
	[FilePath("ProjectSettings/" + nameof(ConstantsGenerator) + ".asset", FilePathAttribute.Location.ProjectFolder)]
	public class Settings : ScriptableSingleton<Settings> {

		/// <summary>
		/// The base menu item for the component tools.
		/// </summary>
		public const string MENU_ITEM_BASE = "Tools/";
		public const string MENU_ITEM = "Tools/" + nameof(ConstantsGenerator);
		public const string MENU_ITEM_ALL = MENU_ITEM + "/Create All Constants";

		/// <summary>
		/// guid of the runtime c# template to use
		/// </summary>
		[Tooltip("guid of the runtime c# template to use")]
		public string guidOfRuntimeTemplate = "dac244c11bf72bd43af6d58170f76dae";
		/// <summary>
		/// guid of the editor only list template to use
		/// </summary>
		[Tooltip("guid of the editor only list template to use")]
		public string guidOfListTemplate = "8a1de681a322f8c4d859436039274167";
		/// <summary>
		/// guid of the lookup table to use
		/// </summary>
		[Tooltip("guid of the lookup table to use")]
		public string guidOfLookupTemplate = "296667c959620de48936f0102855f028";

		/// <summary>
		/// This adapter allows to specify custom logic for our conversion actions
		/// </summary>
		[SerializeReference]
		[Tooltip("This adapter allows to specify custom logic for our conversion actions")]
		public AdapterBase adapter = null;


		[Serializable]
		public class GeneratorItem {
			// logic
			public string logicID;
			public bool isActive = true;
			public bool createEditorOnlyLookup = true;

			// C# Generation
			public string classNameList = "List";
			public string classNameLookup = "Lookup";
			public string classNameKeys = "Keys";
			public string @namespace = "Generator";
			public string namespaceRuntime = "Generator.Runtime";

			// inputs
			public string searchPath = "";

			// outputs
			public string outputPath = "Generated/Localization";
			public string outputEditorPath = "Generated/Localization/Editor";
		}

		/// <summary>
		/// List of all generators that should be used
		/// </summary>
		[Tooltip("List of all generators that should be used")]
		public GeneratorItem[] generators;

		/// <summary>
		/// Enables the settings object.
		/// </summary>
		public void OnEnable() {
			hideFlags &= ~HideFlags.NotEditable;
		}

		/// <summary>
		/// Saves the settings object.
		/// </summary>
		public void Save() {
			Save(true);
		}
	}
}
