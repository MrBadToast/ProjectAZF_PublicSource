using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class FairwindChallengeInstance : MonoBehaviour
{
    //================================================
    //
    // 순풍의 도전 단일 개체에 대한 스크립트입니다.
    //
    //================================================

    static private FairwindChallengeInstance activeChallenge;
    static public FairwindChallengeInstance ActiveChallenge { get { return activeChallenge; } }
    static public bool IsActiveChallengeExists { get { return activeChallenge != null; } }

    [InfoBox("시작점은 붉은색, 경유지는 보라색, 도착점은 초록색으로 표시됩니다.\n 노란색은 플레이어가 순풍의 도전을 진행하면서 유지해야될 거리를 나타냅니다. \n 경로를 편집하고싶다면, Route의 스플라인을 편집하세요.")]
    [SerializeField] private string iD;
    public string ID { get { return iD; } }
    [SerializeField, LabelText("제한 시간 (초)")] private float timelimit = 0;
    [SerializeField, LabelText("경유지 추가 시간 활성화")] private bool checkpointBonus = false;
    [SerializeField, ShowIf("checkpointBonus"), LabelText("경유지 추가 시간")] private float checkpointBonusTime = 10f;
    public float Timelimit { get { return timelimit; } }
    [SerializeField, LabelText("보상 아이템 (선택사항)")] private ItemData[] rewardItems;
    [SerializeField, LabelText("완료시 시퀀스 (선택사항)")] private SequenceBundleAsset sequenceOnFinish;
    [SerializeField, LabelText("음악 (선택사항)")] private EventReference musicOnPlay;


    [InfoBox("절대 이벤트에 순풍의 도전 외부에 있는 오브젝트를 참조하지 마세요!", InfoMessageType = InfoMessageType.Info)]
    [SerializeField, LabelText("도전 시작시 이벤트"), FoldoutGroup("이벤트")] private UnityEvent OnChallengeStart;
    [SerializeField, LabelText("도전 종료시 이벤트"), FoldoutGroup("이벤트")] private UnityEvent OnChallengeEnd;

    [SerializeField, Required, FoldoutGroup("사운드")]
    private EventReference sound_Checkpoint;
    [SerializeField, Required, FoldoutGroup("사운드")]
    private EventReference sound_Finish;
    [SerializeField, Required, FoldoutGroup("사운드")]
    private EventReference sound_Failed;
    [SerializeField, Required, FoldoutGroup("사운드")]
    private EventReference sound_Start;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private GameObject lightPilarObject;
    [SerializeField, Required, FoldoutGroup("ChildReferences")] 
    private SplineContainer route;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private SplineExtrude extrude;

    private float triggerDistance = 5;
    private float distanceAllowence = 10;
    private float distanceAllowenceTime = 5;
    private bool musicEnabled = false;

    /// <summary>
    /// 경로의 스플라인 데이터를 가져옵니다.
    /// </summary>
    public Spline RouteSpline { get { return route.Spline; } }

    enum ChallengeState
    {
        Standby,
        Active,
        Aborted,
        Closed
    }
    private ChallengeState currentState = ChallengeState.Standby;
    [SerializeField,ReadOnly] private int activeKnotIndex;

    // knots Info
    private Vector3[] routeKnotList;
    private Vector3 startKnotPosition;
    private Vector3 endKnotPosition;

    // challenge Info
    private float timer_playCountdown = -1f;
    private float timer_routeCountdown = 0f;

    private SplineSlice<Spline> activeSplineSegment;
    private Vector3 nearestFromPlayer;

    private bool isChallengeDone = false;

    /// <summary>
    /// 해당 순풍의 도전의 스플라인의 연결지점들을 가져옵니다.
    /// </summary>
    /// <param name="positionList"> 할당할 리스트 </param>
    /// <param name="WorldPosition"> true = 월드좌표계, false = 로컬좌표계 </param>
    public void GetRoutePositions(out Vector3[] positionList, bool WorldPosition = true)
    {
        var bezierKnots = route.Spline.Knots.ToArray();
        positionList = new Vector3[bezierKnots.Length];

        for (int i = 0; i < bezierKnots.Length; i++)
        {
            if (WorldPosition)
                positionList[i] = transform.localToWorldMatrix.MultiplyPoint3x4(AZFUtilities.F3ToVec3(bezierKnots[i].Position));
            else
                positionList[i] = AZFUtilities.F3ToVec3(bezierKnots[i].Position);
        }
    }

    Coroutine FairwindProgress;

    /// <summary>
    /// 이 순풍의 도전을 강제로 중지합니다.
    /// </summary>
    public void AbortChallenge()
    {
        FairwindProgress = null;
        extrude.gameObject.SetActive(false);
        PlayerCore.Instance.DisableIndicator();
        lightPilarObject.transform.position = new Vector3(startKnotPosition.x, lightPilarObject.transform.position.y, startKnotPosition.z);
        StopAllCoroutines();
        currentState = ChallengeState.Aborted;
        FieldMusicManager.Instance.StopActiveMusic(3f);
        activeKnotIndex = 0;
        if (OnChallengeEnd != null)
            OnChallengeEnd.Invoke();
    }

    /// <summary>
    /// (static) 진행중인 순풍의 도전을 강제로 중지합니다.
    /// </summary>
    public static void AbortActiveChllenge()
    {
        if (IsActiveChallengeExists) return;

        activeChallenge.AbortChallenge();
        activeChallenge = null;
    }

    /// <summary>
    /// (static) 진행중인 순풍의 도전에 시간을 추가합니다;
    /// </summary>
    /// <param name="time"> 시간 </param>
    /// <returns>true : 진행중인 순풍의 도전이 있어 시간을 추가하는데 성공하였습니다. false : 진행중인 순풍의 도전이 없어 시간을 추가할 수 없었습니다.</returns>
    public static bool AddTimerToActiveChallenge(float time)
    {
        if (!IsActiveChallengeExists) return false;

        UI_FairwindInfo.Instance.OnAdditionalTime(time);

        Debug.Log(ActiveChallenge.name);

        ActiveChallenge.timer_playCountdown += time;
        return true;

    }

    /// <summary>
    /// 현재 활성화된 순풍에 도전의 경유지들 위치에 대한 정보를 받아옵니다.
    /// </summary>
    /// <returns></returns>
    public static Vector3[] GetActiveRoutesKnots()
    {
        if (IsActiveChallengeExists) { Debug.LogWarning("FairwindChallengeInstance : 현재 진행중인 순풍의 도전이 없습니다!"); return null; }
        return ActiveChallenge.routeKnotList;
    }

    /// <summary>
    /// 경로 Spline으로부터 거리가 얼마나 떨어져 있는지와 그와 관련된 정보를 가져옵니다.
    /// </summary>
    /// <param name="spline"></param>
    /// <param name="point"> 계산 지점 </param>
    /// <param name="pointOnSpline"> Spline으로부터 최단지점에 위치한 지점 </param>
    /// <param name="t"> 0-1 값으로 보간된 지점 </param>
    /// <returns></returns>
    public float GetDistanceFromSpline(Spline spline,Vector3 point,out Vector3 pointOnSpline,out float t)
    {
        float3 p;
        point = transform.worldToLocalMatrix.MultiplyPoint3x4(point);
        float distance = SplineUtility.GetNearestPoint(spline, new float3(point.x, point.y, point.z), out p, out t);
        pointOnSpline = new Vector3(p.x, p.y, p.z);
        pointOnSpline = transform.localToWorldMatrix.MultiplyPoint3x4(pointOnSpline);

        return distance;
    }

    /// <summary>
    /// 경로 Spline으로부터 거리가 얼마나 떨어져 있는지와 그와 관련된 정보를 가져옵니다.
    /// </summary>
    /// <param name="spline"></param>
    /// <param name="point"> 계산 지점 </param>
    /// <param name="pointOnSpline"> Spline으로부터 최단지점에 위치한 지점 </param>
    /// <param name="t"> 0-1 값으로 보간된 지점 </param>
    /// <returns></returns>
    public float GetDistanceFromSpline(SplineSlice<Spline> spline, Vector3 point, out Vector3 pointOnSpline, out float t,int resolutions = 2, int iterations = 1)
    {
        float3 p;
        point = transform.worldToLocalMatrix.MultiplyPoint3x4(point);
        float distance = SplineUtility.GetNearestPoint(spline, new float3(point.x, point.y, point.z), out p, out t,2,1);
        pointOnSpline = new Vector3(p.x, p.y, p.z);
        pointOnSpline = transform.localToWorldMatrix.MultiplyPoint3x4(pointOnSpline);

        return distance;
    }


    private void OnChallengeActivated()
    {
        if (FairwindProgress != null) return;
        timer_routeCountdown = distanceAllowenceTime;

        timer_playCountdown = timelimit;
        UI_FairwindInfo.Instance.ToggleFairwindUI(true);
        extrude.gameObject.SetActive(true);
        isChallengeDone = false;

        if(!musicOnPlay.IsNull)
        {
            musicEnabled = true;
            FieldMusicManager.Instance.ChangeActiveMusic(musicOnPlay, 3f, 1f);
        }

        FairwindProgress = StartCoroutine(Cor_FairwindMainProgress());

        if (OnChallengeStart != null)
            OnChallengeStart.Invoke();
    }

    IEnumerator Cor_FairwindMainProgress()
    {
        FMODUnity.RuntimeManager.PlayOneShot(sound_Start);
        for (int i = 0; i < routeKnotList.Length - 1; i++)
        {
            activeKnotIndex++;

            float prevF = RouteSpline.ConvertIndexUnit(activeKnotIndex - 1, PathIndexUnit.Knot, PathIndexUnit.Normalized);
            float nextF = RouteSpline.ConvertIndexUnit(activeKnotIndex, PathIndexUnit.Knot, PathIndexUnit.Normalized);

            activeSplineSegment = new SplineSlice<Spline>(RouteSpline, new SplineRange(i,2,SliceDirection.Forward));

            yield return StartCoroutine(Cor_ChangeDestination(prevF, nextF));
            yield return new WaitUntil(() => (GetProjectedDistanceFromPlayer(routeKnotList[activeKnotIndex]) < triggerDistance));

            if (checkpointBonus) AddTimerToActiveChallenge(checkpointBonusTime);

            FMODUnity.RuntimeManager.PlayOneShot(sound_Checkpoint);
        }
        FMODUnity.RuntimeManager.PlayOneShot(sound_Finish);
        lightPilarObject.SetActive(false);
        route.GetComponent<MeshRenderer>().enabled = false;

        yield return new WaitForSeconds(1f);

        if (SequenceInvoker.IsInstanceValid)
        {
            if (rewardItems.Length > 0)
            {
                for (int i = 0; i < rewardItems.Length; i++)
                {
                    var itemSequence = new Sequence_ObtainItem();
                    itemSequence.item = rewardItems[i];
                    itemSequence.quantity = 1;

                    SequenceInvoker.Instance.StartSequence(itemSequence);
                }

            }

            if (sequenceOnFinish != null)
            {
                SequenceInvoker.Instance.StartSequence(sequenceOnFinish.SequenceBundles);
            }
        }

        isChallengeDone = true;
        PlayerCore.Instance.DisableIndicator();
        UI_FairwindInfo.Instance.OnFairwindSuccessed();

        FieldMusicManager.Instance.StopActiveMusic(3f);

        AlphaEndingPanel alphaEnding = FindObjectOfType<AlphaEndingPanel>(true);
        if (alphaEnding != null) alphaEnding.OnClearedFairwind();

        if (OnChallengeEnd != null)
            OnChallengeEnd.Invoke();
        currentState = ChallengeState.Closed;
        activeChallenge = null;
    }

    readonly float destinationAnimationTime = 1f;

    IEnumerator Cor_ChangeDestination(float prevF, float nextF)
    {
        lightPilarObject.SetActive(false);
        extrude.gameObject.SetActive(true);
        for (float t = 0; t < destinationAnimationTime; t += Time.fixedDeltaTime)
        {
            extrude.Range = new Vector2(prevF, Mathf.Lerp(prevF, nextF, t / destinationAnimationTime));
            extrude.Rebuild();
            yield return new WaitForFixedUpdate();
        }
        lightPilarObject.SetActive(true);
        var knot = routeKnotList[activeKnotIndex];
        lightPilarObject.transform.position = new Vector3(knot.x, lightPilarObject.transform.position.y, knot.z);
        yield return null;
    }

    private void Awake()
    {
        if (rewardItems == null) rewardItems = new ItemData[0];
    }

    private void Start()
    {
        if(route != null)
        {
            GetRoutePositions(out routeKnotList);
            startKnotPosition = routeKnotList[0];
            if(routeKnotList.Length > 1)
            {
                endKnotPosition = routeKnotList[routeKnotList.Length-1];
            }
        }

        lightPilarObject.SetActive(true);
        var knot = routeKnotList[activeKnotIndex];
        lightPilarObject.transform.position = new Vector3(knot.x, lightPilarObject.transform.position.y, knot.z);
    }

    float abortTimer = 0f;

    private void Update()
    {
        if (currentState == ChallengeState.Closed) return;
        else if (currentState == ChallengeState.Active)
        {
            if (!isChallengeDone)
                timer_playCountdown -= Time.deltaTime;

            UI_FairwindInfo.Instance.SetFairwindCountdown(timer_playCountdown);

            if (timer_playCountdown <= 0f)
            {

                UI_FairwindInfo.Instance.OnFairwindTimeoutFailed();
                FMODUnity.RuntimeManager.PlayOneShot(sound_Failed);
                AbortChallenge();
                return;
            }

            float t = 0;
            if (GetDistanceFromSpline(activeSplineSegment, PlayerCore.Instance.transform.position, out nearestFromPlayer, out t) > distanceAllowence)
            {
                UI_FairwindInfo.Instance.ToggleAlertUI(true);
                UI_FairwindInfo.Instance.SetAlertCountdown(timer_routeCountdown);
                timer_routeCountdown -= Time.deltaTime;
                PlayerCore.Instance.EnableAndSetIndicator(nearestFromPlayer);

                if (timer_routeCountdown <= 0f)
                {
                    UI_FairwindInfo.Instance.OnFairwindRouteoutFailed();
                    FMODUnity.RuntimeManager.PlayOneShot(sound_Failed);
                    AbortChallenge();
                    return;
                }
            }
            else
            {
                timer_routeCountdown = distanceAllowenceTime;
                UI_FairwindInfo.Instance.ToggleAlertUI(false);
                PlayerCore.Instance.DisableIndicator();
            }
        }
        else if (currentState == ChallengeState.Standby)
        {
            if (GetProjectedDistanceFromPlayer(startKnotPosition) < triggerDistance)
            {
                currentState = ChallengeState.Active;
                activeChallenge = this;
                OnChallengeActivated();
            }
        }
        else if (currentState == ChallengeState.Aborted)
        {
            abortTimer += Time.deltaTime;

            if (abortTimer > 4.0f)
            {
                abortTimer = 0f;
                currentState = ChallengeState.Standby;
            }

        }
    }

    private float GetProjectedDistanceFromPlayer(Vector3 target)
    {
        if (!PlayerCore.IsInstanceValid) { Debug.LogError("플레이어 코어 없음."); return float.NaN; }
        Vector2 projectedPlayerPositon = new Vector2(PlayerCore.Instance.transform.position.x, PlayerCore.Instance.transform.position.z);
        return Vector2.Distance(projectedPlayerPositon, new Vector2(target.x, target.z));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (route != null)
        {

            if(currentState == ChallengeState.Active)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(nearestFromPlayer, PlayerCore.Instance.transform.position);
                Vector3 o;
                float t;
                UnityEditor.Handles.Label(PlayerCore.Instance.transform.position + Vector3.down * 2f, ((int)GetDistanceFromSpline(activeSplineSegment,PlayerCore.Instance.transform.position, out o, out t)).ToString()+ " M"); ;
            }

            GetRoutePositions(out routeKnotList);
            startKnotPosition = routeKnotList[0];
            if (routeKnotList.Length > 1)
            {
                endKnotPosition = routeKnotList[routeKnotList.Length - 1];
            }

            var knots = route.Spline.Knots.ToArray();
            int knotCount = route.Spline.Knots.Count();

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(startKnotPosition, triggerDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startKnotPosition, distanceAllowence);

            if (knotCount > 2)
            {
                for (int i = 1; i < knotCount - 1; i++)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(transform.localToWorldMatrix.MultiplyPoint3x4(AZFUtilities.F3ToVec3(knots[i].Position)), triggerDistance);
                }
            }

            if (knotCount > 1)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(endKnotPosition, triggerDistance);
            }
        }
    }
#endif
}
