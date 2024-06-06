using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingAnchor : InteractBase
{
    private PlayerMovement player;
    private Vector3 swingPostion {get {return swingAnchor + transform.position;} set {swingAnchor = value - transform.position;}}
    //{get{return new Vector3(maxWidth.x, maxHieght, maxWidth.y) + transform.position;}
    //set{Vector3 offSet = value - transform.position; maxWidth = new Vector2(offSet.x, offSet.y); maxHieght = offSet.y;}}
    [SerializeField] private Vector3 swingAnchor;

    void Start()
    {
        player = PlayerMovement.instance;
    }

    public override void Interact()
    {
        Debug.Log(swingPostion + " | " + transform.position);
        player.StartSwing(swingPostion);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(offSetPosition, 1);
        //Gizmos.DrawWireCube(transform.position, new Vector3(offSetPosition.x, offSetPosition.y, offSetPosition.x));
        Debug.Log(swingPostion + " " + transform.position);
        Vector3 radius = new Vector3(1f, 1f, 1f);
        Gizmos.DrawWireCube(swingPostion, radius);

    }
}
