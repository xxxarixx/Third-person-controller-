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
        /*if (Input.GetKeyDown(KeyCode.C))
        {
            transform.SetParent(null, true);
            canMove = true;
            PlayerCollision.enabled = true;
            gravity.ActiveGravity = true;
            transform.localScale = Vector3.one;
        }*/
      
        if (rb.velocity.sqrMagnitude > .1f)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                speed_Current = SprintSpeed;
                anim.Play(RunAnim);
                isSprinting = true;
            }
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
        _isGrounded = Physics.CheckSphere(_getFeetPos(), groundDistance, groundMask);
        //walk
        //GetOnSmallObstaces();
        //Climbing();
        if(transform.position.y < TeleportWhenPlayerYEqual)
        {
            transform.position = startingPosition;
        }
    }
    private void FixedUpdate()
    {
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
        Movement();
        //StairsHandler();
    }
    private bool lockPlayerInput;
    public void LocklockPlayerInput(bool EnableLock, bool InteractOnVCam = false)
    {
        lockPlayerInput = EnableLock;
        rb.velocity = Vector3.zero;
        if(InteractOnVCam) cinemachineCam.enabled = !EnableLock;
    }
    private Vector3 _getFeetPos(float offsetX = 0f,float offsetY = 0f,float offsetZ = 0f)
    {
        return PlayerCollision.bounds.center - new Vector3(0f, PlayerCollision.bounds.size.y / 2, 0f) + new Vector3(offsetX,offsetY,offsetZ);
    }
    private void OnDrawGizmos()
    {
        PlayerCollision.height = PlayerHeight;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_getFeetPos(), groundDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_getSlopeRaycastPos(), _getSlopeRaycastPos() + new Vector3(0f,-SlopeCheckerSize,0f));

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(HitAndHeadPos, .2f);

        var EndFeetRay = (transform.forward * MaxDistanceToJumpOnObstacle);
        Gizmos.DrawLine(_getFeetPos(0f, .25f, 0f), _getFeetPos(EndFeetRay.x, EndFeetRay.y + .25f, EndFeetRay.z));

        Gizmos.color = Color.green;
        Universal_RaycastAssistance.instance.DrawRaycastGizmo(transform.position + transform.forward * StairsCheckDistanceFromPlayer + transform.up * StairsCheckDistanceFromPlayer, Vector3.down,10f);
        /*foreach (var point in UniformPointsOnSphere(SpherePointsCount, cam.forward, ClimbingRaycastsFov))
        {
            Gizmos.DrawCube(headPos.transform.position + (point.normalized * ClimbingRad), new Vector3(.1f, .1f, .1f));
        }*/
    }
    #region Slope
    private Vector3 _getSlopeRaycastPos()
    {
        var offset =/*input.Moveinput.y * .25f * transform.forward +*/ new Vector3(0f,.25f,0f);
        return _getFeetPos(offset.x, offset.y, offset.z);
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
    private Vector3 get_slopeMoveDir(Vector3 moveDir, Vector3 slopeNormal)
    {
        return Vector3.ProjectOnPlane(moveDir, slopeNormal);
    }
    #endregion
    public Vector3 GetMoveDirection(Vector3 InputDirection, bool applyRotation = true)
    {
        float targetAngle = Mathf.Atan2(InputDirection.x, InputDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        if(/*InputDirection.z >= 0 && */applyRotation && !lockPlayerInput) transform.rotation = Quaternion.Euler(0f, angle, 0f);

        return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
    }
    public float PlayerHeightStanding = 1.85f;
    public float WalkingOnSlopesHeight = 1.2f;
    public bool ProperHeightWorking = false;
    public bool RaycastHitFromToYWorking = false;
    private void Movement()
    {
        if (!canMove) return;
        Vector3 InputDirection = new Vector3(input.Moveinput.x, 0f, input.Moveinput.y).normalized;
        if (InputDirection.magnitude >= 0.01f)
        {
            var moveDir = GetMoveDirection(InputDirection);
            var feetPos = transform.position + new Vector3(0, -0.1f, 0f);
            Universal_RaycastAssistance.instance.RaycastHitFromToZ(transform.position, -transform.up, new Vector3(0f, 1f,0f), transform.forward, 0.85f, .75f, 23, 1 << 8,out RaycastHit _FrontlowestHit,  out RaycastHit _FrontheighestHit);
            ProperHeightWorking = Universal_RaycastAssistance.instance.IsItProperHeight(transform.position, _FrontheighestHit.point, transform.forward, 1f, groundMask, out RaycastHit _heightHit, 0f);
            if (ProperHeightWorking)
            {
                RaycastHitFromToYWorking = Universal_RaycastAssistance.instance.RaycastHitFromToY(transform.position, transform.forward, 2.5f, transform.position.y, transform.position.y + 1.75f, 34, groundMask, out RaycastHit _lowestHit, out RaycastHit _heighestHit, .65f);
                if (RaycastHitFromToYWorking)
                {
                    PlayerHeight = WalkingOnSlopesHeight;
                    if(Vector2.Dot(Vector2.up, _heightHit.normal) > .9f) gravity.ActiveGravity = false;
                    //applay movement from current position to heighest hit
                    Move(((_heighestHit.point + Vector3.up/* *Vector3.Distance(feetPos, _heighestHit.point)*/ * .6f) - transform.position).normalized, speed_Current, true);

                }
                else
                {
                    PlayerHeight = PlayerHeightStanding;
                    //too high slope
                    rb.AddForce(Vector3.down * 100f, ForceMode.Acceleration);
                    gravity.ActiveGravity = true;
                }
            }
            else
            {
                PlayerHeight = PlayerHeightStanding;
                gravity.ActiveGravity = true;
            }

            //var hit = _getSlopeRaycasthit();


            /*if (hit.normal.y < MaxSlope && isOnSlope())
            {
                rb.AddForce(Vector3.down * 100f, ForceMode.Acceleration); 
                Debug.Log("Too high slope");
            }

            if(isOnSlope() && rb.velocity.y < 0 && _isGrounded && !_isJumping && hit.normal.y < DownWardMaxSlope)
            {
                Move(moveDir, speed_Current);
                rb.velocity = new Vector3(rb.velocity.x * ForceForwardMultiplayFromToohighSlope, -.5f, rb.velocity.z * ForceForwardMultiplayFromToohighSlope);
                Debug.Log("NormalMovement too hith slope");
            }
            else
            if (isOnSlope() && !_isJumping && _isGrounded && hit.normal.y >= MaxSlope)
            {
                var NewmoveDir = get_slopeMoveDir(moveDir,_getSlopeRaycasthit().normal);
                Move(NewmoveDir,speed_Current, true);
                Debug.Log("Slope movement");
            }
            else*/ 
            if(_isGrounded)
            {
                Move(moveDir, speed_Current);
                Debug.Log("NormalMovement");
            }
            else if(!_isGrounded)
            {
                Move(moveDir, speed_Current * 0.75f);
            }


            if(!isSprinting) anim.Play(WalkAnim);
        }
        else
        {
            anim.Play(IdleAnim);
            PlayerHeight = PlayerHeightStanding;
            //gravity.ActiveGravity = true;
            if (rb.velocity.y > 0 && !_isJumping)
            {
                rb.velocity = new Vector3(0f, 0f, 0f);
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
    private void GetOnSmallObstaces()
    {

        if (cooldownJumpOverObstacle > 0) 
        { 
            cooldownJumpOverObstacle -= Time.deltaTime; 
            return; 
        }

        var IsHittingObstacle = Physics.Raycast(_getFeetPos(0f,.25f,0f), transform.forward, out RaycastHit forwardHit, MaxDistanceToJumpOnObstacle, groundMask);
        //Debug.Log(forwardHit.normal);
        if(IsHittingObstacle && forwardHit.collider != null && (forwardHit.normal.y < 0.1))
        {
            var isHittingFromHead = Physics.Raycast(headPos.transform.position, transform.forward, out RaycastHit HeadHit, MaxDistanceToJumpOnObstacle * 3, groundMask);
            if (isHittingFromHead)  return; 
            var teleportXZ = forwardHit.point + (transform.forward.normalized * .1f);
            HitAndHeadPos = new Vector3(teleportXZ.x, headPos.transform.position.y, teleportXZ.z);
            var IsHittingGround = Physics.Raycast(HitAndHeadPos, Vector3.down, out RaycastHit GroundHit, groundMask);
            if (!IsHittingGround || GroundHit.collider == null)  return; 
            if (GroundHit.point.x == forwardHit.point.x && GroundHit.point.z == forwardHit.point.z) return; // is hitting straight wall
            var heightOfObstacle = GroundHit.point.y + .1f;
            if (heightOfObstacle > headPos.position.y) return;
            TeleportPlayer(new Vector3(teleportXZ.x, heightOfObstacle, teleportXZ.z));
            cooldownJumpOverObstacle = .2f;
            //_isJumping = false;
        }
    }
    private void StairsHandler()
    {
        if (cooldownJumpOverObstacle > 0f || input.Moveinput == Vector2.zero) return;
        Physics.Raycast(transform.position + transform.forward * StairsCheckDistanceFromPlayer + transform.up * StairsCheckDistanceFromPlayer, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundMask);
        if (hit.collider == null) return;
        Debug.Log($"hit normal: {hit.normal} hit point difference: {hit.point.y - transform.position.y}");
        if (hit.normal == Vector3.up && hit.point.y - transform.position.y > 0.01f) TeleportPlayer(Vector3.Lerp(transform.position, hit.point, Time.deltaTime * 20f));
        cooldownJumpOverObstacle = 0.1f;
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

    //it getting array of points direction on sphere 
    #region Addons
   
    #endregion
}


