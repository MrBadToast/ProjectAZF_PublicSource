using DG.Tweening;
using FMODUnity;
using RichTextSubstringHelper;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

/// <summary>
/// 대사창 데이터
/// </summary>
[System.Serializable]
public class DialogueData
{
    public LocalizedString speecher;        // 대사창에서 말하는 사람 이름
    public LocalizedString context;         // 대사창에서 내용
}

public class UI_DialogueBehavior : StaticSerializedMonoBehaviour<UI_DialogueBehavior>
{
    //============================================
    //
    // [싱글턴 오브젝트]
    // 대사창 UI 표시를 관리하는 클래스입니다.
    // 코루틴 부분은 Sequence 에서만 사용하세요!
    // 
    //============================================

    [SerializeField] private float textInterval = 0.05f;            // 텍스트 출력 시간 간격
    [SerializeField] private float answerSpawnSpace = 50f;          // 선택지 버튼 스폰 간격

    [Title("Sounds")]
    [SerializeField] private EventReference sound_open;             // 소리 : 대사창 오픈
    [SerializeField] private EventReference sound_proceed;          // 소리 : 대사창 진행
    [SerializeField] private EventReference sound_type;             // 소리 : 대사창 텍스트 타이포
    [SerializeField] private EventReference sound_select;           // 소리 : 선택지 선택됨
    [SerializeField] private EventReference sound_close;            // 소리 : 창 닫김

    [Title("References")]
    [SerializeField] private GameObject answerSinglePrefab;
    [SerializeField] private GameObject visualGroup;
    [SerializeField] private TextMeshProUGUI speecher;
    [SerializeField] private TextMeshProUGUI context;
    [SerializeField] private GameObject inputWaitObject;
    [SerializeField] private Transform answerStartPosition;
    [SerializeField] private DOTweenAnimation visualGroupAnim;
    [SerializeField] private DOTweenAnimation dialogueAnswerAnimation;

    private MainPlayerInputActions input;
    public MainPlayerInputActions Input { get { return input; } }

    private bool dialogueOpened = false;
    public bool DialogueOpened { get { return dialogueOpened; } }   // 현재 대사창 열렸는지 여부

    bool dialogueProceed = false;

    protected override void Awake()
    {
        base.Awake();

        input = UI_InputManager.Instance.UI_Input;
    }

    private void Start()
    {
        inputWaitObject.SetActive(false);
        visualGroup.SetActive(false);
        visualGroup.GetComponent<CanvasGroup>().alpha = 0f;
    }

    private void OnEnable()
    {
        input.UI.Positive.performed += OnPressedPositive;
    }

    private void OnDisable()
    {
        input.UI.Positive.performed -= OnPressedPositive;
    }

    private void OnPressedPositive(InputAction.CallbackContext context)
    {
        dialogueProceed = true;
    }

    /// <summary>
    /// Sequence 전용 : 대사창 코루틴
    /// </summary>
    /// <param name="dialogues"> 대사 데이터 </param>
    /// <returns></returns>
    public IEnumerator Cor_DialogueSequence(DialogueData[] dialogues)
    {
        visualGroup.SetActive(true);
        ClearDialogue();
        if (!dialogueOpened)
        {
            yield return StartCoroutine(Cor_OpenDialogue());
        }
        yield return StartCoroutine(Cor_TypeDialogue(dialogues));
    }

    private IEnumerator Cor_TypeDialogue(DialogueData[] dialogues)
    {
        for (int i = 0; i < dialogues.Length; i++)
        {
            inputWaitObject.SetActive(false);
            dialogueProceed = false;

            string localized_speecher = dialogues[i].speecher.GetLocalizedString();
            string localized_context = dialogues[i].context.GetLocalizedString();

            if (localized_speecher == string.Empty) speecher.text = string.Empty;
            else speecher.text = localized_speecher;

            string ctx = localized_context;

            for (int j = 0; j <= ctx.RichTextLength(); j++)
            {
                if (dialogueProceed == true)
                { context.text = ctx; break; }
                context.text = ctx.RichTextSubString(j);
                RuntimeManager.PlayOneShot(sound_type);
                yield return new WaitForSeconds(textInterval);
            }

            dialogueProceed = false;

            inputWaitObject.SetActive(true);

            yield return new WaitUntil(() => dialogueProceed);
            inputWaitObject.SetActive(false);
            RuntimeManager.PlayOneShot(sound_proceed);
        }
    }

    private IEnumerator Cor_OpenDialogue()
    {
        if (dialogueOpened) yield break;

        RuntimeManager.PlayOneShot(sound_open);

        visualGroupAnim.DORestartById("DialogueFadein");
        Tween openTw = visualGroupAnim.GetTweens()[0];

        yield return openTw.WaitForCompletion();
        dialogueOpened = true;
    }

    /// <summary>
    /// Sequence 전용 : 대사창 닫기 코루틴
    /// </summary>
    /// <returns></returns>
    public IEnumerator Cor_CloseDialogue()
    {
        if (!dialogueOpened) yield break;

        RuntimeManager.PlayOneShot(sound_close);
        dialogueOpened = false;
        visualGroupAnim.DORestartById("DialogueFadeout");
        Tween closeTw = visualGroupAnim.GetTweens()[1];
        inputWaitObject.SetActive(false);

        yield return closeTw.WaitForCompletion();

        visualGroup.SetActive(false);

    }

    private UI_DialogueAnswerSingle[] answerObjects;
    int index = 0;
    bool whileBreak = false;
    bool answerEntered = false;
    public bool WhileBreak { get { return whileBreak; } }

    public void OnAnswerSelectedByMouse(int index)
    {
        if (answerEntered == false) return;
        whileBreak = true;
    }

    public void OnAnswerMouseEnter(int index)
    {
        RuntimeManager.PlayOneShot(sound_select);
        answerObjects[this.index].OnDeselected();
        this.index = index;
        answerObjects[this.index].OnSelected();
    }

    /// <summary>
    /// Sequence 전용 : 선택지 코루틴
    /// </summary>
    /// <param name="answerStrings"> 선택지 텍스트 </param>
    /// <param name="outIndex"> 선택된 인덱스를 외부로 보낼 때 사용 </param>
    /// <returns></returns>
    public IEnumerator Cor_Branch(LocalizedString[] answerStrings, Action<int> outIndex)
    {
        index = 0;
        whileBreak = false;

        if (!dialogueOpened)
        {
            yield return StartCoroutine(Cor_OpenDialogue());
        }

        dialogueAnswerAnimation.DORestartById("Branch_Open");
        Tween tw = dialogueAnswerAnimation.GetTweens()[0];
        yield return tw.WaitForCompletion();

        answerObjects = new UI_DialogueAnswerSingle[answerStrings.Length];

        for (int i = 0; i < answerStrings.Length; i++)
        {
            GameObject newObject = Instantiate(answerSinglePrefab, answerStartPosition);
            newObject.transform.localPosition = new Vector3(0f, i * answerSpawnSpace);
            answerObjects[i] = newObject.GetComponent<UI_DialogueAnswerSingle>();
            answerObjects[i].Initialize(this, answerStrings[i].GetLocalizedString(), i);
        }


        yield return new WaitForSeconds(0.5f);

        if (index == 0) answerObjects[0].OnSelected();
        answerEntered = true;

        while (!(input.UI.Positive.WasPerformedThisFrame() && !input.UI.Click.WasPerformedThisFrame()))
        {
            if (input.UI.Navigate.WasPressedThisFrame())
            {
                Vector2 inp = input.UI.Navigate.ReadValue<Vector2>();
                if (inp.y == 1f)
                {
                    RuntimeManager.PlayOneShot(sound_select);
                    answerObjects[index].OnDeselected();

                    if (index == answerObjects.Length - 1)
                        index = 0;
                    else
                        index++;

                    answerObjects[index].OnSelected();
                }
                else if (inp.y == -1f)
                {
                    RuntimeManager.PlayOneShot(sound_select);
                    answerObjects[index].OnDeselected();

                    if (index == 0)
                        index = answerObjects.Length - 1;
                    else
                        index--;

                    answerObjects[index].OnSelected();
                }
            }

            if (whileBreak) break;

            yield return null;
        }

        answerEntered = false;

        RuntimeManager.PlayOneShot(sound_open);

        for (int i = answerObjects.Length - 1; i >= 0; i--)
        {
            Destroy(answerObjects[i].gameObject);
        }

        answerObjects = null;

        dialogueAnswerAnimation.DORestartById("Branch_Close");
        tw = dialogueAnswerAnimation.GetTweens()[1];
        yield return tw.WaitForCompletion();

        outIndex(index);
    }

    private void ClearDialogue()
    {
        speecher.text = string.Empty;
        context.text = string.Empty;
    }

    private void OnDestroy()
    {
        input.UI.Positive.performed -= OnPressedPositive;
    }
}
