using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class GeyserLift : Interactable_Base
{
    [SerializeField, LabelText("일회성")] private bool disableAfterFinished = false;
    [SerializeField, LabelText("상승 높이")] private float liftHeight = 12f;

    [SerializeField, LabelText("상승 애니메이션")] private AnimationCurve liftAnimation;
    [SerializeField, LabelText("상승 시간")] private float liftingDuration = 1.0f;
    [SerializeField, LabelText("착지 포물선")] private AnimationCurve landingCurve;
    [SerializeField, LabelText("포물선 높이")] private float landingCurveHeight = 5.0f;
    [SerializeField, LabelText("착지 시간")] private float landingDuration = 1.0f;

    [SerializeField, Required(), FoldoutGroup("ChildReferences")] private ParticleSystem idleEffect;
    [SerializeField, Required(), FoldoutGroup("ChildReferences")] private ParticleSystem activateEffect;
    [SerializeField, Required(), FoldoutGroup("ChildReferences")] private ParticleSystem activeLoopEffect;
    [SerializeField, Required(), FoldoutGroup("ChildReferences")] private ParticleSystem disableEffect;
    [SerializeField, Required(), FoldoutGroup("ChildReferences")] private Transform landpoint;
    [SerializeField, Required(), FoldoutGroup("ChildReferences")] private GameObject geyserActiveSound;

    private bool liftInProgress = false;
    private bool interacted = false;

    public override void Interact()
    {
        base.Interact();

        if (disableAfterFinished && interacted) return;
        if (liftInProgress) return;

        StartCoroutine(Cor_LiftProgress());

    }

    private void FixedUpdate()
    {
        if (!GlobalOceanManager.IsInstanceValid) return;

       transform.position = new Vector3(transform.position.x,GlobalOceanManager.Instance.GetWaveHeight(transform.position) + 0.2f,transform.position.z);
    }

    private IEnumerator Cor_LiftProgress()
    {
        liftInProgress = true;
        PlayerCore.Instance.DisableControls();

        float alignTime = 0.5f;

        for(float t = 0; t < alignTime; t += Time.fixedDeltaTime)
        {
            PlayerCore.Instance.Rigidbody.MovePosition(Vector3.Lerp(PlayerCore.Instance.transform.position, transform.position,0.1f));
            yield return new WaitForFixedUpdate();
        }

        idleEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        activateEffect.Play(true);
        geyserActiveSound.SetActive(true);
        Vector3 startPos = transform.position;

        for(float t = 0; t < liftingDuration; t += Time.fixedDeltaTime)
        {
            float progress = t / liftingDuration;
            
            PlayerCore.Instance.Rigidbody.MovePosition(Vector3.Lerp(startPos, startPos + Vector3.up * liftHeight, liftAnimation.Evaluate(progress)));
            yield return new WaitForFixedUpdate();
        }

        activateEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        activeLoopEffect.Play(true);

        startPos = startPos + Vector3.up * liftHeight;
        Vector3 velCache = Vector3.zero;

        for (float t = 0; t < landingDuration; t += Time.fixedDeltaTime)
        {
            float progress = t / landingDuration;
            PlayerCore.Instance.Rigidbody.MovePosition(Vector3.Lerp(startPos, landpoint.position, progress) + Vector3.up * landingCurve.Evaluate(progress) * landingCurveHeight);
            velCache = PlayerCore.Instance.Rigidbody.velocity;
            yield return new WaitForFixedUpdate();
        }

        activeLoopEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        disableEffect.Play(true);
        yield return null;

        PlayerCore.Instance.EnableControls();
        PlayerCore.Instance.Rigidbody.AddForce(velCache, ForceMode.VelocityChange);

        yield return new WaitForSeconds(disableEffect.main.duration);
        geyserActiveSound.SetActive(false);
        disableEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        idleEffect.Play(true);

        liftInProgress = false;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        DrawArrow.ForGizmo(transform.position, Vector3.up * liftHeight);
        DrawArrow.ForGizmo(transform.position + Vector3.up * liftHeight, landpoint.position - (transform.position + Vector3.up * liftHeight));
        Gizmos.DrawWireSphere(landpoint.position, 0.5f);
    }

}
