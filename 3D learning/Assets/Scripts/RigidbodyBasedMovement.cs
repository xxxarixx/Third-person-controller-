using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RigidbodyBasedMovement : MonoBehaviour
{
    [Header("Scripts assigment")]
    public static RigidbodyBasedMovement instance;
    [SerializeField]private Animator anim;
    public PlayerInput input;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CustomGravity gravity;
    [SerializeField] private Cinemachine.CinemachineFreeLook cinemachineCam;

    [Header("PlayerParts")]
    [SerializeField] private Transform cam;
    [SerializeField] private bool debug_DrawGroundedSize = false;
    [Header("Collisions")]
    [SerializeField] private CapsuleCollider playerCollision;
    [SerializeField] private Vector3 playerCollision_offset = Vector3.zero;
    [SerializeField] private float playerCollision_radius = .3f;
    [SerializeField] private float playerCollision_height = 1.25f;
    [SerializeField] private SphereCollider groundCollision;
    [SerializeField] private Vector3 groundCollision_offset = Vector3.zero;
    [SerializeField] private float groundCollision_radius = .5f;

    [Header("Casual movement")]
    [SerializeField] private float speed = 100f;
    [SerializeField] private float sprintSpeed = 275f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    public float speed_Current { get;private set; }
    private float _turnSmoothVelocity;
    private bool _isSprinting;

    [Header("Jump")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDistance = 0.4f;
    private bool _isGrounded;
    private bool _isJumping = false;

    [Header("Slopes/Stairs")]
    [SerializeField] private float slopeCheckerSize = 1f;
    [Range(.1f,1f), SerializeField] private float slopeMaxSize = .6f;
    [SerializeField] private float obstacleMaxSize = .5f;
    [SerializeField] private int frontAmount = 10;
    [SerializeField] private int topAmount = 15;
    [SerializeField] private bool debug_ProperFrontWorking = false;
    [SerializeField] private bool debug_ProperHeightWorking = false;
    [SerializeField] private bool debug_RaycastHitFromToYWorking = false;

    [Header("Animations")]
    [SerializeField] private string anim_idle;
    [SerializeField] private string anim_walk;
    [SerializeField] private string anim_run;

    [Header("Teleport player from void")]
    [SerializeField] private float teleportWhenPlayerYEqual = -10f;
    private Vector3 _startingPosition;
    private bool _lockPlayerInput;


    #region Default Functions
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        _GroundCollision_Customize(groundCollision_offset, groundCollision_radius);
        _PlayerCollision_Customize(playerCollision_offset, playerCollision_radius, playerCollision_height);
        _SetupCursor();
        _startingPosition = transform.position;
        speed_Current = speed;

    }
    private void Update()
    {
        Vector3 InputDirection = input.GetMoveDirectionInput();
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed_Current = sprintSpeed;
            anim.Play(anim_run);
            _isSprinting = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && _isSprinting)
        {
            speed_Current = speed;
            anim.Play(anim_walk);
            _isSprinting = false;
        }
        _isGrounded = Physics.CheckSphere(_GetFeetPos(), groundDistance, groundMask);
        if(transform.position.y < teleportWhenPlayerYEqual)
        {
            transform.position = _startingPosition;
        }
    }
    private void FixedUpdate()
    {
        _Movement();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(_GetHeadPlayerCollisionPosition(), Vector3.one * .1f);
        _PlayerCollision_Customize(playerCollision_offset, playerCollision_radius, playerCollision_height);
        _GroundCollision_Customize(groundCollision_offset, groundCollision_radius);
        _Movement_Debug(0f);
        //_Movement_Debug(30f);
        //_Movement_Debug(-30f);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.TryGetComponent(out Rigidbody _colRb))
        {
            //interactive obj
            if(_colRb.velocity.magnitude > 0.01f)
            {
                //impacted with force
                //reset gravity bcs it will impact player and propably will move him
                gravity.ActiveGravity = true;
                groundCollision.enabled = true;
            }
        }
    }
    #endregion
    #region Public Functions
    public void LocklockPlayerInput(bool EnableLock, bool InteractOnVCam = false)
    {
        _lockPlayerInput = EnableLock;
        rb.velocity = Vector3.zero;
        if(InteractOnVCam) cinemachineCam.enabled = !EnableLock;
    }
    public Vector3 GetMoveDirection(bool applyRotation = true)
    {
        Vector3 InputDirection = input.GetMoveDirectionInput();
        float targetAngle = Mathf.Atan2(InputDirection.x, InputDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
        if(/*InputDirection.z >= 0 && */applyRotation && !_lockPlayerInput) transform.rotation = Quaternion.Euler(0f, angle, 0f);

        return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
    }
    public void Move(Vector3 dir, float speed, bool UseCustomDirVelocityY = false)
    {
        if (_lockPlayerInput) return;
        if (UseCustomDirVelocityY)
        {
            rb.velocity = dir * speed * Time.fixedDeltaTime; 
        }
        else
        {
            rb.velocity = new Vector3(dir.x * speed * Time.fixedDeltaTime, rb.velocity.y, dir.z * speed * Time.fixedDeltaTime);
        }
    }
    public void TeleportPlayer(Vector3 newPos)
    {
        transform.position = newPos;
    }
    #endregion
    #region Private Functions
    private void _Movement()
    {
        Vector3 InputDirection = input.GetMoveDirectionInput();
        if (InputDirection.magnitude > 0.01f)
        {
            

            void _DownWardMovement(RaycastHit _FrontheighestHit,Vector3 moveDirection, out bool AnyMovementApplied)
            {
                Debug.Log("Applied DownMovement");
                AnyMovementApplied = false;
                if (!_isJumping && _FrontheighestHit.point.y - rb.position.y < 0.01f && _isGrounded)
                {
                    debug_ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, rb.position + moveDirection * 0.2f, moveDirection, 1f, groundMask, out RaycastHit _heightHit2, 0f);
                    if (Mathf.Abs(_heightHit2.point.y) - Mathf.Abs(rb.position.y) > -0.6f)
                    {
                        Move(((_heightHit2.point + moveDirection * 0.1f /*+ Vector3.down * .05f*/) - transform.position).normalized, speed_Current, true);
                    }
                    AnyMovementApplied = true;
                }
                else
                {
                    debug_ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, rb.position + moveDirection * 0.2f, moveDirection, 1f, groundMask, out RaycastHit _heightHit2, 0f);
                    if (Mathf.Abs(_heightHit2.point.y) - Mathf.Abs(rb.position.y) <= -0.6f)
                    {
                        rb.AddForce(GetMoveDirection(false) * 1000f * Time.fixedDeltaTime, ForceMode.Impulse);
                        AnyMovementApplied = true;
                    }
                    groundCollision.enabled = true;
                    gravity.ActiveGravity = true;
                }
            }
            void _WholeObstacleChecker(float offsetMoveYAngle)
            {
                var moveDirection = Quaternion.Euler(0f, offsetMoveYAngle, 0f) * GetMoveDirection();
                debug_ProperFrontWorking = Universal_RaycastAssistance.instance.RaycastHitFromToZ(_GetRaycastCollisonStartPos(), -transform.up, Vector3.up * obstacleMaxSize, moveDirection, obstacleMaxSize, .75f, frontAmount, groundMask, out RaycastHit _FrontlowestHit, out RaycastHit _FrontheighestHit);
                if (debug_ProperFrontWorking)
                {
                    debug_ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, _FrontheighestHit.point, moveDirection, 1f, groundMask, out RaycastHit _heightHit, 0f);
                    if (debug_ProperHeightWorking)
                    {
                        debug_RaycastHitFromToYWorking = Universal_RaycastAssistance.instance.RaycastHitFromToY(rb.position, moveDirection, 1f, rb.position.y, rb.position.y + obstacleMaxSize, topAmount, groundMask, out RaycastHit _lowestHit, out RaycastHit _heighestHit, slopeMaxSize);
                        if (debug_RaycastHitFromToYWorking)
                        {
                            groundCollision.enabled = false;
                            gravity.ActiveGravity = false;
                            Move(((_heighestHit.point + moveDirection * 0.1f) - rb.position).normalized, speed_Current, true);

                            if (Physics.Raycast(_GetHeadPlayerCollisionPosition(), Vector3.down, out RaycastHit _noneGroundColYCheck_hit, 2f, groundMask))
                            {
                                if (rb.position.y < _noneGroundColYCheck_hit.point.y)
                                {
                                    rb.position = new Vector3(rb.position.x, Mathf.Lerp(rb.position.y, _noneGroundColYCheck_hit.point.y, Time.deltaTime * 10f), rb.position.z);
                                }
                            }
                            Debug.Log("Applied 3check force");
                        }
                        else
                        {
                            //too high slope
                            _DownWardMovement(_FrontheighestHit, moveDirection, out bool _anyMovementApplied);
                            if (!_anyMovementApplied && _isGrounded)
                            {
                                Debug.Log("Applied Force Down");
                                rb.AddForce(Vector3.down * (50f * speed_Current) * Time.fixedDeltaTime, ForceMode.Acceleration);
                            }
                        }
                    }
                    else
                    {
                        groundCollision.enabled = true;
                        gravity.ActiveGravity = true;
                        debug_RaycastHitFromToYWorking = false;
                    }
                }
                else
                {
                    _DownWardMovement(_FrontheighestHit, moveDirection, out bool _anyMovementApplied);
                    debug_ProperHeightWorking = false;
                    debug_RaycastHitFromToYWorking = false;
                }
            }
            _WholeObstacleChecker(0f);
            //_WholeObstacleChecker(-30f);
            //_WholeObstacleChecker(30f);
            var moveDir = GetMoveDirection();
            if (_isGrounded)
            {
                Move(moveDir, speed_Current);
                Debug.Log("NormalMovement");
            }
            else if (!_isGrounded)
            {
                Move(moveDir, speed_Current * 0.75f);
            }


            if (!_isSprinting) anim.Play(anim_walk);
            
           

           

        }
        else
        {
            anim.Play(anim_idle);
            #region stairReset
            groundCollision.enabled = true;
            gravity.ActiveGravity = true;
            #endregion
            if (rb.velocity.y > 0 && !_isJumping)
            {
                rb.velocity = Vector3.zero;
            }
            else
            {
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }
        }
    }
    private Vector3 _GetFeetPos(float offsetX = 0f,float offsetY = 0f,float offsetZ = 0f)
    {
        return transform.position /*PlayerCollision.bounds.center - new Vector3(0f, PlayerCollision.bounds.size.y / 2, 0f)*/ + new Vector3(offsetX,offsetY,offsetZ);
    }
    private Vector3 _GetColliderPos(float offsetX = 0f, float offsetY = 0f, float offsetZ = 0f)
    {
        return playerCollision.bounds.center - new Vector3(0f, playerCollision.bounds.size.y / 2, 0f) + new Vector3(offsetX, offsetY, offsetZ);
    }
    private void _GroundCollision_Customize(Vector3 _centerOffset, float _radius)
    {
        if (groundCollision == null) { Debug.LogError("groundCollision NULL");  return; }

        groundCollision.center = _centerOffset;
        groundCollision.radius = _radius;
    }
    private void _PlayerCollision_Customize(Vector3 _centerOffset, float _radius, float _height)
    {
        if (groundCollision == null) { Debug.LogError("groundCollision NULL"); return; }
        Vector3 a = _centerOffset + Vector3.up * _height / 2 + Vector3.up * groundCollision_radius/*+ Vector3.up * obstacleMaxSize + Vector3.up * 0.025f*/;
        playerCollision.center = a;
        playerCollision.radius = _radius;
        playerCollision.height = _height;
    }
    private Vector3 _GetHeadPlayerCollisionPosition(float xOffset = 0f, float yOffset = 0f, float zOffset = 0f)
    {
        return rb.position + Vector3.up * (2 * groundCollision_radius) + playerCollision_offset + Vector3.up * playerCollision_height + new Vector3(xOffset,yOffset,zOffset);
    }
    private Vector3 _GetRaycastCollisonStartPos() 
    {
        return groundCollision.transform.position + groundCollision_offset - (groundCollision_radius) * Vector3.up + .075f * Vector3.up;
    }
    private void _SetupCursor() 
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void _Movement_Debug(float offsetAngleY)
    {
        if (debug_DrawGroundedSize)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_GetFeetPos(), groundDistance);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_GetRaycastCollisonStartPos(), .05f);

        var moveDirection = Quaternion.Euler(0f, offsetAngleY, 0f) * GetMoveDirection(false);
        if (!Application.isPlaying) moveDirection = Quaternion.Euler(0f, offsetAngleY, 0f) * transform.forward;

        Universal_RaycastAssistance.instance.RaycastHitFromToZGizmos(_GetRaycastCollisonStartPos(), -transform.up, Vector3.up * obstacleMaxSize, moveDirection, obstacleMaxSize, .75f, frontAmount, groundMask, Color.red, Color.blue, Color.yellow, out RaycastHit _lowestHit, out RaycastHit _heighestHit);
        Universal_RaycastAssistance.instance.IsItProperHeightGizmos(rb.position, _heighestHit.point, moveDirection, 1f, groundMask, 0f);
        if (Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, _heighestHit.point, moveDirection, 1f, groundMask, out RaycastHit HeightHit, 0f))
        {
            Gizmos.color = Color.white;
            var yHit = Universal_RaycastAssistance.instance.RaycastHitFromToYGizmos(rb.position, moveDirection, 1f, rb.position.y, rb.position.y + obstacleMaxSize, topAmount, groundMask, Color.red, Color.blue, Color.yellow, out _lowestHit, out _heighestHit, slopeMaxSize);
            if (yHit)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(new Vector3(rb.position.x, _lowestHit.point.y, rb.position.z), _heighestHit.point + moveDirection * 0.05f);
            }
        }
    }

    #endregion
}


