using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class BonePlacerOnSurface : MonoBehaviour
{
   

    [Header("Bones")]
    public CustomGravity gravity;
    public RigidbodyBasedMovement movement;
    //[SerializeField]private TwoBoneIKConstraint NextMove_LeftLegIK;
   // [SerializeField] private TwoBoneIKConstraint NextMove_RightLegIK;
    [SerializeField] private TwoBoneIKConstraint FootOnGround_LeftLegIK;
    [SerializeField] private TwoBoneIKConstraint FootOnGround_RightLegIK;
    private Transform LeftFoot;
    private Transform RightFoot;
    private Transform LeftKnee;
    private Transform RightKnee;
    private Transform LeftToe;
    private Transform RightToe;
    //[SerializeField] private Transform NextMove_LeftLegTarget;
    //[SerializeField] private Transform NextMove_RightLegTarget;
    [SerializeField] private Transform FootOnGround_LeftLegTarget;
    [SerializeField] private Transform FootOnGround_RightLegTarget;
    [SerializeField] private Transform SkeletonOrigin;
    public Transform LeftFootAnimation;
    public Transform RightFootAnimation;
    [Header("Other veriables:")]
    public Animator skeletonAnimator;
    [SerializeField] private LayerMask groundMask;
    public float OffsetDistanceToGround = .1f;
    public Vector3 offsetToe;
    public Rigidbody rb;
   // public bool ActiveMovement;
    //public float MoveSpeed = 100f;
    public float OffsetYOnFootPlace = 0f;
    public bool LeftToeHitting;
    public bool RightToeHitting;
    public float BodyPositoon;
    public float SizeLegsSpacing = 2f;
    public Vector3 FootCheckBoxSize = Vector3.one;
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
    }
    private Vector3 LeftHitPos;
    private Vector3 RightHitPos;
    private Quaternion LeftHitQuat;
    private Quaternion RightHitQuat;
    private float LeftFootGraph;
    private float RightFootGraph;

    public float footForwardSize = .3f;
   // public float LegsBodyCorrectSpeed = 1f;
   // public float LegFuturePosMultiplayer = 10f;
    private void FixedUpdate()
    {
        
        LeftFootAnimation.position = skeletonAnimator.GetIKPosition(AvatarIKGoal.LeftFoot);
        RightFootAnimation.position = skeletonAnimator.GetIKPosition(AvatarIKGoal.RightFoot);
        LeftFootGraph = skeletonAnimator.GetFloat("LeftFoot");
        RightFootGraph = skeletonAnimator.GetFloat("RightFoot");

        // je¿eli wartoœæ curve'a jest 0 to poruszaj siê jak w animacji tylko jedynie trzymaj siê ziemi ( change only y position) (dotyka ziemi)
        //je¿eli wartoœæ curve'a jest 1 to porusz siê do przysz³ej pozycji nogi (change x,y,z position based on weight) (w powietrzu)
        //obliczyæ d³ugoœæ k³adzenia nogi poprzez ró¿nice (pocz¹tku k³adzenia nogi odj¹æ koniec k³adzenia nogi ) przemno¿one przez prêdkoœæ animacji i 


        //if foot is in object then move foot to up of an object
        bool leftFootInsideObj = false;
        bool RightFootInsideObj = false;
        if(LeftFootGraph < .1f)
        {
            leftFootInsideObj = Physics.CheckBox(LeftToe.position, FootCheckBoxSize, LeftToe.rotation, groundMask);
        }
        else
        {
            leftFootInsideObj = Physics.CheckBox(LeftFootAnimation.position, FootCheckBoxSize, LeftToe.rotation, groundMask);
            
        }
        if(RightFootGraph < .1f)
        {
            RightFootInsideObj = Physics.CheckBox(RightToe.position, FootCheckBoxSize, RightToe.rotation, groundMask);
        }
        else
        {
            RightFootInsideObj = Physics.CheckBox(RightFootAnimation.position, FootCheckBoxSize, RightToe.rotation, groundMask);
        }
        if (leftFootInsideObj || LeftFootGraph == 0)
        {
            var LeftFoothitting = Physics.Raycast(LeftFootAnimation.position + new Vector3(0f, GetKneeToeHeight(LeftKnee, LeftFootAnimation), 0f) + offsetToe, -transform.up + OffsetYOnFootPlace * Vector3.up, out RaycastHit LeftHit, OffsetDistanceToGround + GetKneeToeHeight(LeftKnee, LeftFootAnimation), groundMask);
            if (LeftFoothitting)
            {
                FootOnGround_LeftLegIK.weight += Time.fixedDeltaTime * 5f;
                LeftHitPos = new Vector3(LeftFootAnimation.position.x, LeftHit.point.y, LeftFootAnimation.position.z) + OffsetYOnFootPlace * Vector3.up;
                LeftHitQuat = NormalToRotation(LeftFootAnimation, LeftHit.normal);
            }
            
        }
        else
        {
            FootOnGround_LeftLegIK.weight -= Time.fixedDeltaTime * 5f;
            LeftHitPos = LeftFootAnimation.position;
            LeftHitQuat = LeftFootAnimation.rotation;
        }
        if (RightFootInsideObj || RightFootGraph == 0)
        {
            var RightFootHitting = Physics.Raycast(RightFootAnimation.position + new Vector3(0f, GetKneeToeHeight(RightKnee, RightFootAnimation), 0f) + offsetToe + OffsetYOnFootPlace * Vector3.up, -transform.up, out RaycastHit RightHit, OffsetDistanceToGround + GetKneeToeHeight(RightKnee, RightFootAnimation), groundMask);
            if (RightFootHitting)
            {
                FootOnGround_RightLegIK.weight += Time.fixedDeltaTime * 5f;
                RightHitPos = new Vector3(RightFootAnimation.position.x, RightHit.point.y, RightFootAnimation.position.z) + OffsetYOnFootPlace * Vector3.up;
                RightHitQuat = NormalToRotation(LeftFootAnimation, RightHit.normal);

            }
            
        }
        else
        {
            FootOnGround_RightLegIK.weight -= Time.fixedDeltaTime * 5f;
            RightHitPos = RightFootAnimation.position;
            RightHitQuat = RightFootAnimation.rotation;
        }

       

        FootOnGround_LeftLegTarget.position = LeftHitPos;
        FootOnGround_RightLegTarget.position = RightHitPos;

        FootOnGround_LeftLegTarget.rotation = LeftHitQuat;
        FootOnGround_RightLegTarget.rotation = RightHitQuat;
        //NextMove_LeftLegTarget.position = LFhitPos;
        //NextMove_RightLegTarget.position = RFhitPos;









    }
    public Quaternion NormalToRotation(Transform transform, Vector3 normal)
    {
        return Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, normal), normal);
    }
    private void OnDrawGizmos()
    {
        _ConfigureBodyParts();
        

        Gizmos.color = Color.white;
        DrawRaycastGizmo(LeftFootAnimation.position + new Vector3(0f, GetKneeToeHeight(LeftKnee, LeftFootAnimation), 0f) + offsetToe, -transform.up, OffsetDistanceToGround + GetKneeToeHeight(LeftKnee, LeftFootAnimation));
        DrawRaycastGizmo(RightFootAnimation.position + new Vector3(0f, GetKneeToeHeight(RightKnee, RightFootAnimation), 0f) + offsetToe, -transform.up, OffsetDistanceToGround + GetKneeToeHeight(RightKnee, RightFootAnimation));
        Gizmos.color = Color.green;
       

        Gizmos.color = Color.yellow;

        Gizmos.matrix = LeftToe.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, FootCheckBoxSize);
        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.color = Color.blue;

        Gizmos.matrix = RightToe.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, FootCheckBoxSize);
        Gizmos.matrix = Matrix4x4.identity;

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
     void SomeOldStuff()
        {
        #region oldCode
        /* if (LeftFootGraph > .1f)
        {
            //var futureLeftLegPos = LeftFootAnimPos.position + (movement.getMoveDirection(movement.input.Moveinput, false) + rb.velocity * (1f / skeletonAnimator.speed) * Time.fixedDeltaTime) * LegFuturePosMultiplayer;
            NextMove_LeftLegIK.weight = LeftFootGraph;
        }
        else
        {
            var LeftFoothitting = Physics.Raycast(LeftFootAnimPos.position + new Vector3(0f, GetKneeToeHeight(LeftKnee, LeftFootAnimPos), 0f) + offsetToe, -transform.up + OffsetYOnFootPlace * Vector3.up, out RaycastHit LhitGround, OffsetDistanceToGround + GetKneeToeHeight(LeftKnee, LeftFootAnimPos), groundMask);
            if (LeftFoothitting)
            {
                LFhitPos = LhitGround.point;
                NextMove_LeftLegIK.weight = LeftFootGraph;
            }
        }
        if(RightFootGraph > .1f)
        {
            //var futureRightLegPos = RightFootAnimPos.position + (movement.getMoveDirection(movement.input.Moveinput, false) + rb.velocity * (1f / skeletonAnimator.speed) * Time.fixedDeltaTime) * LegFuturePosMultiplayer;
            NextMove_RightLegIK.weight = RightFootGraph;
        }
        else
        {
            var RightFootHitting = Physics.Raycast(RightFootAnimPos.position + new Vector3(0f, GetKneeToeHeight(RightKnee, RightFootAnimPos), 0f) + offsetToe + OffsetYOnFootPlace * Vector3.up, -transform.up, out RaycastHit RhitGround, OffsetDistanceToGround + GetKneeToeHeight(RightKnee, RightFootAnimPos), groundMask);
            if (RightFootHitting)
            {
                RFhitPos = RhitGround.point;
                NextMove_RightLegIK.weight = RightFootGraph;
            }
        }*/
        /*if(SkeletonOrigin.localPosition.y != OffsetYOnFootPlace)
        {
            SkeletonOrigin.localPosition = new Vector3(SkeletonOrigin.localPosition.x, OffsetYOnFootPlace, SkeletonOrigin.localPosition.z);
        }*/
        /*var LeftFoothitting = Physics.Raycast(LeftFootAnimPos.position + new Vector3(0f, GetKneeToeHeight(LeftKnee, LeftFootAnimPos), 0f) + offsetToe, -transform.up, out RaycastHit Lhit, OffsetDistanceToGround + GetKneeToeHeight(LeftKnee, LeftFootAnimPos), groundMask);
        var RightFootHitting = Physics.Raycast(RightFootAnimPos.position + new Vector3(0f, GetKneeToeHeight(RightKnee, RightFootAnimPos), 0f) + offsetToe, -transform.up, out RaycastHit Rhit, OffsetDistanceToGround + GetKneeToeHeight(RightKnee, RightFootAnimPos), groundMask);
        if (LeftFoothitting)
        {
            LeftLegTarget.position = Lhit.point + Lhit.normal * OffsetYOnFootPlace;
            LeftLegTarget.rotation = NormalToRotation(LeftLegTarget.rotation, LeftFoot, Lhit.normal);
        }
        else
        {
            //LeftLegTarget.position = LeftFoot.position;
        }
        
        if (RightFootHitting)
        {
            RightLegTarget.position = Rhit.point + Rhit.normal * OffsetYOnFootPlace;
            RightLegTarget.rotation = NormalToRotation(RightLegTarget.rotation, RightFoot, Rhit.normal);

        }*/
        #endregion
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


        //SkeletonOrigin.localPosition = SkeletonOrigin.InverseTransformPoint(Vector3.Lerp(LeftFoot.position, RightFoot.position, .5f));
        //Debug.Log(Vector3.Lerp(LeftFoot.position, RightFoot.position, .5f));

        //var LegsHeight = 2f;
        /*#region leftFoot
        if (LeftFootGraph < .1f)
        {
            LeftLegIK.weight = 1f;

            //stay feet on ground and keep animation playing freely
            var LeftFoothitting = Physics.Raycast(LeftFootAnimPos.position + new Vector3(0f, GetKneeToeHeight(LeftKnee, LeftFootAnimPos), 0f) + offsetToe, -transform.up, out RaycastHit LhitGround, OffsetDistanceToGround + GetKneeToeHeight(LeftKnee, LeftFootAnimPos), groundMask);

            if (LeftFoothitting)
            {
                LChitPos = LhitGround.point;
            }


            //1. (for 1 foot) target = future position | weight += time.deltatime or get animation curve | if weight > .9f >>> calculate future position
            var futureLeftLegPos = LeftFootAnimPos.position + movement.getMoveDirection(movement.input.Moveinput, false) + rb.velocity * Time.deltaTime * (1f /skeletonAnimator.speed) * SizeLegsSpacing;
            var lHitting = Physics.Raycast(futureLeftLegPos + LegsHeight * Vector3.up, Vector3.down, out RaycastHit Lhit, Mathf.Infinity, groundMask);
            if (lHitting)
            {
                // if(LeftLegIK.weight < .3f)//foot is in air
                {
                    LFhitPos = Lhit.point;
                }
            }
        }
        else
        {
            //pointing to destination point
            LeftLegIK.weight = LeftFootGraph;
            LChitPos = LFhitPos;

        }
        #endregion
        #region RightFoot
        if (RightFootGraph < .1f)
        {
            RightLegIK.weight = 1f;
            var RightFootHitting = Physics.Raycast(RightFootAnimPos.position + new Vector3(0f, GetKneeToeHeight(RightKnee, RightFootAnimPos), 0f) + offsetToe, -transform.up, out RaycastHit RhitGround, OffsetDistanceToGround + GetKneeToeHeight(RightKnee, RightFootAnimPos), groundMask);
            if (RightFootHitting)
            {
                RChitPos = RhitGround.point;
            }


            var futureRightLegPos = RightFootAnimPos.position + movement.getMoveDirection(movement.input.Moveinput, false) + rb.velocity * Time.deltaTime * (1f / skeletonAnimator.speed) * SizeLegsSpacing;
            var rHitting = Physics.Raycast(futureRightLegPos + LegsHeight * Vector3.up, Vector3.down, out RaycastHit Rhit, Mathf.Infinity, groundMask);
            if (rHitting)
            {
                // if(RightLegIK.weight < .3f)//foot is in air
                {
                    RFhitPos = Rhit.point;
                }
            }
        }
        else
        {
            //pointing to destination point
            RightLegIK.weight = RightFootGraph;
            RChitPos = RFhitPos;

        }
        #endregion*/





        //skeletonAnimator.u

        #region OldSttuff
        //float LegDifferenceMultiplayer = 1f;
        //if(Lhit.normal != Vector3.up || Rhit.normal != Vector3.up) LegDifferenceMultiplayer = 4f; else LegDifferenceMultiplayer = 1f;

        /*Debug.Log("isonSlope");
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
        }*/
        /*_LeftFootWeight = skeletonAnimator.GetFloat("LeftFoot");
        _RightFootWeight = skeletonAnimator.GetFloat("RightFoot");
        LeftLegIK.weight = _LeftFootWeight;
        RightLegIK.weight = _RightFootWeight;*/
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
        #endregion
        //RaycastHit LThit = new RaycastHit();
        //RaycastHit RThit = new RaycastHit();
        /*private void _Movement()
        {
            if (!ActiveMovement) return;
            var velocity = transform.forward.normalized * MoveSpeed * Time.fixedDeltaTime;
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
        }*/
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




}
