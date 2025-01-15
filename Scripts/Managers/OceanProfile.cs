using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOceanProfile", menuName = "새 오션 프로필 추가", order = 1)]
public class OceanProfile : SerializedScriptableObject
{
    //================================================
    //
    // 현재 바다 표면의 정보를 담는 스크립터블 오브젝트입니다.
    // 
    //================================================

    [SerializeField,ColorUsage(false,true), LabelText("바다 기본 색상")] private Color oceanColor;           // 바다 머트리얼 Emmision 색상
    [SerializeField, ColorUsage(false, true), LabelText("바다 고점 색상")] private Color oceanTipColor;      // 바다 머트리얼 고점 Emmision 색상
    public Color OceanColor { get { return oceanColor; } }
    public Color OceanTipColor { get { return oceanTipColor; } }
    [SerializeField, Range(0.0f, 2.0f), LabelText("파도 강도 곱")] private float oceanIntensity;
    public float OceanIntensity { get { return oceanIntensity; } }                                          // 파도 강도 곱
    
    public struct Waveform
    {
        [LabelText("벡터")] public Vector3 vector;              // 파도 벡터
        [LabelText("강도")] public float amplitude;             // 파도 강도
        [LabelText("속도")] public float gravity;               // 파도 속도
    }

    [SerializeField, LabelText("1번 파형")] private Waveform waveform1;
    public Waveform Waveform1 { get { return waveform1; } }
    [SerializeField, LabelText("2번 파형")] private Waveform waveform2;
    public Waveform Waveform2 { get { return waveform2; } }
    [SerializeField, LabelText("3번 파형")] private Waveform waveform3;
    public Waveform Waveform3 { get { return waveform3; } }
    [SerializeField, LabelText("4번 파형")] private Waveform waveform4;
    public Waveform Waveform4 { get { return waveform4; } }

    public void InitilzeOceanProfile(Color _color,Color _tipColor, 
        float _oceanIntensity, 
        Vector3 _wv1, float _wa1, float _wg1, 
        Vector3 _wv2, float _wa2, float _wg2, 
        Vector3 _wv3, float _wa3, float _wg3,
        Vector3 _wv4, float _wa4, float _wg4) 
    {   
        oceanColor = _color; oceanTipColor = _tipColor; oceanIntensity = _oceanIntensity; 
        waveform1.vector = _wv1; waveform1.amplitude = _wa1; waveform1.gravity = _wg1;
        waveform2.vector = _wv2; waveform2.amplitude = _wa2; waveform2.gravity = _wg2;
        waveform3.vector = _wv3; waveform3.amplitude = _wa3; waveform3.gravity = _wg3;
        waveform4.vector = _wv4; waveform4.amplitude = _wa4; waveform4.gravity = _wg4;
    }
}
