using UnityEditor.SettingsManagement;
using StringSetting = LocaConstants.SettingsWrapper<string>;
using static LocaConstants.Settings;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace LocaConstants {
    /// <summary>
    /// Code generation class to convert a .json with Loca Keys into concrete c# declarations.
    /// </summary>
    public class Generator {

        private const string paramSign = "%";
        private const string commentSign = "//";
        private const string commentSignAndparamSign = commentSign+paramSign;
        private const string constantLine = commentSignAndparamSign + "0";
        private const string lookupEntryLine = commentSignAndparamSign + "1";
        private const string constKey = paramSign + nameof(constKey);
        private const string constValue = paramSign + nameof(constValue);
        private const string keysEntry = commentSignAndparamSign + nameof(keysEntry);
        private const string keysLookupEntry = commentSignAndparamSign + nameof(keysLookupEntry);
        private const string className = commentSignAndparamSign + nameof(className);

        [UserSetting(generalCategory, nameof(pathToKeysJson), "The path to the keys .json")]
        internal static StringSetting pathToKeysJson = new StringSetting(generalCategoryKey + nameof(pathToKeysJson), "Assets/PackageEditor/Languages/All Languages_Editor.json");
        [UserSetting(generalCategory, nameof(guidOfRuntimeTemplate), "The guid to the C# template file that should be used for all constants.")]
        internal static StringSetting guidOfRuntimeTemplate = new StringSetting(generalCategoryKey + nameof(guidOfRuntimeTemplate), "f48d54b919a0e3646b996be8a91f53fc");
        [UserSetting(generalCategory, nameof(guidOfListTemplate), "The guid to the C# template file used for generating a list of all constants.")]
        internal static StringSetting guidOfListTemplate = new StringSetting(generalCategoryKey + nameof(guidOfListTemplate), "fcd75c38619fd0145bf7a6e1b20174eb");
        [UserSetting(generalCategory, nameof(guidOfLookupTemplate), "The guid to the C# template file used for generating a dicitonary lookup of all constants.")]
        internal static StringSetting guidOfLookupTemplate = new StringSetting(generalCategoryKey + nameof(guidOfLookupTemplate), "9d5f11f97d1a65e458740e647f2e3ee5");
        [UserSetting(generalCategory, nameof(outputPath), "The path where the generated .cs file should be outputted.")]
        internal static StringSetting outputPath = new StringSetting(generalCategoryKey + nameof(outputPath), "Generated/Localization");
        [UserSetting(generalCategory, nameof(outputEditorPath), "The path where the generated editor only .cs files should be outputted.")]
        internal static StringSetting outputEditorPath = new StringSetting(generalCategoryKey + nameof(outputEditorPath), "Generated/Localization/Editor");

        internal static StringBuilder runtimeFileContent = null;
        internal static StringBuilder listFileContent = null;
        internal static StringBuilder lookupFileContent = null;

        internal static string dataPath = null;
        internal static bool isGenerating = false;

        internal static string keysJson = null;
        internal static string outputPathCached = null;
        internal static string outputEditorPathCached = null;
        internal static GeneratorLocaModel keys = null;

        internal static string runtimeFilename = null;
        internal static string pathToRuntimeTemplate = null;

        internal static string listFilename = null;
        internal static string pathToListTemplate = null;

        internal static string lookupFilename = null;
        internal static string pathToLookupTemplate = null;

        /// <summary>
        /// action to execute/initiate the code generation process.
        /// </summary>
        [MenuItem(createConstantsActionMenu)]
        internal static void CreateConstantsAction() {
            CreateConstantsAsync();
        }

        /// <summary>
        /// The asynchronouse code generation process
        /// </summary>
        private static async void CreateConstantsAsync() {
            // do nothing if we are already creating files...
            if (isGenerating) {
                return;
            }
            
            // cache all the relevant data
            UpdateSources();
            
            // run the creation task asynchronously
            await Task.Run(() => CreateConstants());
            
            // refresh the asset database after the change
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Actual constant creation process.
        /// </summary>
        private static void CreateConstants() {
            isGenerating = true;
            CreateFileContents();
            CreateScriptFiles();
            isGenerating = false;
        }

        /// <summary>
        /// Cache all thread unsafe data.
        /// </summary>
        private static void UpdateSources() {
            // get the data path
            dataPath = Application.dataPath;

            // runtime script paths
            string assetPathOfRuntimeTemplate = AssetDatabase.GUIDToAssetPath(guidOfRuntimeTemplate.value);
            runtimeFilename = Path.GetFileNameWithoutExtension(assetPathOfRuntimeTemplate);
            pathToRuntimeTemplate = Path.GetFullPath(assetPathOfRuntimeTemplate);

            // list script paths
            string assetPathOfListTemplate = AssetDatabase.GUIDToAssetPath(guidOfListTemplate.value);
            listFilename = Path.GetFileNameWithoutExtension(assetPathOfListTemplate);
            pathToListTemplate = Path.GetFullPath(assetPathOfListTemplate);

            // lookup script paths
            string assetPathOfLookupTemplate = AssetDatabase.GUIDToAssetPath(guidOfLookupTemplate.value);
            lookupFilename = Path.GetFileNameWithoutExtension(assetPathOfLookupTemplate);
            pathToLookupTemplate = Path.GetFullPath(assetPathOfLookupTemplate);

            // load the .json file
            keysJson = AssetDatabase.LoadAssetAtPath<TextAsset>(pathToKeysJson).text;
            
            // deserialize it (JSON.NET does not work in an asynchronous context)
            keys = JsonConvert.DeserializeObject<GeneratorLocaModel>(keysJson);
            
            // cache our paths that are based on our settings file.
            outputPathCached = outputPath.value;
            outputEditorPathCached= outputEditorPath.value;
        }

        /// <summary>
        /// Create the file contents.
        /// This includes creating the file contents for the contants, and editor only list and lookup table.
        /// </summary>
        private static void CreateFileContents() {
            string line = null;
            string constLineTemplate = null;
            string keysEntryTemplate = null;
            string keysLookupEntryTemplate = null;

            string constLineOriginal = null;
            string keysEntryOriginal = null;
            string keysLookupEntryOriginal = null;

            // go over every line of the runtime template
            runtimeFileContent = new StringBuilder();
            using (StreamReader file = new StreamReader(pathToRuntimeTemplate)) {
                while ((line = file.ReadLine()) != null) {
                    if (constLineTemplate == null && line.Contains(constantLine)) {
                        constLineTemplate = line.Replace(constantLine, "");
                        constLineOriginal = line;
                    }
                    runtimeFileContent.AppendLine(line);
                }
            }

            // go over every line of the list template
            listFileContent = new StringBuilder();
            using (StreamReader file = new StreamReader(pathToListTemplate)) {
                while ((line = file.ReadLine()) != null) {
                    if (keysEntryTemplate == null && line.Contains(keysEntry)) {
                        keysEntryTemplate = line.Replace(commentSignAndparamSign, "");
                        keysEntryOriginal = line;
                    }
                    listFileContent.AppendLine(line);
                }
            }

            // go over every line of the lookup template
            lookupFileContent = new StringBuilder();
            using (StreamReader file = new StreamReader(pathToLookupTemplate)) {
                while ((line = file.ReadLine()) != null) {
                    if (keysLookupEntryTemplate == null && line.Contains(lookupEntryLine)) {
                        keysLookupEntryTemplate = line.Replace(lookupEntryLine, "");
                        keysLookupEntryOriginal = line;
                    }
                    lookupFileContent.AppendLine(line);
                }
            }

            StringBuilder constLines = new StringBuilder();
            StringBuilder keyEntries = new StringBuilder();
            StringBuilder keyLookupEntries = new StringBuilder();

            // go overy every translation key
            string value;
            foreach (KeyValuePair<string, string> keyValuePair in keys.translations) {
                value = keyValuePair.Value;
                value = value.Replace(' ', '_');
                constLines.AppendLine(constLineTemplate.Replace(constKey, keyValuePair.Key).Replace(constValue, value));
                keyEntries.AppendLine(keysEntryTemplate.Replace(nameof(keysEntry), value));
                keyLookupEntries.AppendLine(keysLookupEntryTemplate.Replace(constKey, keyValuePair.Key).Replace(constValue, value));
            }

            // assign correct filename & replace constant template with the concrete data.
            runtimeFileContent = runtimeFileContent.Replace(className, runtimeFilename);
            runtimeFileContent = runtimeFileContent.Replace(constLineOriginal, constLines.ToString());

            // assign correct filename & replace list template with the concrete data.
            listFileContent = listFileContent.Replace(className, listFilename);
            listFileContent = listFileContent.Replace(keysEntryOriginal, keyEntries.ToString());
            
            // assign correct filename & replace lookup template with the concrete data.
            lookupFileContent = lookupFileContent.Replace(className, lookupFilename);
            lookupFileContent = lookupFileContent.Replace(keysLookupEntryOriginal, keyLookupEntries.ToString());
        }

        /// <summary>
        /// Actually create/overwrite all required script files
        /// </summary>
        private static void CreateScriptFiles() {
            // runtime file (only constants)
            File.WriteAllText(Path.Combine(dataPath, outputPathCached, runtimeFilename + ".cs"), runtimeFileContent.ToString());
            // editor only files (list & lookup)
            File.WriteAllText(Path.Combine(dataPath, outputEditorPathCached, listFilename + ".cs"), listFileContent.ToString());
            File.WriteAllText(Path.Combine(dataPath, outputEditorPathCached, lookupFilename + ".cs"), lookupFileContent.ToString());
        }
    }
}

