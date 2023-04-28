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
                main.enableProfile("");
            }
            if (Input.GetKeyUp(Settings.Instance.getKeyMapping(main.getIdentifier() + "/AnarchyProfile_cycle_previous")))
            {
                main.enablePreviousProfile();
            }
            if (Input.GetKeyUp(Settings.Instance.getKeyMapping(main.getIdentifier() + "/AnarchyProfile_cycle_next")))
            {
                main.enableNextProfile();
            }
            foreach (string id in main.profileIds)
            {
                if (Input.GetKeyUp(Settings.Instance.getKeyMapping(main.getIdentifier() + "/AnarchyProfile_" + id)))
                {
                    main.enableProfile(id);
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
                CustomParticleSystemSettings CP = D.gameObject.GetComponent<CustomParticleSystemSettings>();
                data[D.ToString()].Add("heightChangeDelta", D.heightChangeDelta.ToString());
                data[D.ToString()].Add("defaultGridSubdivision", D.defaultGridSubdivision.ToString());
                data[D.ToString()].Add("defaultSnapToGridCenter", D.defaultSnapToGridCenter.ToString());
                data[D.ToString()].Add("buildOnGrid", D.buildOnGrid.ToString());
                data[D.ToString()].Add("customSizeMinimum", CS?.minSize.ToString());
                data[D.ToString()].Add("customSizeMaximum", CS?.maxSize.ToString());
                data[D.ToString()].Add("customParticleMinimum", CP?.multiplierMin.ToString());
                data[D.ToString()].Add("customParticleMaximum", CP?.multiplierMax.ToString());
                data[D.ToString()].Add("orientToSurfaceNormal", D.orientToSurfaceNormal.ToString());
                data[D.ToString()].Add("randomRotation", D.randomRotation.ToString());
                if (bool.Parse(settings["heightChangeDelta_enabled"].ToString()))
                {
                    D.heightChangeDelta = ParseFloat(settings["heightChangeDelta"].ToString());
                }
                if (bool.Parse(settings["defaultGridSubdivision_enabled"].ToString()))
                {
                    D.defaultGridSubdivision = ParseFloat(settings["defaultGridSubdivision"].ToString());
                }
                if (bool.Parse(settings["defaultSnapToGridCenter_enabled"].ToString()))
                {
                    D.defaultSnapToGridCenter = bool.Parse(settings["defaultSnapToGridCenter"].ToString());
                }
                if (bool.Parse(settings["buildOnGrid_enabled"].ToString()))
                {
                    D.buildOnGrid = bool.Parse(settings["buildOnGrid"].ToString());
                }
                if (bool.Parse(settings["orientToSurfaceNormal_enabled"].ToString()))
                {
                    D.orientToSurfaceNormal = bool.Parse(settings["orientToSurfaceNormal"].ToString());
                }
                if (bool.Parse(settings["randomRotation_enabled"].ToString()))
                {
                    D.randomRotation = bool.Parse(settings["randomRotation"].ToString());
                }
                if (bool.Parse(settings["customSizeMinimum_enabled"].ToString()) || bool.Parse(settings["customSizeMaximum_enabled"].ToString()))
                {
                    if (CP == null)
                    {
                        if (CS == null)
                        {
                            CS = D.gameObject.AddComponent<CustomSize>() as CustomSize;
                            CS.minSize = 1;
                            CS.maxSize = 1;
                        }
                        if (bool.Parse(settings["customSizeMinimum_enabled"].ToString()))
                        {
                            CS.minSize = ParseFloat(settings["customSizeMinimum"].ToString());
                        }
                        if (bool.Parse(settings["customSizeMaximum_enabled"].ToString()))
                        {
                            CS.maxSize = ParseFloat(settings["customSizeMaximum"].ToString());
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
                    else
                    {
                        if (bool.Parse(settings["customSizeMinimum_enabled"].ToString()))
                        {
                            CP.multiplierMin = ParseFloat(settings["customSizeMinimum"].ToString());
                        }
                        if (bool.Parse(settings["customSizeMaximum_enabled"].ToString()))
                        {
                            CP.multiplierMax = ParseFloat(settings["customSizeMaximum"].ToString());
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
    public void Disable(bool force = false)
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
                        CustomParticleSystemSettings CP = D.gameObject.GetComponent<CustomParticleSystemSettings>();
                        if (bool.Parse(settings["heightChangeDelta_enabled"].ToString()) || force)
                        {
                            D.heightChangeDelta = ParseFloat(data[D.ToString()]["heightChangeDelta"]);
                        }
                        if (bool.Parse(settings["defaultGridSubdivision_enabled"].ToString()) || force)
                        {
                            D.defaultGridSubdivision = ParseFloat(data[D.ToString()]["defaultGridSubdivision"]);
                        }
                        if (bool.Parse(settings["defaultSnapToGridCenter_enabled"].ToString()) || force)
                        {
                            D.defaultSnapToGridCenter = bool.Parse(data[D.ToString()]["defaultSnapToGridCenter"]);
                        }
                        if (bool.Parse(settings["buildOnGrid_enabled"].ToString()) || force)
                        {
                            D.buildOnGrid = bool.Parse(data[D.ToString()]["buildOnGrid"]);
                        }
                        if (bool.Parse(settings["orientToSurfaceNormal_enabled"].ToString()) || force)
                        {
                            D.orientToSurfaceNormal = bool.Parse(data[D.ToString()]["orientToSurfaceNormal"]);
                        }
                        if (bool.Parse(settings["randomRotation_enabled"].ToString()) || force)
                        {
                            D.randomRotation = bool.Parse(data[D.ToString()]["randomRotation"]);
                        }
                        if (bool.Parse(settings["customSizeMinimum_enabled"].ToString()) || bool.Parse(settings["customSizeMaximum_enabled"].ToString()) || force)
                        {
                            if (CP == null)
                            {
                                if (data[D.ToString()]["customSizeMinimum"] == null)
                                {
                                    Destroy(CS);
                                }
                                else
                                {
                                    CS.minSize = ParseFloat(data[D.ToString()]["customSizeMinimum"]);
                                    CS.maxSize = ParseFloat(data[D.ToString()]["customSizeMaximum"]);
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
                            else
                            {
                                CP.multiplierMin = ParseFloat(data[D.ToString()]["customSizeMinimum"]);
                                CP.multiplierMax = ParseFloat(data[D.ToString()]["customSizeMaximum"]);
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

    public float ParseFloat(string input)
    {
        return float.Parse(input.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture.NumberFormat); ;
    }

    public void WriteToFile(string text)
    {
        sw = File.AppendText(Path + @"/constructionanarchy.log");
        sw.WriteLine(DateTime.Now + ": " + text);
        sw.Flush();
        sw.Close();
    }
}