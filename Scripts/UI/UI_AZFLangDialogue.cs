using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_AZFLangDialogue : StaticSerializedMonoBehaviour<UI_AZFLangDialogue>
{
    [SerializeField] private float typoInterval = 0.05f;
    [SerializeField] private GameObject visualGroup;
    [SerializeField] private TextMeshProUGUI context;
    [SerializeField] private DOTweenAnimation openingTween;
    [SerializeField] private GameObject progressTriangle;

    MainPlayerInputActions input;
    bool keySkip = false;
    bool dialogueRunning = false;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        input = UI_InputManager.Instance.UI_Input;
        visualGroup.SetActive(false);
    }

    private void Update()
    {
        if (!dialogueRunning) return;

        if (input.UI.Positive.WasPressedThisFrame())
            keySkip = true;
    }

    public IEnumerator Cor_Dialogue(string[] contexts)
    {
        if (dialogueRunning) yield break;

        dialogueRunning = true;

        visualGroup.SetActive(true);
        context.text = string.Empty;

        yield return openingTween.tween.WaitForCompletion();

        for (int i = 0; i < contexts.Length; i++)
        {
            string s = contexts[i];
            s = s.Replace("\r", "");
            keySkip = false;

            for (int j = 0; j < s.Length; j++)
            {
                if(keySkip) { context.text = s; break; }
                context.text = s.Substring(0, j+1);
                yield return new WaitForSeconds(typoInterval);
            }

            yield return new WaitForSeconds(0.5f);
            progressTriangle.SetActive(true);

            yield return new WaitUntil(() => input.UI.Positive.WasPressedThisFrame());
            progressTriangle.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        visualGroup.SetActive(false);
        dialogueRunning = false;
    }
}
