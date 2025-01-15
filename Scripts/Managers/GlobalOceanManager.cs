using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Rendering;
using ReadOnly = Sirenix.OdinInspector.ReadOnlyAttribute;
using NUnit.Framework.Api;

[ExecuteAlways]
public class GlobalOceanManager : StaticSerializedMonoBehaviour<GlobalOceanManager>
//================================================
//
// [싱글턴 클래스]
// 현재 월드상의 바다와 관련된 데이터를 관리하고 물리적인 연산을 하는 시스템 클래스입니다.
// 파도의 물리적 연산은 잡시스템을 통해 멀티스레드로 처리됩니다.
// 4개의 파도 벡터와 강도가 중첩되어 바다를 형성합니다.
// 특정 오브젝트가 파도의 영향을 받은 위치를 계산하고자 한다면 이 클래스의 GetWavePosition이나, GetWaveHeight을 사용해야합니다.
// SetWave를 통해 파도 상태를 변형할 수 있습니다.
//
//================================================
{

    #region =============== Properties =====================
    [SerializeField] private Material[] ReferencingMaterials;                           // OceanSurface.mat을 가지고 있는 오브젝트들, 아래 속성들과 머트리얼의 속성을 맟추기 위해 필요

    [Title("GlobalWaveProperties")]
    [SerializeField] private OceanProfile defaultOceanProfile; // 초기 오션 프로파일
    private OceanProfile activeOceanProfile;
    [SerializeField, ReadOnly] private float islandregionIntensityFactor = 1.0f;
    public float IslandregionIntensityFactor
    {
        get { return islandregionIntensityFactor; }
        set { islandregionIntensityFactor = value; UpdateReferencingMaterials(); }
    }
    public float Intensity { get { return intensity * IslandregionIntensityFactor; } }  // (읽기 전용) 최종 파도 강도

    [SerializeField,DisableInPlayMode(),OnValueChanged("UpdateReferencingMaterials")] 
    private float rotation;                        // Gerstner 파도 속성 : 파도 회전값
    [SerializeField,DisableInPlayMode(), OnValueChanged("UpdateReferencingMaterials")] 
    private float depth;                           // Gerstner 파도 속성 : depth 값
    [SerializeField,DisableInPlayMode(), OnValueChanged("UpdateReferencingMaterials")] 
    private float phase;                           // Gerstner 파도 속성 : phase 값
    [SerializeField,DisableInPlayMode(), OnValueChanged("UpdateReferencingMaterials")] 
    private float gravity;                         // Gerstner 파도 속성 : gravity 값

    [Title("ProfileControlledPorperties")]
    [SerializeField, ReadOnly,ColorUsage(false,true)] private Color oceanEmmision;
    [SerializeField, ReadOnly, ColorUsage(false, true)] private Color oceanTipEmmision;
    [SerializeField, ReadOnly] private float intensity;
    [Title("")]
    [SerializeField, ReadOnly] private Vector3 Wave1_Vector;
    [SerializeField, ReadOnly] private float Wave1_Amplitude;
    [SerializeField, ReadOnly] private float Wave1_Gravity;
    [Title("")]
    [SerializeField, ReadOnly] private Vector3 Wave2_Vector;
    [SerializeField, ReadOnly] private float Wave2_Amplitude;
    [SerializeField, ReadOnly] private float Wave2_Gravity;
    [Title("")]
    [SerializeField, ReadOnly] private Vector3 Wave3_Vector;
    [SerializeField, ReadOnly] private float Wave3_Amplitude;
    [SerializeField, ReadOnly] private float Wave3_Gravity;
    [Title("")]
    [SerializeField, ReadOnly] private Vector3 Wave4_Vector;
    [SerializeField, ReadOnly] private float Wave4_Amplitude;
    [SerializeField, ReadOnly] private float Wave4_Gravity;

    #endregion

    #region ================== Job structs ===================

    private struct WavePositionJob : IJob
    {
        public Vector3 input;
        public NativeArray<Vector3> output;

        public float intensity;
        public float rotation;
        public float gravity;
        public float depth;
        public float phase;
        public float time;

        public NativeArray<Vector3> waveVectors;
        public NativeArray<float> waveAmplitudes;
        public NativeArray<float> gravities;

        private Vector3 SingleGerstnerWavePosition(Vector3 position, Vector3 direction, float amplitude, float localGravity)
        {
            float freq = Mathf.Sqrt(gravity * localGravity * direction.magnitude * (float)(System.Math.Tanh(depth * direction.magnitude)));
            float theta = (direction.x * position.x + direction.z * position.z) - freq * time - phase;

            float x = -(amplitude * intensity / ((float)(System.Math.Tanh(direction.magnitude * depth))) * direction.x / direction.magnitude * Mathf.Sin(theta));
            float y = Mathf.Cos(theta) * amplitude * intensity;
            float z = -(amplitude * intensity / ((float)(System.Math.Tanh(direction.magnitude * depth))) * direction.z / direction.magnitude * Mathf.Sin(theta));

            return new Vector3(x, y, z);
        }

        public void Execute()
        {
            Vector3 result = Vector3.zero;

            if (waveVectors.Length != waveAmplitudes.Length) return; // failed for invalid arrayInput;

            for (int i = 0; i < waveVectors.Length; i++)
            {
                Vector3 rotatedVector = Quaternion.AngleAxis(rotation, Vector3.up) * waveVectors[i];
                result += SingleGerstnerWavePosition(input, rotatedVector, waveAmplitudes[i], gravities[i]);
            }

            output[0] = result;
        }

    }

    private struct WaveHeightJob : IJob
    {
        public Vector3 input;
        public NativeArray<float> output;

        public float rotation;
        public float intensity;
        public float gravity;
        public float depth;
        public float phase;
        public float time;

        public NativeArray<Vector3> waveVectors;
        public NativeArray<float> waveAmplitudes;
        public NativeArray<float> gravities;

        private Vector3 SingleGerstnerWavePosition(Vector3 position, Vector3 direction, float amplitude, float localGravity ,bool calculateY = true)
        {
            float freq = Mathf.Sqrt(gravity * localGravity * direction.magnitude * (float)(System.Math.Tanh(depth * direction.magnitude)));
            float theta = (direction.x * position.x + direction.z * position.z) - freq * time - phase;

            float x = -(amplitude * intensity / ((float)(System.Math.Tanh(direction.magnitude * depth))) * direction.x / direction.magnitude * Mathf.Sin(theta));
            float y = 0f;
            if (calculateY) y = Mathf.Cos(theta) * amplitude * intensity;
            float z = -(amplitude * intensity / ((float)(System.Math.Tanh(direction.magnitude * depth))) * direction.z / direction.magnitude * Mathf.Sin(theta));

            return new Vector3(x, y, z);
        }

        private Vector3 GetComlexWavePostion(Vector3 input,bool calculateY = true)
        {
            Vector3 result = Vector3.zero;

            if (waveVectors.Length != waveAmplitudes.Length) return input; // failed for invalid arrayInput;

            for (int i = 0; i < waveVectors.Length; i++)
            {
                Vector3 rotatedVector = Quaternion.AngleAxis(rotation, Vector3.up) * waveVectors[i];
                result += SingleGerstnerWavePosition(input, waveVectors[i], waveAmplitudes[i], gravities[i],calculateY);
            }

            return result;
        }

        public void Execute()
        {
            Vector3 pointXZ = new Vector3(input.x, 0f, input.z);
            Vector3 iteration = pointXZ - GetComlexWavePostion(pointXZ,false);
            iteration = pointXZ - GetComlexWavePostion(iteration,false);
            iteration = pointXZ - GetComlexWavePostion(iteration,false);

            output[0] = GetComlexWavePostion(iteration,true).y;
        }
    }

    #endregion

    private void OnEnable() {
        UpdateReferencingMaterials();
    }

    private void Start()
    {
        SetWaveImmedietly(defaultOceanProfile);
        activeOceanProfile = defaultOceanProfile;

        UpdateReferencingMaterials();
    }

    /// <summary>
    /// GlobalOceanManager의 속성값을 머트리얼의 속성값에 적용시킵니다.
    /// </summary>
    public void UpdateReferencingMaterials()
    {
        foreach(Material m in ReferencingMaterials)
        {
            m.SetColor("_Emmision", oceanEmmision);
            m.SetColor("_TipEmission", oceanTipEmmision);
            m.SetFloat("_Intensity",Intensity);
            m.SetFloat("_Rotation", rotation);
            m.SetFloat("_Depth",depth);
            m.SetFloat("_Phase",phase);
            m.SetFloat("_Gravity",gravity);
            m.SetVector("_Direction1",Wave1_Vector);
            m.SetFloat("_Amplitude1",Wave1_Amplitude);
            m.SetFloat("_Gravity1", Wave1_Gravity);
            m.SetVector("_Direction2",Wave2_Vector);
            m.SetFloat("_Amplitude2",Wave2_Amplitude);
            m.SetFloat("_Gravity2", Wave2_Gravity);
            m.SetVector("_Direction3",Wave3_Vector);
            m.SetFloat("_Amplitude3",Wave3_Amplitude);
            m.SetFloat("_Gravity3", Wave3_Gravity);
            m.SetVector("_Direction4",Wave4_Vector);
            m.SetFloat("_Amplitude4",Wave4_Amplitude);
            m.SetFloat("_Gravity4", Wave4_Gravity);
        }    
    }

    /// <summary>
    /// 바다 상태를 즉시 바꿉니다.
    /// </summary>
    /// <param name="oceanProfile"> 오션프로파일 </param>
#if UNITY_EDITOR
    [Button(ButtonSizes.Small),DisableInEditorMode]
#endif
    public void SetWaveImmedietly(OceanProfile oceanProfile)
    {
        if (oceanProfile == null) return;

        activeOceanProfile = oceanProfile;
        oceanEmmision = oceanProfile.OceanColor;
        oceanTipEmmision = oceanProfile.OceanTipColor;

        intensity = oceanProfile.OceanIntensity;

        Wave1_Vector = oceanProfile.Waveform1.vector;
        Wave1_Amplitude = oceanProfile.Waveform1.amplitude;
        Wave1_Gravity = oceanProfile.Waveform1.gravity;

        Wave2_Vector = oceanProfile.Waveform2.vector;
        Wave2_Amplitude = oceanProfile.Waveform2.amplitude;
        Wave2_Gravity = oceanProfile.Waveform2.gravity;

        Wave3_Vector = oceanProfile.Waveform3.vector;
        Wave3_Amplitude = oceanProfile.Waveform3.amplitude;
        Wave3_Gravity = oceanProfile.Waveform3.gravity;

        Wave4_Vector = oceanProfile.Waveform4.vector;
        Wave4_Amplitude = oceanProfile.Waveform4.amplitude;
        Wave4_Gravity = oceanProfile.Waveform4.gravity;

        UpdateReferencingMaterials();
    }

    Coroutine wavechangeAnimationCoroutine;
    OceanProfile opro_origin;
    OceanProfile opro_destination;
    
    /// <summary>
    /// 바다 상태를 천천히 바꿉니다
    /// </summary>
    /// <param name="oceanProfile"> 오션프로파일 </param>
    /// <param name="time"> 전환 시간 </param>
#if UNITY_EDITOR
    [Button(ButtonSizes.Small),DisableInEditorMode]
#endif
    public void SetWave(OceanProfile oceanProfile, float time = 1.0f)
    {
        Debug.Log("CHANGING WAVE | PROFILE : " + oceanProfile.name + " , TIME : " + time);

        if (oceanProfile == null) return;

        if (opro_origin == null) opro_origin = activeOceanProfile;

        if(wavechangeAnimationCoroutine == null)
        {
            AnimationCurve linear = new AnimationCurve(new Keyframe(0,0), new Keyframe(1, 1));
            wavechangeAnimationCoroutine = StartCoroutine(Cor_AnimateWaveChange(oceanProfile, time, linear));
            opro_origin = activeOceanProfile;
            opro_destination = oceanProfile;
        }
        else
        {
            StopCoroutine(wavechangeAnimationCoroutine);

            activeOceanProfile = opro_destination;
            opro_origin = activeOceanProfile;

            AnimationCurve linear = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            wavechangeAnimationCoroutine = StartCoroutine(Cor_AnimateWaveChange(oceanProfile, time, linear));
            opro_origin = activeOceanProfile;
            opro_destination = oceanProfile;
        }
    }

    /// <summary>
    /// 바다 상태를 애니메이션 커브에 따라 천천히 바꿉니다
    /// </summary>
    /// <param name="oceanProfile"> 오션프로파일 </param>
    /// <param name="time"> 전환 시간 </param>
    /// <param name="curve"> 애니메이션 커브 </param>
    public void SetWave(OceanProfile oceanProfile, float time, AnimationCurve curve)
    {
        Debug.Log("CHANGING WAVE | PROFILE : " + oceanProfile.name + " , TIME : " + time + " , WITH ANIMATION CURVE");

        if (oceanProfile == null) return;

        if (wavechangeAnimationCoroutine == null)
        {
            wavechangeAnimationCoroutine = StartCoroutine(Cor_AnimateWaveChange(oceanProfile, time, curve));
            opro_origin = activeOceanProfile;
            opro_destination = oceanProfile;
        }
        else
        {
            StopCoroutine(wavechangeAnimationCoroutine);

            activeOceanProfile = opro_destination;
            opro_origin = activeOceanProfile;

            wavechangeAnimationCoroutine = StartCoroutine(Cor_AnimateWaveChange(oceanProfile, time, curve));
            opro_origin = activeOceanProfile;
            opro_destination = oceanProfile;
        }

        UpdateReferencingMaterials();
    }

    private IEnumerator Cor_AnimateWaveChange(OceanProfile to, float time, AnimationCurve curve)
    {
        for(float t = 0; t < time; t+= Time.deltaTime)
        {
            float evaluated = curve.Evaluate(t / time);

            intensity = Mathf.Lerp(opro_origin.OceanIntensity, to.OceanIntensity, evaluated);

            oceanEmmision = Color.Lerp(opro_origin.OceanColor, to.OceanColor,evaluated);
            oceanTipEmmision = Color.Lerp(opro_origin.OceanTipColor, to.OceanTipColor, evaluated);

            Wave1_Vector = Vector3.Lerp(opro_origin.Waveform1.vector, to.Waveform1.vector, evaluated);
            Wave1_Amplitude = Mathf.Lerp(opro_origin.Waveform1.amplitude, to.Waveform1.amplitude, evaluated);
            Wave1_Gravity = Mathf.Lerp(opro_origin.Waveform1.gravity, to.Waveform1.gravity, evaluated);

            Wave2_Vector = Vector3.Lerp(opro_origin.Waveform2.vector, to.Waveform2.vector, evaluated);
            Wave2_Amplitude = Mathf.Lerp(opro_origin.Waveform2.amplitude, to.Waveform2.amplitude, evaluated);
            Wave2_Gravity = Mathf.Lerp(opro_origin.Waveform2.gravity, to.Waveform2.gravity, evaluated);

            Wave3_Vector = Vector3.Lerp(opro_origin.Waveform3.vector, to.Waveform3.vector, evaluated);
            Wave3_Amplitude = Mathf.Lerp(opro_origin.Waveform3.amplitude, to.Waveform3.amplitude, evaluated);
            Wave3_Gravity = Mathf.Lerp(opro_origin.Waveform3.gravity, to.Waveform3.gravity, evaluated);

            Wave4_Vector = Vector3.Lerp(opro_origin.Waveform4.vector, to.Waveform4.vector, evaluated);
            Wave4_Amplitude = Mathf.Lerp(opro_origin.Waveform4.amplitude, to.Waveform4.amplitude, evaluated);
            Wave4_Gravity = Mathf.Lerp(opro_origin.Waveform4.gravity, to.Waveform4.gravity, evaluated);

            UpdateReferencingMaterials();
            yield return null;
        }

        intensity = to.OceanIntensity;

        oceanEmmision = to.OceanColor;
        oceanTipEmmision = to.OceanTipColor;

        Wave1_Vector = to.Waveform1.vector;
        Wave1_Amplitude = to.Waveform1.amplitude;
        Wave1_Gravity =  to.Waveform1.gravity;

        Wave2_Vector = to.Waveform2.vector;
        Wave2_Amplitude = to.Waveform2.amplitude;
        Wave2_Gravity = to.Waveform2.gravity;

        Wave3_Vector = to.Waveform3.vector;
        Wave3_Amplitude = to.Waveform3.amplitude;
        Wave3_Gravity = to.Waveform3.gravity;

        Wave4_Vector = to.Waveform4.vector;
        Wave4_Amplitude = to.Waveform4.amplitude;
        Wave4_Gravity = to.Waveform4.gravity;
        UpdateReferencingMaterials();

        activeOceanProfile = to;
        wavechangeAnimationCoroutine = null;
    }

    /// <summary>
    /// 계산 지점에서 바다의 파도로 인해 변화한 위치를 나타냅니다.
    /// </summary>
    /// <param name="point"> 기준 지점 </param>
    /// <returns></returns>
    public Vector3 GetWavePosition(Vector3 point) 

    {
        Vector3[] vecs = new Vector3[] { Wave1_Vector, Wave2_Vector, Wave3_Vector, Wave4_Vector };
        float[] amps = new float[] { Wave1_Amplitude, Wave2_Amplitude, Wave3_Amplitude, Wave4_Amplitude };
        float[] gravs = new float[] { Wave1_Gravity, Wave2_Gravity, Wave3_Gravity, Wave4_Gravity };

        WavePositionJob job = new WavePositionJob()
        {
            input = point,
            output = new NativeArray<Vector3>(1, Allocator.Persistent),
            rotation = this.rotation,
            intensity = this.Intensity,
            gravity = this.gravity,
            depth = this.depth,
            phase = this.phase,
            time = Time.time,

            waveVectors = new NativeArray<Vector3>(vecs, Allocator.Persistent),
            waveAmplitudes = new NativeArray<float>(amps, Allocator.Persistent),
            gravities = new NativeArray<float>(gravs, Allocator.Persistent)
        };

        JobHandle handle = job.Schedule();
        handle.Complete();

        Vector3 result = job.output[0];

        job.output.Dispose();
        job.waveVectors.Dispose();
        job.waveAmplitudes.Dispose();
        job.gravities.Dispose();

        return result;
    }

    /// <summary>
    /// 계산 지점에서 현재 바다 수면의 높이 값을(y) 구합니다.
    /// </summary>
    /// <param name="point"> 계산 지점 </param>
    /// <returns></returns>
    public float GetWaveHeight(Vector3 point) 
    {
        Vector3[] vecs = new Vector3[] { Wave1_Vector, Wave2_Vector, Wave3_Vector, Wave4_Vector };
        float[] amps = new float[] { Wave1_Amplitude, Wave2_Amplitude, Wave3_Amplitude, Wave4_Amplitude };
        float[] gravs = new float[] { Wave1_Gravity, Wave2_Gravity, Wave3_Gravity, Wave4_Gravity };

        WaveHeightJob job = new WaveHeightJob()
        {
            input = point,
            output = new NativeArray<float>(1,Allocator.TempJob),
            rotation = this.rotation,
            intensity = this.Intensity,
            gravity = this.gravity,
            depth = this.depth,
            phase = this.phase,
            time = Time.time,

            waveVectors = new NativeArray<Vector3>(vecs, Allocator.TempJob),
            waveAmplitudes = new NativeArray<float>(amps, Allocator.TempJob),
            gravities = new NativeArray<float>(gravs, Allocator.TempJob)
        };


        JobHandle handle = job.Schedule();
        handle.Complete();

        float result = job.output[0];

        job.output.Dispose();
        job.waveVectors.Dispose();
        job.waveAmplitudes.Dispose();
        job.gravities.Dispose();

        return result;
    }

#if UNITY_EDITOR
    [Button(ButtonSizes.Small,Name = "DefaultOceanProfile 적용"),PropertyOrder(-1)]
#endif
    private void GUI_ChangeProfile()
    {
        SetWaveImmedietly(defaultOceanProfile);
    }

#if UNITY_EDITOR
    [Button(ButtonSizes.Small, Name = "DefaultOceanProfile 적용"), PropertyOrder(-1)]
#endif
    private void Debug_ChangeProfile(OceanProfile profile)
    {
        SetWaveImmedietly(profile);
    }
}
