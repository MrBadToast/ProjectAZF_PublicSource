using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Timeline;
using static UnityEngine.Rendering.DebugUI;

public class SettingsInitializer : MonoBehaviour
{
    private void Start()
    {
        Bus Music;
        Bus SFX;
        Bus Ambient;
        Bus UI;
        Bus Master;

        Master = FMODUnity.RuntimeManager.GetBus("bus:/Master");
        Music = FMODUnity.RuntimeManager.GetBus("bus:/Master/Music");
        Ambient = FMODUnity.RuntimeManager.GetBus("bus:/Master/Ambient");
        UI = FMODUnity.RuntimeManager.GetBus("bus:/Master/UI");
        SFX = FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX");

        // ======= GRAPICS ========

        if (PlayerPrefs.HasKey("renderpipeline"))
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("renderpipeline"));

        if (PlayerPrefs.HasKey("resolution_x"))
            Screen.SetResolution(PlayerPrefs.GetInt("resolution_x"), PlayerPrefs.GetInt("resolution_y"),Screen.fullScreenMode);

        if (PlayerPrefs.HasKey("screenmode"))
            Screen.fullScreenMode = (FullScreenMode)PlayerPrefs.GetInt("screenmode");

        // ======= SOUNDS ========

        if (PlayerPrefs.HasKey("master_volume"))
            Master.setVolume(PlayerPrefs.GetFloat("master_volume"));

        if (PlayerPrefs.HasKey("sfx_volume"))
            SFX.setVolume(PlayerPrefs.GetFloat("sfx_volume"));

        if (PlayerPrefs.HasKey("ambient_volume"))
            Ambient.setVolume(PlayerPrefs.GetFloat("ambient_volume"));

        if (PlayerPrefs.HasKey("ui_volume"))
            UI.setVolume(PlayerPrefs.GetFloat("ui_volume"));

        if (PlayerPrefs.HasKey("music_volume"))
            Music.setVolume(PlayerPrefs.GetFloat("music_volume"));

        // ======= OTHERS ========

        if (PlayerPrefs.HasKey("language")) 
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[PlayerPrefs.GetInt("language")];
    
    }
}
