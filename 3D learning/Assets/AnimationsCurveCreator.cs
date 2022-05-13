using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class AnimationsCurveCreator : MonoBehaviour
{
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

}
