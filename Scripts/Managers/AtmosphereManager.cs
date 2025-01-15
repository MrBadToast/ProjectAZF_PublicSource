using DistantLands.Cozy;
using DistantLands.Cozy.Data;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AtmosphereManager : StaticSerializedMonoBehaviour<AtmosphereManager>
{
    private struct AtmosTransition
    {
        public AzfAtmosProfile atmosProfile;
        public float transitionTime;
    }

    [Title("ChildReference")]
    [SerializeField] private AzfAtmosProfile defaultAtmos;
    [SerializeField] private Volume ppGlobalFirst;
    [SerializeField] private Volume ppGlobalSecond;

    [Title("Debug")]
    [SerializeField, ReadOnly, LabelText("CozyWeather 활성화")] private bool debug_cozyWeatherValid = false;
    [SerializeField, ReadOnly, LabelText("OceanProfile 활성화")] private bool debug_oceanProfileValid = false;

    private CozyWeather cozyWeatherInstance;
    private Coroutine transitionCoroutine;
    [SerializeField,ReadOnly()]private Queue<AtmosTransition> transitionQueue;

    protected override void Awake()
    {
        base.Awake();
        transitionQueue = new Queue<AtmosTransition>();
    }

    private void Start()
    {
        if (cozyWeatherInstance == null)
            cozyWeatherInstance = FindFirstObjectByType<CozyWeather>();

#if UNITY_EDITOR

        if (cozyWeatherInstance == null)
        {
            Debug.LogError("CozyWeather 오브젝트를 찾을 수 없었습니다. 날씨 제어가 제한됩니다.");
            debug_cozyWeatherValid = false;
        }
        else
           debug_cozyWeatherValid = true;

        if (!GlobalOceanManager.IsInstanceValid)
        {
            Debug.LogError("GlobalOceanManager 오브젝트를 찾을 수 없습니다. 바다 제어가 제한됩니다.");
            debug_oceanProfileValid = false;
        }
        else
            debug_oceanProfileValid = true;

#endif
    }

    private void Update()
    {
#if UNITY_EDITOR

        if (cozyWeatherInstance == null)
            debug_cozyWeatherValid = false;
        else
            debug_cozyWeatherValid = true;

        if (!GlobalOceanManager.IsInstanceValid)
            debug_oceanProfileValid = false;
        else
            debug_oceanProfileValid = true;
#endif
    }

    public static void ChangeAtmosphere(AzfAtmosProfile profile, float transitionTime)
    {
        if (Instance == null) { Debug.LogError("AtmosphereManager가 없습니다."); return; }

        AtmosTransition newTransition = new AtmosTransition();
        newTransition.atmosProfile = profile; 
        if(transitionTime <= 0.1f) { newTransition.transitionTime = 0.1f; }
        newTransition.transitionTime = transitionTime;

        Instance.transitionQueue.Enqueue(newTransition);

        if (Instance.transitionCoroutine == null)
            Instance.transitionCoroutine = Instance.StartCoroutine(Instance.Cor_ChangeAtmosInQueued());
    }


#if UNITY_EDITOR
    [Button("(디버그) 기후 프로필 적용"),HideInEditorMode()]
#endif
    public void Debug_TryAtmosphereProfile(AzfAtmosProfile profile,float transitionTime)
    {
        ChangeAtmosphere(profile, transitionTime);
    }

#if UNITY_EDITOR
    [Button("(디버그) 기후 프로필 적용"), HideInPlayMode()]
#endif
    public void Debug_TryAtmosphereProfile_Editor(AzfAtmosProfile profile)
    {
        FindFirstObjectByType<CozyWeather>().weatherModule.currentWeatherProfiles[0].profile = profile.weatherProfile;
        FindFirstObjectByType<CozyWeather>().weatherModule.UpdateWeatherWeights();
        FindFirstObjectByType<GlobalOceanManager>().SetWaveImmedietly(profile.oceanProfile);
        ppGlobalFirst.profile = profile.postprocessProfile;
        ppGlobalFirst.weight = 1.0f;
        ppGlobalSecond.weight = 0f;
        RenderSettings.fogColor = profile.fogProfile.FogColor;
        RenderSettings.fogStartDistance = profile.fogProfile.FogDistance.x;
        RenderSettings.fogEndDistance = profile.fogProfile.FogDistance.y;
    }

#if UNITY_EDITOR
    [Button("(디버그) 기후 프로필 기본값으로 변경"), DisableInPlayMode()]
#endif
    public void Debug_ResetAtmoProfile()
    {
        FindFirstObjectByType<CozyWeather>().weatherModule.currentWeatherProfiles[0].profile = defaultAtmos.weatherProfile;
        FindFirstObjectByType<CozyWeather>().weatherModule.UpdateWeatherWeights();
        FindFirstObjectByType<GlobalOceanManager>().SetWaveImmedietly(defaultAtmos.oceanProfile);
        ppGlobalFirst.profile = defaultAtmos.postprocessProfile;
        ppGlobalFirst.weight = 1.0f;
        ppGlobalSecond.weight = 0f;
        RenderSettings.fogColor = defaultAtmos.fogProfile.FogColor;
        RenderSettings.fogStartDistance = defaultAtmos.fogProfile.FogDistance.x;
        RenderSettings.fogEndDistance = defaultAtmos.fogProfile.FogDistance.y;
    }


    private IEnumerator Cor_ChangeAtmosInQueued()
    {
        while (transitionQueue.Count > 0)
        {
            AtmosTransition currentAtmos = transitionQueue.Dequeue();

            if (currentAtmos.atmosProfile.weatherProfile != null && cozyWeatherInstance != null)
            {
                cozyWeatherInstance.weatherModule.ecosystem.SetWeather(currentAtmos.atmosProfile.weatherProfile, currentAtmos.transitionTime);
            }
            if (currentAtmos.atmosProfile.oceanProfile != null && GlobalOceanManager.IsInstanceValid)
            {
                GlobalOceanManager.Instance.SetWave(currentAtmos.atmosProfile.oceanProfile, currentAtmos.transitionTime);
            }

            if (currentAtmos.atmosProfile.postprocessProfile != null)
            {
                ppGlobalFirst.profile = ppGlobalSecond.profile;
                ppGlobalSecond.profile = currentAtmos.atmosProfile.postprocessProfile;
                ppGlobalFirst.weight = 1.0f;
                ppGlobalSecond.weight = 0f;
            }

            Color fromFogColor = currentAtmos.atmosProfile.fogProfile.FogColor;
            float fromFogStart = RenderSettings.fogStartDistance;
            float fromFogEnd = RenderSettings.fogEndDistance;

            for (float time = 0; time < currentAtmos.transitionTime; time += Time.fixedDeltaTime)
            {
                float t = time / currentAtmos.transitionTime;
                float neg_t = 1f - t;

                if (currentAtmos.atmosProfile.postprocessProfile != null)
                {
                    ppGlobalFirst.weight = neg_t;
                    ppGlobalSecond.weight = t;
                }

                if (!currentAtmos.atmosProfile.fogProfile.NoFogChange)
                {
                    RenderSettings.fogColor = Color.Lerp(fromFogColor, currentAtmos.atmosProfile.fogProfile.FogColor, t);
                    RenderSettings.fogStartDistance = Mathf.Lerp(fromFogStart, currentAtmos.atmosProfile.fogProfile.FogDistance.x, t);
                    RenderSettings.fogEndDistance = Mathf.Lerp(fromFogEnd, currentAtmos.atmosProfile.fogProfile.FogDistance.y, t);
                }

                yield return new WaitForFixedUpdate();
            }

            if (currentAtmos.atmosProfile.postprocessProfile != null)
            {
                ppGlobalFirst.weight = 0f;
                ppGlobalSecond.weight = 1f;
            }

            if (!currentAtmos.atmosProfile.fogProfile.NoFogChange)
            {
                RenderSettings.fogColor = currentAtmos.atmosProfile.fogProfile.FogColor;
                RenderSettings.fogStartDistance = currentAtmos.atmosProfile.fogProfile.FogDistance.x;
                RenderSettings.fogEndDistance = currentAtmos.atmosProfile.fogProfile.FogDistance.y;
            }

            yield return null;
        }

        Instance.transitionCoroutine = null;
        yield return null;
    }
}
