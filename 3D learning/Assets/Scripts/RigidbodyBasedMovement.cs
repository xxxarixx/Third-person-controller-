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
    [SerializeField] private CapsuleCollider playerCollision;
    [SerializeField] private SphereCollider groundCollision;
    [SerializeField] private Transform cam;
    [Header("Casual movement")]
    [SerializeField] private float playerHeight = 1.25f;
    [SerializeField] private float speed = 100f;
    [SerializeField] private float sprintSpeed = 275f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float _turnSmoothVelocity;
    public float speed_Current { get;private set; }
    private bool _isSprinting;
    [Header("Jump")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDistance = 0.4f;
    private bool _isGrounded;
    private bool _isJumping = false;

    [Header("Slopes/Stairs")]
    [SerializeField] private float slopeCheckerSize = 1f;
    [SerializeField] private float skinWidth = .3f;
    [Range(.1f,1f), SerializeField] private float slopeMaxSize = .6f;


    [SerializeField] private bool debug_ProperFrontWorking = false;
    [SerializeField] private bool debug_ProperHeightWorking = false;
    [SerializeField] private bool debug_RaycastHitFromToYWorking = false;

    [Header("Animations")]
    [SerializeField] private string anim_run;
    [SerializeField] private string anim_idle;
    [SerializeField] private string anim_walk;
    private bool _canMove = true;
    [Header("Teleport player from void")]
    [SerializeField] private float teleportWhenPlayerYEqual = -10f;
    private Vector3 _startingPosition;
    private bool _lockPlayerInput;



    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        speed_Current = speed;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _canMove = true;
        playerCollision.height = playerHeight;
        _startingPosition = transform.position;
        var YPositions = Universal_RaycastAssistance.instance.GetBetweenValues(.5f,4.5f,12);
        foreach (var yPos in YPositions)
        {
            Debug.Log($"YPos: {yPos}");
        }

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
        playerCollision.height = playerHeight;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_GetFeetPos(), groundDistance);

        var moveDirection = GetMoveDirection(false);
        if (!Application.isPlaying) moveDirection = transform.forward;

        Universal_RaycastAssistance.instance.RaycastHitFromToZGizmos(rb.position, -transform.up, new Vector3(0f, 0.87f, 0f), moveDirection, 0.85f, .75f, 10, groundMask, Color.red, Color.blue, Color.yellow, out RaycastHit _lowestHit, out RaycastHit _heighestHit);
        Universal_RaycastAssistance.instance.IsItProperHeightGizmos(rb.position, _heighestHit.point, moveDirection, 1f, groundMask, 0f);
        if (Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, _heighestHit.point, moveDirection, 1f, groundMask, out RaycastHit HeightHit, 0f))
        {
            Gizmos.color = Color.white;
            var yHit = Universal_RaycastAssistance.instance.RaycastHitFromToYGizmos(rb.position, moveDirection, 1f, rb.position.y, rb.position.y + 1.5f, 20, groundMask, Color.red, Color.blue, Color.yellow, out _lowestHit, out _heighestHit, slopeMaxSize);
            if (yHit)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(new Vector3(rb.position.x, _lowestHit.point.y, rb.position.z), _heighestHit.point + GetMoveDirection(false) * 0.05f);
            }
        }
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

    private void _Movement()
    {
        Vector3 InputDirection = input.GetMoveDirectionInput();
        if (InputDirection.magnitude > 0.01f)
        {
            

            void _DownWardMovement(RaycastHit _FrontheighestHit, out bool AnyMovementApplied)
            {
                AnyMovementApplied = false;
                if (!_isJumping && _FrontheighestHit.point.y - rb.position.y < 0.01f && _isGrounded)
                {
                    debug_ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, rb.position + GetMoveDirection(false) * 0.2f, GetMoveDirection(false), 1f, groundMask, out RaycastHit _heightHit2, 0f);
                    if (Mathf.Abs(_heightHit2.point.y) - Mathf.Abs(rb.position.y) > -0.6f)
                    {
                        Move(((_heightHit2.point + GetMoveDirection(false) * 0.1f /*+ Vector3.down * .05f*/) - transform.position).normalized, speed_Current, true);
                    }
                    AnyMovementApplied = true;
                }
                else
                {
                    debug_ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, rb.position + GetMoveDirection(false) * 0.2f, GetMoveDirection(false), 1f, groundMask, out RaycastHit _heightHit2, 0f);
                    if (Mathf.Abs(_heightHit2.point.y) - Mathf.Abs(rb.position.y) <= -0.6f)
                    {
                        rb.AddForce(GetMoveDirection(false) * 1000f * Time.fixedDeltaTime, ForceMode.Impulse);
                        AnyMovementApplied = true;
                    }
                    gravity.ActiveGravity = true;
                    groundCollision.enabled = true;
                }
            }

            var feetPos = transform.position + new Vector3(0, -0.1f, 0f);
            debug_ProperFrontWorking = Universal_RaycastAssistance.instance.RaycastHitFromToZ(rb.position, -transform.up, new Vector3(0f, 0.87f, 0f), GetMoveDirection(false), 0.85f, .75f, 10, groundMask, out RaycastHit _FrontlowestHit,  out RaycastHit _FrontheighestHit);
            if (debug_ProperFrontWorking)
            {
                debug_ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, _FrontheighestHit.point, GetMoveDirection(false), 1f, groundMask, out RaycastHit _heightHit, 0f);
                if (debug_ProperHeightWorking)
                {
                    debug_RaycastHitFromToYWorking = Universal_RaycastAssistance.instance.RaycastHitFromToY(rb.position, GetMoveDirection(false), 1f, rb.position.y, rb.position.y + 1.5f, 20, groundMask, out RaycastHit _lowestHit, out RaycastHit _heighestHit, slopeMaxSize);
                    if (debug_RaycastHitFromToYWorking)
                    {
                        groundCollision.enabled = false;
                        gravity.ActiveGravity = false;
                        Move(((_heighestHit.point + GetMoveDirection(false) * 0.1f) - transform.position).normalized, speed_Current, true);

                    }
                    else
                    {
                        //too high slope
                        _DownWardMovement(_FrontheighestHit, out bool _anyMovementApplied);
                        if (!_anyMovementApplied && _isGrounded)
                        {
                            Debug.Log("Applied Force Down");
                            rb.AddForce(Vector3.down * (50f * speed_Current) * Time.fixedDeltaTime, ForceMode.Acceleration);
                        }
                    }
                }
                else
                {
                    gravity.ActiveGravity = true;
                    groundCollision.enabled = true;
                }
            }
            else
            {
                _DownWardMovement(_FrontheighestHit, out bool _anyMovementApplied);
            }
            if (!_canMove) return;
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
    
}


