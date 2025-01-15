using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Animations.Rigging;
using FMODUnity;
using Cinemachine.Utility;
using JetBrains.Annotations;
using System.Linq;

public enum PlayerMovementState
{
    None,
    Ground,
    Swimming,
    Sailboat
}

public class PlayerCore : StaticSerializedMonoBehaviour<PlayerCore>
{
    //============================================
    //
    // [싱글턴 오브젝트]
    // 플레이어 캐릭터에 관한 핵심 코드들입니다.
    // 플레이어의 움직임 상태는 State패턴에 의해 관리되고 있습니다. 자세한 정보는 프로그래머 매뉴얼을 참고해주세요 
    //
    //============================================

    #region ================ Properties ================
    [Title("기본 조작 속성")]
    [SerializeField,LabelText("이동 속도")] private float moveSpeed = 1.0f;
    [SerializeField,LabelText("달리기 속도")] private float sprintSpeedMult = 2.0f;
    [SerializeField,LabelText("수영시 속도")] private float swimSpeed = 1.0f;
    [SerializeField,LabelText("점프 시 수직 힘")] private float jumpPower = 1.0f;
    [SerializeField,Range(0f, 1f), LabelText("들기 속도 감소 곱")] private float holdingMoveSpeedMult = 0.5f;

    [Title("물리")]
    [SerializeField, Range(0f, 1f),LabelText("입력 없을 때 마찰력")] private float horizontalDrag = 0.5f;
    [SerializeField, MinMaxSlider(0f, 80f,true), LabelText("경사면 효과")] private Vector2 slopeEffect;
    [SerializeField, LabelText("바닥 인식 거리")] private float groundCastDistance = 0.1f;
    [SerializeField, LabelText("바닥 인식 제외 레이어")] private LayerMask groundIgnore;
    //[SerializeField, LabelText("미끄러짐 시작 시간")] private float slidingStartTime = 1.0f;
    [SerializeField, Range(0f, 0.8f), LabelText("물 걷기 저항")] private float waterWalkDragging = 0.5f;
    [SerializeField, LabelText("수영시 받는 저항값")] private float swimRigidbodyDrag = 10.0f;
    [SerializeField, LabelText("수영시 추가 부력")] private float swimUpforce = 1.0f;
    [SerializeField, ReadOnly] private bool grounding = false;                      // 디버그 : 바닥 체크
    [SerializeField, ReadOnly] private Vector3 groundNormal = Vector3.up;           // 디버그 : 바닥 법선

    [Title("조각배 속성")]
    [SerializeField, LabelText("기본 선회력")] private float sailboatSteering = 1.0f;
    [SerializeField, LabelText("기본 부력")] private float sailboatByouancy = 1.0f;
    [SerializeField, LabelText("중력")] private float sailboatGravity = 1.0f;
    [SerializeField, LabelText("가속력")] private float sailboatAccelerationForce = 50f;
    [SerializeField, LabelText("수면 각도 영향력")] private float sailboatSlopeInfluenceForce = 20f;
    [SerializeField, LabelText("저공비행 판정 높이")] private float sailboatNearsurf = 0.5f;
    [SerializeField, LabelText("저공비행 추가 속도")] private float sailboatNearsurfBoost = 1.2f;
    [SerializeField, LabelText("완전 침수시 저항")] private float sailboatFullDrag = 10.0f;
    [SerializeField, LabelText("일부 침수시 저항")] private float sailboatScratchDrag = 1.0f;
    [SerializeField, LabelText("최소 저항")] private float sailboatGlidingDrag = 0.0f;
    [SerializeField, LabelText("상하 컨트롤 힘")] private float sailboatVerticalControl = 10.0f;
    [SerializeField, LabelText("활공력")] private float sailboatGliding = 1.0f;
    [SerializeField, LabelText("자동해제 시간")] private float sailboatAutoOffTime = 3.0f;
    [SerializeField, LabelText("바람소리 시작 속도")] private float gustStartVelocity = 10.0f;
    [SerializeField, LabelText("바람소리 시작 속도")] private float gustMaxVelocity = 50.0f;

    [Title("조각배 스킬")]
    [SerializeField, LabelText("부스터-가속도 곱")] private float boosterMult = 2.0f;
    [SerializeField, LabelText("부스터-지속시간")] private float boosterDuration = 1.0f;
    [SerializeField, LabelText("부스터-쿨타임")] private float boosterCooldown = 1.0f;
    [SerializeField, LabelText("도약-수직가속")] private float leapupPower = 10f;
    [SerializeField, LabelText("도약-쿨타임")] private float leapupCooldown = 1.0f;
    [SerializeField, LabelText("도약-지속시간")] private float leapupDuration = 0.5f;
    [SerializeField, LabelText("도약-가속력커브")] private AnimationCurve leapupForceCurve;
    [SerializeField,Range(1.0f,2.0f), LabelText("드리프트-회전값")] private float driftSteer = 1.5f;
    [SerializeField, LabelText("드리프트-순간 추진력")] private float driftKickPower = 10.0f;
    [SerializeField, LabelText("드리프트 순간추진 필요시간")] private float driftKickRequireingTime = 1.0f;

    [Title("소리")]
    [SerializeField, LabelText("입수 소리")] private EventReference sound_splash;
    [SerializeField, LabelText("드리프트 순간추진")] private EventReference sound_driftKick;
    [SerializeField, LabelText("드리프트 충전됨")] private EventReference sound_driftCharged;
    [SerializeField, LabelText("조각배 충돌")] private EventReference sound_SailboatBump;

    [Title("기타")]
    [SerializeField, LabelText("캐릭터 시선 타겟 유지거리")] private float interestDistance = 10.0f;


#if UNITY_EDITOR
#pragma warning disable CS0414

    [Title("Info")]
    [SerializeField, ReadOnly, LabelText("PlayerControl enabled")] private bool control_disabled_debug;
    [SerializeField, ReadOnly, LabelText("Control Disable Stack")] private int control_disableStack_debug;
    [SerializeField, ReadOnly, LabelText("Currentmove")] private string current_move_debug = "";
    [SerializeField, ReadOnly, LabelText("Velocity")] private Vector3 velocity_debug;
    [SerializeField, ReadOnly, LabelText("Velocity magnitude")] private float velocity_mag_debug;
    [SerializeField, ReadOnly, LabelText("Horizontal velocity magnitude")] private float velocity_hor_debug;
    [SerializeField, ReadOnly, LabelText("Current holding item")] private string current_holding_item_debug;
    [SerializeField, ReadOnly, LabelText("CurrentVelocityDelta")] private string current_velocity_delta_debug;

#pragma warning restore CS0414
#endif

    #region ChildReferences
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private DirectionIndicator directionIndicator;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Animator animator;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private BuoyantBehavior buoyant;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Transform RCO_foot;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] new private CapsuleCollider collider;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private SphereCollider bottomColider;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private SailboatBehavior sailboat;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Transform sailboatModelPivot;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private GameObject normalSplashEffectPrefab;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private ParticleSystem sailingSprayEffect;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private ParticleSystem sailingSplashEffect_HighVel;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private ParticleSystem sailingSwooshEffect;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private ParticleSystem footstepEffect;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private ParticleSystem jumpEffect;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private ParticleSystem stoneAttackEffect;
    [SerializeField, Required(), FoldoutGroup("ChildReferences")] private ParticleSystem stunEffect;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Transform headTarget;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Transform leftHandTarget;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Transform rightHandTarget;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Transform holdingItemTarget;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Transform flowerHodlingTarget;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Rig headRig;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Rig sailboatFootRig;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Rig handRig;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Rig holdObjectRig;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private StudioEventEmitter gustSound;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private StudioEventEmitter waterScratchSound;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private StudioEventEmitter sailboatEngineSound;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private StudioEventEmitter driftSound;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private GameObject IsmaelSpiritObject;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Animator IsmaelSpiritAnimator;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Animator bellflowerLockPointAnimator;
    [SerializeField , Required(), FoldoutGroup("ChildReferences")] private Transform IsmaelSpiritLookTarget;

    public Transform HoldingItemTarget { get { return holdingItemTarget; } }
    public Transform FlowerHoldingTarget { get { return flowerHodlingTarget; } }
    #endregion


    #endregion

    private Rigidbody rBody;
    public Rigidbody Rigidbody { get { return rBody; } }
    public Vector3 Velocity { get { return rBody.velocity; } }
    private StudioEventEmitter sound;
    private Transform interestPoint;
    private Interactable_Holding currentHoldingItem;
    private Vector3 previousVelocity;
    /// <summary>
    /// 현재 플레이어가 무언가를 들고 있는지 확인합니다.
    /// </summary>
    public bool IsHoldingSomething { get { return currentHoldingItem != null; } }

    private MainPlayerInputActions input;
    public MainPlayerInputActions Input { get { return input; } }

    /// <summary>
    /// 부스터 스킬이 해금되었는지 확인합니다.
    /// </summary>
    [SerializeField] private bool availableForDrift = false;
    public bool AvailableForDrift {  get { return availableForDrift; } }

    /// <summary>
    /// 조각배를 사용할 수 있는 상황인지 확인합니다.
    /// </summary>
    [SerializeField] private bool availableForSailboat = true;
    public bool AvailableForSailboat { get { return availableForSailboat; } }
    public void SetAvailableForSailboat(bool value) { availableForSailboat = value; }

    private bool sprinting = false;
    /// <summary>
    /// // 현재 플레이어가 땅을 딛고 있는지 확인합니다.
    /// </summary>
    public bool Grounding { get { return grounding; } }

    private float initialRigidbodyDrag = 0f;

    float leapupAvailHeight = 3.0f;

    Vector3 headRigForward;

    int layerIndex_Swim;
    int layerIndex_Boarding;
    int layerIndex_ItemHolding;

    bool boosterActive = false;
    public bool BoosterActive { get { return boosterActive; } }
    bool driftActive = false;
    public bool DriftActive { get { return driftActive; } }
    bool leapupActive = false;
    public bool LeapupActive { get { return leapupActive; } }

    //플레이어 상태 참고용 변수
    public string movementStateRefernce;

    private MovementState currentMovement_hidden;
    private MovementState CurrentMovement
    {
        get { return currentMovement_hidden; }
        set
        {
            if (currentMovement_hidden == null) currentMovement_hidden = value;
            else
            {
                if (currentMovement_hidden.GetType() == value.GetType()) return;
                if (value.GetType() == typeof(Movement_Sailboat))
                    if (!availableForSailboat) return;

                currentMovement_hidden.OnMovementExit(this);
                currentMovement_hidden = value;
                currentMovement_hidden.OnMovementEnter(this);
            }
        }
    }

    /// <summary>
    /// 현재 플레이어의 CurrentMovement정보를 받아올 수 있습니다.
    /// </summary>
    public PlayerMovementState CurrentPlayerState
    {
        get
        {
            if (CurrentMovement.GetType() == typeof(Movement_Ground)) return PlayerMovementState.Ground;
            else if (CurrentMovement.GetType() == typeof(Movement_Swimming)) return PlayerMovementState.Swimming;
            else if (CurrentMovement.GetType() == typeof(Movement_Sailboat)) return PlayerMovementState.Sailboat;
            else return PlayerMovementState.None;
        }
    }

    #region ================ PlayerAbilityAttributes ================

    public enum AbilityAttribute
    {
        MoveSpeed,
        SwimSpeed,
        JumpPower,
        Steering,
        SailboatAcceleration,
        SailboatGliding,
        LeapupPower,
        BoosterDuration,
        BoosterMult
    }

    public class AbilityAttributeUnit
    {
        public AbilityAttribute attribute;
        public float value = 1.0f;
        public float time = 0f;
        public string ID = string.Empty;

    }

    [ShowInInspector, ReadOnly] private List<AbilityAttributeUnit> permenentAttributes;
    [ShowInInspector, ReadOnly] private List<AbilityAttributeUnit> timeAttributes;
    [ShowInInspector, ReadOnly] private List<AbilityAttributeUnit> IDAttributes;

    public float FinalMoveSpeed
    {
        get
        {
            float result = moveSpeed;

            for (int i = 0; i < permenentAttributes.Count; i++)
            {
                if (permenentAttributes[i].attribute == AbilityAttribute.MoveSpeed)
                    result += permenentAttributes[i].value;
            }
            for (int i = 0; i < timeAttributes.Count; i++)
            {
                if (timeAttributes[i].attribute == AbilityAttribute.MoveSpeed)
                    result *= timeAttributes[i].value;
            }
            for (int i = 0; i < IDAttributes.Count; i++)
            {
                if (IDAttributes[i].attribute == AbilityAttribute.MoveSpeed)
                    result *= IDAttributes[i].value;
            }
            return result;
        }
    }

    public float FinalSwimSpeed
    {
        get
        {
            float result = swimSpeed;

            for (int i = 0; i < permenentAttributes.Count; i++)
            {
                if (permenentAttributes[i].attribute == AbilityAttribute.SwimSpeed)
                    result += permenentAttributes[i].value;
            }
            for (int i = 0; i < timeAttributes.Count; i++)
            {
                if (timeAttributes[i].attribute == AbilityAttribute.SwimSpeed)
                    result *= timeAttributes[i].value;
            }
            for (int i = 0; i < IDAttributes.Count; i++)
            {
                if (IDAttributes[i].attribute == AbilityAttribute.SwimSpeed)
                    result *= IDAttributes[i].value;
            }
            return result;
        }
    }

    public float FinalJumpPower
    {
        get
        {
            float result = jumpPower;

            for (int i = 0; i < permenentAttributes.Count; i++)
            {
                if (permenentAttributes[i].attribute == AbilityAttribute.JumpPower)
                    result += permenentAttributes[i].value;
            }
            for (int i = 0; i < timeAttributes.Count; i++)
            {
                if (timeAttributes[i].attribute == AbilityAttribute.JumpPower)
                    result *= timeAttributes[i].value;
            }
            for (int i = 0; i < IDAttributes.Count; i++)
            {
                if (IDAttributes[i].attribute == AbilityAttribute.JumpPower)
                    result *= IDAttributes[i].value;
            }
            return result;
        }
    }

    public float FinalSailboatAcceleration
    {
        get
        {
            float result = sailboatAccelerationForce;

            for (int i = 0; i < permenentAttributes.Count; i++)
            {
                if (permenentAttributes[i].attribute == AbilityAttribute.SailboatAcceleration)
                    result += permenentAttributes[i].value;
            }
            for (int i = 0; i < timeAttributes.Count; i++)
            {
                if (timeAttributes[i].attribute == AbilityAttribute.SailboatAcceleration)
                    result *= timeAttributes[i].value;
            }
            for (int i = 0; i < IDAttributes.Count; i++)
            {
                if (IDAttributes[i].attribute == AbilityAttribute.SailboatAcceleration)
                    result *= IDAttributes[i].value;
            }
            return result;
        }
    }

    public float FinalSailboatGliding
    {
        get
        {
            float result = sailboatGliding;

            for (int i = 0; i < permenentAttributes.Count; i++)
            {
                if (permenentAttributes[i].attribute == AbilityAttribute.SailboatGliding)
                    result += permenentAttributes[i].value;
            }
            for (int i = 0; i < timeAttributes.Count; i++)
            {
                if (timeAttributes[i].attribute == AbilityAttribute.SailboatGliding)
                    result *= timeAttributes[i].value;
            }
            for (int i = 0; i < IDAttributes.Count; i++)
            {
                if (IDAttributes[i].attribute == AbilityAttribute.SailboatGliding)
                    result *= IDAttributes[i].value;
            }
            return result;
        }
    }

    public float FinalSteering
    {
        get
        {
            float result = sailboatSteering;

            for (int i = 0; i < permenentAttributes.Count; i++)
            {
                if (permenentAttributes[i].attribute == AbilityAttribute.Steering)
                    result += permenentAttributes[i].value;
            }
            for (int i = 0; i < timeAttributes.Count; i++)
            {
                if (timeAttributes[i].attribute == AbilityAttribute.Steering)
                    result *= timeAttributes[i].value;
            }
            for (int i = 0; i < IDAttributes.Count; i++)
            {
                if (IDAttributes[i].attribute == AbilityAttribute.Steering)
                    result *= IDAttributes[i].value;
            }
            return result;
        }
    }

    public float FinalLeapupPower
    {
        get
        {
            float result = leapupPower;

            for (int i = 0; i < permenentAttributes.Count; i++)
            {
                if (permenentAttributes[i].attribute == AbilityAttribute.LeapupPower)
                    result += permenentAttributes[i].value;
            }
            for (int i = 0; i < timeAttributes.Count; i++)
            {
                if (timeAttributes[i].attribute == AbilityAttribute.LeapupPower)
                    result *= timeAttributes[i].value;
            }
            for (int i = 0; i < IDAttributes.Count; i++)
            {
                if (IDAttributes[i].attribute == AbilityAttribute.LeapupPower)
                    result *= IDAttributes[i].value;
            }
            return result;
        }
    }

    public float FinalBoosterDuration
    {
        get
        {
            float result = boosterDuration;

            for (int i = 0; i < permenentAttributes.Count; i++)
            {
                if (permenentAttributes[i].attribute == AbilityAttribute.BoosterDuration)
                    result += permenentAttributes[i].value;
            }
            for (int i = 0; i < timeAttributes.Count; i++)
            {
                if (timeAttributes[i].attribute == AbilityAttribute.BoosterDuration)
                    result *= timeAttributes[i].value;
            }
            for (int i = 0; i < IDAttributes.Count; i++)
            {
                if (IDAttributes[i].attribute == AbilityAttribute.BoosterDuration)
                    result *= IDAttributes[i].value;
            }
            return result;
        }
    }

    public float FinalBoosterMult
    {
        get
        {
            float result = boosterMult;

            for (int i = 0; i < permenentAttributes.Count; i++)
            {
                if (permenentAttributes[i].attribute == AbilityAttribute.BoosterMult)
                    result += permenentAttributes[i].value;
            }
            for (int i = 0; i < timeAttributes.Count; i++)
            {
                if (timeAttributes[i].attribute == AbilityAttribute.BoosterMult)
                    result *= timeAttributes[i].value;
            }
            for (int i = 0; i < IDAttributes.Count; i++)
            {
                if (IDAttributes[i].attribute == AbilityAttribute.BoosterMult)
                    result *= IDAttributes[i].value;
            }
            return result;
        }
    }


    /// <summary>
    /// 영구적인 플레이어 속성 값을 더합니다.(합연산)
    /// </summary>
    /// <param name="attr">속성</param>
    public void AddPermernentAttribute(AbilityAttribute ability, float value)
    {
        AbilityAttributeUnit newAttr = new AbilityAttributeUnit();
        newAttr.attribute = ability; newAttr.value = value;
        permenentAttributes.Add(newAttr);
    }

    /// <summary>
    /// 일시적으로 플레이어 수치를 적용합니다.(곱연산)
    /// </summary>
    /// <param name="attr">속성</param>
    /// <param name="time">시간</param>
    public void SetTempoaryAttribute(AbilityAttribute ability, float value, float time)
    {
        AbilityAttributeUnit newAttr = new AbilityAttributeUnit();
        newAttr.attribute = ability; newAttr.value = value; newAttr.time = time;
        timeAttributes.Add(newAttr);
    }

    /// <summary>
    /// 일시적으로 플레이어 속성을 ID를 붙여 적용합니다.(곱연산) 이미 있는 ID에 값을 적용할 경우 기존 값을 바꿉니다.
    /// </summary>
    /// <param name="attr">속성</param>
    /// <param name="ID"></param>
    public void SetAttributeWithID(AbilityAttribute ability, float value, string ID)
    {
        for (int i = 0; i < IDAttributes.Count; i++)
        {
            if (IDAttributes[i].ID.Equals(ID))
            {
                IDAttributes[i].value = value;
                return;
            }
        }

        AbilityAttributeUnit newAttr = new AbilityAttributeUnit();
        newAttr.attribute = ability; newAttr.value = value; newAttr.ID = ID;
        IDAttributes.Add(newAttr);
    }

    /// <summary>
    ///  ID가 붙어있는 플레이어 속성을 해제합니다.
    /// </summary>
    /// <param name="ID"></param>
    public void CancelAttributeWithID(string ID)
    {
        for (int i = 0; i < IDAttributes.Count; i++)
        {
            if (IDAttributes[i].ID == ID)
            {
                IDAttributes.RemoveAt(i);
                return;
            }
        }

        Debug.LogWarning("ATTRIBUTE ID를 찾을 수 없었습니다 :" + ID);
    }

    /// <summary>
    /// 해당 영구 업그레이드가 몇 번 쌓였는지 가져옵니다.
    /// </summary>
    /// <param name="abilityAttribute"></param>
    /// <returns></returns>
    public int GetPermenentUpgradeCount(AbilityAttribute abilityAttribute)
    {
        int count = 0;
        for(int i = 0; i < permenentAttributes.Count; i++)
        {
            if (permenentAttributes[i].attribute == abilityAttribute)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 적용중인 일시 속성을 모두 제거합니다.
    /// </summary>
    public void ClearTempoaryAttribute()
    {
        timeAttributes.Clear();
    }

    /// <summary>
    /// 적용중인 ID가 붙어있는 속성을 모두 제거합니다
    /// </summary>
    public void ClearIDAttribute()
    {
        IDAttributes.Clear();
    }

    #endregion

    public void SailboatQuit()
    {
        CurrentMovement = new Movement_Ground();
    }

    protected override void Awake()
    {
        base.Awake();

        rBody = GetComponent<Rigidbody>();
        sound = GetComponent<StudioEventEmitter>();

        input = new MainPlayerInputActions();
        input.Player.Enable();
        input.Player.Sprint.performed += OnSprint;
        input.Player.Sprint.canceled += OnSprintEnd;
        input.Player.Jump.performed += OnJump;
        input.Player.ToggleSailboat.performed += OnToggleSailboat;
        input.Player.SailboatBooster.performed += OnBoosterStart;
        input.Player.SailboatLeapup.performed += OnLeapupStart;

        CurrentMovement = new Movement_Ground();

        permenentAttributes = new List<AbilityAttributeUnit>();
        timeAttributes = new List<AbilityAttributeUnit>();
        IDAttributes = new List<AbilityAttributeUnit>();

    }

    private void OnEnable()
    {
        var em = sailingSwooshEffect.emission;
        em.rateOverTimeMultiplier = 0f;

        groundNormal = Vector3.up;
    }

    private void Start()
    {
        initialRigidbodyDrag = rBody.drag;
        headRigForward = headTarget.localPosition;
        previousVelocity = rBody.velocity;

        layerIndex_Swim = animator.GetLayerIndex("SwimLayer");
        layerIndex_Boarding = animator.GetLayerIndex("BoardingLayer");
        layerIndex_ItemHolding = animator.GetLayerIndex("ItemHoldingLayer");
    }

    private float headRigTarget = 0.7f;

    private void FixedUpdate()
    {
        if (!Rigidbody.isKinematic)
        {
            // =================== CURRENT MOVEMENT FIXED UPDATE =========================
            CurrentMovement.OnFixedUpdate(this);
            // =================== CURRENT MOVEMENT FIXED UPDATE =========================
        }

        if (interestPoint == null)
        {
            headTarget.parent = transform;
            headRig.weight = Mathf.Lerp(headRig.weight, 0f, 0.05f);
            headTarget.localPosition = Vector3.Lerp(headTarget.localPosition, headRigForward, 0.05f);
        }
        else
        {
            headTarget.parent = null;
            headRig.weight = Mathf.Lerp(headRig.weight, headRigTarget, 0.05f);
            headTarget.position = Vector3.Lerp(headTarget.position, interestPoint.position, 0.05f);


            if (Vector3.Distance(transform.position, interestPoint.position) > interestDistance)
                interestPoint = null;
        }


        //이전 프레임의 플레이어 속도
        Vector3 currentVelocity = rBody.velocity;

        // 이전 프레임과 현재 프레임의 속도를 비교하여 속도의 변화를 확인합니다.
        Vector3 velocityChange = currentVelocity - previousVelocity;

        // 1프레임 전의 속도를 출력합니다.
        //Debug.Log("1프레임 전의 속도: " + previousVelocity.magnitude);

        // 현재 프레임의 속도를 이전 프레임의 속도로 업데이트합니다.
        previousVelocity = currentVelocity;
    }

    private void Update()
    {
        // Sweeptest process
        List<RaycastHit> groundHits_notFiltered = rBody.SweepTestAll(Vector3.down,groundCastDistance,QueryTriggerInteraction.Ignore).ToList();

        if (groundHits_notFiltered.Count == 0)
        {
            groundNormal = Vector3.up;
            grounding = false;
        }
        else
        {
            // get nearest groundhit from sweeptest
            groundHits_notFiltered.RemoveAll((RaycastHit h) => (1 << h.collider.gameObject.layer & groundIgnore) == 1);
            Vector3 groundNormal_nonDamped = groundHits_notFiltered.ToList()[0].normal;
            groundNormal = Vector3.Lerp(groundNormal, groundNormal_nonDamped, 0.25f);

            if (!grounding) OnGroundingEnter();

            grounding = true;

        }

        if (grounding)
        {
            animator.SetBool("Grounding", true);
        }
        else
        {
            animator.SetBool("Grounding", false);
            if (rBody.velocity.y > 0) animator.SetFloat("AirboneBlend", 0f, 0.5f, Time.deltaTime);
            else animator.SetFloat("AirboneBlend", 1f, 0.5f, Time.deltaTime);
        }

        // Movement state change condition
        if (buoyant.SubmergeRateZeroClamped < -0.1f)
        {
            if (CurrentMovement.GetType() == typeof(Movement_Ground) && !grounding)
            {
                if (rBody.velocity.y < -0.5f)
                {
                    if (GlobalOceanManager.IsInstanceValid)
                    {
                        Instantiate(normalSplashEffectPrefab, new Vector3(transform.position.x,GlobalOceanManager.Instance.GetWaveHeight(transform.position),transform.position.z), Quaternion.identity);
                    }
                    RuntimeManager.PlayOneShot(sound_splash);
                }
                CurrentMovement = new Movement_Swimming();
            }
        }
        if (buoyant.SubmergeRateZeroClamped >= -0.1f)
        {
            if (CurrentMovement.GetType() == typeof(Movement_Swimming))
            {
                CurrentMovement = new Movement_Ground();
            }
        }

        // =================== CURRENT MOVEMENT UPDATE =========================
        CurrentMovement.OnUpdate(this);
        // =================== CURRENT MOVEMENT UPDATE =========================

        // animation & audio controls
        if (CurrentMovement.GetType() == typeof(Movement_Swimming))
            animator.SetLayerWeight(layerIndex_Swim, Mathf.Lerp(animator.GetLayerWeight(layerIndex_Swim), 1.0f, Time.deltaTime * 20f * 0.2f));
        else
            animator.SetLayerWeight(layerIndex_Swim, Mathf.Lerp(animator.GetLayerWeight(layerIndex_Swim), 0.0f, Time.deltaTime * 20f * 0.2f));

        if (CurrentMovement.GetType() == typeof(Movement_Sailboat))
        {
            animator.SetLayerWeight(layerIndex_Boarding, Mathf.Lerp(animator.GetLayerWeight(layerIndex_Boarding), 1.0f, 0.2f));
        }
        else
        {
            animator.SetLayerWeight(layerIndex_Boarding, Mathf.Lerp(animator.GetLayerWeight(layerIndex_Boarding), 0.0f, 0.2f));

            float f;
            gustSound.EventInstance.getParameterByName("Speed", out f);
            gustSound.EventInstance.setParameterByName("Speed", Mathf.Lerp(f, 0f, 0.1f));
            waterScratchSound.EventInstance.getParameterByName("BoardWaterScratch", out f);
            waterScratchSound.EventInstance.setParameterByName("BoardWaterScratch", Mathf.Lerp(f, 0f, 0.1f));
        }

        // etc.


        if (IsHoldingSomething)
        {
            if (Input.Player.Interact.WasPressedThisFrame())
            {
                StartCoroutine(Cor_PlaceItem());
            }
        }

        // Time ability attribute update
        for (int ta = 0; ta < timeAttributes.Count; ta++)
        {
            timeAttributes[ta].time -= Time.deltaTime;
            if (timeAttributes[ta].time < 0)
            {
                timeAttributes.RemoveAt(ta);
                ta--;
            }
        }

        if (boosterRecharging)
        {
            boosterGauge += Time.deltaTime;
            UI_SailboatSkillInfo.Instance.SetBoosterRing(boosterGauge/boosterCooldown);

            if (boosterGauge > boosterCooldown)
            {
                boosterRecharging = false;
                boosterGauge = boosterCooldown;
                UI_SailboatSkillInfo.Instance.SetBoosterRing(1f);
                UI_SailboatSkillInfo.Instance.AnimateBoosterRing();
            }
        }

        if (CurrentMovement.GetType() == typeof(Movement_Sailboat))
        {
            if (sailboat.SubmergeRate < 1.0f && rBody.velocity.y < 0)
            {
                leapupRechargeTriggered = true;
            }
        }

        if(leapupRecharging && leapupRechargeTriggered)
        {
            leapupGauge += Time.deltaTime;
            UI_SailboatSkillInfo.Instance.SetLeapupRing(leapupGauge/leapupCooldown);

            if(leapupGauge > leapupCooldown)
            {
                leapupRecharging = false;
                leapupGauge = leapupCooldown;
                UI_SailboatSkillInfo.Instance.SetLeapupRing(1f);
                UI_SailboatSkillInfo.Instance.AnimateLeapupRing();
            }
        }

        if (CurrentMovement.GetType() != typeof(Movement_Sailboat))
        {
            sailboatEngineSound.EventInstance.setParameterByName("SailboatEngine", 0f);
        }

        // info update
#if UNITY_EDITOR
        if (CurrentMovement.GetType() == typeof(Movement_Ground)) current_move_debug = "GROUND";
        else if (CurrentMovement.GetType() == typeof(Movement_Swimming)) current_move_debug = "SWIMMING";
        else if (CurrentMovement.GetType() == typeof(Movement_Sailboat)) current_move_debug = "SAILBOAT";

        if (input.Player.enabled) control_disabled_debug = true;
        else control_disabled_debug = false;

        velocity_hor_debug = Vector3.ProjectOnPlane(rBody.velocity, Vector3.up).magnitude;
        if (currentHoldingItem != null)
            current_holding_item_debug = currentHoldingItem.gameObject.name;
        else
            current_holding_item_debug = "NULL";

        control_disableStack_debug = disableStack;
#endif
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        velocity_debug = rBody.velocity;
        velocity_mag_debug = rBody.velocity.magnitude;
#endif
    }


    #region ================ MovementStates ================

    //============================================
    //
    // MovementStates는 플레이어의 현재 행동을 나타내는 state패턴의 클래스들 입니다.
    // CurrentMovement를 통해 현재 플레이어의 움직임 state를 변경할 수 있습니다.
    // CurrentMovement가 바뀌면 이전 state의 OnMovementExit가 호출되고 바뀔 state의 OnMovementEnter가 호출됩니다.
    //
    //============================================

    protected class MovementState
    {
        /// <summary>
        /// 해당 state로 들어올 때 이 함수가 호출됩니다.
        /// </summary>
        /// <param name="player"> 플레이어 인스턴스 </param>
        public virtual void OnMovementEnter(PlayerCore @player) { }
        /// <summary>
        /// Update 루프 때 이 함수가 호출됩니다
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnUpdate(PlayerCore @player) { }
        /// <summary>
        /// FixedUpdate 루프 때 이 함수가 호출됩니다.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnFixedUpdate(PlayerCore @player) { }
        /// <summary>
        /// 해당 state에서 나가라 때 이 함수가 호출됩니다.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnMovementExit(PlayerCore @player) { }
    }

    float slopeResistence = 1f;


/// <summary>
/// 플레이어가 땅 위를 뛰어다니는 상태일 때
/// </summary>
    protected class Movement_Ground : MovementState
    {
        bool sliding = false;
        float idleAnimationtime = 30f;
        float IdleTimer = 0f;
        private float GetSlopeForwardInterpolation(PlayerCore player,Vector3 forward)
        {
            return Mathf.InverseLerp(player.slopeEffect.x / 90f, player.slopeEffect.y / 90f, Vector3.Dot(forward, Vector3.up));
        }

        public override void OnMovementEnter(PlayerCore player)
        {
            player.movementStateRefernce = "Ground";
        }

        public override void OnFixedUpdate(PlayerCore player)
        {
            base.OnFixedUpdate(player);

            if (player.buoyant.SubmergeRateZeroClamped < 0)
            {
                player.rBody.AddForce(Vector3.up * player.swimUpforce, ForceMode.Acceleration);
            }

            if(sliding)
            {

            }
            else if (player.input.Player.Move.IsPressed())
            {
                //forward velocity
                Vector3 lookTransformedVector = player.GetLookMoveVector(player.input.Player.Move.ReadValue<Vector2>(), Vector3.up);
                Vector3 slopedMoveVector = Vector3.ProjectOnPlane(lookTransformedVector, player.groundNormal).normalized;
                //player.slopeResistence = 1f - Mathf.InverseLerp(player.slopeEffect.x / 90f, player.slopeEffect.y / 90f,Vector3.Dot(slopedMoveVector, Vector3.up));

                float adjuestedScale = (player.sprinting && player.grounding) ? player.sprintSpeedMult : 1.0f;

                Vector3 finalVelocity = slopedMoveVector * adjuestedScale * player.slopeResistence * player.FinalMoveSpeed * ((player.currentHoldingItem == null)?1.0f:player.holdingMoveSpeedMult);
                if (player.buoyant.WaterDetected)
                {
                    finalVelocity = finalVelocity * (1f - Mathf.Lerp(0.5f, 0f, player.buoyant.SubmergeRate) * player.waterWalkDragging);
                }

                player.rBody.velocity = new Vector3(finalVelocity.x, player.rBody.velocity.y, finalVelocity.z);

                bool LargeTurn = Quaternion.Angle(player.transform.rotation, Quaternion.LookRotation(lookTransformedVector, Vector3.up)) > 60f;

                player.transform.rotation = Quaternion.RotateTowards(
                    player.transform.rotation,
                    Quaternion.LookRotation(lookTransformedVector, Vector3.up),
                    LargeTurn ? 30f : 10f
                );

                if (player.buoyant.WaterDetected)
                    player.animator.speed = 1f - Mathf.Lerp(0.5f, 0f, player.buoyant.SubmergeRate) * player.waterWalkDragging;
                else 
                    player.animator.speed = 1.0f;

                player.animator.SetBool("MovementInput", true);

                IdleTimer = 0f;
            }
            else
            {
                player.rBody.velocity = Vector3.Lerp(player.rBody.velocity, new Vector3(0f, player.rBody.velocity.y, 0f), player.horizontalDrag / 0.2f);
                player.animator.SetBool("MovementInput", false);

                IdleTimer += Time.fixedDeltaTime;

                if (IdleTimer > idleAnimationtime)
                {
                    player.animator.SetTrigger("IdleAnimation");
                    IdleTimer = 0f;
                    idleAnimationtime = Random.Range(25f, 40f);
                }

            }
        }

        public override void OnUpdate(PlayerCore player)
        {
            base.OnUpdate(player);
            if (player.sprinting) player.animator.SetFloat("RunBlend", 1f, 0.1f, Time.deltaTime);
            else player.animator.SetFloat("RunBlend", 0f, 0.1f, Time.deltaTime);
        }

        public override void OnMovementExit(PlayerCore player)
        {
            base.OnMovementExit(player);
            player.ReleaseHoldingItem();
        }
    }

    readonly float waterjumpInterval = 1f;
    float waterjumpTimer = 0f;

    /// <summary>
    /// 플레이어가 수영중인 상황일 때
    /// </summary>
    protected class Movement_Swimming : MovementState
    {
        public override void OnMovementEnter(PlayerCore player)
        {
            player.rBody.drag = player.swimRigidbodyDrag;
            base.OnMovementEnter(player);
            player.animator.SetBool("Swimming", true);
            player.animator.SetTrigger("SwimmingEnter");
            player.movementStateRefernce = "Swimming";
        }

        public override void OnFixedUpdate(PlayerCore player)
        {
            base.OnFixedUpdate(player);
            player.waterjumpTimer += Time.fixedDeltaTime;

            if (player.buoyant.SubmergeRateZeroClamped < 0)
            {
                player.rBody.AddForce(Vector3.up * player.swimUpforce * (0.5f + Mathf.Sin(Time.time) / 2f));
            }

            if (player.input.Player.Move.IsPressed())
            {
                Vector3 lookTransformedVector = player.GetLookMoveVector(player.input.Player.Move.ReadValue<Vector2>(), Vector3.up);

                Vector3 finalVelocity = lookTransformedVector * player.swimSpeed;
                player.rBody.velocity = new Vector3(finalVelocity.x, player.rBody.velocity.y, finalVelocity.z);

                player.transform.rotation = Quaternion.RotateTowards(
                    player.transform.rotation,
                    Quaternion.LookRotation(lookTransformedVector, Vector3.up),
                    5f
                );
                player.animator.SetBool("Swimming_Move", true);
            }
            else
            {
                player.rBody.velocity = Vector3.Lerp(player.rBody.velocity, new Vector3(0, player.rBody.velocity.y, 0f), player.horizontalDrag);
                player.animator.SetBool("Swimming_Move", false);
            }
        }

        public override void OnMovementExit(PlayerCore player)
        {
            player.animator.SetBool("Swimming", false);
            player.rBody.drag = player.initialRigidbodyDrag;
            base.OnMovementExit(player);
        }
    }

    /// <summary>
    /// 플레이어가 조각배를 타는 상황일 때
    /// </summary>
    protected class Movement_Sailboat : MovementState
    {
        Vector3 directionCache;
        float GustAmount = 0.0f;
        bool enterFlag = false;
        float driftAngle = 0f;
        float driftAngleMax = 90f;
        float driftTime = 0f;
        bool driftChargeFlag = false;
        Vector3 driftDirection = Vector3.zero;

        public override void OnMovementEnter(PlayerCore player)
        {
            directionCache = player.transform.forward;
            base.OnMovementEnter(player);
            player.sailboat.gameObject.SetActive(true);
            player.sailboatEngineSound.EventInstance.setParameterByName("SailboatEngine", 0f);
            player.sailboatFootRig.weight = 1.0f;
            player.buoyant.enabled = false;
            player.rBody.useGravity = false;
            player.animator.SetBool("Boarding", true);
            player.animator.SetTrigger("BoardingEnter");
            player.animator.SetFloat("BoardBlend", 0.0f);
            UI_SailboatSkillInfo.Instance.ToggleInfo(true);
            player.movementStateRefernce = "Sailboat";
        }

        private Vector3 GetSailboatHeadingVector(PlayerCore player, Vector3 input, Vector3 up)
        {

            Vector3 lookTransformedVector;

            if (player.boosterActive)
            {
                lookTransformedVector = Vector3.RotateTowards(player.transform.forward, Camera.main.transform.TransformDirection(new Vector3(input.x, 0f, 1.0f)), player.FinalSteering, 1.0f);
            }
            if (player.DriftActive)
            {
                lookTransformedVector = Vector3.RotateTowards(player.transform.forward, Camera.main.transform.TransformDirection(new Vector3(input.x, 0f, Mathf.Clamp(input.y, 0.5f, 1.0f))), player.FinalSteering, 1.0f);
            }
            else
            {
                lookTransformedVector = Vector3.RotateTowards(player.transform.forward, Camera.main.transform.TransformDirection(new Vector3(input.x, 0f, input.y)), player.FinalSteering, 1.0f);
            }

            //}
            lookTransformedVector = Vector3.ProjectOnPlane(lookTransformedVector, up);
            return lookTransformedVector;
        }
        
        public override void OnUpdate(PlayerCore player)
        {
            if (player.input.Player.SailboatDrift.WasPressedThisFrame() && player.sailboat.SubmergeRate < 5.0f &&player.input.Player.Move.ReadValue<Vector2>().x != 0)
            {
                //player.driftActive = true;
                driftDirection = new Vector3(player.input.Player.Move.ReadValue<Vector2>().x > 0 ? 1f : -1,0f,0f);
            }

            if (player.driftActive && player.input.Player.SailboatDrift.WasReleasedThisFrame())
            {
                player.driftActive = false;

                if (driftTime > player.driftKickRequireingTime)
                {
                    player.rBody.AddForce(player.sailboatModelPivot.forward * player.driftKickPower, ForceMode.VelocityChange);
                    RuntimeManager.PlayOneShot(player.sound_driftKick);
                }
                driftTime = 0f;
                driftChargeFlag = false;
            }

            Vector2 moveInput = player.input.Player.Move.ReadValue<Vector2>();

            if (player.driftActive)
            {
                if(moveInput.x > 0)
                {
                    driftDirection = new Vector3(1f, 0f, 0f);
                }
                else if(moveInput.x < 0)
                {
                    driftDirection = new Vector3(-1f, 0f, 0f);
                }

                float f;


                driftAngle = Mathf.Lerp(driftAngle, driftAngleMax * moveInput.x, player.driftSteer);
                driftTime += Time.deltaTime;

                if(driftTime > player.driftKickRequireingTime)
                {
                    if (!driftChargeFlag)
                    {
                        driftChargeFlag = true;
                        RuntimeManager.PlayOneShot(player.sound_driftCharged);
                    }

                    player.driftSound.EventInstance.getParameterByName("Drift", out f);
                    player.driftSound.EventInstance.setParameterByName("Drift", Mathf.Lerp(f, 1.0f, 0.1f));
                }
                else
                {
                    player.driftSound.EventInstance.getParameterByName("Drift", out f);
                    player.driftSound.EventInstance.setParameterByName("Drift", Mathf.Lerp(f, 0.5f, 0.1f));
                }
            }
            else
            {
                float f;
                player.driftSound.EventInstance.getParameterByName("Drift", out f);

                player.driftSound.EventInstance.setParameterByName("Drift", Mathf.Lerp(f, 0.0f, 0.1f));

                driftAngle = Mathf.Lerp(driftAngle, 0f, player.driftSteer);
            }

            player.animator.SetFloat("Board_X", moveInput.x, 0.3f, Time.deltaTime);
            player.animator.SetFloat("Board_Y", moveInput.y, 0.3f, Time.deltaTime);

            if (player.sailboat.SubmergeRate < player.leapupAvailHeight)
            {
                UI_SailboatSkillInfo.Instance.SetLeapupAvailable(true);
            }
            else
            {
                UI_SailboatSkillInfo.Instance.SetLeapupAvailable(false);
            }

        }

        public override void OnFixedUpdate(PlayerCore player)
        {
            base.OnFixedUpdate(player);

            SailboatBehavior sailboat = player.sailboat;
            GustAmount = Mathf.InverseLerp(player.gustStartVelocity, player.gustMaxVelocity, Vector3.ProjectOnPlane(player.rBody.velocity, Vector3.up).magnitude);

            float ns_boost = sailboat.SubmergeRate < player.sailboatNearsurf && sailboat.SubmergeRate > -0.5f ? player.sailboatNearsurfBoost : 1.0f;

            Vector2 moveInput = player.input.Player.Move.ReadValue<Vector2>();


            if (player.sailboat.SubmergeRate < -1.5f)
            {
                player.rBody.drag = player.sailboatFullDrag;
                player.rBody.AddForce(Vector3.up * -Mathf.Clamp(sailboat.SubmergeRate, -5.0f, 0.0f) / 3f * player.sailboatByouancy, ForceMode.Acceleration);

                Vector3 lookTransformedVector;

                lookTransformedVector = GetSailboatHeadingVector(player, moveInput, player.sailboat.SurfacePlane.normal);

                player.rBody.AddForce(lookTransformedVector * player.FinalSailboatAcceleration);

            }
            else if (player.sailboat.SubmergeRate < 0.5f)
            {
                player.rBody.drag = player.sailboatScratchDrag;
                player.rBody.AddForce(Vector3.up * -Mathf.Clamp(sailboat.SubmergeRate, -1.0f, 0.0f) * player.sailboatByouancy, ForceMode.Acceleration);
                player.rBody.AddForce(Vector3.ProjectOnPlane(sailboat.SurfacePlane.normal, Vector3.up) * player.sailboatSlopeInfluenceForce, ForceMode.Acceleration);

                Vector3 lookTransformedVector;
                lookTransformedVector = GetSailboatHeadingVector(player, moveInput, player.sailboat.SurfacePlane.normal);

                player.rBody.AddForce(lookTransformedVector * player.FinalSailboatAcceleration * ns_boost, ForceMode.Acceleration);

                if (!enterFlag)
                {
                    enterFlag = true;
                    if (player.rBody.velocity.y < -1f)
                    {
                        RuntimeManager.PlayOneShot(player.sound_splash);
                        if (player.rBody.velocity.ProjectOntoPlane(Vector3.up).magnitude > 10f)
                        {
                            if (!player.sailingSplashEffect_HighVel.isPlaying)
                                player.sailingSplashEffect_HighVel.Play(true);
                        }
                        else
                        {
                            if (GlobalOceanManager.IsInstanceValid)
                            {
                                Instantiate(player.normalSplashEffectPrefab, new Vector3(player.transform.position.x, GlobalOceanManager.Instance.GetWaveHeight(player.transform.position), player.transform.position.z), Quaternion.identity);
                            }
                        }
                    }
                }
            }
            else
            {
                enterFlag = false;

                if (!player.Grounding)
                {
                    player.rBody.drag = player.sailboatGlidingDrag;

                    Vector3 lookTransformedVector;
                    //if (player.boosterActive)
                    //    lookTransformedVector = player.GetLookMoveVector(new Vector2(moveInput.x, 1f), Vector3.up);
                    //else if (player.driftActive)
                    //    lookTransformedVector = player.GetLookMoveVector(new Vector2(moveInput.x, Mathf.Clamp(moveInput.y, 0.5f, 1.0f)), Vector3.up);
                    //else
                    //    lookTransformedVector = player.GetLookMoveVector(moveInput, Vector3.up);

                    lookTransformedVector = GetSailboatHeadingVector(player, moveInput, Vector3.up);

                    player.rBody.AddForce(lookTransformedVector * player.FinalSailboatAcceleration * ns_boost, ForceMode.Acceleration);
                }


                player.rBody.AddForce(Vector3.up * -Mathf.Clamp(sailboat.SubmergeRate, 0f, 1f) * player.sailboatGravity, ForceMode.Acceleration);
            }


            if (Vector3.ProjectOnPlane(player.rBody.velocity, Vector3.up).magnitude > 5.0f)
            {
                Vector3 pivotEuler = Vector3.zero;

                if (player.input.Player.SailboatForward.IsPressed())
                {
                    if (!player.driftActive)
                    {
                        player.rBody.AddForce(Vector3.up * player.sailboatVerticalControl);

                        pivotEuler = new Vector3(-35f, 0f, 0f);
                    }
                }
                else if (player.input.Player.SailboatBackward.IsPressed())
                {
                    if (!player.driftActive)
                    {
                        player.rBody.AddForce(Vector3.down * player.sailboatVerticalControl);

                        pivotEuler = new Vector3(10f, 0f, 0f);
                    }
                }
                else
                {
                    pivotEuler = new Vector3(0f, 0f, 0f);
                }

                if (player.input.Player.Move.IsPressed())
                {
                    sailboat.transform.rotation = Quaternion.Slerp(sailboat.transform.rotation,
                        Quaternion.LookRotation(player.rBody.velocity, sailboat.SurfacePlane.normal),
                        0.1f);

                    Vector3 lookTransformedVector = player.GetLookMoveVector(player.input.Player.Move.ReadValue<Vector2>(), Vector3.up);
                    if (player.driftActive) lookTransformedVector = GetSailboatHeadingVector(player,driftDirection, Vector3.up);
                    float lean = Vector3.Dot(lookTransformedVector, player.transform.right);
                    if (player.driftActive) lean = lean * 1.5f;

                    pivotEuler = pivotEuler + new Vector3(0f, 0f, -lean * 30f);

                    directionCache = Vector3.ProjectOnPlane(player.rBody.velocity, Vector3.up);
                }
                else
                {
                    sailboat.transform.rotation = Quaternion.LookRotation(directionCache, sailboat.SurfacePlane.normal);
                }

                pivotEuler = pivotEuler + new Vector3(0f,driftAngle,0f);

                player.sailboatModelPivot.localRotation = Quaternion.Slerp(player.sailboatModelPivot.localRotation, Quaternion.Euler(pivotEuler), 0.1f);
            }
            else
            {
                sailboat.transform.rotation = Quaternion.LookRotation(directionCache, sailboat.SurfacePlane.normal);
            }


            player.transform.forward = Vector3.ProjectOnPlane(sailboat.transform.forward, Vector3.up);


            if (player.rBody.velocity.magnitude > 10f && sailboat.SubmergeRate < 1f)
            {
                Vector3 pos = player.sailingSprayEffect.transform.position;
                float surfaceHeight = GlobalOceanManager.Instance.GetWaveHeight(pos);
                player.sailingSprayEffect.transform.position = new Vector3(pos.x,surfaceHeight,pos.z);
                player.sailingSprayEffect.transform.rotation.SetLookRotation(directionCache, sailboat.SurfacePlane.normal);


                if (!player.sailingSprayEffect.isPlaying)
                {
                    player.sailingSprayEffect.Play(true);
                }
            }
            else
            {
                if (player.sailingSprayEffect.isPlaying)
                    player.sailingSprayEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            }

            //Vector3 wavePosition = player.sailingFrontwaveEffect.transform.position;
            //if (player.rBody.velocity.magnitude > 10f && sailboat.SubmergeRate < 1.0f)
            //{
            //    player.sailingFrontwaveEffect.Play(true);
            //    player.sailingFrontwaveEffect.transform.position = new Vector3(wavePosition.x, GlobalOceanManager.Instance.GetWaveHeight(wavePosition), wavePosition.z);
            //    player.sailingFrontwaveEffect.transform.up = player.sailboat.SurfacePlane.normal;
            //    player.sailingFrontwaveEffect.transform.forward = player.transform.forward;
            //}
            //else
            //{
            //    player.sailingFrontwaveEffect.Play(false);
            //    player.sailingFrontwaveEffect.transform.position = new Vector3(wavePosition.x, GlobalOceanManager.Instance.GetWaveHeight(wavePosition), wavePosition.z);
            //    player.sailingFrontwaveEffect.transform.up = player.sailboat.SurfacePlane.normal;
            //    player.sailingFrontwaveEffect.transform.forward = player.transform.forward;
            //}

            player.animator.SetFloat("BoardBlend", player.rBody.velocity.y);

            float value = player.sailboatEngineSound.Params[0].Value;

            if (player.input.Player.Move.IsPressed())
            {
                if (player.boosterActive)
                    player.sailboatEngineSound.EventInstance.setParameterByName("SailboatEngine", 1f);
                else
                    player.sailboatEngineSound.EventInstance.setParameterByName("SailboatEngine", Mathf.Clamp(player.rBody.velocity.magnitude / 40f, 0f, 0.8f));

                player.animator.SetFloat("BoardPropellingBlend", 1f, 1f, Time.fixedDeltaTime);
            }
            else
            {
                player.sailboatEngineSound.EventInstance.setParameterByName("SailboatEngine", 0f);
                player.animator.SetFloat("BoardPropellingBlend", 0f, 1f, Time.fixedDeltaTime);
            }

            player.waterScratchSound.EventInstance.setParameterByName("BoardWaterScratch", Mathf.InverseLerp(0.5f, -0.5f, player.sailboat.SubmergeRate) * GustAmount * 1.5f);

            var em = player.sailingSwooshEffect.emission;
            em.rateOverTimeMultiplier = GustAmount * 3f;

            player.gustSound.EventInstance.setParameterByName("Speed", GustAmount);
        }

        public override void OnMovementExit(PlayerCore player)
        {
            base.OnMovementExit(player);
            player.AbortBooster();
            player.sailingSprayEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            player.sailboat.gameObject.SetActive(false);
            player.sailboatFootRig.weight = 0.0f;
            player.buoyant.enabled = true;
            player.rBody.useGravity = true;
            player.driftActive = false;
            player.rBody.drag = player.initialRigidbodyDrag;
            player.animator.SetBool("Boarding", false);
            player.animator.SetFloat("BoardPropellingBlend", 0f);
            player.animator.SetFloat("Board_X", 0f);
            player.animator.SetFloat("Board_Y", 0f);
            player.driftSound.EventInstance.setParameterByName("Drift", 0f);
            UI_SailboatSkillInfo.Instance.ToggleInfo(false);
            UI_SailboatSkillInfo.Instance.SetLeapupAvailable(true);

            var em = player.sailingSwooshEffect.emission;
            em.rateOverTimeMultiplier = 0f;
        }
    }

    #endregion

    #region ================ InputCallbacks ================
    // InputSystem 입력 이벤트

    private void OnToggleSailboat(InputAction.CallbackContext context)
    // @ "조각배소환" 버튼
    {
        if (CurrentMovement.GetType() != typeof(Movement_Sailboat))
        {
            if (buoyant.WaterDetected && buoyant.SubmergeRate < 0.5f)
                CurrentMovement = new Movement_Sailboat();
        }
        else
        {
            CurrentMovement = new Movement_Ground();
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    // @ "점프" 버튼
    {
        if (CurrentMovement.GetType() == typeof(Movement_Ground) && grounding)
        {
            float groundNormAngle = Vector3.Angle(groundNormal, Vector3.up);
            if (groundNormAngle < slopeEffect.y)
            {
                rBody.velocity += Vector3.up * jumpPower * Mathf.InverseLerp(slopeEffect.y, slopeEffect.x, groundNormAngle);
                jumpEffect.Emit((int)jumpEffect.emission.GetBurst(0).count.constant);
                animator.SetFloat("AirboneBlend", 0f);
                PlayFootstepSound();
            }
        }
        else if (CurrentMovement.GetType() == typeof(Movement_Swimming))
        {
            if(buoyant.SubmergeRate < 0f && buoyant.SubmergeRate > -1f && waterjumpTimer > waterjumpInterval)
            {
                waterjumpTimer = 0f;
                rBody.velocity += Vector3.up * jumpPower;
                Instantiate(normalSplashEffectPrefab, new Vector3(transform.position.x, GlobalOceanManager.Instance.GetWaveHeight(transform.position), transform.position.z), Quaternion.identity);
            }
        }
    }

    private void OnSprint(InputAction.CallbackContext context)
    // @ "달리기" 버튼
    {
        sprinting = true;

    }

    private void OnSprintEnd(InputAction.CallbackContext context)
    {
        sprinting = false;
    }

    private void OnBoosterStart(InputAction.CallbackContext context)
    {
        if (CurrentMovement.GetType() != typeof(Movement_Sailboat)) return;
        if (boosterRecharging) return;
        if (boosterCoroutine != null) return;
        if (driftActive) return;

        boosterCoroutine = StartCoroutine(Cor_Booster());

    }

    private void OnLeapupStart(InputAction.CallbackContext context)
    {
        if (CurrentMovement.GetType() != typeof(Movement_Sailboat)) return;
        if (leapupRecharging) return;
        if (sailboat.SubmergeRate > leapupAvailHeight) return;
        if (leapupCoroutine != null) return;
        if (driftActive) return;

        animator.SetTrigger("Leapup");
        leapupCoroutine = StartCoroutine(Cor_Leapup());
    }

    #endregion

    Coroutine boosterCoroutine;
    Coroutine leapupCoroutine;
    float boosterGauge = 0f;
    float leapupGauge = 0f;
    bool boosterRecharging = false;
    bool leapupRecharging = false;
    bool leapupRechargeTriggered = false;

    public void AbortBooster()
    {
        if (boosterCoroutine == null) return;

        CancelAttributeWithID("SailboatBooster");

        StopCoroutine(boosterCoroutine);
        boosterCoroutine = null;

        boosterRecharging = true;
        SpeedLineControl.Instance.SetSpeedLine(0.0f, 0.5f);
        animator.SetBool("Booster", false);
        boosterActive = false;
    }

    IEnumerator Cor_Booster()
    {
        boosterGauge = 1f;
        animator.SetBool("Booster", true);

        SetAttributeWithID(AbilityAttribute.SailboatAcceleration, FinalBoosterMult, "SailboatBooster");

        boosterActive = true;

        for(float t = FinalBoosterDuration; t > 0; t -= Time.deltaTime)
        {
            boosterGauge = t / FinalBoosterDuration;
            UI_SailboatSkillInfo.Instance.SetBoosterRing(boosterGauge);
            SpeedLineControl.Instance.SetSpeedLine(Mathf.Clamp01(rBody.velocity.magnitude / 40f)*2.0f);
            yield return null;
        }

        boosterActive = false;

        SpeedLineControl.Instance.SetSpeedLine(0.0f, 0.5f);
        animator.SetBool("Booster", false);
        boosterRecharging = true;
        CancelAttributeWithID("SailboatBooster");
        boosterCoroutine = null;
    }

    IEnumerator Cor_Leapup()
    {
        leapupGauge = 1f;
        
        leapupActive = true;
        animator.SetBool("Leapup", true);


        for (float t = leapupDuration; t > 0; t -= Time.fixedDeltaTime)
        {
            rBody.AddForce(Vector3.up * FinalLeapupPower * leapupForceCurve.Evaluate(1 - (t/leapupDuration)) , ForceMode.VelocityChange);
            leapupGauge = t / leapupDuration;
            UI_SailboatSkillInfo.Instance.SetLeapupRing(leapupGauge);
            yield return new WaitForFixedUpdate();
        }

        animator.SetBool("Leapup", false);

        leapupActive = false;
        leapupRecharging = true;
        leapupRechargeTriggered = false;

        leapupCoroutine = null;
    }

/// <summary>
/// 플레이어가 얼굴을 향하는 방향을 target으로 맞춥니다.
/// </summary>
/// <param name="target"></param>
    public void SetInterestPoint(Transform target)
    {
        interestPoint = target;
    }

    bool holdItemCoroutineFlag = false;

    public void EnableIsamel()
    {
        IsmaelSpiritObject.SetActive(true);
        IsmaelSpiritAnimator.SetBool("IsmaelActive", true);
        SetInterestPoint(IsmaelSpiritLookTarget);
    }
    public void DisableIsmael()
    {
        IsmaelSpiritAnimator.SetBool("IsmaelActive", false);
        SetInterestPoint(null);
        Invoke("DisableIsmaelDelayed", 1.0f);
    }

    private void DisableIsmaelDelayed()
    {
        IsmaelSpiritObject.SetActive(false);
    }

/// <summary>
///     //Interactable_Holding과 함께 사용합니다. 아이템을 듭니다.
/// </summary>
/// <param name="leftHand"> 왼손 짚는 위치 </param>
/// <param name="rightHand"> 오른손 짚는 위치 </param>
/// <param name="holdingItem"> 잡는 아이템 </param>
/// <returns></returns>
    public bool HoldItem(Transform leftHand, Transform rightHand,Interactable_Holding holdingItem)
    {
        if (holdItemCoroutineFlag) return false;

        if (currentHoldingItem != null) {
            return false; 
        }
        else
        {
            StartCoroutine(Cor_HoldItem(leftHand, rightHand, holdingItem));
            return true;
        }
    }

    float holdItemAnimTime = 1.0f;
    float releaseItemAnimTime = 1.0f;
    float itemAnimationTime = 1.0f;

    private IEnumerator Cor_HoldItem(Transform leftHand, Transform rightHand, Interactable_Holding holdingItem)
    {
        bool inputWasEnabled = Input.Player.enabled;
        Input.Player.Disable();
        holdItemCoroutineFlag = true;
        animator.SetTrigger("ItemHold");

        yield return new WaitForSeconds(holdItemAnimTime / 2f);

        leftHandTarget.SetParent(leftHand, false);
        rightHandTarget.SetParent(rightHand, false);
        holdingItem.transform.parent = holdingItemTarget;
        handRig.weight = 1.0f;

        animator.SetLayerWeight(layerIndex_ItemHolding, 0f);

        for (float t = 0; t < itemAnimationTime / 2f; t += Time.deltaTime)
        {
            animator.SetLayerWeight(layerIndex_ItemHolding, t*2f);
            holdingItem.transform.localPosition = Vector3.Lerp(holdingItem.transform.localPosition, Vector3.zero, 0.4f);
            holdingItem.transform.localRotation = Quaternion.Lerp(holdingItem.transform.localRotation, Quaternion.Euler(Vector3.zero), 0.4f);
            holdObjectRig.weight = Mathf.InverseLerp(0,itemAnimationTime*0.45f,t);
            yield return null;
        }

        if (inputWasEnabled)
            Input.Player.Enable();
        holdObjectRig.weight = 0.9f;

        holdItemCoroutineFlag = false;
        currentHoldingItem = holdingItem;
    }

    private IEnumerator Cor_PlaceItem()
    {
        if (currentHoldingItem == null) yield break;

        bool inputWasEnabled = Input.Player.enabled;
        Input.Player.Disable();
        handRig.weight = 0.0f;
        animator.SetTrigger("ItemRelease");
        holdItemCoroutineFlag = true;
        currentHoldingItem.Release();

        yield return new WaitForSeconds(releaseItemAnimTime / 2f);

        currentHoldingItem.transform.parent = null;
        animator.SetLayerWeight(layerIndex_ItemHolding, 1f);

        for (float t = itemAnimationTime / 2f; t > 0; t -= Time.deltaTime)
        {
            holdObjectRig.weight = Mathf.InverseLerp(0, itemAnimationTime * 0.45f, t);
            yield return null;
        }

        if (inputWasEnabled)
            Input.Player.Enable();

        animator.SetLayerWeight(layerIndex_ItemHolding, 0f);

        currentHoldingItem = null;
        holdItemCoroutineFlag = false;
        handRig.weight = 0.0f;
        holdObjectRig.weight = 0.0f;

    }

    /// <summary>
    /// 현재 플레이어 Movestate를 즉시 바꿉니다.
    /// </summary>
    public void SetMovementState(PlayerMovementState state)
    {
        if (state == PlayerMovementState.Ground) CurrentMovement = new Movement_Ground();
        else if (state == PlayerMovementState.Swimming) CurrentMovement = new Movement_Swimming();
        else if (state == PlayerMovementState.Sailboat) CurrentMovement = new Movement_Sailboat();
    }

    /// <summary>
    /// 현재 플레이어 Movestate를 즉시 바꿉니다. (인덱스)
    /// </summary>
    /// <param name="state"></param>
    public void SetMovementState(int state)
    {
        if (state == (int)PlayerMovementState.Ground) CurrentMovement = new Movement_Ground();
        else if (state == (int)PlayerMovementState.Swimming) CurrentMovement = new Movement_Swimming();
        else if (state == (int)PlayerMovementState.Sailboat) CurrentMovement = new Movement_Sailboat();
    }

    /// <summary>
    /// 현재 들고있는 아이템이 있으면 즉시 놓습니다.
    /// </summary>
    public void ReleaseHoldingItem()
    {
        if (currentHoldingItem == null) return;

        currentHoldingItem.transform.parent = null;
        animator.SetLayerWeight(layerIndex_ItemHolding, 0f);
        currentHoldingItem.Release();
        currentHoldingItem = null;
        handRig.weight = 0.0f;
        holdObjectRig.weight = 0.0f;
    }

    int disableStack = 0;
    public int DisableStack { get { return disableStack; } }

    /// <summary>
    ///  시퀀스 시작시 플레이어의 조작을 비활성화하기 위한 함수.
    /// </summary>
    public void DisableControls()
    {
        if(disableStack <= 0)
        {
            input.Player.Disable();
            Cinemachine.CinemachineInputProvider cameraInputProvider = FindFirstObjectByType<Cinemachine.CinemachineInputProvider>();
            if (cameraInputProvider != null) { cameraInputProvider.enabled = false; }
            if (CurrentMovement.GetType() == typeof(Movement_Sailboat)) UI_SailboatSkillInfo.Instance.ToggleInfo(false);
        }

        disableStack++;
    }

    /// <summary>
    /// 시퀀스 종료시 플레이어의 조작을 활성화하기 위한 함수.
    /// </summary>
    public void EnableControls()
    {
        disableStack--;

        if (disableStack <= 0)
        {
            input.Player.Enable();
            Cinemachine.CinemachineInputProvider cameraInputProvider = FindFirstObjectByType<Cinemachine.CinemachineInputProvider>();
            if (cameraInputProvider != null) { cameraInputProvider.enabled = true; }
            if (CurrentMovement.GetType() == typeof(Movement_Sailboat)) UI_SailboatSkillInfo.Instance.ToggleInfo(true);
            disableStack = 0;
        }
    }

    /// <summary>
    /// 플레이어 방향 지시를 활성화합니다.
    /// </summary>
    /// <param name="target">목표 지점</param>
    public void EnableAndSetIndicator(Vector3 target)
    {
        directionIndicator.EnableAndSetIndicator(target);
    }

    public void OnFlowerPicked(bool value)
    {
        animator.SetBool("PickupBlossom",value);
        bellflowerLockPointAnimator.GetComponent<Animator>().SetBool("Pickup",true);
    }

    /// <summary>
    /// 플레이어 방향 지시를 활성화합니다.
    /// </summary>
    /// <param name="target">목표 지점</param>
    public void EnableAndSetIndicator(Transform target)
    {
        directionIndicator.EnableAndSetIndicator(target);
    }

    /// <summary>
    /// 플레이어 방향 지시를 끕니다.
    /// </summary>
    public void DisableIndicator() 
    {
        directionIndicator.DisableIndicator();
    }

    /// <summary>
    /// @ 애니메이션 용 이벤트 함수 : 플레이어가 발을 딛을 때 호출되는 함수.
    /// </summary>
    public void FootstepEvent()
    {
        if (CurrentMovement.GetType() == typeof(Movement_Ground))
            PlayFootstepSound();
    }

    private Vector3 GetLookMoveVector(Vector2 input, Vector3 up)
    {
        Vector3 lookTransformedVector;

        lookTransformedVector = Camera.main.transform.TransformDirection(new Vector3(input.x, 0f, input.y));

        lookTransformedVector = Vector3.ProjectOnPlane(lookTransformedVector, up).normalized;
        return lookTransformedVector;
    }

    private void OnGroundingEnter()
    {

    }

    private void PlayFootstepSound()
    {
        RaycastHit hit;
        Ray ray = new Ray(RCO_foot.position, Vector3.down);
        if (Physics.Raycast(RCO_foot.position, -groundNormal, out hit, groundCastDistance, ~groundIgnore))
        {
            SoundMaterialBehavior soundMaterialComp;
            SoundMaterial soundMaterial = SoundMaterial.Default;

            sound.EventInstance.setParameterByNameWithLabel("GroundMaterial", "Default");

            if (hit.collider.TryGetComponent(out soundMaterialComp))
            {
                soundMaterial = soundMaterialComp.GetSoundMaterial(RCO_foot.position);

                switch (soundMaterial)
                {
                    case SoundMaterial.Default:
                        sound.EventInstance.setParameterByNameWithLabel("GroundMaterial", "Default");
                        break;
                    case SoundMaterial.Sand:
                        sound.EventInstance.setParameterByNameWithLabel("GroundMaterial", "Sand");
                        break;
                    case SoundMaterial.Water:
                        sound.EventInstance.setParameterByNameWithLabel("GroundMaterial", "Water");
                        break;
                    case SoundMaterial.Grass:
                        sound.EventInstance.setParameterByNameWithLabel("GroundMaterial", "Grass");
                        break;
                    case SoundMaterial.Wood:
                        sound.EventInstance.setParameterByNameWithLabel("GroundMaterial", "Wood");
                        break;

                    default:
                        sound.EventInstance.setParameterByNameWithLabel("GroundMaterial", "Default");
                        break;
                }
            }
            else
            {
                sound.EventInstance.setParameterByNameWithLabel("GroundMaterial", "Default");
            }

            if (buoyant.SubmergeRate < 1.0f && buoyant.SubmergeRate > 0.5f)
            {
                sound.EventInstance.setParameterByNameWithLabel("GroundMaterial", "Water");
            }
            else if (buoyant.SubmergeRate <= 0.5f)
            {
                sound.EventInstance.setParameterByNameWithLabel("GroundMaterial", "WaterSplash");
            }

            sound.Play();
        }
    }

    /// <summary>
    /// DropItemCrash가 플레이어 변수에 접근하기 위한 함수
    /// </summary>>
    public void DropItemCrash(float addMoveSpeed, float addSprintSpeed, float addSwimSpeed, float addJumpPower, float addBoatSpeed)
    {
        moveSpeed += addMoveSpeed;
        sprintSpeedMult += addSprintSpeed;
        swimSpeed += addSwimSpeed;
        jumpPower += addJumpPower;
        sailboatAccelerationForce += addBoatSpeed;
    }

    /// <summary>
    /// 플레이어의 업그레이드 함수관리를 하는 변수
    /// </summary>
    
    public void PlayerUpgradeState(AbilityAttribute Type ,float UpgradeState)
    {

        if(Type == AbilityAttribute.MoveSpeed)
        {
            moveSpeed += UpgradeState;
        }

        if(Type == AbilityAttribute.JumpPower)
        {
            jumpPower += UpgradeState;
        }

        if(Type == AbilityAttribute.LeapupPower)
        {
            leapupPower += UpgradeState;
        }
        
        if(Type == AbilityAttribute.BoosterDuration)
        {
            boosterDuration += UpgradeState;
        }

        if(Type == AbilityAttribute.BoosterMult)
        {
            boosterMult += UpgradeState;
        }


    }

    public float ViewJumpPower{get{return jumpPower;}}
    public float ViewMoveSpeed{get{return moveSpeed;}}
    public float ViewleapupPower {get {return FinalLeapupPower;}}
    public float ViewBoosterDuration { get { return FinalBoosterDuration; } }
    public float ViewBoosterMult { get { return FinalBoosterMult; } }

    /// <summary>
    /// 플레이어가 조각배 탑승 중에 암초에 충돌할 경우
    /// </summary>
    IEnumerator ReefCrash()
    {
        DisableControls();
        animator.SetTrigger("ReefCrash");
        RuntimeManager.PlayOneShot(sound_SailboatBump);
        AbortBooster();
        driftActive = false;
        stunEffect.Play(true);
        stoneAttackEffect.Play(true);

        rBody.velocity = new Vector3(0f, rBody.velocity.y, 0f);
        rBody.AddForce(-transform.forward * reefCrashPower, ForceMode.Impulse);
        
        yield return new WaitForSeconds(reefCrashStifftime);


        SailboatQuit();
        //rBody.AddForce(Vector3.back * reefCrashPower, ForceMode.Impulse);
        //rBody.AddForce(Vector3.down * reefCrashPower, ForceMode.Impulse);
        yield return new WaitForSeconds(reefCrashbindTime);


        EnableControls();
    }

    float reefCrashStifftime = 0.5f;
    float reefCrashbindTime = 3.0f;
    float reefCrashPower = 15.0f;
    float boatGroundingTimer = 0f;

    /// <summary>
    /// 충돌감지
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Reef"))
        {
            //암초충돌감지
            if (previousVelocity.magnitude - rBody.velocity.magnitude > 10)
            {
                StartCoroutine(ReefCrash());
            }
        }

        if(((1 << collision.collider.gameObject.layer) & groundIgnore) == 0)
        {
            boatGroundingTimer = sailboatAutoOffTime;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.collider.gameObject.layer) & groundIgnore) == 0 && grounding)
        {
            if (CurrentMovement.GetType() == typeof(Movement_Sailboat))
            {
                boatGroundingTimer -= Time.deltaTime;
                if(boatGroundingTimer < 0)
                {
                    CurrentMovement = new Movement_Ground();
                }
            }
        }
    }

    public void JumpingFromObj()
    {
        JumpObject jumpObject = FindObjectOfType<JumpObject>();
        if (jumpObject != null)
        {
            float jumpingForce = jumpObject.jumpForce;
            rBody.AddForce(Vector3.up * jumpingForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("JumpObject를 찾을 수 없습니다!");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.magenta;
        if (bottomColider != null)
        {
            if (groundNormal != Vector3.zero)
            {
                DrawArrow.ForGizmo(transform.position + bottomColider.center, -groundNormal * (groundCastDistance + bottomColider.radius));
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position + bottomColider.center, 0.1f);
            }
        }
    }
#endif
}
