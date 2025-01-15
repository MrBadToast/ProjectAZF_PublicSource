using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class StorylineManager : StaticSerializedMonoBehaviour<StorylineManager>
{
    //================================================
    //
    // 스토리라인을 실행시키고 실행중인 스토리라인을 제어하는 스크립트입니다.
    // 
    //================================================

    [SerializeField,Required] private StorylineStash storylineStashAsset;
    [SerializeField] private EventReference sound_questUpdate;
    private StorylineStash storylineStashInstance;
    private string activeStorylineKey;
    private StorylineData activeStoryline;
    /// <summary>
    /// 현재 활성화된 StorylineData를 가져옵니다.
    /// </summary>
    public StorylineData ActiveStoryline { get { return activeStoryline; } }

    [SerializeField, FoldoutGroup("Debug")] private bool InvokeDefaultOnStart = false;
    [SerializeField, FoldoutGroup("Debug")] private string defaultStorylineID = "EXAMPLE";
    [SerializeField, ReadOnly, FoldoutGroup("Debug")] private string debug_active_storyline;
    [SerializeField, ReadOnly ,FoldoutGroup("Debug")] private int debug_objective_index = 0;

    int currentIndex = 0;
    /// <summary>
    /// 현재 활성화된 스토리라인의 인덱스를 가져옵니다.
    /// </summary>
    public int CurrentIndex { get { return currentIndex; } }

    protected override void Awake()
    {
        base.Awake();
        storylineStashInstance = Instantiate(storylineStashAsset);
    }

    private void Start()
    {
        if (InvokeDefaultOnStart) StartCoroutine(StartNewStroyline(defaultStorylineID));
    }

    private void Update()
    {
#if UNITY_EDITOR
        debug_active_storyline = (activeStoryline != null) ? activeStorylineKey : "null" ;
        debug_objective_index = currentIndex;
#endif
    }

    bool progress = false;

    /// <summary>
    /// 실행중인 Storyline이 없으면 새로운 Storyline을 실행합니다.
    /// </summary>
    /// <param name="stroylineID"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    /// 

    public void StartNewStoryline(string stroylineID)
    {
        if (activeStoryline != null) { Debug.Log("StroylineManager : 이미 실행중인 Stroyline이 있습니다. "); return; }

        StartCoroutine(StartNewStroyline(stroylineID, 0));
        return;
    }

    public bool StartNewStoryline(string stroylineID, int index = 0)
    {
        if (activeStoryline != null) { Debug.Log("StroylineManager : 이미 실행중인 Stroyline이 있습니다. "); return false; }

        StartCoroutine(StartNewStroyline(stroylineID, index));
        return true;
    }

    /// <summary>
    /// 키, 번호 정보를 입력하여 진행시킬 스토리와 일치하면 스토리라인을 진행시킵니다.
    /// </summary>
    /// <param name="KeyIndexPair"> [키],[번호] 형식의 문자열 </param>
    public void MakeProgressStoryline(string KeyIndexPair)
    {
        string[] parsed = KeyIndexPair.Split(",");
        Debug.Log(" 스토리라인 진행됨 : "+ parsed[0] + " / " + parsed[1]);
        int index = 0;

        if(parsed.Length == 2) 
        {
            if (!int.TryParse(parsed[1], out index))
                Debug.LogError("MakeProgressStoryline : 값을 잘못 입력하였습니다. [키],[번호] 형식으로 입력하세요");
        }
        else
        {
            Debug.LogError("MakeProgressStoryline : 값을 잘못 입력하였습니다. [키],[번호] 형식으로 입력하세요");
        }

        if (storylineStashInstance.packedStoryline.ContainsKey(parsed[0]))
        {
            if (currentIndex == index)
                progress = true;
            else if(currentIndex < index)
            {
                progress = true;
                currentIndex = index;
            }
            else
                Debug.Log("MakeProgressStoryline : 현재 Storyline의 키,번호 값과 입력한 키,번호 값이 다릅니다. 입력 :" + KeyIndexPair +" / 현재 :" +activeStorylineKey+"," + currentIndex);
        }
        else
        {
            Debug.LogError("MakeProgressStoryline : StorylineStash 에서 Key " + parsed[0] + " 를 찾을 수 없었습니다.");
        }
    }

    /// <summary>
    /// 현재 활성화된 스토리라인을 무조건 진행시킵니다. (사용 권장하지 않음)
    /// </summary>
    public void MakeProgressStoryline()
    {
        progress = true;
    }

    /// <summary>
    /// 현재 활성화된 스토리라인을 중지시킵니다.
    /// </summary>
    /// <param name=""></param>
    public void AbortActiveStoryline()
    {
        if (activeStoryline == null) return;

        StopAllCoroutines();
        progress = false;
        activeStoryline = null;
        UI_Objective.Instance.CloseObjective();
        UI_Marker.Instance.DisableMarker();
    }

    private IEnumerator StartNewStroyline(string storylineKey, int index = 0)
    {
        if (!storylineStashInstance.packedStoryline.ContainsKey(storylineKey))
        {
            Debug.LogError("StroylineManager : StorylineStash 에서 StroylineKey " + storylineKey + " 를 찾을 수 없었습니다.");
            yield break;
        }

        if (index > storylineStashInstance.packedStoryline[storylineKey].Objectives.Length) { Debug.LogWarning("StroylineManager : 시작하려는 Storyline의 인덱스를 초과하였습니다. index : " + index); yield break; }

        activeStorylineKey = storylineKey;
        activeStoryline = storylineStashInstance.packedStoryline[storylineKey];

        currentIndex = index;

        var sequence = SequenceInvoker.Instance;
        for (; currentIndex < activeStoryline.Objectives.Length; currentIndex++)
        {
            if (activeStoryline.Objectives[currentIndex].sequenceOnStart != null)
            {
                if (sequence.IsSequenceRunning) SequenceInvoker.Instance.ForceAbortAllSequences();

                PlayerCore.Instance.DisableControls(); UI_PlaymenuBehavior.Instance.DisableInput();
                yield return SequenceInvoker.Instance.Cor_RecurciveSequenceChain(activeStoryline.Objectives[currentIndex].sequenceOnStart.SequenceBundles);
                PlayerCore.Instance.EnableControls(); UI_PlaymenuBehavior.Instance.EnableInput();
            }

            UI_Objective.Instance.OpenObjective(activeStoryline.QuestNameText.GetLocalizedString(), activeStoryline.Objectives[currentIndex].objectiveText.GetLocalizedString());

            Transform dest;
            if (!activeStoryline.Objectives[currentIndex].destinationTransformName.IsNullOrWhitespace())
            {
                GameObject destObject = GameObject.Find(activeStoryline.Objectives[currentIndex].destinationTransformName);
                if (destObject != null) dest = destObject.transform;
                else { dest = null; }

                if (dest != null)
                    UI_Marker.Instance.SetMarker(dest);
            }
            else
            {
                UI_Marker.Instance.DisableMarker();
            }


            RuntimeManager.PlayOneShot(sound_questUpdate);

            yield return new WaitUntil(() => progress == true);
            progress = false;

            if (activeStoryline.Objectives[currentIndex].sequenceOnFinished != null)
            {
                if (sequence.IsSequenceRunning) SequenceInvoker.Instance.ForceAbortAllSequences();

                PlayerCore.Instance.DisableControls(); UI_PlaymenuBehavior.Instance.DisableInput();
                yield return sequence.Cor_RecurciveSequenceChain(activeStoryline.Objectives[currentIndex].sequenceOnFinished.SequenceBundles);
                PlayerCore.Instance.EnableControls(); UI_PlaymenuBehavior.Instance.EnableInput();
            }

        }

        activeStoryline = null;
        UI_Objective.Instance.CloseObjective();
        UI_Marker.Instance.DisableMarker();
    }
}
