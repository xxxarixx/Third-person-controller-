using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class FootPlacer : MonoBehaviour
{
    public Transform LeftFoot;
    public Transform RightFoot;
    public float FootdetectionRange = .1f;
    public LayerMask groundLayer;
    public Rig LeftLegRig;
    public Rig RightLegRig;
    public Transform LeftLegTarget;
    public Transform RightLegTarget;
    public GameObject DebugHit;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(LeftFoot.transform.position + new Vector3(0f, FootdetectionRange / 2, 0f), LeftFoot.transform.position + -LeftFoot.transform.up * FootdetectionRange);
        Gizmos.DrawLine(RightFoot.transform.position + new Vector3(0f, FootdetectionRange / 2, 0f), RightFoot.transform.position + -RightFoot.transform.up * FootdetectionRange);

        Gizmos.color = Color.black;
        Gizmos.DrawLine(LeftFoot.transform.position + new Vector3(0f, 1f, 0f), LeftFoot.transform.position + Vector3.down * 20f);
        Gizmos.DrawLine(RightFoot.transform.position + new Vector3(0f, 1f, 0f), RightFoot.transform.position + Vector3.down * 20f);
    }


    private void Update()
    {
        Physics.Raycast(LeftFoot.transform.position + new Vector3(0f, FootdetectionRange / 2, 0f), -LeftFoot.transform.up, out RaycastHit LeftHit, FootdetectionRange, groundLayer);
        Physics.Raycast(RightFoot.transform.position + new Vector3(0f, FootdetectionRange / 2, 0f), -RightFoot.transform.up, out RaycastHit RightHit, FootdetectionRange, groundLayer);

        if (LeftHit.collider != null)
        {

            //Debug.Log($"Left foot hit: x{ LeftHit.point.x },y{ LeftHit.point.y }");
            Physics.Raycast(LeftFoot.transform.position + new Vector3(0f, 1f, 0f), Vector3.down, out RaycastHit LeftTopHit, Mathf.Infinity, groundLayer);

            if (LeftTopHit.collider != null)
            {
                LeftLegRig.weight = 1f;
                LeftLegTarget.transform.position = LeftTopHit.point;
                Destroy(Instantiate(DebugHit, LeftTopHit.point, Quaternion.identity), .05f);
            }
            Debug.Log($"Left foot hit");
        }
        else
        {
            LeftLegRig.weight = 0f;
        }

        if(RightHit.collider != null)
        {
            Debug.Log($"Right foot hit");
            Physics.Raycast(RightFoot.transform.position + new Vector3(0f, 1f, 0f), Vector3.down, out RaycastHit RightTopHit, Mathf.Infinity, groundLayer);
            if(RightTopHit.collider != null)
            {
                //Debug.Log($"Right foot hit: x{ RightHit.point.x },y{ RightHit.point.y }");
                RightLegRig.weight = 1f;
                RightLegTarget.transform.position = RightTopHit.point;
                Destroy(Instantiate(DebugHit, RightTopHit.point, Quaternion.identity), .05f);
            }

        }
        else
        {
            Debug.Log($"Right foot NOT hit");
            RightLegRig.weight = 0f;
        }

    }
}
