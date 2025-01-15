using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_Tutorials : StaticSerializedMonoBehaviour<UI_Tutorials>
{
    [System.Serializable]
    public struct TutorialCollective
    {
        public GameObject keyboardPrefab;
        public GameObject gamepadPrefab;
    }

    [SerializeField] private RectTransform tutorialWindowPoint;
    [SerializeField] private Dictionary<string, TutorialCollective> tutorialObjects;

    GameObject currentWindowObject;
    Queue tutorialKeyQueue;

    protected override void Awake()
    {
        base.Awake();
        tutorialKeyQueue = new Queue();
    }

    public void OpenTutorial(string tutorialKey)
    {
        if (tutorialKeyQueue.Count == 0)
            StartCoroutine(Cor_OpenTutorial(tutorialKey));
        else
        {
            if (!tutorialObjects.ContainsKey(tutorialKey))
                Debug.LogWarning("TutorialObject key doesn't exists : " + tutorialKey); 
            else
                tutorialKeyQueue.Enqueue(tutorialKey);
        }
    }

    public void AbortAllTutorials()
    {
        StopAllCoroutines();
        Destroy(currentWindowObject);
        tutorialKeyQueue.Clear();
    }

    private IEnumerator Cor_OpenTutorial(string tutorialKey)
    {
        if (!tutorialObjects.ContainsKey(tutorialKey)) { Debug.LogWarning("TutorialObject key doesn't exists : " + tutorialKey); yield break; }
        tutorialKeyQueue.Enqueue(tutorialKey);

        while (tutorialKeyQueue.Count > 0)
        {
            string key = tutorialKeyQueue.Peek() as string;

            if (PlayerInput.all[0].currentControlScheme.Equals("Keyboard&Mouse"))
            {
                if (tutorialObjects[key].keyboardPrefab != null)
                    currentWindowObject = Instantiate(tutorialObjects[key].keyboardPrefab, tutorialWindowPoint);
            }
            else if (PlayerInput.all[0].currentControlScheme.Equals("GamePad"))
            {
                if (tutorialObjects[key].gamepadPrefab != null)
                    currentWindowObject = Instantiate(tutorialObjects[key].gamepadPrefab, tutorialWindowPoint);
            }

            DOTweenAnimation anim;

            if(currentWindowObject.TryGetComponent<DOTweenAnimation>(out anim))
            {
                Tween closeTween = anim.GetTweens()[1];
                yield return closeTween.WaitForCompletion();
            }
            else
            {
                yield return new WaitForSeconds(8f);
            }
            yield return new WaitForSeconds(0.5f);

            Destroy(currentWindowObject);
            currentWindowObject = null;

            tutorialKeyQueue.Dequeue();
        }
    }
}
