using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        bool touchingBox = Physics.CheckBox(transform.position, Vector3.one * .1f / 2, Quaternion.identity, 1 << 8);
        Gizmos.color = touchingBox ? Gizmos.color = Color.green : Gizmos.color = Color.red;
        if (touchingBox)
        {
            Universal_RaycastAssistance.instance.DrawRaycastGizmo(transform.position + Vector3.up * 1f, Vector3.down, 2f);
            Physics.Raycast(transform.position + Vector3.up * .1f, Vector3.down, out RaycastHit hit, 2f, 1 << 8);
            Gizmos.matrix = Matrix4x4.TRS(hit.point, NormalToRotation(Quaternion.identity, transform, hit.normal), transform.lossyScale);
            Gizmos.DrawCube(hit.point, Vector3.zero);
            Gizmos.matrix = Matrix4x4.identity;
        }
        Gizmos.DrawWireCube(transform.position,Vector3.one * .1f);


    }
    public Quaternion NormalToRotation(Quaternion currentRoation, Transform transform, Vector3 normal)
    {
        return Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, normal), normal);
    }
}
