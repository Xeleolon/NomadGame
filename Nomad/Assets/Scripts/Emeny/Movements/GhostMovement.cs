using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostMovement : BaseEmenyMovement
{
    [SerializeField] float damage = 1;
    [SerializeField] float damageDistance = 0.5f;
    [SerializeField] float damageSpeed = 2;
    private float damageClock;
    [Header("Movement")]
    [SerializeField] float speedStandard = 2;
    [SerializeField] float speedChase = 4;
    [SerializeField] float speedRetreat = 2;
    [SerializeField] Vector2 fireAvoidanceRange = new Vector2(2, 4);
    private float travelClock;
    [Header("Random Movement")]
    [SerializeField] Vector3 regionOffset;
    [SerializeField] Vector2 regionSize = new Vector2(2,2);
    [SerializeField] float minTravelDistance = 2;
    [SerializeField] float maxTravelClock = 2;
    private enum MovementType {disable, neutral, chase, run}
    MovementType movementType = MovementType.neutral;
    private bool avoidPlayer;
    private bool dectectingPlayer;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Transform player;


    void Start()
    {
        startPosition = transform.position;
        targetPosition = transform.position;
    }

    void Update()
    {
        switch (movementType)
        {
            case MovementType.neutral:
            NeutralMovement();

            return;

            case MovementType.chase:
            ChaseMovement();

            return;

            case MovementType.run:
            RunMovement();


            return;
        }
    }

    public override void DisableMovement(bool disable)
    {
        if (disable)
        {
            movementType = MovementType.disable;
            targetPosition = transform.position;
        }
        else
        {
            movementType = MovementType.neutral;
            targetPosition = transform.position;
        }
    }


    void NeutralMovement()
    {
        if (dectectingPlayer)
        {
            movementType = MovementType.chase;
        }
        if (Vector3.Distance(targetPosition, transform.position) < 0.2f || travelClock > maxTravelClock)
        {
            pickrandomPosition();
            travelClock = 0;
        }
        Move(new Vector2(speedStandard, 0), targetPosition, true);

        travelClock += 1 * Time.deltaTime;
        return;


        Vector3 pickrandomPosition()
        {
            Vector3 RandomOffset = startPosition + new Vector3(Random.Range(-regionSize.x/2, regionSize.x/2), 0 , Random.Range(-regionSize.y/2, regionSize.y/2));
            
            if (Vector3.Distance(RandomOffset, targetPosition) < minTravelDistance)
            {
                return targetPosition;
            }
            targetPosition = RandomOffset;
            return RandomOffset;
        }
    }

    void ChaseMovement()
    {
        Move(new Vector2(speedChase, 0), player.position, true);

        if (Vector3.Distance(transform.position, player.position) <= damageDistance)
        {
            if (damageClock >= damageSpeed)
            {
                damageClock = 0;
                PlayerLife.instance.AlterHealth(-damage);
            }
            else
            {
                damageClock += 1 * Time.deltaTime;
            }
        }

        if (!dectectingPlayer)
        {
            movementType = MovementType.neutral;
            
        }
    }

    void RunMovement()
    {
        if (Vector3.Distance(transform.position, targetPosition) >= fireAvoidanceRange.y)
        {
            movementType = MovementType.neutral;
            return;
        }
        
        Move(new Vector2(-speedRetreat, 0), targetPosition, true);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.tag == "Player" && !dectectingPlayer)
        {
            dectectingPlayer = true;
            if (player == null)
            {
                player = other.gameObject.transform;
            }
        }
    }
    private void OnTriggerExit(Collider other) 
    {
        if (other.gameObject.tag == "Player" && dectectingPlayer)
        {
            dectectingPlayer = false;
            if (movementType == MovementType.chase)
            {
                movementType = MovementType.neutral;
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (PlayerLife.instance.curTool == PlayerLife.ToolType.torch && PlayerLife.instance.torchState == PlayerLife.TorchStates.lit 
            && Vector3.Distance(other.gameObject.transform.position, transform.position) <= fireAvoidanceRange.x)
            {
                movementType = MovementType.run;
                targetPosition = other.gameObject.transform.position;
                targetPosition.y = transform.position.y;
                avoidPlayer = true;
            }
            else if (avoidPlayer)
            {
                avoidPlayer = false;
            }
            if (!dectectingPlayer)
            {
                dectectingPlayer = true;
                if (player == null)
                {
                    player = other.gameObject.transform;
                }
            }
         
        }
        if (other.gameObject.tag == "Fire" && Vector3.Distance(other.gameObject.transform.position, transform.position) <= fireAvoidanceRange.x)
        {
            movementType = MovementType.run;
            targetPosition = other.gameObject.transform.position;
            targetPosition.y = transform.position.y;
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + regionOffset, new Vector3(regionSize.x, 2, regionSize.y));

    }
}
