using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testint : MonoBehaviour
{
    public RigidbodyBasedMovement rigidbodyBased;
    public enum Debugs
    {
        Height,
        ForwardHits,
        ForwardHitsZ,
        both
    }
    public Debugs debugs;
    [Header("Forward")]
    public int amount = 20;
    public float destinationY = 2f;

    public float Distance = 2f;
    public bool DebugFromFirstHitYPos = false;
    public float FirstHitoffsetY = .1f;
    [Header("Height")]
    public float maxHeight = 1f;
    public float distanceForwardFromPlayer = .2f;
    [Header("ForwardZ")]
    public Vector3 offset;
    public int amountZ = 20;
    public float DistanceZ = 2f;
    public float destinationZ = 2f;

    [Header("both")]
    [Range(0f,1f), Tooltip("Closer to 1 means closer to flat ground")]public float DotMaxSlope;
    public Rigidbody rb;
    //public float distanceYFromPlayer = .2f;
    private void OnDrawGizmos()
    {
        /* Gizmos.color = Color.red;
         Gizmos.DrawWireSphere(lowest, .3f);
         Gizmos.color = Color.green;
         Gizmos.DrawWireSphere(heighest, .3f);*/
        RaycastHit _lowestHit = new RaycastHit();
        RaycastHit _heighestHit = new RaycastHit();
        var moveDirection = rigidbodyBased.GetMoveDirection(false);
        if (!Application.isPlaying) moveDirection = transform.forward;
        switch (debugs)
        {
            case Debugs.Height:
                Universal_RaycastAssistance.instance.IsItProperHeightGizmos(rb.position, rb.position, moveDirection, maxHeight, distanceForwardFromPlayer, 1 << 8, 0f);
                break;
            case Debugs.ForwardHits:
                Gizmos.color = Color.white;
                Universal_RaycastAssistance.instance.RaycastHitFromToYGizmos(rb.position, moveDirection, Distance, rb.position.y, rb.position.y + destinationY, amount, 1 << 8, Color.red, Color.blue, Color.yellow, out _lowestHit, out _heighestHit);
                if (DebugFromFirstHitYPos)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(_lowestHit.point + Vector3.up * FirstHitoffsetY, _heighestHit.point + rigidbodyBased.GetMoveDirection(false) * 0.05f);
                }
                else
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(new Vector3(transform.position.x, _lowestHit.point.y, transform.position.z), _heighestHit.point + rigidbodyBased.GetMoveDirection(false) * 0.05f);
                }
                break;
            case Debugs.both:

                Universal_RaycastAssistance.instance.RaycastHitFromToZGizmos(rb.position, transform.position, -transform.up, offset, moveDirection, DistanceZ, destinationZ, amountZ, 1 << 8, Color.red, Color.blue, Color.yellow, out _lowestHit, out _heighestHit);
                Universal_RaycastAssistance.instance.IsItProperHeightGizmos(rb.position, _heighestHit.point, moveDirection, maxHeight, distanceForwardFromPlayer, 1 << 8, 0f);
                if (Universal_RaycastAssistance.instance.IsItProperHeight(rb.position, _heighestHit.point, moveDirection, maxHeight, distanceForwardFromPlayer, 1 << 8, DotMaxSlope, out RaycastHit HeightHit, 0f))
                {
                    Gizmos.color = Color.white;
                    var yHit = Universal_RaycastAssistance.instance.RaycastHitFromToYGizmos(rb.position, moveDirection, Distance, rb.position.y, rb.position.y + destinationY, amount, 1 << 8, Color.red, Color.blue, Color.yellow, out _lowestHit, out _heighestHit);
                    if (yHit)
                    {
                        if (DebugFromFirstHitYPos)
                        {
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawLine(_lowestHit.point + Vector3.up * FirstHitoffsetY, _heighestHit.point + rigidbodyBased.GetMoveDirection(false) * 0.05f);
                        }
                        else
                        {
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawLine(new Vector3(rb.position.x, _lowestHit.point.y, rb.position.z), _heighestHit.point + rigidbodyBased.GetMoveDirection(false) * 0.05f);
                        }
                    }
                }
                break;
            case Debugs.ForwardHitsZ:
                Universal_RaycastAssistance.instance.RaycastHitFromToZGizmos(rb.position, transform.position, -transform.up, offset, moveDirection, DistanceZ, destinationZ, amountZ, 1 << 8, Color.red, Color.blue, Color.yellow, out _lowestHit, out _heighestHit);
                break;
            default:
                break;
        }
        
    
    }
}
