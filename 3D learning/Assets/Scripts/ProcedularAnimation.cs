using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcedularAnimation : MonoBehaviour
{

    //1. https://cjacksonartworks.com/projects/8JRvQ run/move motion curve
    //2. https://www.youtube.com/watch?v=LEwYmFT3xDk animation rigging tools
    //3. https://www.youtube.com/watch?v=e6Gjhr1IP6w steps to make procedular animation
    //4. https://www.youtube.com/watch?v=PryJ3CpHcXQ&list=PL87LLcjGQEfOUKsxvE-1KcfOJofr7y-ek&index=106 explained how legs are placed on any terrain

    //5. https://www.youtube.com/watch?v=1A_0AxOvey0 exmaple already done

    //zrobic dwa curve'y jeden robiacy motion do przodu a drugi do tylu na podstawie 1. filmiku 
    #region OldVersion
    /* public Transform LeftFootTarget;
     public Transform RightFootTarget;
     public float FootdetectionRange = .1f;
     public LayerMask groundLayer;
     public float ForwardOffset = 2f;
     public float OffsetLegsV = .25f;
     Vector3 OldLeftFootPosition,LeftFootPosition, LeftFootNormal;
     Vector3 OldRightFootPosition,RightFootPosition, RightFootNormal;
     Rigidbody rb;
     private void Start()
     {
         rb = GetComponent<Rigidbody>();
         OldLeftFootPosition = LeftFootPosition = LeftFootTarget.transform.position + transform.forward * OffsetLegsV;
         OldRightFootPosition = RightFootPosition = RightFootTarget.transform.position - transform.forward * OffsetLegsV;
     }

     private void OnDrawGizmos()
     {
         Gizmos.color = Color.red;
         Gizmos.DrawLine(transform.position + new Vector3(.2f, FootdetectionRange / 2, 0f) + transform.forward * ForwardOffset + transform.forward * OffsetLegsV, transform.position + new Vector3(.2f,0f,0f) + transform.forward * ForwardOffset + Vector3.down * FootdetectionRange +transform.forward * OffsetLegsV); //left
         Gizmos.DrawLine(transform.position + new Vector3(-.2f, FootdetectionRange / 2, 0f) + transform.forward * ForwardOffset - transform.forward * OffsetLegsV, transform.position + new Vector3(-.2f, 0f, 0f) + transform.forward * ForwardOffset + Vector3.down * FootdetectionRange - transform.forward * OffsetLegsV); //right

         Gizmos.color = Color.green;
         Gizmos.DrawWireSphere(LeftFootPosition, .1f);
         Gizmos.DrawWireSphere(RightFootPosition, .1f);
     }
     bool CanUpdateFoot(Vector3 footPosition,Vector3 offset, float Distance = .5f)
     {
         return Vector3.Distance(transform.position + offset, footPosition) > Distance;
     }
     public float MoveSpeed = 1f;
     public float SmoothSpeedMultiplayer = 1f;
     float LerpProgress = 0f;
     private void Update()
     {
         var leftFootHitted = Physics.Raycast(transform.position + new Vector3(.2f, FootdetectionRange / 2, 0f) + transform.forward * ForwardOffset + transform.forward * OffsetLegsV, Vector3.down, out RaycastHit LeftHit, FootdetectionRange, groundLayer);//left
         var RightFootHitted = Physics.Raycast(transform.position + new Vector3(-.2f, FootdetectionRange / 2, 0f) + transform.forward * ForwardOffset - transform.forward * OffsetLegsV, Vector3.down, out RaycastHit RightHit, FootdetectionRange, groundLayer);//right
         if (leftFootHitted && CanUpdateFoot(LeftFootPosition, transform.forward * OffsetLegsV, 1f))
         {
             OldLeftFootPosition = LeftFootPosition;
             LeftFootTarget.transform.position = LeftFootPosition;
             LeftFootPosition = LeftHit.point;
             LeftFootNormal = LeftHit.normal;
             LerpProgress = 0f;
         }
         if(RightFootHitted && CanUpdateFoot(RightFootPosition, transform.forward * -OffsetLegsV, 1f))
         {
             OldRightFootPosition = RightFootPosition;
             RightFootTarget.transform.position = RightFootPosition;
             RightFootPosition = RightHit.point;
             RightFootNormal = RightHit.normal;
             LerpProgress = 0f;
         }
         if(LerpProgress <= 1f)
         {
             LerpProgress += Time.deltaTime * SmoothSpeedMultiplayer; 
             if(LerpProgress >= 1f)
             {
                 LeftFootTarget.transform.position = LeftFootPosition;
                 RightFootTarget.transform.position = RightFootPosition;
             }
         }
         LeftFootTarget.transform.position = Vector3.Lerp(OldLeftFootPosition, LeftFootPosition, LerpProgress);
         RightFootTarget.transform.position = Vector3.Lerp(OldRightFootPosition, RightFootPosition, LerpProgress);
     }
     Quaternion NormalToRotation(Quaternion rotation, Transform From, Vector3 normal)
     {
         return Quaternion.FromToRotation(From.up, normal) * rotation;
     }
     public bool ActiveMovement = true;
     private void FixedUpdate()
     {
         if (!ActiveMovement) return;
         var velocity = -transform.forward.normalized * MoveSpeed * Time.fixedDeltaTime;
         rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
     }*/
    #endregion

    public Transform LeftLegTarget;
    public Transform RightLegTarget;
    public LayerMask groundLayer;
    public float GroundRaycastLength = 1f;
    public float LegsOffset = .25f;
    public float OffsetGroundRaycastY = .5f;
    public float DistanceLegsFromBodyToMove = .5f;
    public float MoveLerpMultiplayer = 1f;
    private Vector3 _LeftLegOldPosition,_LeftLegDifPosition, _LeftLegNewPosition;
    private Vector3 _RightLegOldPosition, _RightLegDifPosition, _RightLegNewPosition;

    public bool ActiveMovement;
    public Rigidbody rb;
    public float MoveSpeed = 125f;
    public AnimationCurve MoveForwardCurve;
    public AnimationCurve MoveBackwardCurve;
    [Header("Gizmos")]
    [SerializeField]private float _Giz_LegPlacementSize = .1f;
    private void Start()
    {
        _LeftLegDifPosition = (LeftLegTarget.transform.position - transform.position);
        _RightLegDifPosition = (RightLegTarget.transform.position - transform.position);
        _LeftLegOldPosition = LeftLegTarget.transform.position = GetLegWorldPosition(_LeftLegDifPosition);
        _RightLegOldPosition = RightLegTarget.transform.position = GetLegWorldPosition(_RightLegDifPosition);
    }
    float MoveLegProgress = 0f;
    private void Update()
    {
        #region Left Leg
        {
            var leftLegHit = Physics.Raycast(GetLegWorldPosition(_LeftLegDifPosition) + _Legoffsets(true) + V3OffsetY(OffsetGroundRaycastY), Vector3.down, out RaycastHit hit, GroundRaycastLength, groundLayer); //left Leg Raycast
            if (leftLegHit)
            {
                _LeftLegNewPosition = hit.point;
                if (CanMoveLeg(LeftLegTarget))
                {
                    //
                    MoveLegProgress = 0f;
                    Debug.Log("OldLegNewPosition");
                }
            }
        }
        #endregion
        #region Right Leg
        {
            var RightLegHit = Physics.Raycast(GetLegWorldPosition(_RightLegDifPosition) + _Legoffsets(false) + V3OffsetY(OffsetGroundRaycastY), Vector3.down, out RaycastHit hit, GroundRaycastLength, groundLayer); //left Leg Raycast
            if (RightLegHit)
            {
                _RightLegNewPosition = hit.point;
                if (CanMoveLeg(RightLegTarget))
                {
                    //_RightLegOldPosition = _RightLegNewPosition;
                    MoveLegProgress = 0f;
                }
            }
        }
        if(MoveLegProgress <= 1f)
        {
            MoveLegProgress += Time.deltaTime  * MoveLerpMultiplayer;
            LeftLegTarget.transform.position = Vector3.Lerp(_LeftLegOldPosition, _LeftLegNewPosition, MoveLegProgress);
            RightLegTarget.transform.position = Vector3.Lerp(_RightLegOldPosition, _RightLegNewPosition, MoveLegProgress);
            _LeftLegOldPosition = LeftLegTarget.transform.position;
            _RightLegOldPosition = RightLegTarget.transform.position;
            if (Vector3.Distance(LeftLegTarget.transform.position, _LeftLegNewPosition) < .1f || Vector3.Distance(RightLegTarget.transform.position, _RightLegNewPosition) < .1f) MoveLegProgress = 1.2f;
        }
        else
        {
            SetLegPosition(LeftLegTarget, _LeftLegOldPosition);
            SetLegPosition(RightLegTarget, _RightLegOldPosition);
        }

        #endregion
    }
    private void FixedUpdate()
    {
        if (!ActiveMovement) return;
        var velocity = transform.forward.normalized * MoveSpeed * Time.fixedDeltaTime;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }
    private void OnDrawGizmos()
    {
        DrawGizmosRaycast(GetLegWorldPosition(LeftLegTarget) + V3OffsetY(OffsetGroundRaycastY) + _Legoffsets(true), Vector3.down, GroundRaycastLength);
        DrawGizmosRaycast(GetLegWorldPosition(RightLegTarget) + V3OffsetY(OffsetGroundRaycastY) + _Legoffsets(false), Vector3.down, GroundRaycastLength);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_LeftLegNewPosition, _Giz_LegPlacementSize);
        Gizmos.DrawSphere(_RightLegNewPosition, _Giz_LegPlacementSize);

        Gizmos.DrawWireSphere(GetLegWorldPosition(LeftLegTarget) + _Legoffsets(true) + transform.forward * DistanceLegsFromBodyToMove, _Giz_LegPlacementSize);// left leg forward
        Gizmos.DrawWireSphere(GetLegWorldPosition(RightLegTarget) + _Legoffsets(false) + transform.forward * DistanceLegsFromBodyToMove, _Giz_LegPlacementSize); // right leg forward
        Gizmos.DrawWireSphere(GetLegWorldPosition(LeftLegTarget) + _Legoffsets(true) - transform.forward * DistanceLegsFromBodyToMove, _Giz_LegPlacementSize); //left leg Backward
        Gizmos.DrawWireSphere(GetLegWorldPosition(RightLegTarget) + _Legoffsets(false) - transform.forward * DistanceLegsFromBodyToMove, _Giz_LegPlacementSize); // right leg backward
    }
    private void DrawGizmosRaycast(Vector3 pivolt, Vector3 direction, float Length)
    {
        Gizmos.DrawLine(pivolt, pivolt + direction.normalized * Length);
    }
    private bool CanMoveLeg(Transform FootCurrentPosition)
    {
        return Vector3.Distance(transform.position, FootCurrentPosition.transform.position) > DistanceLegsFromBodyToMove;
    }
    private Vector3 _Legoffsets(bool LeftLeg)
    {
        if (LeftLeg)
        {
            return transform.forward * (LegsOffset * -1);
        }
        else
        {
            return transform.forward * LegsOffset;
        }
        //return Vector3.zero;
    }
    private Vector3 V3OffsetY(float YOffsetValue = .5f)
    {
        return Vector3.up * YOffsetValue;
    }
    private Vector3 GetLegWorldPosition(Vector3 FootDifferencePosition)
    {
        return transform.position + FootDifferencePosition;
    }
    private Vector3 GetLegWorldPosition(Transform FootTransform)
    {
       return transform.position + (FootTransform.transform.position - transform.position);
    }
    private void SetLegPosition(Transform Target, Vector3 newPosition)
    {
        Target.transform.position = newPosition;
    }
}
