using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RigidbodyBasedMovement : MonoBehaviour
{
    /// <summary>
    /// movement handler:
    ///  movement based on rigidbody velocity
    /// slope handler:
    ///  if slope is too high then apply force torward to simulate jump from cliff
    /// jump handler
    /// 
    /// </summary>
    [Header("Scripts assigment")]
    public static RigidbodyBasedMovement instance;
    [SerializeField]private Animator anim;
    public PlayerInput input;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CustomGravity gravity;
    [SerializeField] private Cinemachine.CinemachineFreeLook cinemachineCam;

    [Header("PlayerParts")]
    [SerializeField] private CapsuleCollider PlayerCollision;
    [SerializeField] private Transform cam;
    [SerializeField] private Transform headPos;
    [SerializeField] private Transform TorsowPos;
    [Header("Casual movement")]
    [SerializeField] private float PlayerHeight = 1.85f;
    [SerializeField] private float Speed;
    [SerializeField] private float SprintSpeed;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float MaxDistanceToJumpOnObstacle;

    private float turnSmoothVelocity;
    public float speed_Current { get;private set; }
    private Vector3 HitAndHeadPos;
    private float cooldownJumpOverObstacle = 0f;
    private bool isSprinting;
    [Header("Stairs handler")]
    [SerializeField] private float StairsCheckDistanceFromPlayer = .1f;
    [Header("Jump")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private AnimationCurve JumpHeight;
    [SerializeField] private float groundDistance = 0.4f;
    private float _jumpHeightProgress;
    private bool _isGrounded;
    private bool _isJumping = false;

    [Header("Slopes")]
    [SerializeField] private float SlopeCheckerSize = .1f;
    [Range(0.01f,1f), Tooltip("closer to 0 is heigher slope "), SerializeField] private float MaxSlope;
    [Range(0.01f, 1f), Tooltip("closer to 0 is heigher slope "), SerializeField] private float DownWardMaxSlope;
    [SerializeField] private float ForceForwardMultiplayFromToohighSlope = 5f;

    [Header("Animations")]
    [SerializeField] private string RunAnim;
    [SerializeField] private string IdleAnim;
    [SerializeField] private string WalkAnim;
    [Header("Climbing")]
    [SerializeField] private float ClimbingRad = 3f;
    [SerializeField] private float SpherePointsCount = 15;
    [Range(-1f,1f)]public float ClimbingRaycastsFov = .5f;
    [SerializeField] private float MaxWidthOfClimbingObj = 2f;
    [SerializeField] private Material SelectedMat;
    [SerializeField] private Material DeselectedMat;
    [Header("Teleport player from void")]
    [SerializeField] private float TeleportWhenPlayerYEqual = -10f;
    private Vector3 startingPosition;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        input.OnPressedJump += Input_OnPressedJump;
        speed_Current = Speed;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        canMove = true;
        PlayerCollision.height = PlayerHeight;
        startingPosition = transform.position;
        var YPositions = Universal_RaycastAssistance.instance.GetBetweenValues(.5f,4.5f,12);
        foreach (var yPos in YPositions)
        {
            Debug.Log($"YPos: {yPos}");
        }

    }

    private void Input_OnPressedJump()
    {
       /* if (!_isGrounded) { return; }
        _isJumping = true;
        
        Debug.Log("Jumping");*/
    }
    
    // Update is called once per frame
    private void Update()
    {
        Vector3 InputDirection = input.GetMoveDirectionInput();
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed_Current = SprintSpeed;
            anim.Play(RunAnim);
            isSprinting = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && isSprinting)
        {
            speed_Current = Speed;
            anim.Play(WalkAnim);
            isSprinting = false;
        }
        if (_isJumping) _jumpHeightProgress += Time.deltaTime;
        if (_jumpHeightProgress >= JumpHeight.keys[JumpHeight.length - 1].time && _isJumping) { _isJumping = false; _jumpHeightProgress = 0f; }
        //jump
        _isGrounded = Physics.CheckSphere(_GetFeetPos(), groundDistance, groundMask);
        if(transform.position.y < TeleportWhenPlayerYEqual)
        {
            transform.position = startingPosition;
        }

    }
    private void FixedUpdate()
    {
        Movement();
        if (_isJumping && _jumpHeightProgress < JumpHeight.keys[JumpHeight.length - 1].time)
        {
            rb.velocity += Vector3.up * (CustomGravity.globalGravity * -1f) * JumpHeight.Evaluate(_jumpHeightProgress) * Time.fixedDeltaTime;
        }
        if (isOnSlope())
        {
            var hit = _getSlopeRaycasthit();
            PlayerCollision.height = PlayerHeight - Mathf.Abs(1 - hit.normal.y);
        }
        else
        {
            PlayerCollision.height = PlayerHeight;
        }
        //StairsHandler();
    }
    private bool lockPlayerInput;
    public void LocklockPlayerInput(bool EnableLock, bool InteractOnVCam = false)
    {
        lockPlayerInput = EnableLock;
        rb.velocity = Vector3.zero;
        if(InteractOnVCam) cinemachineCam.enabled = !EnableLock;
    }
    private Vector3 _GetFeetPos(float offsetX = 0f,float offsetY = 0f,float offsetZ = 0f)
    {
        return transform.position /*PlayerCollision.bounds.center - new Vector3(0f, PlayerCollision.bounds.size.y / 2, 0f)*/ + new Vector3(offsetX,offsetY,offsetZ);
    }
    private Vector3 _GetColliderPos(float offsetX = 0f, float offsetY = 0f, float offsetZ = 0f)
    {
        return PlayerCollision.bounds.center - new Vector3(0f, PlayerCollision.bounds.size.y / 2, 0f) + new Vector3(offsetX, offsetY, offsetZ);
    }
    public float SkinWidth;
    private void OnDrawGizmos()
    {
        PlayerCollision.height = PlayerHeight;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_GetFeetPos(), groundDistance);
    }
    #region Slope
    private Vector3 _getSlopeRaycastPos()
    {
        var offset =/*input.Moveinput.y * .25f * transform.forward +*/ new Vector3(0f,.25f,0f);
        return _GetFeetPos(offset.x, offset.y, offset.z);
    }
    private RaycastHit _getSlopeRaycasthit()
    {
        Physics.Raycast(_getSlopeRaycastPos(), Vector3.down, out RaycastHit hit, SlopeCheckerSize, groundMask);
        return hit;
    }
    public bool isOnSlope()
    {
        var hit = _getSlopeRaycasthit();
        //Debug.Log($"Slope {hit.normal}");
        return hit.collider != null && hit.normal != Vector3.up;
    }
    #endregion
    public Vector3 GetMoveDirection(bool applyRotation = true)
    {
        Vector3 InputDirection = input.GetMoveDirectionInput();
        float targetAngle = Mathf.Atan2(InputDirection.x, InputDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        if(/*InputDirection.z >= 0 && */applyRotation && !lockPlayerInput) transform.rotation = Quaternion.Euler(0f, angle, 0f);

        return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
    }
    public bool debug_ProperFrontWorking = false;
    public bool debug_ProperHeightWorking = false;
    public bool debug_RaycastHitFromToYWorking = false;
    [Header("newStuff")]
    public float RaycastLength = 1f;
    public float MaxWallHeight = .75f;
    public float OffsetGroundCheckerY = .2f;
    bool pushOverride = false;
    public SphereCollider groundCol;
    private void Movement()
    {
        Vector3 InputDirection = input.GetMoveDirectionInput();
        if (pushOverride) return;
        if (InputDirection.magnitude > 0.01f)
        {
            

            bool _DownWardMovement(RaycastHit _FrontheighestHit)
            {
                if (!_isJumping && _FrontheighestHit.point.y - rb.position.y < 0.01f && _isGrounded)
                {
                    debug_ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, rb.position + GetMoveDirection(false) * 0.2f, GetMoveDirection(false), 1f, groundMask, out RaycastHit _heightHit2, 0f);
                    if (Mathf.Abs(_heightHit2.point.y) - Mathf.Abs(rb.position.y) > -0.6f)
                    {
                        Move(((_heightHit2.point + GetMoveDirection(false) * 0.1f /*+ Vector3.down * .05f*/) - transform.position).normalized, speed_Current, true);
                    }
                    return true;
                }
                else
                {
                    debug_ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, rb.position + GetMoveDirection(false) * 0.2f, GetMoveDirection(false), 1f, groundMask, out RaycastHit _heightHit2, 0f);
                    if (Mathf.Abs(_heightHit2.point.y) - Mathf.Abs(rb.position.y) <= -0.6f)
                    {
                        rb.AddForce(GetMoveDirection(false) * 1000f * Time.fixedDeltaTime, ForceMode.Impulse);
                        return true;
                    }
                    gravity.ActiveGravity = true;
                    groundCol.enabled = true;
                }
                return false;
            }

            var feetPos = transform.position + new Vector3(0, -0.1f, 0f);
            debug_ProperFrontWorking = Universal_RaycastAssistance.instance.RaycastHitFromToZ(rb.position, -transform.up, new Vector3(0f, 0.87f, 0f), GetMoveDirection(false), 0.85f, .75f, 10, groundMask, out RaycastHit _FrontlowestHit,  out RaycastHit _FrontheighestHit);
            if (debug_ProperFrontWorking)
            {
                debug_ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, _FrontheighestHit.point, GetMoveDirection(false), 1f, groundMask, out RaycastHit _heightHit, 0f);
                if (debug_ProperHeightWorking)
                {
                    debug_RaycastHitFromToYWorking = Universal_RaycastAssistance.instance.RaycastHitFromToY(rb.position, GetMoveDirection(false), 1f, rb.position.y, rb.position.y + 1.5f, 20, groundMask, out RaycastHit _lowestHit, out RaycastHit _heighestHit, .6f);
                    if (debug_RaycastHitFromToYWorking)
                    {
                        groundCol.enabled = false;
                        gravity.ActiveGravity = false;
                        Move(((_heighestHit.point + GetMoveDirection(false) * 0.1f) - transform.position).normalized, speed_Current, true);

                    }
                    else
                    {
                        //too high slope
                        if (!_DownWardMovement(_FrontheighestHit))
                        {
                            Debug.Log("Applied Force Down");
                            rb.AddForce(Vector3.down * (50f * speed_Current) * Time.fixedDeltaTime, ForceMode.Acceleration);
                        }
                    }
                }
                else
                {
                    gravity.ActiveGravity = true;
                    groundCol.enabled = true;
                }
            }
            else
            {
                if (!_DownWardMovement(_FrontheighestHit))
                {
                    Debug.Log("Applied Force Down");
                    rb.AddForce(Vector3.down * (50f * speed_Current) * Time.fixedDeltaTime, ForceMode.Acceleration);
                }
            }
            if (!canMove) return;
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


            if (!isSprinting) anim.Play(WalkAnim);
            
           

           

        }
        else
        {
            anim.Play(IdleAnim);
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
    public void Move(Vector3 dir, float speed, bool UseCustomDirVelocityY = false)
    {
        if (lockPlayerInput) return;
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
    public GameObject checkGo;
    private List<MeshRenderer> lastSelectedMeshes = new List<MeshRenderer>();
    private bool canMove = true;
    private void Climbing()
    {
        //var hits = Physics.SphereCastAll(headPos.transform.position, ClimbingRad,transform.forward, ClimbingRad,groundMask);
        foreach (var lastMeshes in lastSelectedMeshes)
        {
            lastMeshes.material = DeselectedMat;
        }
        lastSelectedMeshes.Clear();
        RaycastHit ClosestHit = new RaycastHit();
        float DotClosest = -2f;
        float ClosestDistance = 0f;
        foreach (var point in Universal_RaycastAssistance.instance.UniformPointsOnSphere(SpherePointsCount, cam.forward, ClimbingRaycastsFov))
        {
            Physics.Raycast(headPos.transform.position, point.normalized, out RaycastHit forwardHit, ClimbingRad, groundMask);
            if (forwardHit.collider == null) continue;
            if (forwardHit.normal.y != 0) continue;

            Physics.Raycast(forwardHit.point + new Vector3(0f, MaxWidthOfClimbingObj/2, 0f),Vector3.down, out RaycastHit downHit, MaxWidthOfClimbingObj, groundMask);
            if (forwardHit.collider != downHit.collider) continue;
            if (downHit.collider == null) continue;
            var CheckmeshRend = forwardHit.collider.GetComponent<MeshRenderer>();
            if (CheckmeshRend == null) continue;
            if (downHit.collider.transform == transform.parent) continue;
            var dot = Vector3.Dot(ClosestHit.point, downHit.point);
            if (dot > DotClosest)
            {
                ClosestHit = downHit;
                DotClosest = dot;
            }

           
        }
        if(ClosestHit.collider != null) 
        {
            var meshRend = ClosestHit.collider.GetComponent<MeshRenderer>();
            meshRend.material = SelectedMat;
            lastSelectedMeshes.Add(meshRend);
            Destroy(Instantiate(checkGo, ClosestHit.point, Quaternion.identity), .1f);
            if (Input.GetKeyDown(KeyCode.E))
            {
                rb.velocity = Vector3.zero;
                transform.rotation =  Quaternion.LookRotation(new Vector3((meshRend.transform.position - transform.position).normalized.x,0f, (meshRend.transform.position - transform.position).normalized.z));
                transform.SetParent(meshRend.transform, true);
                TeleportPlayer(ClosestHit.point - new Vector3(0f,PlayerCollision.bounds.size.y,0f) + ((meshRend.transform.position - transform.position).normalized * -.5f));
                PlayerCollision.enabled = false;
                canMove = false;
                gravity.ActiveGravity = false;
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
                groundCol.enabled = true;
            }
        }
    }
    //it getting array of points direction on sphere 
    #region Addons

    #endregion
}


