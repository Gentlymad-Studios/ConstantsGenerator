using EditorHelper;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ConstantsGenerator {
	/// <summary>
	/// Settings provider, so our settings show up in the default ProjectSettings panel.
	/// </summary>
	public class SettingsProvider: ScriptableSingletonProviderBase {
		private static readonly string[] Tags = new string[] { nameof(ConstantsGenerator), "Constants", "Generator"};

		/// <summary>
		/// Creates the custom settings provider for Component Tools.
		/// </summary>
		[SettingsProvider]
		public static UnityEditor.SettingsProvider CreateMyCustomSettingsProvider() {
			return Settings.instance ? new SettingsProvider() : null;
		}

		public override dynamic GetInstance() {
			Settings.instance.OnEnable();
			return Settings.instance;
		}

		public override Type GetDataType() {
			return typeof(Settings);
		}

		protected override string GetHeader() {
			return nameof(ConstantsGenerator);
		}

		protected override EventCallback<SerializedPropertyChangeEvent> GetValueChangedCallback() {
			return ValueChanged;
		}

		private void ValueChanged(SerializedPropertyChangeEvent evt) {
			serializedObject.ApplyModifiedProperties();
			Settings.instance.Save();
		}


		/// <summary>
		/// Constructs a new instance of the SettingsProvider class.
		/// </summary>
		/// <param name="scope">The scope of the settings provider.</param>
		[UnityEngine.Tooltip("Constructs a new instance of the SettingsProvider class.")]
		public SettingsProvider(SettingsScope scope = SettingsScope.Project) : base(Settings.MENU_ITEM_BASE + nameof(ConstantsGenerator), scope) {
			keywords = Tags;
		}
	}
}
