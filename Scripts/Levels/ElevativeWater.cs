using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEditor;
using UnityEngine;


/// <summary>
/// 가변적인 수위 오브젝트에 적용되는 스크립트입니다.
/// </summary>
[RequireComponent(typeof(StudioEventEmitter))]
public class ElevativeWater : MonoBehaviour
{
    [System.Serializable]
    class WaterChange
    {
        [LabelText("수위 (M)")] public float height = 0f;
        [LabelText("시간 (초)")] public float time = 1f;
    }

    [Title("")]
    [SerializeField,LabelText("수위 조절 옵션")] private WaterChange[] waterLevelOptions;
    [SerializeField,LabelText("수위 조절 커브")] private AnimationCurve changeCurve;

    [Title("")]
    [SerializeField] private Transform waterTF;

    private StudioEventEmitter sound;
    private Coroutine transition;
    private int currentindex = 0;


    private void Awake()
    {
        sound = GetComponent<StudioEventEmitter>();
    }

    private void Start()
    {
        waterTF.localScale = new Vector3(waterTF.localScale.x, waterLevelOptions[0].height , waterTF.localScale.z);
    }

    /// <summary>
    /// 수면 높이를 조절합니다.
    /// </summary>
    /// <param name="waterLevel"> 높이 </param>
    public void SetWaterlevel(float waterLevel)
    {
        if (transition != null) return;

        transition = StartCoroutine(Cor_SetWaterlevel(waterLevel,1.0f));
        return;
    }

    /// <summary>
    /// 특정한 시간동안 수면 높이를 조절합니다.
    /// </summary>
    /// <param name="waterLevel"> 높이 </param>
    /// <param name="time"> 시간 </param>
    public void SetWaterlevel(float waterLevel, float time)
    {
        if (transition != null) return;

        transition = StartCoroutine(Cor_SetWaterlevel(waterLevel, time));
        return;
    }

    /// <summary>
    /// waterLevelOptions에서 해당 인덱스의 해수면 높이로 맞춥니다.
    /// </summary>
    /// <param name="index"></param>
    public void SetWaterlevel(int index)
    {
        if (transition != null) return;

        transition = StartCoroutine(Cor_SetWaterlevel(waterLevelOptions[index].height,waterLevelOptions[index].time));
        currentindex = index;
        return;
    }

    /// <summary>
    /// 현재 인덱스에서 다음 인덱스로 수면을 조절합니다.
    /// </summary>
    public void SetWaterlevelAuto()
    {
        if (transition != null) return;

        transition = StartCoroutine(Cor_SetWaterlevel(waterLevelOptions[currentindex].height, waterLevelOptions[currentindex].time));
        currentindex++;
        if (currentindex >= waterLevelOptions.Length) currentindex = 0;
        return;
    }

    IEnumerator Cor_SetWaterlevel(float waterLevel,float time)
    {
        float from = waterTF.localScale.y;

        sound.Play();
        sound.SetParameter("ElevativeWaterParam", 0);

        for(float t = 0; t < time; t+= Time.fixedDeltaTime)
        {
            float segment = changeCurve.Evaluate(t/time);
            waterTF.localScale = new Vector3(waterTF.localScale.x,Mathf.Lerp(from,waterLevel,segment) ,waterTF.localScale.z);
            yield return new WaitForFixedUpdate();
        }

        sound.SetParameter("ElevativeWaterParam", 1);

        waterTF.localScale = new Vector3(waterTF.localScale.x, waterLevel, waterTF.localScale.z);

        transition = null;
    }

    // duration(초) 동안 value값을 향해 curve의 형태에 따라 부드럽게 옆으로 이동합니다.
    IEnumerator Cor_AnimateProperty(float destination, float duration, AnimationCurve curve)
    {
        float from = transform.position.x;

        for(float time = 0; time < duration; time += Time.deltaTime)
        {
            float t = curve.Evaluate(duration/time);
            transform.position = new Vector3(t, 0, 0);
            yield return null;
        }
       
        transform.position = new Vector3(destination, 0, 0);

    }

    private void OnDrawGizmosSelected()  
    {
        Gizmos.color = Color.blue;
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = 18;
        labelStyle.fontStyle = FontStyle.Bold;

        if (waterLevelOptions != null && waterLevelOptions.Length > 0)
        {
            for (int i = 0; i < waterLevelOptions.Length; i++)
            {
                if (i != 0) DrawArrow.ForGizmo(transform.position + Vector3.up*waterLevelOptions[i - 1].height + Vector3.right*(i-2)*0.2f, Vector3.up * (waterLevelOptions[i].height - waterLevelOptions[i - 1].height));

                Gizmos.DrawWireCube(transform.position + Vector3.up * waterLevelOptions[i].height, new Vector3(2f, 0f, 2f));
#if UNITY_EDITOR
                Handles.Label(transform.position + Vector3.up * waterLevelOptions[i].height + Vector3.right * -1f, i.ToString() + "번 수위 : " + waterLevelOptions[i].height + "M",labelStyle);
#endif
            }
            int last = waterLevelOptions.Length-1;
            Gizmos.color = Color.magenta;
            DrawArrow.ForGizmo(transform.position + Vector3.up * waterLevelOptions[last].height + Vector3.right * (last - 2) * 0.2f, Vector3.up * (waterLevelOptions[0].height - waterLevelOptions[last].height));

        }
    }

}
