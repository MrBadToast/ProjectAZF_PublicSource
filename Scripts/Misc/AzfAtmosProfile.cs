using DistantLands.Cozy.Data;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "NewAtmosphereProfile", menuName = "새 기후 설정 추가", order = 1)]
public class AzfAtmosProfile : SerializedScriptableObject
{
    public struct FogSetting
    {
        [LabelText("안개 설정 안함")] public bool NoFogChange;
        [LabelText("안개 색상")]public Color FogColor;
        [MinMaxSlider(0f,5000f,ShowFields = true),LabelText("안개 거리 (최소, 최대)")] public Vector2 FogDistance;
    }

    [SerializeField,LabelText("날씨 설정")] public WeatherProfile weatherProfile;
    [SerializeField, LabelText("안개 설정")] public FogSetting fogProfile;
    [SerializeField, LabelText("바다 설정")] public OceanProfile oceanProfile;
    [SerializeField,LabelText("포스트 프로세싱 설정")] public VolumeProfile postprocessProfile;
    [SerializeField,LabelText("배경 음악 설정")] public EventReference fieldMusic;

}
