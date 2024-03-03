using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZoner : MonoBehaviour
{
    //set rules for ai in the area of this zone, establishing the events that must occur, additional used to reduce resource cost by turning off AI when player not near the area

    public Vector3 areaSize = new Vector3(6, 6, 6);
    public virtual void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, areaSize);
    }

}
