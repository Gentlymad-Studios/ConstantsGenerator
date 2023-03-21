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
                }

                return instance;
            }
        }

        public static void Save() {
            Instance.Save();
        }

        public static T Get<T>(string key, SettingsScope scope = SettingsScope.Project, T fallback = default) {
            return Instance.Get(key, scope, fallback);
        }

        public static void Set<T>(string key, T value, SettingsScope scope = SettingsScope.Project) {
            Instance.Set(key, value, scope);
        }

        public static bool ContainsKey<T>(string key, SettingsScope scope = SettingsScope.Project) {
            return Instance.ContainsKey<T>(key, scope);
        }
    }
}
