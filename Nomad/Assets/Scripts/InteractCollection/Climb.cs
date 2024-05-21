using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climb : InteractBase
{

    private PlayerMovement player;
    private Vector3 offSetPosition{get {return offSet + transform.position;} set {offSet = value - transform.position;}}

    [SerializeField] public Vector3 offSet;
    void Start()
    {
        player = PlayerMovement.instance;
    }
    public override void Interact()
    {
        if (!player.CurMovmenentMatch(PlayerMovement.MovementType.climbing))
        {
            player.ChangeMovement(PlayerMovement.MovementType.climbing, offSetPosition);
        }
    }

    public override bool Requirements()
    {
        if (player.CurMovmenentMatch(PlayerMovement.MovementType.climbing))
        {
            return false;
        }
        return true;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(offSetPosition, 1);
        Vector3 radius = new Vector3(1, 2, 1);
        Gizmos.DrawWireCube(offSetPosition, radius);

    }
}
