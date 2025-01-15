using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_GoldenCarrot : Interactable_Base
{
    [SerializeField] private string _ID;
    public string ID { get { return _ID; } }
    [SerializeField, LabelText("공중에 떠있음")] private bool floating = false;
    [SerializeField, LabelText("황금 당근 데이터")] private ItemData carrotItem;
    [SerializeField, LabelText("황금 당근 개수")] private int carrotQuantity = 1;
    [SerializeField, Required(), FoldoutGroup("ChildReferences")] private GameObject stemObject;
    [SerializeField, Required(), FoldoutGroup("ChildReferences")] private GameObject flowerFullObject;
    [SerializeField, Required(), FoldoutGroup("ChildReferences")] private GameObject budObject;
    [SerializeField, Required(), FoldoutGroup("ChildReferences")] private Transform playerAnchor;
    [SerializeField, Required(), FoldoutGroup("ChildReferences")] private GameObject sparkleEffect;

    Animator stemAnimator;
    Animator flowerAnimator;

    [LabelText("")]
    [SerializeField] private EventReference sound_CarrotPicked;
    [SerializeField] private EventReference sound_CarrotGone;

    bool flowerPickFlag = false;

    private void Awake()
    {   
        stemAnimator = stemObject.GetComponent<Animator>();
        flowerAnimator = flowerFullObject.GetComponent<Animator>();
    }

    public void OnFlowerPicked()
    {
        stemObject.SetActive(true);
        flowerFullObject.SetActive(false);
        budObject.SetActive(true);
        budObject.transform.SetParent(PlayerCore.Instance.FlowerHoldingTarget,false);       
    }

    public void DestroyBud()
    {
        Destroy(budObject);
    }

    public override void Interact()
    {
        if (flowerPickFlag) return;

        GetComponent<Collider>().enabled = false;
        StartCoroutine(Cor_PickFlower());
    }

    IEnumerator Cor_PickFlower()
    {
        flowerPickFlag = true;
        PlayerCore.Instance.DisableControls();

        Vector3 prevPosition = PlayerCore.Instance.transform.position;
        Quaternion prevRotation = PlayerCore.Instance.transform.rotation;

        float transitionTime = 0.5f;
        for(float t = 0; t < transitionTime; t+= Time.fixedDeltaTime)
        {
            PlayerCore.Instance.transform.position = Vector3.Lerp(PlayerCore.Instance.transform.position, playerAnchor.position, 0.2f);
            PlayerCore.Instance.transform.rotation = Quaternion.Lerp(PlayerCore.Instance.transform.rotation, playerAnchor.rotation, 0.2f);
            yield return new WaitForFixedUpdate();
        }

        PlayerCore.Instance.transform.position = playerAnchor.position;
        PlayerCore.Instance.transform.rotation = playerAnchor.rotation;

        PlayerCore.Instance.OnFlowerPicked(true);
        flowerAnimator.SetTrigger("Picked");

        RuntimeManager.PlayOneShot(sound_CarrotPicked);
        yield return new WaitForSeconds(3.0f); 

        yield return StartCoroutine(PlayerInventoryContainer.Instance.Cor_ItemWindow(carrotItem, carrotQuantity));
        PlayerInventoryContainer.Instance.AddItem(carrotItem, carrotQuantity);

        RuntimeManager.PlayOneShot(sound_CarrotGone);
        budObject.GetComponent<DOTweenAnimation>().DORestart();
        PlayerCore.Instance.OnFlowerPicked(false);
        yield return new WaitForSeconds(1.0f);

        PlayerCore.Instance.OnFlowerPicked(false);
        PlayerCore.Instance.EnableControls();
        flowerPickFlag = false;
        sparkleEffect.SetActive(false);

        base.OnDisable();
    }

    #region LegacyCodes
    //public override void Interact()
    //{
    //    flowerAnimator.SetTrigger("Picked");

    //    if (!floating)
    //    {
    //        carrotAquiredSequences = new Sequence_Base[5];

    //        Sequence_WaitForSeconds wait1 = new Sequence_WaitForSeconds();
    //        wait1.time = 2.5f;

    //        Sequence_PlaySound sound1 = new Sequence_PlaySound();
    //        sound1.sound = sound_CarrotPicked;

    //        Sequence_ObtainItem obtain = new Sequence_ObtainItem();
    //        obtain.item = carrotItem;
    //        obtain.quantity = carrotQuantity;

    //        Sequence_PlaySound sound2 = new Sequence_PlaySound();
    //        sound2.sound = sound_CarrotGone;

    //        Sequence_Animation animation2 = new Sequence_Animation();
    //        animation2.objectName = gameObject.name;
    //        animation2.stateName = "Gone";

    //        Sequence_WaitForSeconds wait2 = new Sequence_WaitForSeconds();
    //        wait1.time = 1.5f;

    //        carrotAquiredSequences[0] = sound1;
    //        carrotAquiredSequences[1] = wait1;
    //        carrotAquiredSequences[2] = obtain;
    //        carrotAquiredSequences[3] = sound2;
    //        carrotAquiredSequences[4] = wait2;
    //    }
    //    else
    //    {
    //        carrotAquiredSequences = new Sequence_Base[4];

    //        Sequence_PlaySound sound2 = new Sequence_PlaySound();
    //        sound2.sound = sound_CarrotGone;

    //        Sequence_WaitForSeconds wait1 = new Sequence_WaitForSeconds();
    //        wait1.time = 1.5f;

    //        Sequence_ObtainItem obtain = new Sequence_ObtainItem();
    //        obtain.item = carrotItem;
    //        obtain.quantity = 1;

    //        Sequence_WaitForSeconds wait2 = new Sequence_WaitForSeconds();
    //        wait2.time = 0.5f;

    //        carrotAquiredSequences[0] = sound2;
    //        carrotAquiredSequences[1] = wait1;
    //        carrotAquiredSequences[2] = obtain;
    //        carrotAquiredSequences[3] = wait2;

    //    }

    //    GetComponent<Collider>().enabled = false;
    //    SequenceInvoker.Instance.StartSequence(carrotAquiredSequences);
    //    base.OnDisable();
    //}
    #endregion

}
