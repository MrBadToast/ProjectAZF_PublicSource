using DG.Tweening;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class UI_IntroCanvas : StaticSerializedMonoBehaviour<UI_IntroCanvas>
{
    [SerializeField] private GameObject visualGroupObject;
    [SerializeField] private TextMeshProUGUI textmesh;
    [SerializeField] private DOTweenAnimation textTween;
    [SerializeField] private EventReference sound_progress;

    protected override void Awake()
    {
         base.Awake();
    }

    private void Start()
    {

    }

    public IEnumerator Cor_PrintText(LocalizedString[] texts, float interval)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(1.0f);

        visualGroupObject.SetActive(true);
        DOTweenAnimation anim = textTween;
        Tween tw = anim.tween;

        for(int i = 0; i < texts.Length; i++)
        {
            anim.DORewind();
            RuntimeManager.PlayOneShot(sound_progress);
            textmesh.text = texts[i].GetLocalizedString();
            yield return new WaitForSeconds(interval);
            anim.DORestart();
            yield return tw.WaitForCompletion();
            yield return new WaitForSeconds(1.0f);
        }

        DOTweenAnimation groupAnim = visualGroupObject.GetComponent<DOTweenAnimation>();
        Tween groupTw = groupAnim.tween;
        groupAnim.DORestartById("Fadeout");
        yield return groupTw.WaitForCompletion();
    }
}
