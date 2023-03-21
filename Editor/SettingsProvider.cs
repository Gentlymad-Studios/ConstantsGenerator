using UnityEditor;
using UnityEditor.SettingsManagement;

namespace LocaConstants {
    /// <summary>
    /// Settings provider, so our settings show up in the default ProjectSettings panel.
    /// </summary>
    static class SettingsProvider {
#pragma warning disable IDE0051 // Remove unused private members
        [SettingsProvider]
        private static UnityEditor.SettingsProvider CreateSettingsProvider() {
            UserSettingsProvider provider = new UserSettingsProvider(Settings.projectSettingsMenu, Settings.Instance, new[] { typeof(SettingsProvider).Assembly }, SettingsScope.Project);
            return provider;
        }
#pragma warning restore IDE0051 // Remove unused private members
    }
}
