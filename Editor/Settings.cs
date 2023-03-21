using UnityEditor;

namespace LocaConstants {
    /// <summary>
    /// Settings data based on UnityEditor.SettingsManagement package.
    /// </summary>
    static class Settings {
        // menus
        internal const string toolsBasepath = "Tools/";
        internal const string createConstantsActionMenu = toolsBasepath+"Create Constants";
        internal const string projectSettingsMenu = toolsBasepath + nameof(LocaConstants);

        // categories
        internal const string generalCategory = "General";
        internal const string generalCategoryKey = "general.";

        // package name
        internal const string packageName = "com.gentlymadstudios.locaconstants";

        /// <summary>
        /// The instance to our settings file.
        /// </summary>
        private static UnityEditor.SettingsManagement.Settings instance = null;
        internal static UnityEditor.SettingsManagement.Settings Instance {
            get {
                if (instance == null) {
                    instance = new UnityEditor.SettingsManagement.Settings(packageName);
                    Save();
                }

                return instance;
            }
        }

        public static void Save() {
            Instance.Save();
        }
    }
}
