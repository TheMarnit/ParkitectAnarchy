using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using Parkitect.UI;
using UnityEngine;
using Anarchy;

public class AnarchyObject : MonoBehaviour
{
	private Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();
	private StreamWriter sw;
	private Type type;
	public string Path { get; set; }
	public Dictionary<string, object> settings { get; set; }
	public bool isenabled = false;
	public Anarchy.Main main;
	private void Update() {
		if(!string.IsNullOrEmpty(settings["anarchyHotkey"].ToString())){
			if (Input.GetKeyUp(settings["anarchyHotkey"].ToString().ToLower()) && !UIUtility.isInputFieldFocused())
			{
				if((bool)settings["anarchyEnabled"]) {
					main.settings_bool["anarchyEnabled"] = false;
				} else {
					main.settings_bool["anarchyEnabled"] = true;
				}
				main.writeSettingsFile();
			}
		}
	}
    public void Enable()
    {
		if(isenabled==true) {
			Disable();
		}
        List<Deco> list =  AssetManager.Instance.getDecoObjects().ToList();
        foreach (Deco D in list) {
			data[D.ToString()] = new Dictionary<string, string>();
			foreach (KeyValuePair<string, object> S in settings)
			{
				try{
					if(S.Key != "anarchyEnabled" && S.Key != "anarchyHotkey" && S.Key != "anarchyEnforced") {
						if(
						(S.Key != "defaultGridSubdivision")||
						(bool.Parse(D.GetType().GetField("buildOnGrid").GetValue(D).ToString()))||
						(((float)(double)S.Value)>(float.Parse(D.GetType().GetField(S.Key).GetValue(D).ToString())))||
						((bool)settings["anarchyEnforced"])
						){
							if(
							(S.Key != "heightChangeDelta")||
							((((float)(double)S.Value)>0)&&(((float)(double)S.Value)<(float.Parse(D.GetType().GetField(S.Key).GetValue(D).ToString()))))||
							((bool)settings["anarchyEnforced"])
							){
								data[D.ToString()].Add(S.Key, D.GetType().GetField(S.Key).GetValue(D).ToString());
								type = D.GetType().GetField(S.Key).GetValue(D).GetType();
								if(type==typeof(float)) {
									D.GetType().GetField(S.Key).SetValue(D,(float)(double)S.Value);
								} else if(type==typeof(bool)) {
									D.GetType().GetField(S.Key).SetValue(D,(bool)S.Value);
								} else {
									WriteToFile("Enable: "+type+": Type unsupported ("+S.Key+")");
								}
							}
						}
					}
				} catch(Exception e) {
					WriteToFile("Enable: "+S.Key+": Field unsupported");
					WriteToFile(e.ToString());
				}
			}
		}
		isenabled = true;
    }
	public void Disable()
    {
		if(isenabled == true) {
			List<Deco> list =  AssetManager.Instance.getDecoObjects().ToList();
			foreach (Deco D in list) {
				foreach (KeyValuePair<string, string> S in data[D.ToString()])
				{
					try {
						type = D.GetType().GetField(S.Key).GetValue(D).GetType();
						if(type==typeof(float)) {
							D.GetType().GetField(S.Key).SetValue(D,float.Parse(S.Value));
						} else if(type==typeof(bool)) {
							D.GetType().GetField(S.Key).SetValue(D,bool.Parse(S.Value));
						} else {
							WriteToFile("Disable: "+type+": Type unsupported ("+S.Key+")");
						}
					} catch {
						WriteToFile("Disable: "+S.Key+": Field unsupported");
					}
				}
			}
		}
		isenabled = false;
    }
	public void WriteToFile(string text) {
		sw = File.AppendText(Path+@"/mod.log");
        sw.WriteLine(text);
        sw.Flush();
        sw.Close();
	}
}