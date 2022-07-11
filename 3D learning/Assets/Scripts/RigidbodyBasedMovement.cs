using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RigidbodyBasedMovement : MonoBehaviour
{
    [Header("Scripts assigment")]
    [Header("IMPORTANT: Object holding this script pivolt must be just around down end of ground collision")]
    [SerializeField] private Animator anim;
    public static RigidbodyBasedMovement instance;
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
    [SerializeField] private CapsuleCollider groundCollision;
    [SerializeField] private Vector3 groundCollision_offset = Vector3.zero;
    [SerializeField] private float groundCollision_height_Offset = .5f;
    [SerializeField] private float groundCollision_radius = .5f;

    [Header("Casual movement")]
    [SerializeField] private float speed = 100f;
    [SerializeField] private float sprintSpeed = 275f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    public float speed_Current { get; private set; }
    private float _turnSmoothVelocity;
    private bool _isSprinting;

    [Header("Jump")]
    [SerializeField] private float groundDistance = 0.4f;
    public LayerMask groundMask;
    private bool _isGrounded;
    private bool _isJumping = false;

    [Header("Slopes/Stairs")]
    [SerializeField] private float slopeCheckerSize = 1f;
    [Range(.1f, 1f)] public float slopeMaxSize= .6f;
    public float obstacleMaxHeight  = .35f;
    public float maxLengthFromPlayerToObstacle  = .4f;
    [SerializeField] private int frontAmount = 10;
    [SerializeField] private int topAmount = 15;
    [SerializeField] private bool debug_ProperFrontWorking = false;
    [SerializeField] private bool debug_ProperHeightWorking = false;
    [SerializeField] private bool debug_RaycastHitFromToYWorking = false;

    [Header("Animations")]
    private float anim_xVelocity;
    private float anim_zVelocity;
    private float anim_lastUpdated_xVelocity;
    private float anim_lastUpdated_zVelocity;
    private float anim_progress;
    //[SerializeField] private string anim_idle;
    //[SerializeField] private string anim_walk;
    //[SerializeField] private string anim_run;

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
        playerCollision.transform.position = transform.position;
        _PlayerCollision_Customize(playerCollision_offset, playerCollision_radius, playerCollision_height);
        _GroundCollision_Customize(groundCollision_offset, groundCollision_radius, obstacleMaxHeight);
        _SetupCursor();
        _startingPosition = transform.position;
        speed_Current = speed;
    }
    private void Update()
    {
        Vector3 InputDirection = input.GetMoveDirectionInput();
        anim_xVelocity = Mathf.Lerp(anim_xVelocity, input.Moveinput.x, Time.deltaTime * 7f);
        anim_zVelocity = Mathf.Lerp(anim_zVelocity, input.Moveinput.y, Time.deltaTime * 7f);
        anim.SetFloat("xVelocity", anim_xVelocity);
        anim.SetFloat("zVelocity", anim_zVelocity);
        anim.SetBool("Running", _isSprinting);
        /*if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed_Current = sprintSpeed;
            //anim.Play(anim_run);
            _isSprinting = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && _isSprinting)
        {
            speed_Current = speed;
            //anim.Play(anim_walk);
            _isSprinting = false;
        }*/
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
        playerCollision.transform.position = transform.position;
        _PlayerCollision_Customize(playerCollision_offset, playerCollision_radius, playerCollision_height);
        _GroundCollision_Customize(groundCollision_offset, groundCollision_radius, obstacleMaxHeight);
        _Movement_Debug(0f);
        Gizmos.color = Color.yellow;
        Universal_RaycastAssistance.instance.DrawRaycastGizmo(_GetFeetPos(0f, 0.06f), Vector3.down, 0.1f);
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
        if(applyRotation && !_lockPlayerInput) transform.rotation = Quaternion.Euler(0f, angle, 0f);

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
            //RigidbodyFootInteraction.instance.SetMovementState(true);
            void _DownWardMovement(RaycastHit _FrontheighestHit,Vector3 moveDirection, out bool AnyMovementApplied)
            {
                Debug.Log("Applied DownMovement");
                AnyMovementApplied = false;
                if (!_isJumping && _FrontheighestHit.point.y - rb.position.y < 0.01f /*&& _isGrounded*/)
                {
                    debug_ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, rb.position + moveDirection * 0.2f, moveDirection, obstacleMaxHeight, maxLengthFromPlayerToObstacle, groundMask, slopeMaxSize, out RaycastHit _heightHit2, 0f);
                    if (Mathf.Abs(_heightHit2.point.y) - Mathf.Abs(rb.position.y) > -obstacleMaxHeight)
                    {
                        Move(((_heightHit2.point + moveDirection * 0.1f /*+ Vector3.down * .05f*/) - transform.position).normalized, speed_Current, true);
                        //RigidbodyFootInteraction.instance.setDestinationPos(_heightHit2.point + moveDirection * 0.1f);
                    }
                    else
                    {
                        rb.AddForce(GetMoveDirection(false) * 1000f * Time.fixedDeltaTime, ForceMode.Impulse);
                        groundCollision.enabled = true;
                        gravity.ActiveGravity = true;
                    }

                    AnyMovementApplied = true;
                }
                else
                {
                    debug_ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, rb.position + moveDirection * 0.2f, moveDirection, obstacleMaxHeight, maxLengthFromPlayerToObstacle, groundMask, slopeMaxSize, out RaycastHit _heightHit2, 0f);
                    if (Mathf.Abs(_heightHit2.point.y) - Mathf.Abs(rb.position.y) <= -obstacleMaxHeight)
                    {
                        rb.AddForce(GetMoveDirection(false) * 1000f * Time.fixedDeltaTime, ForceMode.Impulse);
                        //RigidbodyFootInteraction.instance.setDestinationPos(_heightHit2.point + moveDirection * 0.1f);
                        AnyMovementApplied = true;
                    }
                    groundCollision.enabled = true;
                    gravity.ActiveGravity = true;
                }
            }
            void _WholeObstacleChecker(float offsetMoveYAngle)
            {
                var moveDirection = Quaternion.Euler(0f, offsetMoveYAngle, 0f) * GetMoveDirection();
                debug_ProperFrontWorking = Universal_RaycastAssistance.instance.RaycastHitFromToZ(_GetRaycastCollisonStartPos(),_GetFeetPos(), -transform.up, Vector3.up * obstacleMaxHeight, moveDirection, obstacleMaxHeight, maxLengthFromPlayerToObstacle, frontAmount, groundMask, out RaycastHit _FrontlowestHit, out RaycastHit _FrontheighestHit);
                if (debug_ProperFrontWorking)
                {
                    debug_ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, _FrontheighestHit.point, moveDirection, obstacleMaxHeight,maxLengthFromPlayerToObstacle, groundMask, slopeMaxSize, out RaycastHit _heightHit, 0f);
                    if (debug_ProperHeightWorking)
                    {
                        debug_RaycastHitFromToYWorking = Universal_RaycastAssistance.instance.RaycastHitFromToY(rb.position, moveDirection, obstacleMaxHeight, rb.position.y, rb.position.y + obstacleMaxHeight, topAmount, groundMask, out RaycastHit _lowestHit, out RaycastHit _heighestHit);
                        if (debug_RaycastHitFromToYWorking)
                        {
                            groundCollision.enabled = false;
                            gravity.ActiveGravity = false;
                            Move(((_heighestHit.point + moveDirection * 0.1f) - rb.position).normalized, speed_Current, true);
                            //RigidbodyFootInteraction.instance.setDestinationPos(_heighestHit.point + moveDirection * 0.1f);
                            if (Physics.Raycast(_GetHeadPlayerCollisionPosition(), Vector3.down, out RaycastHit _noneGroundColYCheck_hit, 2f, groundMask))
                            {
                                if (rb.position.y < _noneGroundColYCheck_hit.point.y)
                                {
                                    rb.position = new Vector3(rb.position.x, Mathf.Lerp(rb.position.y, _noneGroundColYCheck_hit.point.y, Time.deltaTime * 10f), rb.position.z);
                                   // RigidbodyFootInteraction.instance.setDestinationPos(_noneGroundColYCheck_hit.point + moveDirection * 0.5f);
                                }
                            }
                            Debug.Log("Applied 3check force");
                        }
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
            anim_lastUpdated_xVelocity = anim_xVelocity;
            anim_lastUpdated_zVelocity = anim_zVelocity;
            anim_progress = 0f;
            //if (!_isSprinting) anim.Play(anim_walk);
        }
        else
        {
            //anim.Play(anim_idle);
            //RigidbodyFootInteraction.instance.SetMovementState(false);
            anim_xVelocity = Mathf.Lerp(anim_lastUpdated_xVelocity, 0f, anim_progress);
            anim_zVelocity = Mathf.Lerp(anim_lastUpdated_zVelocity, 0f, anim_progress);
            anim_progress = Mathf.Clamp01(anim_progress + Time.deltaTime * 4f);
            #region stairReset
            groundCollision.enabled = true;
            #endregion
            if(!_isJumping && Physics.Raycast(_GetFeetPos(0f, 0.06f), Vector3.down, out RaycastHit slopeHit, 0.1f, groundMask) && Vector2.Dot(Vector2.up, slopeHit.normal) > slopeMaxSize)
            {
                gravity.ActiveGravity = false;
            }
            else
            {
                gravity.ActiveGravity = true;
            }
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
    public Vector3 _GetFeetPos(float offsetX = 0f,float offsetY = 0f,float offsetZ = 0f)
    {
        return rb.position /*PlayerCollision.bounds.center - new Vector3(0f, PlayerCollision.bounds.size.y / 2, 0f)*/ + new Vector3(offsetX,offsetY,offsetZ);
    }
    private Vector3 _GetColliderPos(float offsetX = 0f, float offsetY = 0f, float offsetZ = 0f)
    {
        return playerCollision.bounds.center - new Vector3(0f, playerCollision.bounds.size.y / 2, 0f) + new Vector3(offsetX, offsetY, offsetZ);
    }
    private void _GroundCollision_Customize(Vector3 _centerOffset, float _radius,float _height)
    {
        if (groundCollision == null) { Debug.LogError("groundCollision NULL");  return; }
        Vector3 snapToPivolt = _centerOffset + Vector3.up * (_height / 2 + groundCollision_height_Offset / 2) - Vector3.up * 0.02f;
        groundCollision.center = snapToPivolt;
        groundCollision.height = _height + groundCollision_height_Offset;
        groundCollision.radius = _radius;
    }
    private void _PlayerCollision_Customize(Vector3 _centerOffset, float _radius, float _height)
    {
        if (groundCollision == null) { Debug.LogError("groundCollision NULL"); return; }
        Vector3 snapToPivolt = _centerOffset + Vector3.up * _height / 2 + Vector3.up * (obstacleMaxHeight + .1f);
        playerCollision.center = snapToPivolt;
        playerCollision.radius = _radius;
        playerCollision.height = _height;
    }
    private Vector3 _GetHeadPlayerCollisionPosition(float xOffset = 0f, float yOffset = 0f, float zOffset = 0f)
    {
        return rb.position + Vector3.up * (obstacleMaxHeight + .1f) + Vector3.up * playerCollision_height + new Vector3(xOffset,yOffset,zOffset);
    }
    public Vector3 _GetRaycastCollisonStartPos() 
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
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(rb.position, .05f);
        if (debug_DrawGroundedSize)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_GetFeetPos(), groundDistance);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_GetRaycastCollisonStartPos(), (.3f * groundCollision_radius));

        

        var moveDirection = Quaternion.Euler(0f, offsetAngleY, 0f) * GetMoveDirection(false);
        if (!Application.isPlaying) moveDirection = Quaternion.Euler(0f, offsetAngleY, 0f) * transform.forward;

        Universal_RaycastAssistance.instance.RaycastHitFromToZGizmos(_GetRaycastCollisonStartPos(), _GetFeetPos(), -transform.up, Vector3.up * obstacleMaxHeight, moveDirection, obstacleMaxHeight, maxLengthFromPlayerToObstacle, frontAmount, groundMask, Color.red, Color.blue, Color.yellow, out RaycastHit _lowestHit, out RaycastHit _heighestHit);
        Universal_RaycastAssistance.instance.IsItProperHeightGizmos(rb.position, _heighestHit.point, moveDirection, obstacleMaxHeight,maxLengthFromPlayerToObstacle, groundMask, 0f);
        if (Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, _heighestHit.point, moveDirection, obstacleMaxHeight, maxLengthFromPlayerToObstacle, groundMask, slopeMaxSize, out RaycastHit HeightHit, 0f))
        {
            Gizmos.color = Color.white;
            var yHit = Universal_RaycastAssistance.instance.RaycastHitFromToYGizmos(rb.position, moveDirection, maxLengthFromPlayerToObstacle, rb.position.y, rb.position.y + obstacleMaxHeight, topAmount, groundMask, Color.red, Color.blue, Color.yellow, out _lowestHit, out _heighestHit);
            if (yHit)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(new Vector3(rb.position.x, _lowestHit.point.y, rb.position.z), _heighestHit.point + moveDirection * 0.05f);
            }
        }
        else
        {
            /*Gizmos.color = Color.white;
            var _headPos = _GetHeadPlayerCollisionPosition();
            foreach (var _point in Universal_RaycastAssistance.instance.UniformPointsOnYAxis(_GetFeetPos(), (_headPos.y - _GetFeetPos().y) / 2, 7, 2))
            {

                //check if from this point is possible player to cross over the obstacle with small positive Y offset
                var _feetpos = _GetFeetPos();
                var _hitted = Physics.Raycast(_point, moveDirection, out RaycastHit hit, maxLengthFromPlayerToObstacle + .1f,groundMask);
                var _hittedDown = Physics.Raycast(hit.point, Vector3.down, out RaycastHit hitDown, Mathf.Infinity, groundMask);
                Gizmos.color = (_hitted && _hittedDown) ? Color.red : Color.white;
                
                Universal_RaycastAssistance.instance.DrawRaycastGizmo(_point, moveDirection, maxLengthFromPlayerToObstacle + .1f);
                if(!_hitted && _hittedDown && Mathf.Abs(hitDown.point.y) - Mathf.Abs(_GetFeetPos().y) < 0f)
                {
                    foreach (var _direction in Universal_RaycastAssistance.instance.GetDirectionsAroundDirectionInYAngle(moveDirection, 90,6))
                    {
                        Gizmos.color = Color.magenta;
                        Universal_RaycastAssistance.instance.DrawRaycastGizmo(_point, _direction, maxLengthFromPlayerToObstacle + .5f);
                    }
                }

                Gizmos.DrawSphere(_point, .05f);
            }*/
        }
    }

    #endregion
}


