using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class RigidbodyFootInteraction : MonoBehaviour
{
    public static RigidbodyFootInteraction instance;
    public RigidbodyBasedMovement rbMovement; 
    [SerializeField]private Animator skeletonAnimator;
    private Transform _leftFoot;
    private Transform _rightFoot;
    [SerializeField] private TwoBoneIKConstraint leftFootIK;
    [SerializeField] private TwoBoneIKConstraint rightFootIK;
    [SerializeField] private Transform heightHandler;
    public float footOffsetY;
    public float bodyOffsetY;
    public float MaxWeight = .9f;
    public bool IsOn = true;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        _leftFoot = skeletonAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        _rightFoot = skeletonAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
    }
    private void Update()
    {
        _currentWeightReachSpeed = WeightReachSpeed * skeletonAnimator.speed;
        heightHandler.localPosition = new Vector3(Mathf.Lerp(heightHandler.localPosition.x, 0f, Time.deltaTime * 2f), Mathf.Clamp(heightHandler.localPosition.y, Mathf.NegativeInfinity,0f), Mathf.Lerp(heightHandler.localPosition.z, 0f, Time.deltaTime * 2f));

    }
    /*
     first solution whenever foot isnt touching floor and distance from floor is hiegher then X then find neareast wall edge and snap foot into it
     
     */
    private float leftFootWeight;
    private float rightFootWeight;
    [SerializeField] private float WeightReachSpeed = 3f;
    private float _currentWeightReachSpeed;
    Quaternion _LeftFootQuat_lastUpdated;
    Vector3 leftFootHitPoint;
    private void OnAnimatorIK()
    {
        if (!IsOn) return;
        var _leftFootSkeletonPos = skeletonAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
        var _rightFootSkeletonPos = skeletonAnimator.GetBoneTransform(HumanBodyBones.RightFoot).position;
        var _leftFootSkeleton = skeletonAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        var _rightFootSkeleton = skeletonAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
        Debug.Log(skeletonAnimator.pivotWeight);
        bool lFootHitting = Physics.Raycast(_leftFootSkeletonPos + (rbMovement.obstacleMaxHeight) * Vector3.up, Vector3.down, out RaycastHit lFHit, rbMovement.obstacleMaxHeight * 2.5f, rbMovement.groundMask);
        bool rFootHitting = Physics.Raycast(_rightFootSkeletonPos + (rbMovement.obstacleMaxHeight) * Vector3.up, Vector3.down, out RaycastHit RFHit, rbMovement.obstacleMaxHeight * 2.5f, rbMovement.groundMask);
        leftFootHitPoint = lFHit.point;
        var lowestDot = (Vector2.Dot(Vector2.up, lFHit.normal) < Vector2.Dot(Vector2.up, RFHit.normal)) ? Vector2.Dot(Vector2.up, lFHit.normal) : Vector2.Dot(Vector2.up, RFHit.normal);
        if(rbMovement.input.GetMoveDirectionInput().magnitude > 0.01f)
        {
            if (skeletonAnimator.pivotWeight < 0.5f)
            {
                if(lFootHitting) leftFootWeight = Mathf.Clamp(leftFootWeight + Time.deltaTime * _currentWeightReachSpeed, 0f, MaxWeight);
                if (rFootHitting) rightFootWeight = Mathf.Clamp(rightFootWeight - Time.deltaTime * _currentWeightReachSpeed, 0f, MaxWeight);
            }
            else
            {
                if (lFootHitting) leftFootWeight = Mathf.Clamp(leftFootWeight - Time.deltaTime * _currentWeightReachSpeed, 0f, MaxWeight);
                if (rFootHitting) rightFootWeight = Mathf.Clamp(rightFootWeight + Time.deltaTime * _currentWeightReachSpeed, 0f, MaxWeight);
            }
        }
        else
        {
            if (lFootHitting) leftFootWeight = Mathf.Clamp(leftFootWeight + Time.deltaTime * (_currentWeightReachSpeed / 1.5f), 0f, MaxWeight);
            if (rFootHitting) rightFootWeight = Mathf.Clamp(rightFootWeight + Time.deltaTime * (_currentWeightReachSpeed / 1.5f), 0f, MaxWeight);
        }
        /*
         if player is facing slope then heigher player torsow
         otherwise is not facing slope then lower player torsow
         */
        var _leftFootYDifPosY = lFHit.point.y - transform.parent.position.y;
        var _rightFootYDifPosY = RFHit.point.y - transform.parent.position.y;
        bool LeftFootLower = false;
        bool RightFootLower = false;
        var lowerYPos = 0f;
        if (_leftFootYDifPosY < _rightFootYDifPosY)
        {
            lowerYPos = _leftFootYDifPosY;
            LeftFootLower = true;
            RightFootLower = false;
        }
        else
        {
            lowerYPos = _rightFootYDifPosY;
            LeftFootLower = false;
            RightFootLower = true;
        }
        float GetLegPos(float curLegAnimationPosY,float newIKposY)
        {
            return (Mathf.Abs(curLegAnimationPosY - newIKposY) > 0.05f) ? Mathf.Abs(curLegAnimationPosY - newIKposY) : 0f;
        }
        if (lFootHitting)
        {
            var curLegAnimation = skeletonAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
            var curLegAnimationPos = skeletonAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
            //Debug.Log($"Left foot Dot: {Vector3.Dot(Vector3.up, lFHit.normal)}");
            //move slightly back based on slope angle bcs when slope is heigher then legs are doing much larger step
            var legDot = Vector3.Dot(Vector3.up, lFHit.normal);
            var legOffsetYBasedOnSlope = 0f;
            // if (isMovingOffSlope()) legOffsetYBasedOnSlope = (footOffsetY + footOffsetY * legDot);
            var newIKpos = new Vector3(curLegAnimationPos.x, /*curLegAnimationPos.y - (lFHit.point.y - curLegAnimationPos.y)*/lFHit.point.y + footOffsetY, curLegAnimationPos.z);
            //Debug.Log((newIKpos.y - curLegAnimationPos.y));
            Debug.Log($"{(curLegAnimationPos.y - newIKpos.y)}");

            skeletonAnimator.SetIKPosition(AvatarIKGoal.LeftFoot,new Vector3(curLegAnimationPos.x, lFHit.point.y + footOffsetY, curLegAnimationPos.z) /*new Vector3(newIKpos.x, Mathf.Abs(curLegAnimationPos.y - lFHit.point.y), newIKpos.z)*/ /*+ (Mathf.Abs(heightHandler.localPosition.y) + skeletonAnimator.rootPosition.y) * Vector3.up*//* (Mathf.Abs(curLegAnimationPos.y - newIKpos.y)) * Vector3.up*//*Mathf.Abs(heightHandler.localPosition.y) * Vector3.up*/);
            skeletonAnimator.SetIKRotation(AvatarIKGoal.LeftFoot, NormalToRotation(_leftFootSkeleton.forward, lFHit.normal));
            skeletonAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootWeight);
            skeletonAnimator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootWeight);
            _LeftFootQuat_lastUpdated = curLegAnimation.rotation;
        }
        if (rFootHitting)
        {
            var curLegAnimation = skeletonAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
            var curLegAnimationPos = skeletonAnimator.GetBoneTransform(HumanBodyBones.RightFoot).position;
            var legDot = Vector3.Dot(Vector3.up, RFHit.normal);
            var legOffsetYBasedOnSlope = 0f;
            // if (isMovingOffSlope()) legOffsetYBasedOnSlope = (footOffsetY + footOffsetY * legDot);
            var newIKpos = new Vector3(curLegAnimationPos.x, /*curLegAnimationPos.y - (RFHit.point.y - curLegAnimationPos.y)*/RFHit.point.y + footOffsetY, curLegAnimationPos.z);
            skeletonAnimator.SetIKPosition(AvatarIKGoal.RightFoot, newIKpos);
            skeletonAnimator.SetIKRotation(AvatarIKGoal.RightFoot, NormalToRotation(_rightFootSkeleton.forward,RFHit.normal));
            skeletonAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootWeight);
            skeletonAnimator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootWeight);
        }
        //if (lowerYPos < rbMovement.obstacleMaxHeight && lowerYPos > -rbMovement.obstacleMaxHeight)
        {
            //skeletonAnimator.rootPosition = new Vector3(skeletonAnimator.rootPosition.x, Mathf.Lerp(skeletonAnimator.rootPosition.y, lowerYPos + bodyOffsetY, Time.deltaTime * 3f), skeletonAnimator.rootPosition.z);
            heightHandler.localPosition = new Vector3(heightHandler.localPosition.x, Mathf.Lerp(heightHandler.localPosition.y, lowerYPos + bodyOffsetY, Time.deltaTime * 3f), heightHandler.localPosition.z);
            //Debug.Log(lowerYPos);
            //heightHandler.localPosition = new Vector3(heightHandler.localPosition.x, lowerYPos, heightHandler.localPosition.z);
        }
    }
    private void OnDrawGizmos()
    {
        if(_leftFoot == null) _leftFoot = skeletonAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        if (_rightFoot == null) _rightFoot = skeletonAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(_leftFoot.position + .5f * Vector3.up, Vector3.one * .03f);
        Universal_RaycastAssistance.instance.DrawRaycastGizmo(_leftFoot.position + (rbMovement.obstacleMaxHeight) * Vector3.up, Vector3.down, rbMovement.obstacleMaxHeight * 2.5f);
        Gizmos.DrawSphere(/*skeletonAnimator.GetIKPosition(AvatarIKGoal.LeftFoot) - Mathf.Abs(_leftFoot.position.y - skeletonAnimator.GetIKPosition(AvatarIKGoal.LeftFoot).y) * Vector3.up*/leftFootHitPoint, .1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(_rightFoot.position + .5f * Vector3.up, Vector3.one * .03f);
        Universal_RaycastAssistance.instance.DrawRaycastGizmo(_rightFoot.position + (rbMovement.obstacleMaxHeight) * Vector3.up, Vector3.down, rbMovement.obstacleMaxHeight * 2.5f);
        Gizmos.DrawSphere(skeletonAnimator.GetIKPosition(AvatarIKGoal.RightFoot) - Mathf.Abs(_rightFoot.position.y - skeletonAnimator.GetIKPosition(AvatarIKGoal.RightFoot).y) * Vector3.up, .1f);

        var moveDir = transform.forward;
        if (Application.isPlaying) moveDir = rbMovement.GetMoveDirection();
        Gizmos.color = Color.red;
        Universal_RaycastAssistance.instance.DrawRaycastGizmo(rbMovement._GetFeetPos(0f,.1f), moveDir, rbMovement.maxLengthFromPlayerToObstacle);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(skeletonAnimator.rootPosition, .1f);
    }
    private bool isMovingOffSlope()
    {
        return !Physics.Raycast(rbMovement._GetFeetPos(0f, .1f), rbMovement.GetMoveDirection(), rbMovement.maxLengthFromPlayerToObstacle);
    }
    private float DistanceFromFootToToe_Left()
    {
        return Vector3.Distance(skeletonAnimator.GetBoneTransform(HumanBodyBones.LeftToes).position, skeletonAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).position);
    }
    private float DistanceFromFootToToe_Right()
    {
        return Vector3.Distance(skeletonAnimator.GetBoneTransform(HumanBodyBones.RightToes).position, skeletonAnimator.GetBoneTransform(HumanBodyBones.RightFoot).position);
    }
    public Quaternion rotationFromTwoVectors(Vector3 v1Pos,Vector3 v2Pos)
    {
        return Quaternion.FromToRotation(Vector3.forward, (v2Pos - v1Pos).normalized);
    }
    public Quaternion NormalToRotation(Vector3 LookDirection, Vector3 normal)
    {
        return Quaternion.LookRotation(Vector3.ProjectOnPlane(LookDirection, normal), normal);
    }
}
