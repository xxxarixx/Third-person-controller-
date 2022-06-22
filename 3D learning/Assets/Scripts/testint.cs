using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testint : MonoBehaviour
{
    public enum Debugs
    {
        Height,
        ForwardHits,
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

    [Header("both")]
    [Range(0f,1f), Tooltip("Closer to 1 means closer to flat ground")]public float DotMaxSlope;
    //public float distanceYFromPlayer = .2f;
    private void OnDrawGizmos()
    {
        /* Gizmos.color = Color.red;
         Gizmos.DrawWireSphere(lowest, .3f);
         Gizmos.color = Color.green;
         Gizmos.DrawWireSphere(heighest, .3f);*/
        RaycastHit _lowestHit = new RaycastHit();
        RaycastHit _heighestHit = new RaycastHit();
        
        switch (debugs)
        {
            case Debugs.Height:
                Universal_RaycastAssistance.instance.IsItProperHeightGizmos(transform.position,transform.forward, maxHeight, 1 << 8, distanceForwardFromPlayer);
                break;
            case Debugs.ForwardHits:
                Gizmos.color = Color.white;
                Universal_RaycastAssistance.instance.RaycastHitFromToYGizmos(transform.position, transform.forward, Distance, transform.position.y, transform.position.y + destinationY, amount, 1 << 8, Color.red, Color.blue, Color.yellow, out _lowestHit, out _heighestHit);
                if (DebugFromFirstHitYPos)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(_lowestHit.point + Vector3.up * FirstHitoffsetY, _heighestHit.point);
                }
                else
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(new Vector3(transform.position.x, _lowestHit.point.y, transform.position.z), _heighestHit.point);
                }
                break;
            case Debugs.both:
                Universal_RaycastAssistance.instance.IsItProperHeightGizmos(transform.position, transform.forward, maxHeight, 1 << 8, distanceForwardFromPlayer);
                if (Universal_RaycastAssistance.instance.IsItProperHeight(transform.position, transform.forward, maxHeight, 1 << 8, out RaycastHit HeightHit, distanceForwardFromPlayer))
                {
                    Gizmos.color = Color.white;
                    Universal_RaycastAssistance.instance.RaycastHitFromToYGizmos(transform.position, transform.forward, Distance, transform.position.y, transform.position.y + destinationY, amount, 1 << 8, Color.red, Color.blue, Color.yellow, out _lowestHit, out _heighestHit, DotMaxSlope);
                    //if (Vector3.Angle(_lowestHit.point, _heighestHit.point) > MaxSlopeAngle) return;
                    if (DebugFromFirstHitYPos)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(_lowestHit.point + Vector3.up * FirstHitoffsetY, _heighestHit.point);
                    }
                    else
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(new Vector3(transform.position.x, _lowestHit.point.y, transform.position.z), _heighestHit.point);
                    }
                }
                break;
            default:
                break;
        }
        
    
    }
}
