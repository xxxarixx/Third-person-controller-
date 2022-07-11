using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class VectorsRotationTest : MonoBehaviour
{
    public Transform toRotate;
    public Transform v1;
    public Transform v2;


    private void Update()
    {
        toRotate.position = (v1.position + v2.position) / 2;
        toRotate.rotation = Quaternion.FromToRotation(Vector3.forward, (v2.position - v1.position).normalized);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(v1.position, v2.position);
    }
}
