using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using MiniJSON;
using System.Text.RegularExpressions;

namespace Anarchy
{
    public class Main : AbstractMod, IModSettings
    {
		private StreamWriter sw;
        private GameObject _go;
		private AnarchyObject Anar;
		public Dictionary<string, object> anarchy_settings;
		public Dictionary<string, object> anarchy_strings;
		public Dictionary<string, string> settings_string = new Dictionary<string, string>();
		public Dictionary<string, bool> settings_bool = new Dictionary<string, bool>();
		private Type type;
		public bool modIsEnabled = false;
        public List<string> profileIds;
        public string activeProfile = "1";
		private int result;
        private bool isinitiated = false;
        private double settingsVersion = 1.2;
        private double dictionaryVersion = 1.13;

		public Main()
        {
            Init();
            SetupKeyBinding();
        }

		public void Init()
        {
            if (isinitiated == false)
            {
                profileIds = new List<string>();
                try
                {
                    System.IO.Directory.CreateDirectory(Path);
                }
                catch
                {
                    Debug.LogError("Creating path failed: " + Path);
                    return;
                }
                isinitiated = true;
                if (!File.Exists(Path + @"/constructionanarchy.settings." + activeProfile + ".json"))
                {
                    if (File.Exists(LegacyPath + @"/settings.json"))
                    {
                        anarchy_settings = Json.Deserialize(File.ReadAllText(LegacyPath + @"/settings.json")) as Dictionary<string, object>;
                    }
                    generateSettingsFile(activeProfile);
                }
                loadActiveProfile();

                if (File.Exists(Path + @"/constructionanarchy.dictionary.json"))
                {
                    anarchy_strings = Json.Deserialize(File.ReadAllText(Path + @"/constructionanarchy.dictionary.json")) as Dictionary<string, object>;
                }
                else if (File.Exists(LegacyPath + @"/dictionary.json"))
                {
                    anarchy_strings = Json.Deserialize(File.ReadAllText(LegacyPath + @"/dictionary.json")) as Dictionary<string, object>;
                }
                else {
                    generateDictionaryFile();
                    anarchy_strings = Json.Deserialize(File.ReadAllText(Path + @"/constructionanarchy.dictionary.json")) as Dictionary<string, object>;
                }
                if (anarchy_strings == null || string.IsNullOrEmpty(anarchy_strings["version"].ToString()) || Double.Parse(anarchy_strings["version"].ToString()) < dictionaryVersion)
                {
                    generateDictionaryFile();
                    anarchy_strings = Json.Deserialize(File.ReadAllText(Path + @"/constructionanarchy.dictionary.json")) as Dictionary<string, object>;
                }
                DirectoryInfo dir = new DirectoryInfo(Path);
                FileInfo[] info = dir.GetFiles("constructionanarchy.settings.*.json");
                foreach (FileInfo f in info)
                {
                    string[] file = f.ToString().Split('.');
                    profileIds.Add(file[file.Length - 2]);
                }
            }
        }

        public void loadActiveProfile(bool recursive = false)
        {
            if (File.Exists(Path + @"/constructionanarchy.settings." + activeProfile + ".json"))
            {
                anarchy_settings = Json.Deserialize(File.ReadAllText(Path + @"/constructionanarchy.settings." + activeProfile + ".json")) as Dictionary<string, object>;
            }
            if (anarchy_settings == null || string.IsNullOrEmpty(anarchy_settings["version"].ToString()) || Double.Parse(anarchy_settings["version"].ToString()) < settingsVersion)
            {
                if(recursive == true)
                {
                    if (modIsEnabled)
                    {
                        onDisabled();
                    }
                    return;
                }
                generateSettingsFile(activeProfile);
                loadActiveProfile(true);
            }
            if (anarchy_settings.Count > 0)
            {
                foreach (KeyValuePair<string, object> S in anarchy_settings)
                {
                    type = S.Value.GetType();
                    if (type == typeof(bool))
                    {
                        settings_bool[S.Key] = bool.Parse(S.Value.ToString());
                    }
                    else
                    {
                        settings_string[S.Key] = S.Value.ToString();
                    }
                }
            }
        }
		
        public override void onEnabled()
        {
            _go = new GameObject();
            Anar = _go.AddComponent<AnarchyObject>();
			Anar.settings = anarchy_settings;
			Anar.Path = Path;
			Anar.main = this;
            if(activeProfile != "")
            {
                loadActiveProfile();
                Anar.Enable();
            }
			modIsEnabled = true;
        }
		
		public override void onDisabled()
        {
            Anar.Disable();
            UnityEngine.Object.Destroy(_go);
			modIsEnabled = false;
        }
		
		public void onDrawSettingsUI()
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.margin = new RectOffset(15, 0, 10, 0);
            labelStyle.alignment = TextAnchor.MiddleLeft;
            GUIStyle displayStyle = new GUIStyle(GUI.skin.label);
            displayStyle.margin = new RectOffset(0, 10, 10, 0);
            displayStyle.alignment = TextAnchor.MiddleLeft;
            GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.margin = new RectOffset(0, 10, 17, 0);
            toggleStyle.alignment = TextAnchor.MiddleLeft;
            GUIStyle textfieldStyle = new GUIStyle (GUI.skin.textField); 
			textfieldStyle.margin=new RectOffset(0,10,10,0);
			textfieldStyle.alignment = TextAnchor.MiddleCenter;
			GUIStyle buttonStyle = new GUIStyle (GUI.skin.button); 
			buttonStyle.margin=new RectOffset(0,10,10,0);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(getTranslation("versionLabel"), labelStyle, GUILayout.Height(30));
            GUILayout.Label(getTranslation("activeProfile"), labelStyle, GUILayout.Height(30));
            GUILayout.Label(" ", labelStyle, GUILayout.Height(30));
            GUILayout.Label(getTranslation("field"), labelStyle, GUILayout.Height(30));
            foreach (KeyValuePair<string, object> S in anarchy_settings)
            {
                if (S.Key != "version" && S.Key != "profileName" && S.Key.Substring(S.Key.Length - 8, 8) != "_enabled" && activeProfile != "")
                {
                    GUILayout.Label(getTranslation(S.Key), labelStyle, GUILayout.Height(30));
                }
            }
            GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
            GUILayout.Label(getVersionNumber(), displayStyle, GUILayout.Height(30));
            GUILayout.Label(activeProfile != "" ? activeProfile.ToString() : getTranslation("profileNull"), displayStyle, GUILayout.Height(30));
            GUILayout.Label(" ", displayStyle, GUILayout.Height(30));
            GUILayout.Label(getTranslation("value"), displayStyle, GUILayout.Height(30));
            foreach (KeyValuePair<string, object> S in anarchy_settings) {
				type = S.Value.GetType();
                if (S.Key != "version" && S.Key != "profileName" && S.Key.Substring(S.Key.Length - 8, 8) != "_enabled" && activeProfile != "")
                {
                    if (type == typeof(bool))
                    {
                        settings_bool[S.Key] = GUILayout.Toggle(settings_bool[S.Key], "", toggleStyle, GUILayout.Width(130), GUILayout.Height(23));
                    }
                    else
                    {
                        settings_string[S.Key] = GUILayout.TextField(settings_string[S.Key], textfieldStyle, GUILayout.Width(130), GUILayout.Height(30));
                    }
                }
			}
			GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label(" ", displayStyle, GUILayout.Height(30));
            if (activeProfile == null)
            {
                GUILayout.Label(" ", displayStyle, GUILayout.Height(30));
            }
            else
            {
                settings_string["profileName"] = GUILayout.TextField(settings_string["profileName"], textfieldStyle, GUILayout.Width(130), GUILayout.Height(30));
            }
            GUILayout.Label(" ", displayStyle, GUILayout.Height(30));
            GUILayout.Label(getTranslation("enabled"), displayStyle, GUILayout.Height(30));
            foreach (KeyValuePair<string, object> S in anarchy_settings)
            {
                if (S.Key != "version" && S.Key != "profileName" && S.Key.Substring(S.Key.Length - 8, 8) != "_enabled" && activeProfile != "")
                {
                    settings_bool[S.Key + "_enabled"] = GUILayout.Toggle(settings_bool[S.Key + "_enabled"], "", toggleStyle, GUILayout.Height(23));
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
		}
		
        public string getTranslation(string key)
        {
            return anarchy_strings.ContainsKey(key) ? anarchy_strings[key].ToString() : key;
        }

		public void onSettingsOpened()
		{
            Init();
            loadActiveProfile();
        }
		
		public void onSettingsClosed()
        {
            if (activeProfile != "")
            {
                if (settings_bool.Count > 0)
                {
                    foreach (KeyValuePair<string, bool> S in settings_bool)
                    {
                        anarchy_settings[S.Key] = S.Value;
                    }
                }
                if (settings_string.Count > 0)
                {
                    foreach (KeyValuePair<string, string> S in settings_string)
                    {
                        anarchy_settings[S.Key] = S.Value;
                    }
                }
                generateSettingsFile(activeProfile);
                if (modIsEnabled)
                {
                    Anar.Disable();
                    Anar.Enable();
                }
            }
        }
		
        public override string getName() { return "Construction Anarchy"; }
        public override string getDescription() { return "Lifts building restrictions for assets."; }
        public override string getIdentifier() { return "Marnit@ParkitectAnarchy"; }
        public override string getVersionNumber() { return "2.5.0"; }
        public override int getOrderPriority() { return 9999; }
        public override bool isMultiplayerModeCompatible() { return true; }
        public override bool isRequiredByAllPlayersInMultiplayerMode() { return false; }

        public string LegacyPath { get { return System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+ "/Parkitect/Mods/ConAnarchySettings"; } }

        public string Path { get { return System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Parkitect"; } }

        public void WriteToFile(string text) {
            try
            {
                sw = File.AppendText(Path + @"/constructionanarchy.log");
                sw.WriteLine(DateTime.Now + ": " + text);
                sw.Flush();
                sw.Close();
            }
            catch
            {
                Debug.LogError("Failed writing to: " + Path + @"/constructionanarchy.log");
            }
		}

        public void generateSettingsFile(string id, bool forceDefault = false)
        {
            try
            {
                File.Delete(Path + @"/constructionanarchy.settings."+id+".json");
                File.Delete(LegacyPath + @"/settings.json");
            }
            catch
            {

            }
            try
            {
                sw = File.CreateText(Path + @"/constructionanarchy.settings." + id + ".json");
                sw.WriteLine("{");
                sw.WriteLine("	\"version\": " + settingsVersion.ToString().Replace(",",".") + (int.TryParse(settingsVersion.ToString(), out result) ? ".0" : "") + ",");
                writeSettingLine(sw, forceDefault, "profileName", typeof(string), getTranslation("profileNameDefault") + id);
                writeSettingLine(sw, forceDefault, "heightChangeDelta", typeof(string), "0.01");
                writeSettingLine(sw, forceDefault, "heightChangeDelta_enabled", typeof(bool), true);
                writeSettingLine(sw, forceDefault, "defaultGridSubdivision", typeof(string), "1.0");
                writeSettingLine(sw, forceDefault, "defaultGridSubdivision_enabled", typeof(bool), true);
                writeSettingLine(sw, forceDefault, "defaultSnapToGridCenter", typeof(bool), true);
                writeSettingLine(sw, forceDefault, "defaultSnapToGridCenter_enabled", typeof(bool), true);
                writeSettingLine(sw, forceDefault, "buildOnGrid", typeof(bool), false);
                writeSettingLine(sw, forceDefault, "buildOnGrid_enabled", typeof(bool), true);
                writeSettingLine(sw, forceDefault, "customSizeMinimum", typeof(string), "0.1");
                writeSettingLine(sw, forceDefault, "customSizeMinimum_enabled", typeof(bool), true);
                writeSettingLine(sw, forceDefault, "customSizeMaximum", typeof(string), "10");
                writeSettingLine(sw, forceDefault, "customSizeMaximum_enabled", typeof(bool), true);
                sw.WriteLine("}");
            }
            catch
            {
                Debug.LogError("Failed writing to: " + Path + @"/settings.json");
            }
            finally
            {
                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public void writeSettingLine(StreamWriter sw, bool forceDefault, string setting, Type type, object defaultValue)
        {
            try
            {
                if (type == typeof(string))
                {
                    sw.WriteLine("	\"" + setting + "\": \"" + (anarchy_settings.ContainsKey(setting) && !forceDefault ? anarchy_settings[setting] : defaultValue) + "\",");
                }
                if (type == typeof(bool))
                {
                    sw.WriteLine("	\"" + setting + "\": " + (anarchy_settings.ContainsKey(setting) && !forceDefault ? anarchy_settings[setting].ToString().ToLower() : defaultValue.ToString().ToLower()) + ",");
                }
            }
            catch
            {
                Debug.LogError("Failed writing setting: " + setting);
            }
        }

        public void generateDictionaryFile()
        {
            try
            {
                File.Delete(Path + @"/constructionanarchy.dictionary.json");
                File.Delete(LegacyPath + @"/dictionary.json");
            }
            catch
            {

            }
            try { 
            sw = File.CreateText(Path + @"/constructionanarchy.dictionary.json");
            sw.WriteLine("{");
            sw.WriteLine("	\"version\": " + dictionaryVersion.ToString().Replace(",", ".") + (int.TryParse(dictionaryVersion.ToString(), out result) ? ".0" : "") + ",");
            writeDictionaryLine(sw, "versionLabel", "Version");
            writeDictionaryLine(sw, "profileName", "Profile name");
            writeDictionaryLine(sw, "profileNameDefault", "Profile ");
            writeDictionaryLine(sw, "activeProfile", "Active Profile");
            writeDictionaryLine(sw, "profileEnabled", "Profile enabled: ");
            writeDictionaryLine(sw, "profileEnabledNull", "Construction Anarchy disabled");
            writeDictionaryLine(sw, "anarchySettings", "Open mods settings panel");
            writeDictionaryLine(sw, "anarchySettingsDescript", "Can be used for easy access to Construction Anarchy settings panel");
            writeDictionaryLine(sw, "setProfile", "Enable profile ");
            writeDictionaryLine(sw, "setProfileDescript", "Enable all settings applied by profile ");
            writeDictionaryLine(sw, "setProfileNull", "Disable Construction Anarchy");
            writeDictionaryLine(sw, "setProfileNullDescript", "Revert all settings applied by Construction Anarchy");
            writeDictionaryLine(sw, "profileNull", "None");
            writeDictionaryLine(sw, "heightChangeDelta", "Vertical Grid Size");
            writeDictionaryLine(sw, "field", "Setting");
            writeDictionaryLine(sw, "value", "Value");
            writeDictionaryLine(sw, "enabled", "Enabled");
            writeDictionaryLine(sw, "defaultGridSubdivision", "Horizontal Grid Subdivision");
            writeDictionaryLine(sw, "defaultSnapToGridCenter", "Default To Grid Center");
            writeDictionaryLine(sw, "buildOnGrid", "Force On Grid");
            writeDictionaryLine(sw, "customSizeMinimum", "Minimum Size");
            writeDictionaryLine(sw, "customSizeMaximum", "Maximum Size");
            sw.WriteLine("}");
            }
            catch
            {
                Debug.LogError("Failed writing to: " + Path + @"/constructionanarchy.dictionary.json");
            }
            finally
            {
                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public void writeDictionaryLine(StreamWriter sw, string setting, string defaultValue)
        {
            try
            {
                sw.WriteLine("	\"" + setting + "\": \"" + (anarchy_strings.ContainsKey(setting) ? anarchy_strings[setting] : defaultValue) + "\",");
            }
            catch
            {
                Debug.LogError("Failed writing dictionary entry: " + setting);
            }
        }


        private void SetupKeyBinding()
        {
            KeyGroup keyGroup = new KeyGroup(getIdentifier());
            keyGroup.keyGroupName = getName();
            ScriptableSingleton<InputManager>.Instance.registerKeyGroup(keyGroup);
            RegisterKey("AnarchySettings", KeyCode.None, getTranslation("anarchySettings"), getTranslation("anarchySettingsDescript"));
            RegisterKey("AnarchyProfile_0", KeyCode.None, getTranslation("setProfileNull"), getTranslation("setProfileNullDescript"));
            foreach (string id in profileIds)
            {
                RegisterKey("AnarchyProfile_" + id, KeyCode.None, getTranslation("setProfile") + id, getTranslation("setProfileDescript") + id);
            }
        }

        private KeyMapping RegisterKey(string identifier, KeyCode keyCode, string Name, string Description = "")
        {
            KeyMapping keyMapping = new KeyMapping(getIdentifier() + "/" + identifier, keyCode, KeyCode.None);
            keyMapping.keyGroupIdentifier = getIdentifier();
            keyMapping.keyName = Name;
            keyMapping.keyDescription = Description;
            ScriptableSingleton<InputManager>.Instance.registerKeyMapping(keyMapping);
            return keyMapping;
        }
    }
}
