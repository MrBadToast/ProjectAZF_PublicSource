using Cinemachine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

[RequireComponent(typeof(PlayableDirector))]
public class SequenceInvoker : StaticSerializedMonoBehaviour<SequenceInvoker>
{
    //===============================
    //
    // [싱글턴 오브젝트]
    // 시퀀스 리스트를 재생할 수 있는 스크립트 입니다.
    // StartSequence를 통해 매개변수로 시퀀스 리스트를 넣어 해당 시퀀스 묶음을 순차적으로 재생합니다. 
    // 작동중인 시퀀스가 있다면, 새로 재생을 시도한 시퀀스가 무시됩니다.
    //
    //===============================
    private UI_DialogueBehavior dialogue;
    public UI_DialogueBehavior Dialogue { get { return dialogue; } }
    private UI_DisplayImage displayImage;
    public UI_DisplayImage DisplayImage { get { return displayImage; } }
    
    private PlayerInventoryContainer inventoryContainer;
    public PlayerInventoryContainer InventoryContainer { get { return inventoryContainer; } }
    private PlayableDirector playable;
    public PlayableDirector Playable { get { return playable; } }
    private BindFromSequences bindFromSequences;
    public BindFromSequences BindfromSequences { get { return bindFromSequences; } }

    [SerializeField,ReadOnly()] private Queue<Sequence_Base> sequenceQueue;
    private CinemachineVirtualCameraBase sequenceVirtualCamera;

    public bool isPlayerFixedBySequence = false;

    protected override void Awake()
    {
        base.Awake();

        dialogue = UI_DialogueBehavior.Instance;
        inventoryContainer = PlayerInventoryContainer.Instance;
        displayImage = UI_DisplayImage.Instance;
        bindFromSequences = BindFromSequences.Instance;

        playable = GetComponent<PlayableDirector>();
        sequenceQueue = new Queue<Sequence_Base>();
        //SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private bool sequenceRunning = false;
    public bool IsSequenceRunning { get { return sequenceRunning; } }

    public void StartSequence(Sequence_Base sequence)
    {
        if (sequenceRunning)
        {
            Debug.Log("작동중인 시퀀스가 있습니다. 하나의 시퀀스 큐에 추가됩니다.");
                sequenceQueue.Enqueue(sequence);
            
            Debug.Log("현재 시퀀스 큐 : " + Debug_GetQueuedSequencesInfo());
            return;
        }


        sequenceQueue.Enqueue(sequence);

        Debug.Log("현재 시퀀스 큐 : " + Debug_GetQueuedSequencesInfo());

        StartCoroutine(Cor_StartSequenceQueue());
    }

    public void StartSequence(SequenceBundleAsset sequenceBundleAsset)
    {
        StartSequence(sequenceBundleAsset.SequenceBundles);
    }
    public void StartSequence(Sequence_Base[] sequenceChain)
    {
        if (sequenceRunning) {
            Debug.Log("작동중인 시퀀스가 있습니다. " + (sequenceChain.Length + 1) + "개의 시퀀스가 시퀀스 큐에 추가됩니다.");
            foreach (var seq in sequenceChain)
            {
                sequenceQueue.Enqueue(seq);
            }
            Debug.Log("현재 시퀀스 큐 : " + Debug_GetQueuedSequencesInfo());
            return;
        }

        foreach (var seq in sequenceChain)
        {
            sequenceQueue.Enqueue(seq);
        }

        Debug.Log("현재 시퀀스 큐 : " + Debug_GetQueuedSequencesInfo());

        StartCoroutine(Cor_StartSequenceQueue());
    }

    public void ForceAbortAllSequences()
    {
        Debug.Log("진행중인 시퀀스가 종료되었습니다. " + Debug_GetQueuedSequencesInfo());

        StopAllCoroutines();

        sequenceRunning = false;
        sequenceQueue.Clear();

        if (dialogue.DialogueOpened) { dialogue.StopAllCoroutines(); dialogue.StartCoroutine(dialogue.Cor_CloseDialogue()); }

        UI_PlaymenuBehavior.Instance.EnableInput();
        PlayerCore.Instance.EnableControls();
    }

    public void SetSequenceCamera(CinemachineVirtualCameraBase cam)
    {
        sequenceVirtualCamera = cam;
        sequenceVirtualCamera.gameObject.SetActive(true);
    }

    public void EndSequenceCamera()
    {
        if (sequenceVirtualCamera == null) return;

        sequenceVirtualCamera.gameObject.SetActive(false);
        sequenceVirtualCamera = null;
    }

    private IEnumerator Cor_StartSequenceQueue()
    {
        sequenceRunning = true;
        UI_PlaymenuBehavior playmenu = UI_PlaymenuBehavior.Instance;
        playmenu.DisableInput();
        PlayerCore player = PlayerCore.Instance;
        player.DisableControls();

        while (sequenceQueue.Count > 0)
        {
            yield return StartCoroutine(sequenceQueue.Dequeue().Sequence(this));
        }

        if (dialogue.DialogueOpened) { yield return dialogue.StartCoroutine(dialogue.Cor_CloseDialogue()); }

        yield return null;

        if (isPlayerFixedBySequence) player.Rigidbody.isKinematic = false;
        EndSequenceCamera();
        playmenu.EnableInput();
        player.EnableControls();

        sequenceRunning = false;
    }

    public IEnumerator Cor_RecurciveSequenceChain(Sequence_Base[] sequenceChain)
    {
        for (int i = 0; i < sequenceChain.Length; i++)
        {
            yield return StartCoroutine(sequenceChain[i].Sequence(this));
        }
    }

    public IEnumerator Cor_RecurciveSequenceChain(Sequence_Base sequence)
    {
        yield return StartCoroutine(sequence.Sequence(this));
    }

    public static string Debug_GetQueuedSequencesInfo(string splitRule = ",")
    {
        if (!SequenceInvoker.IsInstanceValid) return "No Valid Sequence Invoker";
        if (SequenceInvoker.Instance.sequenceQueue.Count == 0) return "SequenceQueue Is Empty";

        List<string> queuedSequencesInfo = new List<string>();
        foreach(var q in SequenceInvoker.Instance.sequenceQueue)
        {
            queuedSequencesInfo.Add(q.GetType().Name);
        }

        string r = queuedSequencesInfo[0];

        for(int i = 1; i < queuedSequencesInfo.Count; i++)
        {
            r += splitRule;
            r += queuedSequencesInfo[i];
        }

        return r;
    }


}
