using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SkellyMovement : MonoBehaviour
{
    UnityEngine.AI.NavMeshAgent navMesh;
    [SerializeField] float damage = 1;
    [SerializeField] Vector2 strikePauses = new Vector2(1, 3);
    [SerializeField] int repeatStrikes = 2;
    int repeatStrikeCount;
    [SerializeField] float strikeSpeed = 3;
    [SerializeField] float strikeHold = 1;
    [SerializeField] float playerMaxDistance = 10;
    private int strikeMode;
    private Vector3 strikePosition;
    private float strikeClock; // the timer while ai incircle player
    private float strikeHoldClock; //the actacks hold 

    [Header("ActackMovements")]
    [SerializeField] private float dectectRange = 2;
    public Color standardColor;
    public Color actackColor;
    GameObject player;
    Transform target;
    public int mode = 0; // 0 for not actack 1 for chasing target, 2 for incircling, 3 for actack
    [SerializeField] float speed = 3;
    [SerializeField] float rotationSpeed = 3;
    [SerializeField] float inCircleSpeed = 3;
    [SerializeField] Vector2 actackDistance = new Vector2(3,5);
    bool rotationAntiClockWise;
    float rotationClock;
    float rotationMaxTurn = 5;
    float alter;
    bool alterState;
    float inCircleDistance;
    Vector3 Velocity;
    bool targeting = false;


    [Header("Random Movment")]
    [Range(1, 20)]
    [SerializeField] float travelDistance = 5;
    [SerializeField] AIZoner perimeter;

    Vector3 maxPerimeter;
    Vector3 minPerimeter;



    [Header("Idle")]
    bool idle;
    [Range(1, 10)]
    [SerializeField] float maxIdle = 10;
    [Range(0.1f, 2)]
    [SerializeField] float minIdle = 0.4f;
    [Range(0.1f, 1)]
    [SerializeField] float chanceIdea = 0.5f;
    float idleClock;


    void Start()
    {
        navMesh = GetComponent<UnityEngine.AI.NavMeshAgent>();

        player = GameObject.FindWithTag("Player");
        target = player.transform;
        if (perimeter != null)
        {
            Vector3 rangePerimeter = perimeter.areaSize/2;
            Vector3 orginPerimeter = perimeter.transform.position;
            Vector3 objectSize = new Vector3(transform.lossyScale.x/2, 0, transform.lossyScale.z/2);

            //max Permiter
            maxPerimeter = orginPerimeter + rangePerimeter - objectSize;
            minPerimeter = orginPerimeter - rangePerimeter + objectSize;
        }
        repeatStrikeCount = repeatStrikes;
        standardColor = GetComponent<MeshRenderer>().material.color;

    }

    void Update()
    {
        switch (mode)
        {

            case 1:
            Approach();
            break;

            case 2:
            InCirlce();
            break;

            case 3:
            UpdateStrike();
            break;

            default:
            RandomMovement();
            break;
        }
    }

    void FixedUpdate()
    {
        DecectPlayer();
        switch (mode) //what current states is the movement of the ai IN.
        {
            case 1:
            FixedApproach();
            break;

            case 2:
            FixedInCircle();
            break;

            case 3:
            FixedStrike();
            break;

            default:
            if (navMesh != null)
            {
                RandomMovement();
            }

            break;
        }
    }
    public void TakeDamage()
    {
        if (navMesh.hasPath)
        {
            navMesh.ResetPath();
            
        }
        Velocity = -transform.forward * speed;
        navMesh.Move(Velocity);

        if (mode == 0)
        {
            mode = 1;
        }

    }

    #region DecectPlayer
    void DecectPlayer()
    {
        Ray ray;
            ray = new Ray(gameObject.transform.position, gameObject.transform.forward);
    
            RaycastHit hit;
    

    
            if (Physics.Raycast(ray, out hit, dectectRange))
            {
                if (hit.collider.gameObject.name == "Player")
                {
                    if (!targeting)
                    {
                        Debug.Log("Player Targeted");
                        mode = 1;
                        inCircleDistance = Random.Range(actackDistance.x, actackDistance.y);
                        navMesh.updateRotation = false;
                        targeting = true;
                        GetComponent<MeshRenderer>().material.color = actackColor;
                    }
                    
                    target = player.transform;

                }
            }
    }
    #endregion

    #region ActackMovement

    void Approach()
    {

        Velocity = transform.forward * speed * Time.deltaTime;

        Vector3 newRotation = Vector3.RotateTowards(transform.forward, target.position - transform.position, rotationSpeed * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newRotation);
    }
    void FixedApproach()
    {
        if (navMesh.hasPath)
        {
            navMesh.ResetPath();
        }

        if (Vector3.Distance(target.position, transform.position) < inCircleDistance)
        {
                mode = 2;
        }
        else
        {
            if (Vector3.Distance(transform.position, player.transform.position) > playerMaxDistance)
            {
                Debug.Log("Lost Player");
                mode = 0;
            }
            else
            {
                navMesh.Move(Velocity);
            }
        }
    }

    void InCirlce()
    {
        Strike();

        if (alterState)
        {
            if (alter < 1)
            {
                alter =+ 1 * Time.deltaTime;
            }
            else
            {
                alterState = !alterState;
            }
        }
        else
        {
            if (alter < 0)
            {
                alter =- 1 * Time.deltaTime;
            }
            else
            {
                alterState = !alterState;
            }
        }
        if (rotationClock > rotationMaxTurn)
        {
            rotationAntiClockWise = !rotationAntiClockWise;
            rotationClock = 0;
        }
        else
        {
            rotationClock += 1 * Time.deltaTime;
        }
        inCircleDistance = Mathf.Lerp(actackDistance.x, actackDistance.y, alter);


    }

    void FixedInCircle()
    {
        if (Vector3.Distance(player.transform.position, transform.position) > actackDistance.y + 0.2f)
        {
            mode = 1;
        }
        else if (!rotationAntiClockWise)
        {
            transform.RotateAround(player.transform.position, Vector3.up * inCircleDistance, inCircleSpeed * Time.deltaTime);
        }
        else if (rotationAntiClockWise)
        {
            transform.RotateAround(player.transform.position, Vector3.up * inCircleDistance, -inCircleSpeed * Time.deltaTime);
        }
    }
    void Strike()
    {
        if (strikeClock > Random.Range(strikePauses.x, strikePauses.y))
        {
            mode = 3;
            strikeMode = 0;
            strikeHoldClock = 0;
            
            repeatStrikeCount -= 1;
        
            strikePosition = player.transform.position;
        }
        else
        {
            strikeClock += 1 * Time.deltaTime;
        }
    }
    void UpdateStrike()
    {
        switch (strikeMode)
        {
            case 0: //hold strike
            if (strikeHoldClock < strikeHold)
            {
                strikeHoldClock += 1 * Time.deltaTime;
    
                Vector3 newRotation = Vector3.RotateTowards(transform.forward, strikePosition - transform.position, rotationSpeed * 2 * Time.deltaTime, 0.0f);
                transform.rotation = Quaternion.LookRotation(newRotation);
            }
            else
            {
                strikeMode = 1;
            }
            break;

            case 1: //strike
            if (Vector3.Distance(transform.position, strikePosition) > 1.1f)
            {
                Velocity = transform.forward * strikeSpeed * Time.deltaTime;
            }
            else
            {
                strikeMode = 2;
            }
            break;
            
            case 2:

            Ray ray;
            ray = new Ray(gameObject.transform.position, gameObject.transform.forward);
    
            RaycastHit hit;
    

    
            if (Physics.Raycast(ray, out hit, 1.2f))
            {
                PlayerLife playerLife = hit.collider.gameObject.GetComponent<PlayerLife>();
                if (playerLife != null)
                {
                    playerLife.AlterHealth(-damage);
                    Debug.Log("Hit player");
                }
            }

            strikeMode = 3;
            break;

            case 3: //retreat
            if (Vector3.Distance(transform.position, target.position) >= Random.Range(actackDistance.x, actackDistance.y))
            {
                if (repeatStrikeCount > 0 && Random.Range(0.0f, 1.0f) > 0.5f)
                {
                    Strike();
                }
                else
                {
                    mode = 1;
                    strikeClock = 0;
                }
            }
            else
            {
                Velocity = transform.forward * -1 * strikeSpeed * Time.deltaTime;
                Vector3 newRotation = Vector3.RotateTowards(transform.forward, strikePosition - transform.position, rotationSpeed * Time.deltaTime, 0.0f);
                transform.rotation = Quaternion.LookRotation(newRotation);
            }
            break;

        }


    }

    void FixedStrike()
    {
        switch (strikeMode)
        {
            case 1: //strike
            navMesh.Move(Velocity);
            break;

            case 3: //retreat
            
            navMesh.Move(Velocity);
            
            break;

        }
    }



    #endregion

    #region AIGeneralMovement
    void RandomMovement()
    {
        if (targeting)
        {
            targeting = false;
            GetComponent<MeshRenderer>().material.color = standardColor;
            navMesh.updateRotation = true;
        }
        bool lastIdling = false;
        if (idle)
        {
            idleClock -= 1 * Time.deltaTime;
            if (idleClock <= 0)
            {
                lastIdling = idle;
                idle = !idle;
            }
        }

        if (!navMesh.hasPath && !idle)
        {
            // Idle
            if (!lastIdling && Random.Range(0.0f, 1.0f) > chanceIdea)
            {
                idleClock = Random.Range(minIdle, maxIdle);
                idle = true;
                //Debug.Log("AI Idling");
            }
            else //generate random movement
            {
                Vector3 newPath;
                Vector3 currentPosition = transform.position;
                newPath.x = Random.Range(currentPosition.x - travelDistance, currentPosition.x + travelDistance);
                if (newPath.x < minPerimeter.x)
                {
                    newPath.x = minPerimeter.x;
                }
                else if (newPath.x > maxPerimeter.x)
                {
                    newPath.x = maxPerimeter.x;
                }
                newPath.y = Random.Range(currentPosition.y - travelDistance, currentPosition.y + travelDistance);
                if (newPath.y < minPerimeter.y)
                {
                    newPath.y = minPerimeter.y;
                }
                else if (newPath.y > maxPerimeter.y)
                {
                    newPath.y = maxPerimeter.y;
                }
                newPath.z = Random.Range(currentPosition.z - travelDistance, currentPosition.z + travelDistance);
                if (newPath.z < minPerimeter.z)
                {
                    newPath.z = minPerimeter.z;
                }
                else if (newPath.z > maxPerimeter.z)
                {
                    newPath.z = maxPerimeter.z;
                }
                navMesh.destination = newPath;

            }
        }
    }
    #endregion
}
