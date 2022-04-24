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
    public Animator anim;
    public PlayerInput input;
    public Rigidbody rb;
    public CustomGravity gravity;

    [Header("PlayerParts")]
    public Collider PlayerCollision;
    public Transform cam;
    public Transform headPos;
    public Transform TorsowPos;
    [Header("Casual movement")]
    public float Speed;
    public float turnSmoothTime = 0.1f;
    public float MaxDistanceToJumpOnObstacle;
    private float turnSmoothVelocity;
    private float _Speed;
    private Vector3 HitAndHeadPos;
    private float cooldownJumpOverObstacle = 0f;
    [Header("Jump")]
    public LayerMask groundMask;
    public AnimationCurve JumpHeight;
    public float groundDistance = 0.4f;
    private float _jumpHeightProgress;
    private bool _isGrounded;
    private bool _isJumping = false;

    [Header("Slopes")]
    public float SlopeCheckerSize = .1f;
    [Range(0.01f,1f), Tooltip("closer to 0 is heigher slope ")]public float MaxSlope;
    [Range(0.01f, 1f), Tooltip("closer to 0 is heigher slope ")] public float DownWardMaxSlope;
    public float ForceForwardMultiplayFromToohighSlope = 5f;

    [Header("Animations")]
    public string RunAnim;
    public string IdleAnim;

    [Header("Climbing")]
    public float ClimbingRad = 3f;
    public float SpherePointsCount = 15;
    [Range(-1f,1f)]public float ClimbingRaycastsFov = .5f;
    public float MaxWidthOfClimbingObj = 2f;
    public Material SelectedMat;
    public Material DeselectedMat;
    private void Start()
    {
        input.OnPressedJump += Input_OnPressedJump;
        _Speed = Speed;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        canMove = true;
    }

    private void Input_OnPressedJump()
    {
        if (!_isGrounded) { return; }
        _isJumping = true;
        
        Debug.Log("Jumping");
    }
    
    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            transform.SetParent(null, true);
            canMove = true;
            PlayerCollision.enabled = true;
            gravity.ActiveGravity = true;
            transform.localScale = Vector3.one;
        }
        if (_isJumping) _jumpHeightProgress += Time.deltaTime;
        if (_jumpHeightProgress >= JumpHeight.keys[JumpHeight.length - 1].time && _isJumping) { _isJumping = false; _jumpHeightProgress = 0f; }
        //jump
        _isGrounded = Physics.CheckSphere(_getFeetPos(), groundDistance, groundMask);
        //walk
        GetOnSmallObstaces();
        //Climbing();
    }
    private void FixedUpdate()
    {
        if (_isJumping && _jumpHeightProgress < JumpHeight.keys[JumpHeight.length - 1].time)
        {
            rb.velocity += Vector3.up * (CustomGravity.globalGravity * -1f) * JumpHeight.Evaluate(_jumpHeightProgress) * Time.fixedDeltaTime;
        }
        
        Movement();
    }
    private Vector3 _getFeetPos(float offsetX = 0f,float offsetY = 0f,float offsetZ = 0f)
    {
        return PlayerCollision.bounds.center - new Vector3(0f, PlayerCollision.bounds.size.y / 2, 0f) + new Vector3(offsetX,offsetY,offsetZ);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_getFeetPos(), groundDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_getSlopeRaycastPos(), _getSlopeRaycastPos() + new Vector3(0f,-SlopeCheckerSize,0f));

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(HitAndHeadPos, .2f);

        var EndFeetRay = (transform.forward * MaxDistanceToJumpOnObstacle);
        Gizmos.DrawLine(_getFeetPos(0f, .25f, 0f), _getFeetPos(EndFeetRay.x, EndFeetRay.y + .25f, EndFeetRay.z));


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
    public Vector3 getMoveDirection(Vector3 InputDirection)
    {
        float targetAngle = Mathf.Atan2(InputDirection.x, InputDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        return InputDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
    }
    private void Movement()
    {
        if (!canMove) return;
        Vector3 InputDirection = new Vector3(input.Moveinput.x, 0f, input.Moveinput.y).normalized;
        if (InputDirection.magnitude >= 0.1f)
        {
            var moveDir = getMoveDirection(InputDirection);
            var hit = _getSlopeRaycasthit();


            if (hit.normal.y < MaxSlope && isOnSlope())
            {
                rb.AddForce(Vector3.down * 100f, ForceMode.Acceleration); 
                Debug.Log("Too high slope");
            }

            if(isOnSlope() && rb.velocity.y < 0 && _isGrounded && !_isJumping && hit.normal.y < DownWardMaxSlope)
            {
                Move(moveDir, _Speed);
                rb.velocity = new Vector3(rb.velocity.x * ForceForwardMultiplayFromToohighSlope, -.5f, rb.velocity.z * ForceForwardMultiplayFromToohighSlope);
                Debug.Log("NormalMovement too hith slope");
            }
            else
            if (isOnSlope() && !_isJumping && _isGrounded && hit.normal.y >= MaxSlope)
            {
                var NewmoveDir = get_slopeMoveDir(moveDir,_getSlopeRaycasthit().normal);
                Move(NewmoveDir,_Speed, true);
                Debug.Log("Slope movement");
            }
            else if(_isGrounded)
            {
                Move(moveDir, _Speed);
                Debug.Log("NormalMovement");
            }
            else if(!_isGrounded)
            {
                Move(moveDir, _Speed * 0.75f);
            }


            anim.Play(RunAnim);
        }
        else
        {
            anim.Play(IdleAnim);
            if(rb.velocity.y > 0 && !_isJumping)
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
       
        if (UseCustomDirVelocityY)
        {
            rb.velocity = new Vector3(dir.x * speed * Time.fixedDeltaTime, dir.y * speed * Time.fixedDeltaTime, dir.z * speed * Time.fixedDeltaTime);
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
        Debug.Log(forwardHit.normal);
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
        foreach (var point in UniformPointsOnSphere(SpherePointsCount, cam.forward, ClimbingRaycastsFov))
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
    public static Vector3[] UniformPointsOnSphere(float numberOfPoints)
    {
        List<Vector3> points = new List<Vector3>();
        float i = Mathf.PI * (3 - Mathf.Sqrt(5));
        float offset = 2 / numberOfPoints;
        float halfOffset = 0.5f * offset;
        int currPoint = 0;
        for (; currPoint < numberOfPoints; currPoint++)
        {
            float y = currPoint * offset - 1 + halfOffset;
            float r = Mathf.Sqrt(1 - y * y);
            float phi = currPoint * i;
            Vector3 point = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
            if (!points.Contains(point)) points.Add(point);
        }
        return points.ToArray();
    }
    public static Vector3[] UniformPointsOnSphere(float numberOfPoints,Vector3 LookDir, float MinDotProduct = .5f)
    {
        List<Vector3> points = new List<Vector3>();
        float i = Mathf.PI * (3 - Mathf.Sqrt(5));
        float offset = 2 / numberOfPoints;
        float halfOffset = 0.5f * offset;
        int currPoint = 0;
        for (; currPoint < numberOfPoints; currPoint++)
        {
            float y = currPoint * offset - 1 + halfOffset;
            float r = Mathf.Sqrt(1 - y * y);
            float phi = currPoint * i;
            Vector3 point = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
            if (!points.Contains(point))
            {
                if(Vector3.Dot(LookDir.normalized, point.normalized) > MinDotProduct)
                {
                    points.Add(point);
                }
            }
        }
        return points.ToArray();
    }
}


