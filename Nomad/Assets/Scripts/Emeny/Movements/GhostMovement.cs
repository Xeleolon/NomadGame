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
    [SerializeField] GameObject bodyMesh;
    private enum MovementType {disable, neutral, chase, run}
    MovementType movementType = MovementType.neutral;
    private bool avoidPlayer;
    private bool dectectingPlayer;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Transform player;
    private Collider collider;
    private ParticleSystem particleSystem;
    private bool playingParticle;
    public bool checkRegionInPlay;


    void Start()
    {
        startPosition = transform.position;
        targetPosition = transform.position;
        particleSystem = GetComponent<ParticleSystem>();

        LevelManager.instance.onResetRespawn += Reset;
    }

    void Update()
    {

        if (playingParticle && particleSystem.isStopped)
        {
            Died();
            playingParticle = false;
        }


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

        Vector3 target = player.position;
        target.y = transform.position.y;
        if (Vector3.Distance(transform.position, target) <= damageDistance)
        {
            Debug.Log(gameObject.name + " is damage player at " + transform.position + " with player at " + player.position + " distance is " + damageDistance);
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
        if (movementType == MovementType.disable)
        {
            return;
        }

        if (other.gameObject.tag == "Player" && !dectectingPlayer)
        {
            dectectingPlayer = true;
            if (player == null)
            {
                player = other.gameObject.transform;
            }

            if (PlayerLife.instance.ghost != this)
            {
                PlayerLife.instance.ghost = this;
            }
        }
    }
    private void OnTriggerExit(Collider other) 
    {
        if (movementType == MovementType.disable)
        {
            return;
        }
        

        if (other.gameObject.tag == "Player" && dectectingPlayer)
        {
            dectectingPlayer = false;
            if (PlayerLife.instance.ghost == this)
            {
                PlayerLife.instance.ghost = null;
            }

            if (movementType == MovementType.chase)
            {
                movementType = MovementType.neutral;
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (movementType == MovementType.disable)
        {
            return;
        }
        
        if (other.gameObject.tag == "Player")
        {
            if (PlayerLife.instance.curTool == PlayerLife.ToolType.torch && PlayerLife.instance.torchState == PlayerLife.TorchStates.lit 
            && Vector3.Distance(other.gameObject.transform.position, transform.position) <= fireAvoidanceRange.x)
            {
                CheckOnFireState(other.gameObject.transform.position);
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
            CheckOnFireState(other.gameObject.transform.position);
            movementType = MovementType.run;
            targetPosition = other.gameObject.transform.position;
            targetPosition.y = transform.position.y;
        }



        void CheckOnFireState(Vector3 target)
        {
            if (target.y < transform.position.y - 1 || target.y > transform.position.y + 1)
            {
                return;
            }
    
            target.y = transform.position.y;
            if (Vector3.Distance(transform.position, target) <= 0.5f)
            {
                //lit on fire dire and die
                particleSystem.Play(false);
                playingParticle = true;
            }
        }
    }

    public void SetOnFire(Vector3 target)
    {
        if (target.y < transform.position.y - 1 || target.y > transform.position.y + 1)
        {
            return;
        }
    
        target.y = transform.position.y;
        if (Vector3.Distance(transform.position, target) <= 2f)
        {
            Debug.Log("Play lit ghost on fire");
            //lit on fire dire and die
            particleSystem.Play(false);
            playingParticle = true;
        }
    }

    public void Died()
    {
        if (bodyMesh != null && bodyMesh.activeSelf)
        {
            bodyMesh.SetActive(false);
        }
        PlayerLife.instance.AddItem(PlayerLife.CollectableItemType.fobiddenCoin, 1);
        
        if (collider == null)
        {
            collider = gameObject.GetComponent<Collider>();
        }
        collider.enabled = false;

        transform.position = startPosition;
        movementType = MovementType.disable;
    }

    private void Reset()
    {
        if (bodyMesh != null && !bodyMesh.activeSelf)
        {
            bodyMesh.SetActive(true);
        }
        
        if (collider == null)
        {
            collider = gameObject.GetComponent<Collider>();
        }
        collider.enabled = true;
        transform.position = startPosition;
        movementType = MovementType.neutral;
    }


    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position;
        if (checkRegionInPlay)
        {
            origin = startPosition;
        }
        Gizmos.DrawWireCube(origin + regionOffset, new Vector3(regionSize.x, 2, regionSize.y));

    }
}
