using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System;
using System.Linq;

namespace ConstantsGenerator {
	using static Settings;

	/// <summary>
	/// Code generation class to convert a .json with Loca Keys into concrete c# declarations.
	/// </summary>
	public class Generator {

        private const string PARAM_SIGN = "%";
        private const string COMMENT_SIGN = "//";
        private const string COMMENT_SIGN_AND_PARAM_SIGN = COMMENT_SIGN+PARAM_SIGN;
        private const string CONSTANT_LINE = COMMENT_SIGN_AND_PARAM_SIGN + "0";
        private const string LOOKUP_ENTRY_LINE = COMMENT_SIGN_AND_PARAM_SIGN + "1";
        private const string CONST_KEY = PARAM_SIGN + "constKey";
        private const string CONST_VALUE = PARAM_SIGN + "constValue";
        private const string CONST_TYPE = PARAM_SIGN + "constType";
        private const string CONST_COMMENT = PARAM_SIGN + "constComment";
        private const string KEYS_ENTRY_RAW = "keysEntry";
		private const string KEYS_ENTRY = PARAM_SIGN + KEYS_ENTRY_RAW;
		private const string CLASS_NAME_LIST = COMMENT_SIGN_AND_PARAM_SIGN + nameof(classNameList);
		private const string CLASS_NAME_LOOKUP = COMMENT_SIGN_AND_PARAM_SIGN + nameof(classNameLookup);
		private const string CLASS_NAME_KEYS = COMMENT_SIGN_AND_PARAM_SIGN + nameof(classNameKeys);
		private const string NAMESPACE = COMMENT_SIGN_AND_PARAM_SIGN + nameof(@namespace);
		private const string NAMESPACE_RUNTIME = COMMENT_SIGN_AND_PARAM_SIGN + nameof(namespaceRuntime);
		private const string CSHARP_FILE_ENDING = ".cs";

		internal static StringBuilder runtimeFileContent = null;
        internal static StringBuilder listFileContent = null;
        internal static StringBuilder lookupFileContent = null;

        internal static string dataPath = null;
        internal static bool isGenerating = false;

        internal static string outputPathCached = null;
        internal static string outputEditorPathCached = null;
        internal static ILookupModel model = null;

        internal static string pathToRuntimeTemplate = null;
        internal static string pathToListTemplate = null;
        internal static string pathToLookupTemplate = null;

		internal static string classNameList = null;
		internal static string classNameKeys = null;
		internal static string classNameLookup = null;
		internal static string @namespace = null;
		internal static string namespaceRuntime = null;

		internal static bool createEditorScripts = false;

		internal static Dictionary<string, Action<GeneratorItem>> conversionActions = null;
		internal static Dictionary<string, Action<GeneratorItem>> ConversionActions {
			get {
				if (conversionActions == null) {
					conversionActions = Settings.instance.adapter.GetConversionActions();
				}
				return conversionActions;
			}
		}

		public static void SetModel<T>(LookupModel<T> model) {
			Generator.model = model;
		}

		/// <summary>
		/// action to execute/initiate the code generation process.
		/// </summary>
		[MenuItem(MENU_ITEM_ALL)]
        internal static void CreateConstantsAction() {
			CreateConstantsAsync();
        }

        /// <summary>
        /// The asynchronouse code generation process
        /// </summary>
        public static async void CreateConstantsAsync(string[] generatorIds = null) {
            // do nothing if we are already creating files...
            if (isGenerating) {
                return;
            }

			// cache all the relevant data
			UpdateSources();

			for (int i=0; i< Settings.instance.generators.Length; i++) {
				GeneratorItem generatorItem = Settings.instance.generators[i];
				
				if ((generatorIds == null || generatorIds.Contains(generatorItem.logicID)) && ConversionActions.ContainsKey(generatorItem.logicID)) {
					UpdateSpecificSources(generatorItem);

					// run the creation task asynchronously
					await Task.Run(() => CreateConstants());
				}
			}

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

		private static void UpdateSpecificSources(GeneratorItem generatorItem) {
			conversionActions[generatorItem.logicID](generatorItem);

			// cache our paths that are based on our settings file.
			outputPathCached = generatorItem.outputPath;
			outputEditorPathCached = generatorItem.outputEditorPath;
			@namespace = generatorItem.@namespace;
			namespaceRuntime = generatorItem.namespaceRuntime;
			classNameKeys = generatorItem.classNameKeys;
			classNameList = generatorItem.classNameList;
			classNameLookup = generatorItem.classNameLookup;
			createEditorScripts = generatorItem.createEditorOnlyLookup;
		}

		/// <summary>
		/// Cache all thread unsafe data.
		/// </summary>
		private static void UpdateSources() {
            // get the data path
            dataPath = Application.dataPath;

            // runtime script paths
            string assetPathOfRuntimeTemplate = AssetDatabase.GUIDToAssetPath(Settings.instance.guidOfRuntimeTemplate);
            pathToRuntimeTemplate = Path.GetFullPath(assetPathOfRuntimeTemplate);

            // list script paths
            string assetPathOfListTemplate = AssetDatabase.GUIDToAssetPath(Settings.instance.guidOfListTemplate);
            pathToListTemplate = Path.GetFullPath(assetPathOfListTemplate);

            // lookup script paths
            string assetPathOfLookupTemplate = AssetDatabase.GUIDToAssetPath(Settings.instance.guidOfLookupTemplate);
            pathToLookupTemplate = Path.GetFullPath(assetPathOfLookupTemplate);
        }

        /// <summary>
        /// Create the file contents.
        /// This includes creating the file contents for the contants, and editor only list and lookup table.
        /// </summary>
        private static void CreateFileContents() {
            string line = null;
            string constLineTemplate = null;
            string commentTemplate = null;
            string keysEntryTemplate = null;
            string keysLookupEntryTemplate = null;

            string constLineOriginal = null;
            string keysEntryOriginal = null;
            string keysLookupEntryOriginal = null;
            string commentTemplateOriginal = null;

            // go over every line of the runtime template
            runtimeFileContent = new StringBuilder();
            using (StreamReader file = new StreamReader(pathToRuntimeTemplate)) {
                while ((line = file.ReadLine()) != null) {
                    if (constLineTemplate == null) {
                        if (line.Contains(CONSTANT_LINE)) {
                            constLineTemplate = line.Replace(CONSTANT_LINE, "");
                            constLineOriginal = line;
                        } else if (line.Contains(LOOKUP_ENTRY_LINE)) {
                            commentTemplate = line.Replace(LOOKUP_ENTRY_LINE, "");
							commentTemplateOriginal = line;
                        }
                    }
                    runtimeFileContent.AppendLine(line);
                }
            }
            constLineTemplate = constLineTemplate.Replace(CONST_TYPE, model.GetTypeName());

            // go over every line of the list template
            listFileContent = new StringBuilder();
			using (StreamReader file = new StreamReader(pathToListTemplate)) {
				while ((line = file.ReadLine()) != null) {
					if (keysEntryTemplate == null && line.Contains(KEYS_ENTRY)) {
						keysEntryTemplate = line.Replace(COMMENT_SIGN_AND_PARAM_SIGN, "");
						keysEntryOriginal = line;
					}
					listFileContent.AppendLine(line);
				}
			}

			// go over every line of the lookup template
			lookupFileContent = new StringBuilder();
			using (StreamReader file = new StreamReader(pathToLookupTemplate)) {
				while ((line = file.ReadLine()) != null) {
					if (keysLookupEntryTemplate == null && line.Contains(LOOKUP_ENTRY_LINE)) {
						keysLookupEntryTemplate = line.Replace(LOOKUP_ENTRY_LINE, "");
						keysLookupEntryOriginal = line;
					}
					lookupFileContent.AppendLine(line);
				}
			}

            StringBuilder constLines = new StringBuilder();
            StringBuilder keyEntries = new StringBuilder();
            StringBuilder keyLookupEntries = new StringBuilder();

            // go overy every lookup key
			void TransformValue(string key, string value, string comment) {
				if(comment != null) {
					constLines.AppendLine(commentTemplate.Replace(CONST_COMMENT, comment));
                }
                constLines.AppendLine(constLineTemplate.Replace(CONST_KEY, key).Replace(CONST_VALUE, value));
				keyEntries.AppendLine(keysEntryTemplate.Replace(KEYS_ENTRY_RAW, value));
                keyLookupEntries.AppendLine(keysLookupEntryTemplate.Replace(CONST_KEY, key).Replace(CONST_VALUE, value));
            }
			model.SetTransformAction(TransformValue);
			model.TransformEach();

            // assign correct filename & replace constant template with the concrete data.
            runtimeFileContent = runtimeFileContent.Replace(CLASS_NAME_KEYS, classNameKeys);
			runtimeFileContent = runtimeFileContent.Replace(NAMESPACE_RUNTIME, namespaceRuntime);
			runtimeFileContent = runtimeFileContent.Replace(NAMESPACE, @namespace);
			runtimeFileContent = runtimeFileContent.Replace(constLineOriginal, constLines.ToString());
            runtimeFileContent = runtimeFileContent.Replace(commentTemplateOriginal, "");

            // assign correct filename & replace list template with the concrete data.
            listFileContent = listFileContent.Replace(CLASS_NAME_LIST, classNameList);
			listFileContent = listFileContent.Replace(CLASS_NAME_KEYS, classNameKeys);
			listFileContent = listFileContent.Replace(NAMESPACE_RUNTIME, namespaceRuntime);
			listFileContent = listFileContent.Replace(NAMESPACE, @namespace);
			listFileContent = listFileContent.Replace(keysEntryOriginal, keyEntries.ToString());

			// assign correct filename & replace lookup template with the concrete data.
			lookupFileContent = lookupFileContent.Replace(CLASS_NAME_LOOKUP, classNameLookup);
			lookupFileContent = lookupFileContent.Replace(CLASS_NAME_KEYS, classNameKeys);
			lookupFileContent = lookupFileContent.Replace(NAMESPACE_RUNTIME, namespaceRuntime);
			lookupFileContent = lookupFileContent.Replace(NAMESPACE, @namespace);
			lookupFileContent = lookupFileContent.Replace(keysLookupEntryOriginal, keyLookupEntries.ToString());
        }

        /// <summary>
        /// Actually create/overwrite all required script files
        /// </summary>
        private static void CreateScriptFiles() {

			// runtime file (only constants)
			CreateDirectoriesAndScriptFile(ref outputPathCached, ref classNameKeys, runtimeFileContent.ToString());

			// editor only files (list & lookup)
			if (createEditorScripts) {
				CreateDirectoriesAndScriptFile(ref outputEditorPathCached, ref classNameList, listFileContent.ToString());
				CreateDirectoriesAndScriptFile(ref outputEditorPathCached, ref classNameLookup, lookupFileContent.ToString());
			}
        }

		private static void CreateDirectoriesAndScriptFile(ref string outputPath, ref string scriptName, string fileContent) {
			string finalPath = Path.Combine(dataPath, outputPath, scriptName + CSHARP_FILE_ENDING);
			Directory.CreateDirectory(Path.GetDirectoryName(finalPath));
			File.WriteAllText(finalPath, fileContent);
		}
    }
}

