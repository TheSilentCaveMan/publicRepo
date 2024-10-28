using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AIMove),typeof(AIAbility),typeof(AISense))]
[RequireComponent(typeof(AILinkTraverse),typeof(AIAttack), typeof(CharacterController))]
public class AIBehaviour : MonoBehaviour
{
    //AIMOVE
    internal AIMove aiMove;
    public float idleTime = 1f;
    public float moveDistance = 200f;
    //AIABILITY
    internal AIAbility aiAbility;
    public float abiltyDelay = 0;
    public AIState[] excludeBehaviours = new AIState[0];
    //AISENSE
    internal AISense aiSense;
    public Transform aiHead;
    public float aiSmell = 0;
    public float aiHearing = 0;
    public AISixthSense defaultAISixthSense;
    //AIVISIONCUBE
    public AIVisionCube aiVisionCube;
    public float visionPower = 2f;
    //LINKTRAVERSE
    internal AILinkTraverse aiLinkTraverse;
    public OffMeshLinkMoveMethod m_Method = OffMeshLinkMoveMethod.Parabola;
    public AnimationCurve m_Curve = new AnimationCurve();
    //AIATTACK
    internal AIAttack aiAttack;
    public AIAttackRadius attackRadius;
    public float attackDelay = 2f;
    //AIAUTOCRAWL
    public AIAutoCrouch aiAutoCrouch;
    //AITRIGGERCOLLIDER
    public Collider aiTriggerCollider;
    //GENERIC
    public Animator animator;
    public GameObject target;
    public string entityWeakness = "";
    public bool aiStop = false;
    public GameObject[] players;
    public LayerMask triggerLayerMask;
    public LayerMask attackLayerMask;
    public AIState defaultAIState;
    public AIState _aiState;
    internal bool isFedPath = false;
    private LayerMask playerLayerMask;
    private LayerMask environmentLayerMask;
    private CharacterController characterController;
    private Transform aiBody;
    private NavMeshAgent entityNav;
    private float obstacleMass;
    private ObjectGrabbable obstacle;
    public AIState aiState
    {
        get
        {
            return _aiState;
        }
        set
        {
            OnAIStateChange?.Invoke(_aiState, value);
            _aiState = value;
        }
    }
    public delegate void AIStateChangeEvent(AIState oldState, AIState newState);
    public AIStateChangeEvent OnAIStateChange;
    private void OnDisable()
    {
        _aiState = defaultAIState;
    }
    public void Spawn()
    {
        OnAIStateChange?.Invoke(AIState.Spawn, defaultAIState);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<ObjectGrabbable>())
        {
            IAttackable attackable = collision.gameObject.GetComponent<IAttackable>();
            obstacle = collision.gameObject.GetComponent<ObjectGrabbable>();
            obstacleMass = collision.gameObject.GetComponent<Rigidbody>().mass;
            entityNav.velocity /= obstacleMass;
            if (obstacle != null)
            {
                if (obstacle.grabbed)
                {
                    obstacle.Drop();
                    Debug.Log("Knocking item out of hand");
                }
                switch (obstacleMass)
                {
                    case < 5:
                        Debug.Log("Not heavy enough for destruction");
                        break;
                    case > 5:
                        attackable.IsAttackable = true;
                        aiAttack.attackRadius.AddAttackable(attackable);
                        Debug.Log("Adding to be destroyed");
                        break;
                }
            }
        }
    }
    private void Awake()
    {
        environmentLayerMask = LayerMask.GetMask("Environment") | LayerMask.GetMask("RayObstruction");
        playerLayerMask = LayerMask.GetMask("Player");
        aiBody = GetComponent<Transform>();
        entityNav = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
        players = GameObject.FindGameObjectsWithTag("Player");
        OnAIStateChange += HandleStateChange;
        //ATTACK
        aiAttack = GetComponent<AIAttack>();
        aiAttack.animator = animator;
        aiAttack.aiBehav = this;
        aiAttack.entityNav = entityNav;
        aiAttack.attackRadius = attackRadius;
        aiAttack.attackRadius.onAttack += aiAttack.OnAttack;
        //ATTACKRADIUS
        attackRadius.attackDelay = attackDelay;
        attackRadius.controllerOffset = characterController.center;
        attackRadius.environmentLayerMask = environmentLayerMask;
        attackRadius.attackLayerMask = attackLayerMask;
        attackRadius.Initalize();
        //MOVE
        aiMove = GetComponent<AIMove>();
        aiMove.aiBehav = this;
        aiMove.entityNav = entityNav;
        aiMove.entityTransform = aiBody;
        aiMove.animator = animator;
        aiMove.idleTime = idleTime;
        aiMove.moveDistance = moveDistance;
        aiMove.ogSpeed = entityNav.speed;
        aiMove.ogAngularSpeed = entityNav.angularSpeed;
        aiMove.ogAcceleration = entityNav.acceleration;
        //AUTOCROUCH
        aiAutoCrouch.aiMove = aiMove;
        aiAutoCrouch.environmentLayerMask = environmentLayerMask;
        aiAutoCrouch.aiCharController = characterController;
        aiAutoCrouch.height = characterController.height;
        aiAutoCrouch.Initalize();
        //ABILITY
        aiAbility = GetComponent<AIAbility>();
        aiAbility.aiBehav = this;
        aiAbility.abiltyDelay = abiltyDelay;
        aiAbility.excludeBehaviours = excludeBehaviours;
        //SENSE
        aiSense = GetComponent<AISense>();
        aiSense.aiBehav = this;
        aiSense.aiSmell = aiSmell;
        aiSense.aiHearing = aiHearing;
        aiSense.aiHead = aiHead;
        aiSense.senseLayerMask = environmentLayerMask | playerLayerMask;
        aiSense.defaultAISixthSense = defaultAISixthSense;
        //AIVISIONCUBE
        aiVisionCube.aiSense = aiSense;
        aiVisionCube.playerLayerMask = playerLayerMask;
        visionPower *= MainManager.gameConfig.gameDifficulty;
        aiVisionCube.visionPower = visionPower;
        aiVisionCube.Initalize();
        //LINKTRAVERSE
        aiLinkTraverse = GetComponent<AILinkTraverse>();
        aiLinkTraverse.m_Curve = m_Curve;
        aiLinkTraverse.m_Method = m_Method;
        //AITRIGGERCOLLODER
        aiTriggerCollider.excludeLayers = ~triggerLayerMask;

        Spawn();
    }
    private void Start()
    {
        entityNav.speed *= MainManager.gameConfig.gameDifficulty;
        entityNav.angularSpeed *= MainManager.gameConfig.gameDifficulty;
        entityNav.acceleration *= MainManager.gameConfig.gameDifficulty;
        aiSense.entityNav = entityNav;
    }
    private void Update()
    {
        Sense();
        Ability();
        Move();
    }
    private void HandleStateChange(AIState oldState, AIState newState)
    {
        if (oldState != newState)
        {
            switch(newState)
            {
                case AIState.Spawn:
                    break;
                case AIState.Idle:
                    aiMove.IdleMode();
                    break;
                case AIState.Roam:
                    aiMove.RoamMode();
                    break;
                case AIState.Careful:
                    aiMove.CarefulMode();
                    break;
                case AIState.Chase:
                    aiMove.ChaseMode();
                    break;
                case AIState.Stunned:
                    break;
                case AIState.Hurt:
                    break;
            }
        }
    }
    private void Sense()
    {
        aiSense.CallSense(players);
    }
    private void Move()
    {
        aiMove.CallMove(target);
        aiAttack.originalDest = entityNav.destination;
    }
    private void Ability()
    {
        aiAbility.CallAbility();
    }   
}
