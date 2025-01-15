using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BreakableObject : MonoBehaviour
{
    [InfoBox("플레이어가 부스터인 상태에서 표시된 \"파괴 방향\"화살표와 같은 방향으로 돌진하면 오브젝트가 파괴됩니다.")]
    [SerializeField,LabelText("파괴에 필요한 속도")] private float requireingVelocity = 10f;
    [SerializeField, LabelText("파괴시 이벤트")] private UnityEvent eventOnDestructed;
    [SerializeField] EventReference sound_break;

    [Title("")]
    [SerializeField] private Collider trigger;
    [SerializeField] private GameObject modelObject;
    [SerializeField] private ParticleSystem debriesParticle;


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerCore player = other.attachedRigidbody.GetComponent<PlayerCore>();
            float projectedVelocity = Vector3.Dot(player.Velocity.normalized, -transform.forward) * player.Velocity.magnitude;

            if(projectedVelocity > requireingVelocity && player.BoosterActive && player.CurrentPlayerState == PlayerMovementState.Sailboat)
            {
                modelObject.SetActive(false);
                debriesParticle.Play();
                FMODUnity.RuntimeManager.PlayOneShot(sound_break);
                trigger.enabled = false;
                if (eventOnDestructed != null) eventOnDestructed.Invoke();
                StartCoroutine(Cor_TimeStiff(0.1f, 0.1f));
            }
        }
    }

    IEnumerator Cor_TimeStiff(float duration, float timescale)
    {
        Time.timeScale = timescale; Time.fixedDeltaTime = Time.timeScale * 0.02f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1.0f; Time.fixedDeltaTime = 0.02f;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        DrawArrow.ForGizmo(trigger.bounds.center, -transform.forward*3f, 0.25f, 30f);
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 16;
        textStyle.alignment = TextAnchor.LowerCenter;
        UnityEditor.Handles.Label(trigger.bounds.center, new GUIContent("파괴 방향"), textStyle);
    }
#endif

}
