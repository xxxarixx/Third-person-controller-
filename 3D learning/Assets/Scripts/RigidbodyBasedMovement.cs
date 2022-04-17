using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyBasedMovement : MonoBehaviour
{
    public PlayerInput input;

    public Rigidbody rb;
    public CustomGravity gravity;
    public Transform cam;

    public float Speed;
    private float _Speed;
    public AnimationCurve JumpHeight;
    private float _jumpHeightProgress;
    bool _isGrounded;
    [Header("Slopes")]
    public float SlopeCheckerSize = .1f;
    [Range(0.01f,1f)]public float MaxSlope;
    
    bool _isSlope;

    public Collider groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;

    public Animator anim;
    public string RunAnim;
    public string IdleAnim;

    private void Start()
    {
        input.OnPressedJump += Input_OnPressedJump;
        _Speed = Speed;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Input_OnPressedJump()
    {
        if (!_isGrounded) { return; }
        _isJumping = true;
        
        Debug.Log("Jumping");
    }
    bool _isJumping = false;
    // Update is called once per frame
    void Update()
    {
        if(_isJumping) _jumpHeightProgress += Time.deltaTime;
        if (_jumpHeightProgress >= JumpHeight.keys[JumpHeight.length - 1].time && _isJumping) { _isJumping = false; _jumpHeightProgress = 0f; }
        //jump
        _isGrounded = Physics.CheckSphere(_getFeetPos(), groundDistance, groundMask);
        //walk
       
    }
    bool _isOnSlope;
    private void FixedUpdate()
    {
        if (_isJumping && _jumpHeightProgress < JumpHeight.keys[JumpHeight.length - 1].time)
        {
            rb.velocity = new Vector3(rb.velocity.x, JumpHeight.Evaluate(_jumpHeightProgress), rb.velocity.z);
        }
        
        Movement();
    }
    private Vector3 _getFeetPos(float offsetX = 0f,float offsetY = 0f,float offsetZ = 0f)
    {
        return groundCheck.bounds.center - new Vector3(0f, groundCheck.bounds.size.y / 2, 0f) + new Vector3(offsetX,offsetY,offsetZ);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_getFeetPos(), groundDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_getSlopeRaycastPos(), _getSlopeRaycastPos() + new Vector3(0f,-SlopeCheckerSize,0f));
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
    public void Movement()
    {
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
            if(rb.velocity.y > 0)
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
}
