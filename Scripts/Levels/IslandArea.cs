using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

public class IslandArea : MonoBehaviour
{
    //================================================
    //
    // 섬의 고유 정보와 영역을 나타내는 스크립트 입니다.
    // 플레이어가 fullArea범위 내로 섬에 접근하면 고유의 연출을 낼 수 있습니다.
    // 또한, 플레이어가 섬에 가까워짐에 따라 파도를 잦아들게 설정할 수 있습니다.
    //
    //================================================

    public static List<IslandArea> EnteredArea { get { return enteredArea; } }
    private static List<IslandArea> enteredArea;

#if UNITY_EDITOR
    private List<IslandArea> deubg_currentEnteredArea;
#endif

    [SerializeField] private string islandID;
    [SerializeField] private Transform spawnTransform;
    public string IslandID { get { return islandID; } }                         // 섬 구분 ID
    [SerializeField] private LocalizedString islandName;    
    public LocalizedString IslandName { get { return islandName; } }            // 섬 이름 ( UI )
    [SerializeField] private float innerArea = 50f;                             // “내부 구역” 경계
    [SerializeField] private float outerArea = 100f;                            // “외부 구역” 경계
    [SerializeField] private UnityEvent eventOnInnerEnter;                   // ※섬 내부에서만 사용할 것※ 섬 내부 구역 진입시 발생하는 이벤트
    [SerializeField] private EventReference sound_Enter;                        // 섬 진입 시 사운드

    [FoldoutGroup("EnvoirmentSettings"), SerializeField]
    private bool supressWave = true;                        // true일 시 섬 구역 진입시 파도를 잦아들게함
    [FoldoutGroup("EnvoirmentSettings"), SerializeField]
    private float waveIntensity = 0.1f;                     // 섬 구역 진입시 파도가 얼마나 잦아들지 설정


    private bool playerEnterFlag = false;
    private Transform playerPosition;
    bool regionEnterIgnore = true;

#if UNITY_EDITOR
#pragma warning disable CS0414
    [Title("Info")]
    [SerializeField, ReadOnly, LabelText("DistanceFromPlayer")] private float debug_distanceFromPlayer;
#pragma warning restore CS0414
#endif

    private void Awake()
    {
        if(enteredArea == null) enteredArea = new List<IslandArea>();
    }

    private void Start()
    {
        if(PlayerCore.IsInstanceValid)
        {
            playerPosition = PlayerCore.Instance.transform;
            if (GetAreaInterpolation(playerPosition.position) > 0) playerEnterFlag = true;
        }

        if (Vector3.Distance(playerPosition.position, transform.position) < innerArea)
        {
            playerEnterFlag = true;

            enteredArea.Add(this);

            if (EnterUIFilter())
            {
                OnInnerAreaEnter();
            }
        }

    }

    private void OnEnable()
    {
        if (PlayerCore.IsInstanceValid)
        {
            playerPosition = PlayerCore.Instance.transform;
            if (GetAreaInterpolation(playerPosition.position) > 0) playerEnterFlag = true;
        }
    }

    private float enterInterval = 10f;
    private float enterTimer = 1000f;

    private void Update()
    {
#if UNITY_EDITOR

#endif

        enterTimer += Time.deltaTime;

        if(playerPosition != null)
        {
            float distanceValue = GetAreaInterpolation(playerPosition.position);
#if UNITY_EDITOR
            debug_distanceFromPlayer = distanceValue;
#endif

            if (playerEnterFlag == false)
            {
                if (Vector3.Distance(playerPosition.position, transform.position) < innerArea)
                {
                    playerEnterFlag = true;

                    enteredArea.Add(this);

                    if (EnterUIFilter())
                    {
                        OnInnerAreaEnter();
                    }
                }
            }
            else
            {
                if (Vector3.Distance(playerPosition.position, transform.position) > innerArea)
                {
                    enteredArea.Remove(this);
                    playerEnterFlag = false;
                }
            }
        
            if(distanceValue > 0)
            {
                if (supressWave)
                {
                    float value = Mathf.Lerp(1.0f, waveIntensity, distanceValue);

                    if (value < 0.01f) value = 0f;
                    else if (value > 0.99f) value = 1f;

                    GlobalOceanManager.Instance.IslandregionIntensityFactor = value;
                }
            }
        }
    }

    /// <summary>
    /// 해당 위치가 외부 구역 ~ 내부 구역 사이에서 어느정도 거리에 있는지 0~1 로 표현합니다.
    /// </summary>
    /// <param name="t_postion"> 위치 </param>
    /// <returns></returns>
    public float GetAreaInterpolation(Vector3 t_postion)
    {
        if (Vector3.Distance(transform.position, t_postion) > outerArea) return 0;
        else if(Vector3.Distance(transform.position, t_postion) < innerArea) return 1;
        else
        {
            float value = Mathf.InverseLerp(outerArea, innerArea, Vector3.Distance(transform.position, t_postion));

            if (value < 0.01f) value = 0f;
            else if (value > 0.99f) value = 1f;

            return value;
        }
    }

    private void OnInnerAreaEnter()
    {
        if (eventOnInnerEnter != null)
            eventOnInnerEnter.Invoke();

        UI_RegionEnter regionEnter = UI_RegionEnter.Instance;
        if (regionEnter != null)
        {
            if (!regionEnterIgnore)
            {
                regionEnter.OnRegionEnter(islandName.GetLocalizedString());
                RuntimeManager.PlayOneShot(sound_Enter);
            }
            else
                regionEnterIgnore = false;

            if (spawnTransform != null)
                AreaControl.RecentLandRecord(islandID, spawnTransform.position);
        }
    }

    private bool EnterUIFilter()
    {
        if (FairwindChallengeInstance.IsActiveChallengeExists) return false;
        if (enterTimer > enterInterval) { Debug.Log("Island Enter : " + IslandName.GetLocalizedString()); enterTimer = 0f; return true; }
        else return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 1.0f);
        Gizmos.DrawWireSphere(transform.position, innerArea);
        Gizmos.color = new Color(1f, 1f, 0f, 1.0f);
        Gizmos.DrawWireSphere(transform.position, outerArea);
    }

    public void IslandDebug()
    {
        Debug.Log("Entered");
    }
}
