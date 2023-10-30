using System.IO;
using UnityEditor;
using UnityEditor.SettingsManagement;
using FloatSetting = LocaConstants.SettingsWrapper<float>;
using static LocaConstants.Settings;

namespace LocaConstants {
    /// <summary>
    /// Detect changes to our .json file with the c# native FileWatcher.
    /// This will create a background task on another thread to detect changes.
    /// With this we can avoid having to create an AssetPostprocessor.
    /// </summary>
    public class GeneratorFileWatcher {
        private static FileSystemWatcher watcher = null;
        private static bool fileChanged = false;
        private static string lastPathToKeys;
        private static double nextTick = 0;
        private static bool isInitialized = false;

        [UserSetting(generalCategory, nameof(tickRateInSeconds), "The tick rate to detect file changes in seconds (requires Unity restart).")]
        internal static FloatSetting tickRateInSeconds = new FloatSetting(generalCategoryKey + nameof(tickRateInSeconds), 3f);

        /// <summary>
        /// This methid will be executed automatically after recompilation.
        /// </summary>
        //[InitializeOnLoadMethod]
        private static void HookIntoEditorUpdate() {
            // we need to synchronize the context an therefor we need to be hooked into the editor update method.
            EditorApplication.update -= OnEditorApplicationUpdate;
            EditorApplication.update += OnEditorApplicationUpdate;
        }

        /// <summary>
        /// Called when a file change was detected.
        /// We can't execute any Unity APIs here!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnFileChange(object sender, FileSystemEventArgs e) {
            // just set a bool flag we'll check in our editor update method.
            fileChanged = true;
        }

        /// <summary>
        /// Called on every Editor update tick.
        /// </summary>
        private static void OnEditorApplicationUpdate() {
            // throttle ticks, since we are not that time dependent...
            if (EditorApplication.timeSinceStartup > nextTick) {
                nextTick = EditorApplication.timeSinceStartup + tickRateInSeconds.value;

                // are we initialized?
                if (!isInitialized) {
                    // no so initialize for the first time...
                    Initialize();
                    isInitialized = true;
                } else {
                    // did the settings path change? Re-initialize!
                    if (lastPathToKeys != Generator.pathToKeysJson.value) {
                        Initialize();
                    }
                    // was the fileChanged flag set?
                    if (fileChanged) {
                        // act on the file change!
                        HandleFileChange();
                        fileChanged = false;
                    }
                }
            }
        }

        /// <summary>
        /// Initialize & setup the file watcher
        /// </summary>
        private static void Initialize() {
            lastPathToKeys = Generator.pathToKeysJson.value;

            // make sure to dispose the current watcher properly...
            if (watcher != null) {
                watcher.Changed -= OnFileChange;
                watcher.Dispose();
                watcher = null;
            }

            // get the full path to the file we want to watch.
            string fullpath = Path.GetFullPath(lastPathToKeys);

            // only do something if the file actually exists...
            if (File.Exists(fullpath)) {
                // create a new watcher
                watcher = new FileSystemWatcher(Path.GetDirectoryName(fullpath), Path.GetFileName(fullpath));
                
                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                
                // Begin watching
                watcher.EnableRaisingEvents = true;
                
                // setup the callback when the file changed.
                watcher.Changed += OnFileChange;
            }
        }

        /// <summary>
        /// Executed method when a file change was recognized.
        /// </summary>
        private static void HandleFileChange() {
            // call the generator action to create the relevant code.
            Generator.CreateConstantsAction();
        }
    }
}
