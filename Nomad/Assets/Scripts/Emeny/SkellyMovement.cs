using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkellyMovement : MonoBehaviour
{
    public float walkSpeed = 2;
    public float runSpeed = 3;
    public float rotationSpeed;
    private Vector3 target;


    [Header("Premiter")]
    public AIZoner aiZoner;
    private Vector3 areaSize;
    private Vector3 areaOrgine;
    void Start()
    {
        areaSize = aiZoner.areaSize/2;
        areaOrgine = aiZoner.transform.position;
        RandomTarget();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RandomMovement();
    }


    void RandomMovement()
    {
        if (transform.position.x != target.x && transform.position.z != target.z)
        {
            if (Quaternion.LookRotation(target) != Quaternion.LookRotation(transform.forward))
            {
                Vector3 tempDirection = Vector3.RotateTowards(transform.forward, target, rotationSpeed * Time.deltaTime, 0.0f);
                transform.rotation = Quaternion.LookRotation(tempDirection);
            }

            transform.position = Vector3.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
        }
        else
        {
            RandomTarget();
        }
    }

    void RandomTarget()
    {
        Vector2 tempTarget;
        tempTarget.x = Mathf.Round(Random.Range(areaOrgine.x - areaSize.x, areaOrgine.x + areaSize.x));
        tempTarget.y = Mathf.Round(Random.Range(areaOrgine.z - areaSize.z, areaOrgine.z + areaSize.z));

        target = new Vector3(tempTarget.x, transform.position.y, tempTarget.y);
    }
}
