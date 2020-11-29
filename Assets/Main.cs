using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using MiniJSON;

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
		public bool isenabled = false;
		private string output;
		private int i;
		private int result;
        private bool isinitiated = false;
        private double settingsVersion = 1.1;
        private double dictionaryVersion = 1.1;

		public Main()
        {
            Init();
            SetupKeyBinding();
        }

		public void Init()
        {
            if (isinitiated == false)
            {
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
                if (!File.Exists(Path + @"/settings.json"))
                {
                    generateSettingsFile();
                }
                if (!File.Exists(Path + @"/dictionary.json"))
                {
                    generateDictionaryFile();
                }
                anarchy_settings = Json.Deserialize(File.ReadAllText(Path + @"/settings.json")) as Dictionary<string, object>;
                if (anarchy_settings == null || string.IsNullOrEmpty(anarchy_settings["version"].ToString()) || Double.Parse(anarchy_settings["version"].ToString()) < settingsVersion)
                {
                    generateSettingsFile();
                    anarchy_settings = Json.Deserialize(File.ReadAllText(Path + @"/settings.json")) as Dictionary<string, object>;
                }
                anarchy_strings = Json.Deserialize(File.ReadAllText(Path + @"/dictionary.json")) as Dictionary<string, object>;
                if (anarchy_strings == null || string.IsNullOrEmpty(anarchy_strings["version"].ToString()) || Double.Parse(anarchy_strings["version"].ToString()) < dictionaryVersion)
                {
                    generateDictionaryFile();
                    anarchy_strings = Json.Deserialize(File.ReadAllText(Path + @"/dictionary.json")) as Dictionary<string, object>;
                }
                if(anarchy_settings.Count > 0) {
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
        }
		
        public override void onEnabled()
        {
            _go = new GameObject();
            Anar = _go.AddComponent<AnarchyObject>();
			Anar.settings = anarchy_settings;
			Anar.Path = Path;
			Anar.main = this;
			if((bool)anarchy_settings["anarchyEnabled"]) {
				Anar.Enable();
			}
			isenabled = true;
        }
		
		public override void onDisabled()
        {
            Anar.Disable();
            UnityEngine.Object.Destroy(_go);
			isenabled = false;
        }
		
		public void onDrawSettingsUI()
        {
            anarchy_settings = Json.Deserialize(File.ReadAllText(Path + @"/settings.json")) as Dictionary<string, object>;
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.margin = new RectOffset(15, 0, 10, 0);
            labelStyle.alignment = TextAnchor.MiddleLeft;
            GUIStyle displayStyle = new GUIStyle(GUI.skin.label);
            displayStyle.margin = new RectOffset(0, 10, 10, 0);
            displayStyle.alignment = TextAnchor.MiddleLeft;
            GUIStyle toggleStyle = new GUIStyle (GUI.skin.toggle); 
			toggleStyle.margin=new RectOffset(0,10,19,16);
			toggleStyle.alignment = TextAnchor.MiddleLeft;
			GUIStyle textfieldStyle = new GUIStyle (GUI.skin.textField); 
			textfieldStyle.margin=new RectOffset(0,10,10,0);
			textfieldStyle.alignment = TextAnchor.MiddleCenter;
			GUIStyle buttonStyle = new GUIStyle (GUI.skin.button); 
			buttonStyle.margin=new RectOffset(0,10,10,0);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Version", labelStyle, GUILayout.Height(30));
            foreach (KeyValuePair<string, object> S in anarchy_settings)
            {
                if (S.Key != "version")
                {
                    GUILayout.Label(anarchy_strings.ContainsKey(S.Key) ? anarchy_strings[S.Key].ToString() : S.Key, labelStyle, GUILayout.Height(30));
                }
            }
            GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
            GUILayout.Label(getVersionNumber(), displayStyle, GUILayout.Height(30));
            foreach (KeyValuePair<string, object> S in anarchy_settings) {
				type = S.Value.GetType();
                if (S.Key != "version")
                {
                    if (type == typeof(bool))
                    {
                        settings_bool[S.Key] = GUILayout.Toggle(settings_bool[S.Key], "", toggleStyle, GUILayout.Width(16), GUILayout.Height(16));
                    }
                    else
                    {
                        settings_string[S.Key] = GUILayout.TextField(settings_string[S.Key], textfieldStyle, GUILayout.Width(130), GUILayout.Height(30));
                    }
                }
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		
		private bool FetchKey(out KeyCode outKey)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown (key)) {
                    outKey = key;
                    return true;
                }
            }
            outKey = KeyCode.A;
            return false;
        }
		
		public void onSettingsOpened()
		{
            Init();
        }
		
		public void onSettingsClosed()
		{
			writeSettingsFile();
		}

		public void writeSettingsFile()
        {
            sw = File.CreateText(Path+@"/settings.json");
			sw.WriteLine("{");
			i = 0;
            foreach (KeyValuePair<string, object> S in anarchy_settings) {
				type = S.Value.GetType();
				i++;
				output = "	\""+S.Key+"\": ";
				if(type==typeof(bool)) {
					output += settings_bool[S.Key].ToString().ToLower();
				} else if(type==typeof(double)) {
					output += settings_string[S.Key].Replace(",",".");
					if(int.TryParse(settings_string[S.Key],out result)) {
						output += ".0";
					}
				} else if(type==typeof(string)) {
					output += "\""+settings_string[S.Key]+"\"";
				}
				if(i!=anarchy_settings.Count){
					output += ",";
				}
				sw.WriteLine(output);
			}
			sw.WriteLine("}");
			sw.Flush();
			sw.Close();
            isinitiated = false;
            Init();
            if (isenabled==true) {
                if ((bool)anarchy_settings["anarchyEnabled"])
                {
                    Anar.settings = anarchy_settings;
					Anar.Enable();
				} else if(Anar.isEnabled==true)
                {
                    Anar.settings = anarchy_settings;
					Anar.Disable();
                }
            }
		}
		
        public override string getName() { return "Construction Anarchy"; }
        public override string getDescription() { return "Lifts building restrictions for assets."; }
        public override string getIdentifier() { return "Marnit@ParkitectAnarchy"; }
        public override string getVersionNumber() { return "2.4.1"; }
        public override int getOrderPriority() { return 9999; }
        public override bool isMultiplayerModeCompatible() { return true; }
        public override bool isRequiredByAllPlayersInMultiplayerMode() { return false; }

        public string Path { get { return System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+ "/Parkitect/Mods/ConAnarchySettings"; } }


        public void WriteToFile(string text) {
            try
            {
                sw = File.AppendText(Path + @"/mod.log");
                sw.WriteLine(DateTime.Now + ": " + text);
                sw.Flush();
                sw.Close();
            }
            catch
            {
                Debug.LogError("Failed writing to: " + Path + @"/mod.log");
            }
		}

        public void generateSettingsFile()
        {
            try
            {
                sw = File.CreateText(Path + @"/settings.json");
                sw.WriteLine("{");
                sw.WriteLine("	\"version\": " + settingsVersion.ToString().Replace(",",".") + (int.TryParse(settingsVersion.ToString(), out result) ? ".0" : "") + ",");
                sw.WriteLine("	\"anarchyEnabled\": true,");
                sw.WriteLine("	\"anarchyEnforced\": false,");
                sw.WriteLine("	\"heightChangeDelta\": 0.01,");
                sw.WriteLine("	\"defaultGridSubdivision\": 1.0,");
                sw.WriteLine("	\"defaultSnapToGridCenter\": true,");
                sw.WriteLine("	\"buildOnGrid\": false");
                sw.WriteLine("}");
                sw.Flush();
                sw.Close();
            }
            catch
            {
                Debug.LogError("Failed writing to: " + Path + @"/settings.json");
            }
        }
    
        public void generateDictionaryFile()
        {
            try { 
            sw = File.CreateText(Path + @"/dictionary.json");
            sw.WriteLine("{");
            sw.WriteLine("	\"version\": " + dictionaryVersion.ToString().Replace(",", ".") + (int.TryParse(dictionaryVersion.ToString(), out result) ? ".0" : "") + ",");
            sw.WriteLine("	\"anarchyEnabled\": \"Anarchy Enabled\",");
            sw.WriteLine("	\"anarchyEnforced\": \"Always Override Settings\",");
            sw.WriteLine("	\"heightChangeDelta\": \"Vertical Grid Size\",");
            sw.WriteLine("	\"defaultGridSubdivision\": \"Horizontal Grid Subdivision\",");
            sw.WriteLine("	\"defaultSnapToGridCenter\": \"Default To Grid Center\",");
            sw.WriteLine("	\"buildOnGrid\": \"Force On Grid\"");
            sw.WriteLine("}");
            sw.Flush();
            sw.Close();
            }
            catch
            {
                Debug.LogError("Failed writing to: " + Path + @"/dictionary.json");
            }
        }

        private void SetupKeyBinding()
        {
            KeyGroup keyGroup = new KeyGroup(getIdentifier());
            keyGroup.keyGroupName = getName();
            ScriptableSingleton<InputManager>.Instance.registerKeyGroup(keyGroup);
            RegisterKey("AnarchyToggle", KeyCode.None, "Toggle Construction Anarchy", "Used to enable or disable all settings applied by Construction Anarchy");
            RegisterKey("AnarchySettings", KeyCode.None, "Open mods settings panel", "Can be used for easy access to Construction Anarchy settings panel");
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
