using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;

public class PlayerReefCrash : StaticSerializedMonoBehaviour<PlayerReefCrash>
{
    [SerializeField] private PlayerCore player;
    [SerializeField] private float reefCrashMaxVelocity = 13.0f;                    //암초대충돌 감지속도
    [SerializeField] private float reefCrashMinVelocity = 3.0f;                     //암초대충돌 감속감지속도
    [SerializeField] private float reefCrashBindTime = 5.0f;                     //암초대충돌 시 기절시간
    [SerializeField] private float reefCrashPower = 50.0f;                           //암초대충돌 시 밀려나는 힘
    private Rigidbody rBody;
    private Vector3 previousVelocity;

    protected override void Awake()
    {
        base.Awake();
        //player = PlayerCore.Instance;
        rBody = GetComponent<Rigidbody>();

    }

    private void Start()
    {
        previousVelocity = rBody.velocity;
        player = PlayerCore.Instance;
    }

    private void Update()
    {
        //이전 프레임의 플레이어 속도
        Vector3 currentVelocity = rBody.velocity;

        // 이전 프레임과 현재 프레임의 속도를 비교하여 속도의 변화를 확인합니다.
        Vector3 velocityChange = currentVelocity - previousVelocity;

        // 1프레임 전의 속도를 출력합니다.
        //Debug.Log("1프레임 전의 속도: " + previousVelocity.magnitude);

        // 현재 프레임의 속도를 이전 프레임의 속도로 업데이트합니다.
        previousVelocity = currentVelocity;
    }

    /// <summary>
    /// 플레이어가 조각배 탑승 중에 암초에 충돌할 경우
    /// </summary>
    IEnumerator ReefCrash()
    {
        player.DisableControls();
        player.SailboatQuit();
        
        yield return new WaitForSeconds(reefCrashBindTime);

        player.EnableControls();
    }

    /// <summary>
    /// 충돌감지
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Reef"))
        {
            Debug.Log("충돌" + previousVelocity.magnitude + ", " + rBody.velocity.magnitude);
            ///<summary>
            ///암초충돌감지
            /// </summary>
            if (previousVelocity.magnitude > reefCrashMaxVelocity && rBody.velocity.magnitude < reefCrashMinVelocity)
            {
                Debug.Log("대충돌" + previousVelocity.magnitude + ", " + rBody.velocity.magnitude);
                rBody.AddForce(Vector3.back * reefCrashPower, ForceMode.Impulse);
                StartCoroutine(ReefCrash());
            }

        }
    }
}
