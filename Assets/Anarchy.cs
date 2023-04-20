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
    public bool isEnabled = false;
    public Anarchy.Main main;
    private SettingsWindow settingsWindow;
    private void Update()
    {
        if (!UIUtility.isInputFieldFocused())
        {
            if (Input.GetKeyUp(Settings.Instance.getKeyMapping(main.getIdentifier() + "/AnarchySettings")))
            {
                if (settingsWindow)
                {
                    settingsWindow.close();
                }
                else
                {
                    settingsWindow = Instantiate(ScriptableSingleton<UIAssetManager>.Instance.settingsWindowGO);
                    settingsWindow.GetComponent<UITabGroup>().openTab(3);
                }
            }
            if (Input.GetKeyUp(Settings.Instance.getKeyMapping(main.getIdentifier() + "/AnarchyProfile_0")))
            {
                Disable();
                main.activeProfile = "";
                Notification notification = new Notification(main.getTranslation("profileEnabledNull"));
                NotificationBar.Instance.addNotification(notification);
            }
            foreach (string id in main.profileIds)
            {
                if (Input.GetKeyUp(Settings.Instance.getKeyMapping(main.getIdentifier() + "/AnarchyProfile_" + id)))
                {
                    Disable();
                    main.activeProfile = id;
                    main.loadActiveProfile();
                    settings = main.anarchy_settings;
                    Enable();
                    Notification notification = new Notification(main.getTranslation("profileEnabled") + settings["profileName"].ToString());
                    NotificationBar.Instance.addNotification(notification);
                }
            }
        }
    }
    public void Enable()
    {
        if (isEnabled == false)
        {
            Disable();
        }
        List<Deco> list = AssetManager.Instance.getDecoObjects().ToList();
        try
        {
            foreach (Deco D in list)
            {
                data[D.ToString()] = new Dictionary<string, string>();
                CustomSize CS = D.gameObject.GetComponent<CustomSize>();
                data[D.ToString()].Add("heightChangeDelta", D.heightChangeDelta.ToString());
                data[D.ToString()].Add("defaultGridSubdivision", D.defaultGridSubdivision.ToString());
                data[D.ToString()].Add("defaultSnapToGridCenter", D.defaultSnapToGridCenter.ToString());
                data[D.ToString()].Add("buildOnGrid", D.buildOnGrid.ToString());
                data[D.ToString()].Add("customSizeMinimum", CS?.minSize.ToString());
                data[D.ToString()].Add("customSizeMaximum", CS?.maxSize.ToString());
                if (bool.Parse(settings["heightChangeDelta_enabled"].ToString()))
                {
                    D.heightChangeDelta = float.Parse(settings["heightChangeDelta"].ToString());
                }
                if (bool.Parse(settings["defaultGridSubdivision_enabled"].ToString()))
                {
                    D.defaultGridSubdivision = float.Parse(settings["defaultGridSubdivision"].ToString());
                }
                if (bool.Parse(settings["defaultSnapToGridCenter_enabled"].ToString()))
                {
                    D.defaultSnapToGridCenter = bool.Parse(settings["defaultSnapToGridCenter"].ToString());
                }
                if (bool.Parse(settings["buildOnGrid_enabled"].ToString()))
                {
                    D.buildOnGrid = bool.Parse(settings["buildOnGrid"].ToString());
                }
                if (bool.Parse(settings["customSizeMinimum_enabled"].ToString()) || bool.Parse(settings["customSizeMaximum_enabled"].ToString()))
                {
                    if (CS == null)
                    {
                        CS = D.gameObject.AddComponent<CustomSize>() as CustomSize;
                    }
                    CS.minSize = bool.Parse(settings["customSizeMinimum_enabled"].ToString()) ? float.Parse(settings["customSizeMinimum"].ToString()) : 1;
                    CS.maxSize = bool.Parse(settings["customSizeMaximum_enabled"].ToString()) ? float.Parse(settings["customSizeMaximum"].ToString()) : 1;
                    foreach (FieldInfo fi in D.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                    {
                        if (fi.Name == "customSizeBehaviourLoaded")
                        {
                            fi.SetValue(D, false);
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            WriteToFile(e.ToString());
        }
        isEnabled = true;
    }
    public void Disable()
    {
        if (isEnabled == true)
        {
            List<Deco> list = AssetManager.Instance.getDecoObjects().ToList();
            try
            {
                foreach (Deco D in list)
                {
                    if (data.ContainsKey(D.ToString()))
                    {
                        CustomSize CS = D.gameObject.GetComponent<CustomSize>();
                        if (bool.Parse(settings["heightChangeDelta_enabled"].ToString()))
                        {
                            D.heightChangeDelta = float.Parse(data[D.ToString()]["heightChangeDelta"]);
                        }
                        if (bool.Parse(settings["defaultGridSubdivision_enabled"].ToString()))
                        {
                            D.defaultGridSubdivision = float.Parse(data[D.ToString()]["defaultGridSubdivision"]);
                        }
                        if (bool.Parse(settings["defaultSnapToGridCenter_enabled"].ToString()))
                        {
                            D.defaultSnapToGridCenter = bool.Parse(data[D.ToString()]["defaultSnapToGridCenter"]);
                        }
                        if (bool.Parse(settings["buildOnGrid_enabled"].ToString()))
                        {
                            D.buildOnGrid = bool.Parse(data[D.ToString()]["buildOnGrid"]);
                        }
                        if (bool.Parse(settings["customSizeMinimum_enabled"].ToString()) || bool.Parse(settings["customSizeMaximum_enabled"].ToString()))
                        {
                            if (data[D.ToString()]["customSizeMinimum"] == null)
                            {
                                Destroy(CS);
                            }
                            else
                            {
                                CS.minSize = float.Parse(data[D.ToString()]["customSizeMinimum"]);
                                CS.maxSize = float.Parse(data[D.ToString()]["customSizeMaximum"]);
                            }
                            foreach (FieldInfo fi in D.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                            {
                                if (fi.Name == "customSizeBehaviourLoaded")
                                {
                                    fi.SetValue(D, false);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WriteToFile(e.ToString());
            }
        }
        isEnabled = false;
    }
    public void WriteToFile(string text)
    {
        sw = File.AppendText(Path + @"/constructionanarchy.log");
        sw.WriteLine(DateTime.Now + ": " + text);
        sw.Flush();
        sw.Close();
    }
}