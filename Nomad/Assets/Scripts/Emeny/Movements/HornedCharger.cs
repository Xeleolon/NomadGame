using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HornedCharger : BaseEmenyMovement
{
    //possible could be the base script for Emeny movement, if built right
    [Header("HornedCharger")]
    [SerializeField] float speedCharge = 6;
    [SerializeField] float speedNeutral = 3;
    [SerializeField] float damage = 1;

    [Header("PlayerTargeting")]
    [SerializeField] Vector2 AgroDistance = new Vector2(3, 5);
    public bool targetPlayer;
    private Transform player;
    private PlayerLife playerLife;
    private Vector3 ChargePosition;
    public Transform targetBush;
    public int curMode; //this contains the mode which the Ai is directed by
    [SerializeField] private Vector2 actackPause = new Vector2(0.5f, 1);
    private float actackClock;
    [SerializeField] float actackOverShoot = 3;
    private bool collided;
    private int collisionType;
    private bool hitPlayer;
    [SerializeField] string ground;
    


    [Header("Temp Color Changes")]
    public Color agroColor;
    public Color actackingColor;
    private Color neutralColor;
    public override void Start()
    {
        base.Start();
        neutralColor = GetComponent<MeshRenderer>().material.color;
        player = GameObject.FindWithTag("Player").transform;
        playerLife = player.gameObject.GetComponent<PlayerLife>();
    }

    void Update()
    {
        switch (curMode)
        {
            case (0):
            //Debug.Log("eatingBush");
            NeutralEatingBush();
            break;

            case (1):
            //Debug.Log("Emeny agro");
            AgroWarning();
            break;

            case (2):
            //Debug.Log("actackWait");
            ActackWait();
            break;

            case (3):
            //Debug.Log("charge Player");
            ActackCharge();
            break;
        }
    }

    //Mode 0 neutral
    void NeutralEatingBush()
    {
        if (Vector3.Distance(player.position, transform.position) < AgroDistance.y)
        {
            //switch to curMode to agressive at player
            curMode = 1;
            GetComponent<MeshRenderer>().material.color = agroColor;
        }

        if (targetBush == null)
        {
            targetBush = GameObject.FindWithTag("Bush").transform;
        }
        if (Vector3.Distance(transform.position, targetBush.position) > 1)
        {
            Move(new Vector2(speedNeutral, 0), targetBush.position, true);
        }
        else
        {
            Move(Vector2.zero, targetBush.position, true);
        }
    }

    void AgroWarning()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance > AgroDistance.y)
        {
            curMode = 0;
            GetComponent<MeshRenderer>().material.color = neutralColor;
        }
        else if (distance < AgroDistance.x)
        {
            curMode = 2;
            GetComponent<MeshRenderer>().material.color = actackingColor;
        }
        
        Move(Vector2.zero, player.position, true);
    }

    void ActackWait()
    {
        if (Vector3.Distance(player.position, transform.position) > AgroDistance.x)
        {
            curMode = 1;
        }
        else if (actackClock < 0 && RaycasyCheck(AgroDistance.y))
        {
            curMode = 3;
            actackClock = Random.Range(actackPause.x, actackPause.y);
            //ChargePosition = Vector3.forward * (Vector3.Distance(player.position, transform.position) /*+ actackOverShoot*/);
            ChargePosition = player.position;
            Debug.Log("charging at " + ChargePosition);
        }
        else
        {
            actackClock -= 1 * Time.deltaTime;
        }
        Move(Vector2.zero, player.position, true);
    }

    void ActackCharge()
    {
        Vector3 alterChargePosition = new Vector3(ChargePosition.x, transform.position.y, ChargePosition.z);
        //Debug.Log("charging at " + alterChargePosition);
        if (Vector3.Distance(transform.position, alterChargePosition) > 0.1f )
        {
            if (collided)
            {
                switch (collisionType)
                {
                    case 0: //player
                    playerLife.AlterHealth(-damage);
                    collided = false;
                    break;

                    case 1:
                    //Debug.Log("Exiting out here at collisionCheck");
                    //Move(Vector2.zero, alterChargePosition, true);
                    ExitCharge(); 
                    break;

                    case 2:
                    //collider with moveable object some effect should occur
                    collided = false;
                    break;
                }
            }

            Move(new Vector2(speedCharge, 0), alterChargePosition, true);
        }
        else
        {
            //Debug.Log("Exiting out here at Else");
            ExitCharge();
        }

        void ExitCharge()
        {
            //Debug.Log("Exiting out here");
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > AgroDistance.x)
            {
                curMode = 1;
            }
            else if (distance > AgroDistance.y)
            {
                curMode = 0;
            }
            else
            {
                curMode = 2;
                actackClock = Random.Range(actackPause.x, actackPause.y);
            }
        }
        
    }

    bool RaycasyCheck(float maxDistance)
    {
        Ray ray;
        ray = new Ray(gameObject.transform.position, gameObject.transform.forward);
    
        RaycastHit hit;
    

    
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                return true;
            }
        }
            return false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collided && collision.gameObject.tag != ground && curMode == 3)
        {

            if (collision.gameObject.tag == "Player" && !hitPlayer)
            {
                collisionType = 0;
                collided = true;
                hitPlayer = true;
            }
            else if (collision.gameObject.isStatic)
            {
                //Debug.Log("colliding with" + collision.gameObject.name);
                collisionType = 1;
                collided = true;
            }
            else
            {
                collisionType = 2;
                collided = true;
            }
        }
        else if (collided)
        {
            collided = false;
            hitPlayer = true;
        }
    }


}
