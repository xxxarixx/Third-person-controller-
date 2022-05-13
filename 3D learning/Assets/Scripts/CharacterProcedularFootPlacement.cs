using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class CharacterProcedularFootPlacement : MonoBehaviour
{
    public Transform LeftLegTarget, LeftLegToe;
    public Transform RightLegTarget, RightLegToe;
    public TwoBoneIKConstraint LeftLegRig, RightLegRig;
    public LayerMask groundLayer;
    public float GroundRaycastLength = 1f;
    private Vector3 _LeftLegOldPosition, _LeftLegDifPosition, _LeftLegNewPosition;
    private Vector3 _RightLegOldPosition, _RightLegDifPosition, _RightLegNewPosition;
    public bool ActiveMovement;
    public Rigidbody rb;
    public float MoveSpeed = 125f;
    public float OffsetToUpDivided = 1.20f;
    private void Start()
    {
        _LeftLegDifPosition = (LeftLegTarget.transform.position - transform.position);
        _RightLegDifPosition = (RightLegTarget.transform.position - transform.position);
    }
    private void Update()
    {
        {
            var leftLegHit = Physics.Raycast(GetLegWorldPosition(_LeftLegDifPosition) + V3OffsetY(GroundRaycastLength / OffsetToUpDivided), -LeftLegToe.transform.up, out RaycastHit hit, GroundRaycastLength, groundLayer); //left Leg Raycast
            if (leftLegHit)
            {
                LeftLegRig.weight = 1f;
                Debug.Log("LeftLegHit");
                _LeftLegNewPosition = hit.point;
            }
            SetLegPosition(LeftLegTarget, _LeftLegNewPosition);
        }
        {
            var RightLegHit = Physics.Raycast(GetLegWorldPosition(_RightLegDifPosition) + V3OffsetY(GroundRaycastLength / OffsetToUpDivided), -RightLegToe.transform.up, out RaycastHit hit, GroundRaycastLength, groundLayer); //left Leg Raycast
            if (RightLegHit)
            {
                RightLegRig.weight = 1f;
                Debug.Log("RightLegHit");
                _RightLegNewPosition = hit.point;
            }
            SetLegPosition(RightLegTarget, _RightLegNewPosition);
        }
    }
    private void FixedUpdate()
    {
        if (!ActiveMovement) return;
        var velocity = transform.forward.normalized * MoveSpeed * Time.fixedDeltaTime;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        DrawGizmosRaycast(GetLegWorldPosition(LeftLegTarget) + V3OffsetY(GroundRaycastLength / OffsetToUpDivided), -LeftLegToe.transform.up, GroundRaycastLength);
        DrawGizmosRaycast(GetLegWorldPosition(RightLegTarget) + V3OffsetY(GroundRaycastLength / OffsetToUpDivided), -RightLegToe.transform.up, GroundRaycastLength);
    }
    private void DrawGizmosRaycast(Vector3 pivolt, Vector3 direction, float Length)
    {
        Gizmos.DrawLine(pivolt, pivolt + direction * Length);
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
