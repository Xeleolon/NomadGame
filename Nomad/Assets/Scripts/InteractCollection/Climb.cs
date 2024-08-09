using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climb : InteractBase
{

    private PlayerMovement player;
    private Vector3 offSetPosition {get {return offSet + transform.position;} set {offSet = value - transform.position;}}
    //{get{return new Vector3(maxWidth.x, maxHieght, maxWidth.y) + transform.position;}
    //set{Vector3 offSet = value - transform.position; maxWidth = new Vector2(offSet.x, offSet.y); maxHieght = offSet.y;}}
    [SerializeField] private Vector3 offSet;
    //[SerializeField] private float maxHieght;

    //[SerializeField] private Vector2 maxWidth;
    void Start()
    {
        player = PlayerMovement.instance;
    }
    public override void Interact()
    {
        if (!player.CurMovmenentMatch(PlayerMovement.MovementType.climbing))
        {
            player.ChangeMovement(PlayerMovement.MovementType.climbing, offSetPosition, transform);
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
        //Gizmos.DrawWireCube(transform.position, new Vector3(offSetPosition.x, offSetPosition.y, offSetPosition.x));
        Vector3 radius = new Vector3(1, 2, 1);
        Gizmos.DrawWireCube(offSetPosition, radius);

    }
}
