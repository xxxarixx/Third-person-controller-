using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class Player_Visualizesion : MonoBehaviour
{
    public RigidbodyBasedMovement Movement;
    public GameObject movementVisualizesion;
    public GameObject Vis_ArrowMovement;
    public float default_ArrowRange;
    private void Update()
    {
        //movementVisualizesion.transform.position = transform.position;
        //if (movementVisualizesion != null) /*movementVisualizesion.transform.rotation = Quaternion.LookRotation(Movement.GetMoveDirection(Movement.input.Moveinput, false));*/ movementVisualizesion.transform.forward = Movement.input.Moveinput;
        //Vis_ArrowMovement.transform.localPosition = new Vector3(Vis_ArrowMovement.transform.localPosition.x,.5f, Vector3.forward.z * (default_ArrowRange + Movement.speed_Current * 0.005f));
    }
}
