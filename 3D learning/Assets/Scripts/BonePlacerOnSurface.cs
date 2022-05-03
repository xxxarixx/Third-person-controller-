using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEditor;
public class BonePlacerOnSurface : MonoBehaviour
{
    /*[Header("Bones")]
    [SerializeField]private TwoBoneIKConstraint LeftLegIK;
    [SerializeField] private TwoBoneIKConstraint RightLegIK;
    [SerializeField] private Transform LeftSkeletonLeg;
    [SerializeField] private Transform RightSkeletonLeg;
    [SerializeField] private Transform LeftLegTarget;
    [SerializeField] private Transform RightLegTarget;
    [Header("Veriables")]
    public float OffsetForward = .2f;
    public float OffsetBackward = -.2f;
    public float offsetUp = -.2f;
    public float OffsetYOnFootPlace = .1f;
    public float SmoothSpeedMultiplayer = 3f;
    public Rigidbody rb;
    public bool ActiveMovement;
    public float MoveSpeed = 100f;
    public bool ActiveIK = true;
    public bool LeftFoothitting;
    public bool RightFootHitting;
    [SerializeField] private LayerMask groundMask;
    public float DistanceToGround = .1f;
    private void Update()
    {
        IkProcced();
    }
    private void FixedUpdate()
    {
        


        if(!ActiveMovement) return;
        var velocity = transform.forward.normalized * MoveSpeed * Time.fixedDeltaTime;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }
    private float LeftFootRotProgress;
    private float RightFootRotProgress;
    private void IkProcced()
    {
        if (!ActiveIK) return;
        LeftFoothitting = Physics.Raycast(LeftSkeletonLeg.position + new Vector3(0f, DistanceToGround / 2 + offsetUp, 0f) + LeftSkeletonLeg.forward * OffsetForward, -LeftSkeletonLeg.up, out RaycastHit Lhit, DistanceToGround, groundMask);
        RightFootHitting = Physics.Raycast(RightSkeletonLeg.position + new Vector3(0f, DistanceToGround / 2 + offsetUp, 0f) + RightSkeletonLeg.forward * OffsetForward, -RightSkeletonLeg.up, out RaycastHit Rhit, DistanceToGround, groundMask);
        var LeftFoothittingBack = Physics.Raycast(LeftSkeletonLeg.position + new Vector3(0f, DistanceToGround / 2 + offsetUp, 0f) + LeftSkeletonLeg.forward * OffsetBackward, -LeftSkeletonLeg.up, out RaycastHit LhitB, DistanceToGround, groundMask);
        var RightFootHittingBack = Physics.Raycast(RightSkeletonLeg.position + new Vector3(0f, DistanceToGround / 2 + offsetUp, 0f) + RightSkeletonLeg.forward * OffsetBackward, -RightSkeletonLeg.up, out RaycastHit RhitB, DistanceToGround, groundMask);
        if (LeftFoothittingBack*//* && LeftFoothitting*//*)
        {
            LeftLegTarget.position = LhitB.point + LeftLegTarget.up * OffsetYOnFootPlace;
            LeftLegIK.weight += Mathf.Clamp01(Time.deltaTime * SmoothSpeedMultiplayer);

            //if(LeftFootRotProgress <= 1f) LeftFootRotProgress += Mathf.Clamp01(Time.deltaTime * SmoothSpeedMultiplayer);
            //LeftLegTarget.rotation = Quaternion.Lerp(LeftLegTarget.rotation,NormalToRotation(LeftLegTarget.rotation,LeftLegTarget, LhitB.normal),LeftFootRotProgress);
        }
       *//* else
        if (LeftFoothitting)
        {
            LeftLegTarget.position = Lhit.point + LeftLegTarget.up * OffsetYOnFootPlace;
            LeftLegIK.weight += Mathf.Clamp01(Time.deltaTime * SmoothSpeedMultiplayer);
            //if(LeftFootRotProgress >= 0f) LeftFootRotProgress = -0.1f;
            //LeftLegTarget.rotation = Quaternion.Lerp(LeftLegTarget.rotation, NormalToRotation(LeftLegTarget.rotation, LeftLegTarget, Lhit.normal), LeftFootRotProgress);
        }*//*
        else
        {
            LeftLegIK.weight = 0f;
        }
        if (RightFootHittingBack *//*&& RightFootHitting*//*)
        {
            RightLegTarget.position = RhitB.point + RightLegTarget.up * OffsetYOnFootPlace;
            RightLegIK.weight += Time.deltaTime * SmoothSpeedMultiplayer;
           // if(RightFootRotProgress <= 1f) RightFootRotProgress += Mathf.Clamp01(Time.deltaTime * SmoothSpeedMultiplayer);
            //RightLegTarget.rotation = Quaternion.Lerp(RightLegTarget.rotation, NormalToRotation(RightLegTarget.rotation, RightLegTarget, RhitB.normal), RightFootRotProgress);
        }
        *//*else
        if (RightFootHitting)
        {
            RightLegTarget.position = Rhit.point + RightLegTarget.up * OffsetYOnFootPlace;
            RightLegIK.weight += Mathf.Clamp01(Time.deltaTime * SmoothSpeedMultiplayer);
            //if(RightFootRotProgress >= 0f) RightFootRotProgress = -0.1f;
            //RightLegTarget.rotation = Quaternion.Lerp(RightLegTarget.rotation, NormalToRotation(RightLegTarget.rotation, RightLegTarget, Rhit.normal), RightFootRotProgress);
        }*//*
        else
        {
            RightLegIK.weight = 0f;
        }
    }
    private void OnDrawGizmos()
    {
        DrawRaycastGizmo(LeftSkeletonLeg.position + new Vector3(0f, DistanceToGround / 2  + offsetUp, 0f) + LeftSkeletonLeg.forward * OffsetForward, -LeftSkeletonLeg.up, DistanceToGround);
        DrawRaycastGizmo(RightSkeletonLeg.position + new Vector3(0f, DistanceToGround / 2 + offsetUp, 0f) + RightSkeletonLeg.forward * OffsetForward, -RightSkeletonLeg.up, DistanceToGround);

        DrawRaycastGizmo(LeftSkeletonLeg.position + new Vector3(0f, DistanceToGround / 2 + offsetUp, 0f) + LeftSkeletonLeg.forward * OffsetBackward, -LeftSkeletonLeg.up, DistanceToGround);
        DrawRaycastGizmo(RightSkeletonLeg.position + new Vector3(0f, DistanceToGround / 2 + offsetUp, 0f) + RightSkeletonLeg.forward * OffsetBackward, -RightSkeletonLeg.up, DistanceToGround);
    }
    private void DrawRaycastGizmo(Vector3 origin, Vector3 direciton, float Distance)
    {
        Gizmos.DrawLine(origin, origin + direciton * Distance);
    }
    public Quaternion NormalToRotation(Quaternion currentRoation, Transform transform, Vector3 normal)
    {
        return Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, normal), normal);
    }*/

    [Header("Bones")]
    [SerializeField]private TwoBoneIKConstraint LeftLegIK;
    [SerializeField] private TwoBoneIKConstraint RightLegIK;
    private Transform LeftFoot;
    private Transform RightFoot;
    private Transform LeftKnee;
    private Transform RightKnee;
    private Transform LeftToe;
    private Transform RightToe;
    [SerializeField] private Transform LeftLegTarget;
    [SerializeField] private Transform RightLegTarget;
    [SerializeField] private Transform SkeletonOrigin;
    public Transform LeftFootAnimPos;
    public Transform RightFootAnimPos;
    [Header("Other veriables:")]
    public Animator skeletonAnimator;
    [SerializeField] private LayerMask groundMask;
    public float OffsetDistanceToGround = .1f;
    public Vector3 offsetToe;
    public Rigidbody rb;
   // public bool ActiveMovement;
    //public float MoveSpeed = 100f;
    public float OffsetYOnFootPlace = 0f;
    private float _LeftFootWeight;
    private float _RightFootWeight;
    public bool LeftToeHitting;
    public bool RightToeHitting;
    public float BodyPositoon;
    private void _ConfigureBodyParts()
    {
        LeftFoot = skeletonAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        RightFoot = skeletonAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
        LeftKnee = skeletonAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        RightKnee = skeletonAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        LeftToe = skeletonAnimator.GetBoneTransform(HumanBodyBones.LeftToes);
        RightToe = skeletonAnimator.GetBoneTransform(HumanBodyBones.RightToes);
    }
    private void Awake()
    {
        _ConfigureBodyParts();
        LeftLegIK.weight = 1f;
        RightLegIK.weight = 1f; 
    }
    private float SkeletonOriginProgress = 0f;
    private void FixedUpdate()
    {
        if(rb.velocity.sqrMagnitude > .1f)
        {

            var moveDirection = transform.forward;



        }



        LeftFootAnimPos.position = skeletonAnimator.GetIKPosition(AvatarIKGoal.LeftFoot);
        RightFootAnimPos.position = skeletonAnimator.GetIKPosition(AvatarIKGoal.RightFoot);
        /*if(SkeletonOrigin.localPosition.y != OffsetYOnFootPlace)
        {
            SkeletonOrigin.localPosition = new Vector3(SkeletonOrigin.localPosition.x, OffsetYOnFootPlace, SkeletonOrigin.localPosition.z);
        }*/
        var LeftFoothitting = Physics.Raycast(LeftFootAnimPos.position + new Vector3(0f, GetKneeToeHeight(LeftKnee, LeftFootAnimPos), 0f) + offsetToe, -transform.up, out RaycastHit Lhit, OffsetDistanceToGround + GetKneeToeHeight(LeftKnee, LeftFootAnimPos), groundMask);
        var RightFootHitting = Physics.Raycast(RightFootAnimPos.position + new Vector3(0f, GetKneeToeHeight(RightKnee, RightFootAnimPos), 0f) + offsetToe, -transform.up, out RaycastHit Rhit, OffsetDistanceToGround + GetKneeToeHeight(RightKnee, RightFootAnimPos), groundMask);
        if (LeftFoothitting)
        {
            LeftLegTarget.position = Lhit.point + Lhit.normal * OffsetYOnFootPlace;
            LeftLegTarget.rotation = NormalToRotation(LeftLegTarget.rotation, LeftFoot, Lhit.normal);
        }
        /*else
        {
            LeftLegTarget.position = LeftFoot.position;
        }*/
        
        if (RightFootHitting)
        {
            RightLegTarget.position = Rhit.point + Rhit.normal * OffsetYOnFootPlace;
            RightLegTarget.rotation = NormalToRotation(RightLegTarget.rotation, RightFoot, Rhit.normal);

        }
        float LegDifferenceMultiplayer = 1f;
        //if(Lhit.normal != Vector3.up || Rhit.normal != Vector3.up) LegDifferenceMultiplayer = 4f; else LegDifferenceMultiplayer = 1f;

        Debug.Log("isonSlope");
        if (LeftFoothitting || RightFootHitting) 
        {
            if(Rhit.point.y - Lhit.point.y > .1f || Rhit.point.y - Lhit.point.y < .1f)
            {
                if (Rhit.point.y < Lhit.point.y)
                {
                    //right leg is Lower
                    Debug.Log("right leg is Lower");
                    var legDifference = Rhit.point.y - Lhit.point.y;
                    SkeletonOrigin.localPosition = new Vector3(SkeletonOrigin.localPosition.x, legDifference / LegDifferenceMultiplayer, SkeletonOrigin.localPosition.z);

                }
                else
                {
                    //left leg is lower
                    Debug.Log("left leg is Lower");
                    var legDifference = Lhit.point.y - Rhit.point.y;
                    SkeletonOrigin.localPosition = new Vector3(SkeletonOrigin.localPosition.x, legDifference / LegDifferenceMultiplayer, SkeletonOrigin.localPosition.z);
                }
            }
        }
            /*if(Rhit.point.y - transform.position.y > Lhit.point.y - transform.position.y)
            {
                if (-(transform.position.y -) / 2 > -1f)
                {
                    //right leg is lower
                    //SkeletonOrigin.localPosition = Vector3.Lerp(SkeletonOrigin.localPosition, new Vector3(SkeletonOrigin.localPosition.x, -Rhit.normal.y / 5, SkeletonOrigin.localPosition.z), SkeletonOriginProgress);
                    SkeletonOrigin.localPosition = new Vector3(SkeletonOrigin.localPosition.x, -(transform.position.y - Rhit.point.y) / 2, SkeletonOrigin.localPosition.z);
                }
            }
            else
            {
                if(-(transform.position.y - Lhit.point.y) / 2 > -1f)
                {
                    //left leg is lower
                    SkeletonOrigin.localPosition = new Vector3(SkeletonOrigin.localPosition.x, -(transform.position.y - Lhit.point.y) / 2, SkeletonOrigin.localPosition.z);
                }
            }*/
            //SkeletonOriginProgress += Time.deltaTime;
        
        /*else
        {
            SkeletonOrigin.localPosition = Vector3.zero;
            SkeletonOriginProgress = 0f;

        }*/
        /*else
        {
            RightLegTarget.position = RightFoot.position;
        }*/


        /*LeftToeHitting = Physics.CheckBox(LeftToe.position + new Vector3(0f, .07f, 0f), new Vector3(.1f, 0.02f, .2f), LeftToe.rotation, groundMask);
        RightToeHitting = Physics.CheckBox(RightToe.position + new Vector3(0f, .07f, 0f), new Vector3(.1f, 0.02f, .2f), RightToe.rotation, groundMask);
        if (LeftToeHitting)
        {
            var LeftToehitting = Physics.Raycast(LeftToe.position + GetKneeToeHeight(LeftKnee, LeftToe) * Vector3.up + offsetToe, -Vector2.up, out LThit, OffsetDistanceToGround + GetKneeToeHeight(LeftKnee, LeftToe), groundMask);
            if (LeftToehitting)
            {
                LeftLegTarget.position = LThit.point + LThit.normal * OffsetYOnFootPlace;
                //LeftLegTarget.rotation = NormalToRotation(LeftLegTarget.rotation, LeftLegTarget, LThit.normal);
                _LeftFootWeight += Time.deltaTime * 9f;
            }
        }
        if (RightToeHitting)
        {
            var RightToeHitting = Physics.Raycast(RightToe.position + GetKneeToeHeight(RightKnee, RightToe) * Vector3.up + offsetToe, -Vector2.up, out RThit, OffsetDistanceToGround + GetKneeToeHeight(RightKnee, RightToe), groundMask);
            if (RightToeHitting)
            {
                RightLegTarget.position = RThit.point + RThit.normal * OffsetYOnFootPlace;
                //RightLegTarget.rotation = NormalToRotation(RightLegTarget.rotation, RightLegTarget, RThit.normal);
                _RightFootWeight += Time.deltaTime * 9f;
            }
        }
        if(!LeftToeHitting && !RightToeHitting)
        {
            _LeftFootWeight = skeletonAnimator.GetFloat("LeftFoot");
            _RightFootWeight = skeletonAnimator.GetFloat("RightFoot");
        }

        LeftLegIK.weight = _LeftFootWeight;
        RightLegIK.weight = _RightFootWeight;*/
        //_Movement();
    }
    RaycastHit LThit = new RaycastHit();
    RaycastHit RThit = new RaycastHit();
    /*private void _Movement()
    {
        if (!ActiveMovement) return;
        var velocity = transform.forward.normalized * MoveSpeed * Time.fixedDeltaTime;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }*/
    private void OnDrawGizmos()
    {
        _ConfigureBodyParts();
        
        Gizmos.color = Color.white;
        DrawRaycastGizmo(LeftFoot.position + new Vector3(0f, GetKneeToeHeight(LeftKnee, LeftFoot), 0f) + offsetToe, -transform.up, OffsetDistanceToGround + GetKneeToeHeight(LeftKnee, LeftFoot));
        DrawRaycastGizmo(RightFoot.position + new Vector3(0f, GetKneeToeHeight(RightKnee, RightFoot), 0f) + offsetToe, -transform.up, OffsetDistanceToGround + GetKneeToeHeight(RightKnee, RightFoot));
        Gizmos.color = Color.green;
        /*Gizmos.matrix = LeftToe.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(0f,.07f,0f), new Vector3(.1f,0.02f,.2f));
        Gizmos.matrix = RightToe.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(0f, .07f, 0f), new Vector3(.1f, 0.02f, .2f));*/


        /* Gizmos.color = Color.yellow;
         Gizmos.matrix = Matrix4x4.identity;
         DrawRaycastGizmo(LeftToe.position + GetKneeToeHeight(LeftKnee, LeftToe) * Vector3.up, -Vector2.up, OffsetDistanceToGround + GetKneeToeHeight(LeftKnee, LeftToe));

         Gizmos.DrawSphere(LThit.point + LThit.normal * OffsetYOnFootPlace, 0.05f);

         Gizmos.color = Color.blue;
         Gizmos.matrix = Matrix4x4.identity;
         DrawRaycastGizmo(RightToe.position + GetKneeToeHeight(RightKnee, RightToe) * Vector3.up, -Vector2.up, OffsetDistanceToGround + GetKneeToeHeight(RightKnee, RightToe));

         Gizmos.DrawSphere(RThit.point + RThit.normal * OffsetYOnFootPlace, 0.05f);*/

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(LeftLegTarget.position, .1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(RightLegTarget.position, .1f);


        Gizmos.color = Color.black;
        Debug.Log(rb.velocity.magnitude);
        var futureLeftLegPos = skeletonAnimator.GetIKPosition(AvatarIKGoal.LeftFoot) + transform.forward.normalized * rb.velocity.magnitude * .3f;
        var lHitting = Physics.Raycast(futureLeftLegPos + new Vector3(0f, 2f, 0f), Vector3.down, out RaycastHit Lhit, Mathf.Infinity, groundMask);
        if(lHitting) Gizmos.DrawSphere(Lhit.point, .1f);

        var futureRightLegPos = skeletonAnimator.GetIKPosition(AvatarIKGoal.RightFoot) + transform.forward.normalized * rb.velocity.magnitude * .3f;
        var rHitting = Physics.Raycast(futureRightLegPos + new Vector3(0f, 2f, 0f), Vector3.down, out RaycastHit Rhit, Mathf.Infinity, groundMask);
        if(rHitting) Gizmos.DrawSphere(Rhit.point, .1f);
    }
    private float GetKneeToeHeight(Transform knee, Transform Toe)
    {
        return knee.position.y - Toe.position.y;
    }

    private void DrawRaycastGizmo(Vector3 origin, Vector3 direciton, float Distance)
    {
        Gizmos.DrawLine(origin, origin + direciton * Distance);
    }
    public Quaternion NormalToRotation(Quaternion currentRoation, Transform transform, Vector3 normal)
    {
        return Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, normal), normal);
    }
    /*public Animator skeletonAnimator;
    public Rigidbody rb;
    private Transform _LeftFoot;
    private Transform _RightFoot;
    public AnimationCurve LeftFootCurve;
    public AnimationCurve RightFootCurve;
    private float _LeftFootWeight;
    private float _RightFootWeight;
    private float _Footsprogress;
    public bool isMoving;
    public TwoBoneIKConstraint LeftFootIK;
    public TwoBoneIKConstraint RightFootIK;
    private void Awake()
    {
        _LeftFoot = skeletonAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        _RightFoot = skeletonAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
    }
    private void Start()
    {
        LeftFootIK.weight = 1f;
        RightFootIK.weight = 1f;
    }
    private void Update()
    {

        //if (rb.velocity.sqrMagnitude > .1f) isMoving = true; else isMoving = false;
        if (isMoving)
        {
            _Footsprogress += Time.deltaTime;
            if(_Footsprogress >= 1f)
            {
                _Footsprogress = 0f;
            }

        }
        else
        {
            _Footsprogress = 0f;
        }
        _LeftFootWeight = skeletonAnimator.GetFloat("LeftFoot");
        _RightFootWeight = skeletonAnimator.GetFloat("RightFoot");


        //LeftFootIK.weight = _LeftFootWeight;
        //RightFootIK.weight = _RightFootWeight;
    }
    private void OnDrawGizmos()
    {
        
    }*/



}
class CurveCreator
{
    public Animator SkeletonAnimator;
    private Transform leftFoot;
    private Transform rightFoot;
    public string ClipName = "Run";
    public AnimationCurve LeftFootCurve;
    public AnimationCurve RightFootCurve;
    private bool LeftFootFounded;
    private bool RightFootFounded;

    public AnimationCurve Repainted_LeftFootCurve;
    public AnimationCurve Repainted_RightFootCurve;
    private void Awake()
    {
        leftFoot = SkeletonAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFoot = SkeletonAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
        Debug.Log(leftFoot.name);
        var LeftFootPath = SkeletonAnimator.name + " : Animator.Left Foot T";
        //var RightFootPath = SkeletonAnimator.name + " : Animator.Right Foot T";
        AnimationClip _clip = null;
        foreach (var clips in SkeletonAnimator.GetCurrentAnimatorClipInfo(0))
        {
            if (clips.clip.name == ClipName)
            {
                _clip = clips.clip;
                break;
            }
        }
        //create curve
        LeftFootFounded = false;
        RightFootFounded = false;
        var curveBindings = UnityEditor.AnimationUtility.GetCurveBindings(_clip);

        foreach (var curveBinding in curveBindings)
        {
            Debug.Log(curveBinding.path + ", " + curveBinding.propertyName);

            if (curveBinding.propertyName == "LeftFootT.y")
            {
                LeftFootCurve = AnimationUtility.GetEditorCurve(_clip, curveBinding);
                Debug.Log("curve> " + " " + curveBinding.path + ", " + curveBinding.propertyName);
                LeftFootFounded = true;
            }
            else
            if (curveBinding.propertyName == "RightFootT.y")
            {
                RightFootCurve = AnimationUtility.GetEditorCurve(_clip, curveBinding);
                Debug.Log("curve> " + " " + curveBinding.path + ", " + curveBinding.propertyName);
                RightFootFounded = true;
            }
            if (LeftFootFounded && RightFootFounded) break;
        }

        int heighestKeyID = 0;
        int LowestKeyID = 0;
        for (int i = 0; i < LeftFootCurve.keys.Length; i++)
        {
            var curveKey = LeftFootCurve.keys[i];
            if (curveKey.value > LeftFootCurve[heighestKeyID].value) heighestKeyID = i;
            if (curveKey.value < LeftFootCurve[LowestKeyID].value) LowestKeyID = i;
        }
        Repainted_LeftFootCurve.AddKey(LeftFootCurve[heighestKeyID].time, LeftFootCurve[heighestKeyID].value);
        Repainted_LeftFootCurve.AddKey(LeftFootCurve[LowestKeyID].time, LeftFootCurve[LowestKeyID].value);

    }
}
