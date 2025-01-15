using Cinemachine;
using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Playables;
using UnityEngine.Timeline;

//===============================
//
// 시퀀스번들의 정보를 나타내는 스크립트입니다.
// 시퀀스 : 게임플레이 중 대사, 이벤트, 타임라인 같은 것들을 코루틴을 이용해 연속적으로 순서대로 재생할 수 있도록 합니다.
// Sequence_Base를 상속한 자식 클래스를 만들고 IEnumerator Sequence(SequenceInvoker invoker)를 오버라이드 하여 여러 유형의 시퀀스 내용들을 정의할 수 있습니다.
//
// 시퀀스번들의 정보는 SequenceBundleAsset 스크립터블오브젝트 파일을 생성하여 작성할 수 있습니다.
// 만들어진 시퀀스번들에셋은 SequenceInvoker 인스턴스를 통해 재생할 수 있습니다. 해당 스크립트를 참고하세요.
//
//===============================

[CreateAssetMenu(fileName = "NewSequenceData", menuName = "새 시퀀스 번들 에셋 추가", order = 1)]
public class SequenceBundleAsset : SerializedScriptableObject
{
    public Sequence_Base[] SequenceBundles;
}

public class Sequence_Base 
{
    public virtual IEnumerator Sequence(SequenceInvoker invoker) { yield return null; }
}

/// <summary>
/// 아무것도 하지 않고 time만큼 기다립니다.
/// </summary>
[System.Serializable]
public class Sequence_WaitForSeconds : Sequence_Base
{
    [InfoBox("아무것도 하지 않고 time만큼 기다립니다.",InfoMessageType = InfoMessageType.None)]
    public float time;      // 기다리는 시간
    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        yield return new WaitForSeconds(time);
    }
}

/// <summary>
/// 대사창을 열고 dialogues의 대사 데이터들을 순서대로 출력합니다.
/// </summary>
[System.Serializable]
public class Sequence_Dialogue : Sequence_Base
{
    [InfoBox("대사창을 열고 dialogues의 대사 데이터들을 순서대로 출력합니다.", InfoMessageType = InfoMessageType.None)]
    public DialogueData[] dialogues;                    // 대사 데이터들
    public bool CloseDialogueAfterFinish = true;        // true일 시 대사창이 모두 재생되면 대사창 UI를 닫습니다.

    public override IEnumerator Sequence(SequenceInvoker invoker) 
    {
        if (invoker.Dialogue == null)
        { Debug.Log("Dialogue UI 인스턴스가 없습니다!"); yield break; }
        yield return invoker.Dialogue.StartCoroutine(invoker.Dialogue.Cor_DialogueSequence(dialogues));
        if(CloseDialogueAfterFinish)
        {
            yield return invoker.Dialogue.StartCoroutine(invoker.Dialogue.Cor_CloseDialogue());
        }
    }
}

/// <summary>
/// 대사창이 닫히지 않은 상태라면 대사창을 닫습니다.
/// </summary>
[System.Serializable]
public class Sequence_CloseDialogue : Sequence_Base
{
    [InfoBox("대사창이 닫히지 않은 상태라면 대사창을 닫습니다.", InfoMessageType = InfoMessageType.None)]
    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        yield return invoker.Dialogue.StartCoroutine(invoker.Dialogue.Cor_CloseDialogue());
    }
}

/// <summary>
/// 대사창이 활성화 되어있는 도중 플레이어가 선택할 수 있는 창을 만듭니다.
/// </summary>
[System.Serializable]
public class Sequence_DialogueBranch : Sequence_Base
{
    [InfoBox("대사창이 활성화 되어있는 도중 플레이어가 선택할 수 있는 창을 만듭니다.", InfoMessageType = InfoMessageType.None)]
    public LocalizedString[] branchAnswers;                 // 선택할 수 있는 텍스트 (UI)
    public SequenceBundleAsset[] sequenceAssets;            // 선택지들에 대응되는 새롭게 시작할 시퀀스 에셋들

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        if (branchAnswers.Length != sequenceAssets.Length) { Debug.LogError("branchAnswers와 sequenceAssets의 개수는 같아야 합니다."); yield break; }

        int index = 0;
        yield return invoker.Dialogue.StartCoroutine(invoker.Dialogue.Cor_Branch(branchAnswers, (value) => { index = value; }));

        Debug.Log(sequenceAssets[index]);

        if (sequenceAssets[index] != null)
            yield return invoker.StartCoroutine(invoker.Cor_RecurciveSequenceChain(sequenceAssets[index].SequenceBundles));

    }
}

/// <summary>
/// 타임라인을 재생합니다.
/// </summary>
[System.Serializable]
public class Sequence_Timeline : Sequence_Base
{
    [InfoBox("타임라인을 재생합니다.", InfoMessageType = InfoMessageType.None)]
    public TimelineAsset timeline;      // 실행할 타임라인 에셋

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        PlayableDirector playable = invoker.Playable;
        playable.Play(timeline);
        yield return new WaitUntil(() => playable.state != PlayState.Playing);
    }
}

/// <summary>
/// Pause상태인 현재의 타임라인을 다시 재생합니다.
/// </summary>
[System.Serializable]
public class Sequence_ResumeTimeline : Sequence_Base
{
    [InfoBox("Pause상태인 현재의 타임라인을 다시 재생합니다.", InfoMessageType = InfoMessageType.None)]
    [HideLabel()]public string fakePorperty;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        PlayableDirector playable = invoker.Playable;
        playable.Resume();
        yield return new WaitUntil(() => playable.state != PlayState.Playing);
    }
}


/// <summary>
/// 조개를 amount 만큼 지급합니다.
/// </summary>
[System.Serializable]
public class Sequence_GainMoney : Sequence_Base
{
    [InfoBox("조개를 amount 만큼 지급합니다.", InfoMessageType = InfoMessageType.None)]
    public int amount;      // 획득할 조개 개수

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        if (invoker.InventoryContainer == null) { Debug.LogError("PlayerInvnentoryContainer를 찾을 수 없습니다."); yield break; }
        invoker.InventoryContainer.AddMoney(amount);
        yield return null;
    }
    
}

/// <summary>
/// 아이템을 인벤토리에 추가합니다.
/// </summary>
[System.Serializable]
public class Sequence_ObtainItem : Sequence_Base
{
    [InfoBox("아이템을 인벤토리에 추가합니다.", InfoMessageType = InfoMessageType.None)]
    public ItemData item;               // 획득할 아이템 데이터
    public int quantity = 1;            // 개수

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        invoker.InventoryContainer.AddItem(item, quantity);
        yield return invoker.StartCoroutine(invoker.InventoryContainer.Cor_ItemWindow(item, quantity));
    }
}

/// <summary>
/// 글로벌 값 목록에서 ID를 찾아 해당 값을 설정합니다. 
/// 새로운 글로벌 값 ID를 생성하고자 한다면, 
/// 프로젝트 파일에서 "Assets/ScriptableObjects/GlobalParamSettings"에 정보를 추가하고, 
/// 노션에서 "프로그래머문서/글로벌 값/값 리스트"에 해당 정보를 적어두고 사용하세요
/// </summary>
[System.Serializable]
public class Sequence_SetGlobalParameter : Sequence_Base
{
    [InfoBox("글로벌 값 목록에서 ID를 찾아 해당 값을 설정합니다.\r새로운 글로벌 값 ID를 생성하고자 한다면\r " +
        "프로젝트 파일에서 \"Assets/ScriptableObjects/GlobalParamSettings\"에 정보를 추가하고\r " +
        "노션에서 \"프로그래머문서/글로벌 값/값 리스트\"에 해당 정보를 적어두고 사용하세요", InfoMessageType = InfoMessageType.None)]
    public string paramKey;         // ID
    public int value;            // 지정할 값

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        if(!GlobalGameParameters.IsInstanceValid) { Debug.LogError("GlobalGameParameters가 없습니다!"); yield break; }

        GlobalGameParameters.Instance.Data[paramKey] = value;

        yield return null;
    }

}

/// <summary>
/// 대사창이 활성화 되어있는 도중 플레이어가 선택에 따라 각자 다른 글로벌 값을 설정해줄 수 있습니다.
/// </summary>
[System.Serializable]
public class Sequence_SelectGlobalParameter : Sequence_Base
{
    [InfoBox("대사창이 활성화 되어있는 도중 플레이어가 선택에 따라 각자 다른 글로벌 값을 설정해줄 수 있습니다.", InfoMessageType = InfoMessageType.None)]
    public string paramKey;                              // ID
    public LocalizedString[] branchAnswers;              // 선택할 수 있는 텍스트 (UI)
    public int[] values;                                 // 각 선택지들에 대응되는 값들

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        if (!GlobalGameParameters.IsInstanceValid) { Debug.LogError("글로벌 패러미터 오브젝트가 없습니다."); yield break; }

        if (string.IsNullOrEmpty(paramKey))
        {
            Debug.LogError("paramKey가 비어있습니다.");
            yield break;
        }

        if (branchAnswers.Length != values.Length) { Debug.LogError("branchAnswers와 values의 개수는 같아야 합니다."); yield break; }

        int index = 0;
        yield return invoker.Dialogue.StartCoroutine(invoker.Dialogue.Cor_Branch(branchAnswers, (value) => { index = value; }));

        int value = values[index];
        GlobalGameParameters.Instance.Data[paramKey] = value;

        yield return null;
    }
}


/// <summary>
/// 글로벌 값에서 ID를 가져와 값을 비교하여 특정 숫자 값일 때 해당하는 시퀀스 에셋을 재생합니다.
/// </summary>
[System.Serializable]
public class Sequence_BranchByParameter : Sequence_Base
{
    [InfoBox("글로벌 값에서 ID를 가져와 값을 비교하여 특정 숫자 값일 때 해당하는 시퀀스 에셋을 재생합니다.", InfoMessageType = InfoMessageType.None)]
    public string paramKey;                         // ID
    public int[] valueCases;                        // 값 리스트
    public SequenceBundleAsset[] sequences;         // 시퀀스 리스트
    public SequenceBundleAsset defaultSequence;     // 아무것도 만족하지 않을 때 시퀀스 ( 비워둘 수 있음 )

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        if (!GlobalGameParameters.IsInstanceValid) { Debug.LogError("글로벌 패러미터 오브젝트가 없습니다."); yield break; }
        if (sequences.Length != sequences.Length) { Debug.LogError("valueCases와 sequences의 개수가 같아야 합니다."); yield break; }
        if (string.IsNullOrEmpty(paramKey))
        {
            Debug.LogError("paramKey가 비어있습니다.");
            yield break;
        }

        int value = GlobalGameParameters.Instance.Data[paramKey];

        Debug.Log("Sequence_BranchByParameter / Global parameter : " + paramKey + " is " + value);

        for(int i = 0; i < valueCases.Length; i++)
        {
            if(value == valueCases[i])
            {
                if (sequences[i] == null) yield break;
                else
                {
                    yield return invoker.StartCoroutine(invoker.Cor_RecurciveSequenceChain(sequences[i].SequenceBundles));
                    yield break;
                }
            }
        }

        if (defaultSequence == null) yield break;
        yield return invoker.StartCoroutine(invoker.Cor_RecurciveSequenceChain(defaultSequence.SequenceBundles));
    }

}

public class Sequence_ShowImage : Sequence_Base
{
    [InfoBox("이미지묶음을 보여줍니다.", InfoMessageType = InfoMessageType.None)]
    [LabelText("이미지 모두 표시 후 닫기")] public bool closeImageAfterFinish = true;
    [PreviewField(Alignment = ObjectFieldAlignment.Center,Height = 100)] public Sprite[] Images;


    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        yield return invoker.DisplayImage.ImageProgress(Images,closeImageAfterFinish);
    }
}

public class Sequence_CloseImage : Sequence_Base
{
    [InfoBox("열려있는 이미지 창을 닫습니다.")]
    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        invoker.DisplayImage.CloseImage();
        yield return null;
    }
}

public class Sequence_Event : Sequence_Base
{
    [InfoBox("BindFromSequences에서 Key값에 해당하는 이벤트를 실행합니다.")]
    public string key;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        invoker.BindfromSequences.Invoke(key);
        yield return null;
    }
}

public class Sequence_EnableVCam : Sequence_Base
{
    [InfoBox("name 이름을 가진 카메라를 활성화합니다.")]
    public string name;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        CinemachineVirtualCameraBase[] vcams = GameObject.FindObjectsByType<CinemachineVirtualCameraBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (CinemachineVirtualCameraBase vcam in vcams)
        {
            if (vcam.name == name)
            {
                vcam.gameObject.SetActive(true);
                yield break;
            }    
        }

        Debug.Log(name + " 이름을 가진 Virtual Camera오브젝트를 찾지 못했습니다.");
        yield return null;
    }

}

public class Sequence_DisableVCam : Sequence_Base
{
    [InfoBox("name 이름을 가진 카메라를 끕니다.")]
    public string name;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        CinemachineVirtualCameraBase[] vcams = GameObject.FindObjectsByType<CinemachineVirtualCameraBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (CinemachineVirtualCameraBase vcam in vcams)
        {
            if (vcam.name == name)
            {
                vcam.gameObject.SetActive(false);
                yield break;
            }
        }

        Debug.Log(name + " 이름을 가진 Virtual Camera오브젝트를 찾지 못했습니다.");
        yield return null;
    }

}

public class Sequence_Animation : Sequence_Base
{
    [InfoBox("name 이름을 가진 오브젝트의 애니메이터에서 해당 state를 재생합니다.")]
    public string objectName;
    public string stateName;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        Animator[] anims = GameObject.FindObjectsByType<Animator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Animator anim in anims)
        {
            if (anim.name == objectName)
            {
                anim.Play(stateName);
                yield break;
            }
        }

        Debug.Log(objectName + " 이름을 가진 Animator 오브젝트를 찾지 못했습니다.");
        yield return null;
    }

}

public class Sequence_AnimationEscape : Sequence_Base
{
    [InfoBox("name 이름을 가진 오브젝트의 애니메이터에 \"Escape\" 트리거를 발동시킵니다. 일반적으로 시퀀스에서 재생한 루프 애니메이션을 탈출하는데 쓰입니다.")]
    public string objectName;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        Animator[] anims = GameObject.FindObjectsByType<Animator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Animator anim in anims)
        {
            if (anim.name == objectName)
            {
                anim.SetTrigger("Escape");
                yield break;
            }
        }

        Debug.Log(objectName + " 이름을 가진 Animator 오브젝트를 찾지 못했습니다.");
        yield return null;
    }
}

public class Sequence_PlaySound : Sequence_Base
{
    [InfoBox("사운드를 재생합니다.")]
    public EventReference sound;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        RuntimeManager.PlayOneShot(sound);
        yield return null;
    }

}

public class Sequence_DotweenAnimation : Sequence_Base
{
    [InfoBox("DoTween 애니메이션을 DoPlayAllByID을 통해 재생합니다.")]
    public string dotweenID;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        Tween[] tweens = DOTween.TweensById(dotweenID).ToArray();
        if (tweens.Length == 0 || tweens == null) yield break;

        foreach(Tween t in tweens)
        {
            t.Restart();
        }

        yield return tweens[0].WaitForCompletion();

    }
}

public class Sequence_IntroCanvas : Sequence_Base
{
    public LocalizedString[] texts;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        UI_IntroCanvas intro = UI_IntroCanvas.Instance;
        yield return intro.StartCoroutine(intro.Cor_PrintText(texts, 3.0f));
    }
}

public class Sequence_ToggleIsmael : Sequence_Base
{
    [InfoBox("플레이어로부터 이스마엘 파티클을 소환합니다.")]
    public bool value;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        if (value)
            PlayerCore.Instance.EnableIsamel();
        else
            PlayerCore.Instance.DisableIsmael();

        yield return new WaitForSeconds(0.5f);
    }
}

public class Sequence_PlayOtherSequence : Sequence_Base
{
    [InfoBox("다른 시퀀스 에셋 번들을 이어서 재생합니다.")]
    public SequenceBundleAsset sequenceBundle;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        yield return invoker.StartCoroutine(invoker.Cor_RecurciveSequenceChain(sequenceBundle.SequenceBundles));
    }
}

public class Sequence_StorylineProgress : Sequence_Base
{
    [InfoBox("스토리라인을 진행시킵니다 [스토리라인이름,번호]")]
    public string storyline;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        StorylineManager.Instance.MakeProgressStoryline(storyline);
        yield return null;
    }
}

public class Sequence_AZFLangDialogue : Sequence_Base
{
    [InfoBox("AZFLang폰트, 고대어를 출력하는 대사창을 띄웁니다. 영문을 입력하세요"),TextArea()]
    public string[] contexts;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        yield return UI_AZFLangDialogue.Instance.StartCoroutine(UI_AZFLangDialogue.Instance.Cor_Dialogue(contexts));
    }
}

public class Sequence_PlayMusic : Sequence_Base
{
    [InfoBox("새로운 배경음악을 재생합니다.")]
    public EventReference music;
    public float fadeTime;
    public float waitTime;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        FieldMusicManager.Instance.ChangeActiveMusic(music,fadeTime,waitTime);
        yield return null;
    }
}

public class Sequence_StopMusic : Sequence_Base
{
    [InfoBox("재생중인 배경음악을 정지합니다.")]
    public float fadeTime;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        FieldMusicManager.Instance.StopActiveMusic(fadeTime);
        yield return null;
    }

}

public class Sequence_ImageCutscene : Sequence_Base
{
    [LabelText("(선택) 배경음악")] public EventReference music;
    [LabelText("")] public ImgCutsceneSubsequence_Base[] subsequences;

    public class ImgCutsceneSubsequence_Base
    {
        [LabelText("암전효과")]public bool blackout;
        [LabelText("이미지")] public Sprite sprite;
    }

    public class ImgCutsceneSubsequence_Short:ImgCutsceneSubsequence_Base
    {
        //[LabelText("대기 시간")] public float waitTime;
        [LabelText("텍스트")] public LocalizedString[] context;
        //[LabelText("(선택)효과음")] public EventReference sound;
    }

    public class ImgCutsceneSubsequence_Long:ImgCutsceneSubsequence_Base
    {
        public class LongCutsceneElement
        {
            public float scrollPoint;
            public float scrollTime;
            public LocalizedString[] context;
        }

        public LongCutsceneElement[] elements;
    }

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        yield return null;
        yield return UI_ImageCutscene.Instance.StartCutsceneProgress(subsequences);
    }

}

public class Sequence_FixPlayerPosition : Sequence_Base
{
    [InfoBox("플레이어가 그자리에서 고정되어 물리가 비활성화됩니다. 고정 위치를 지정하면 해당 월드 좌표 위치에 플레이어가 이동되고 고정됩니다.")]
    //public string fixPlayerTo_Transform;
    [LabelText("플레이어 위치 설정 여부")] public bool hasVector;
    [LabelText("플레이어 위치 좌표"),ShowIf("hasVector")]public Vector3 fixPlayerTo_Absolute;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        if (invoker.isPlayerFixedBySequence) yield break;

        invoker.isPlayerFixedBySequence = true;
        if(hasVector)
        {
            PlayerCore.Instance.transform.position = fixPlayerTo_Absolute;
        }

        if(PlayerCore.IsInstanceValid)
        {
            PlayerCore.Instance.Rigidbody.isKinematic = true;
        }
        yield return null;
    }
}

public class Sequence_UnfixPlayerPosition : Sequence_Base
{
    [InfoBox("FixPlayerPosition에 의해 고정된 플레이어를 해제하고 물리를 다시 활성화합니다.")]
    [HideLabel()]public string fakeData;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        invoker.isPlayerFixedBySequence = false;

        if(PlayerCore.IsInstanceValid)
        {
            PlayerCore.Instance.Rigidbody.isKinematic = false;
        }

        yield return null;
    }
}

public class Sequence_Blackout : Sequence_Base
{
    public enum FadeMode
    {
        FadeOut,
        FadeIn
    }

    [InfoBox("페이드 아웃/페이드 인을 합니다.")]
    [LabelText("모드")] public FadeMode fademode;
    [LabelText("시간")] public float duration = 1.0f;
    [LabelText("페이드 까지 기다림")] public bool waitWhileFade;
    [LabelText("(선택) 애니메이션 커브")] public AnimationCurve curve;

    public override IEnumerator Sequence(SequenceInvoker invoker)
    {
        UI_Blackout.Instance.StopAllCoroutines();

        if (waitWhileFade)
        {
            if (fademode == FadeMode.FadeOut)
            {
                if (curve.IsUnityNull())
                    yield return UI_Blackout.Instance.StartCoroutine(UI_Blackout.Instance.Cor_FadeOut(duration));
                else
                    yield return UI_Blackout.Instance.StartCoroutine(UI_Blackout.Instance.Cor_FadeOut(duration, curve));
            }
            else if (fademode == FadeMode.FadeIn)
            {
                if (curve.IsUnityNull())
                    yield return UI_Blackout.Instance.StartCoroutine(UI_Blackout.Instance.Cor_FadeIn(duration));
                else
                    yield return UI_Blackout.Instance.StartCoroutine(UI_Blackout.Instance.Cor_FadeIn(duration, curve));
            }
        }
        else
        {
            if (fademode == FadeMode.FadeOut)
            {
                if (curve.IsUnityNull())
                    UI_Blackout.Instance.FadeOut(duration);
                else
                    UI_Blackout.Instance.FadeOut(duration,curve);
            }
            else if (fademode == FadeMode.FadeIn)
            {
                if (curve.IsUnityNull())
                    UI_Blackout.Instance.FadeIn(duration);
                else
                    UI_Blackout.Instance.FadeIn(duration, curve);
            }
        }
    }
}
