using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SkellyMovement : MonoBehaviour
{
    UnityEngine.AI.NavMeshAgent navMesh;
    public Transform testdenistation;
    public bool moveGo;
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

        if (perimeter != null)
        {
            Vector3 rangePerimeter = perimeter.areaSize/2;
            Vector3 orginPerimeter = perimeter.transform.position;
            Vector3 objectSize = new Vector3(transform.lossyScale.x/2, 0, transform.lossyScale.z/2);

            //max Permiter
            maxPerimeter = orginPerimeter + rangePerimeter - objectSize;
            minPerimeter = orginPerimeter - rangePerimeter + objectSize;
        }

    }

    void FixedUpdate()
    {
        if (moveGo)
        {
            navMesh.destination = testdenistation.position;
            moveGo = false;
        }
        RandomMovement();
    }

    void RandomMovement()
    {
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
                Debug.Log("AI Idling");
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
}
