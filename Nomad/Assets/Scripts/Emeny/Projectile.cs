using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Vector3 target;
    public float speed = 3;
    public float damage = 1;
    public float lifeLength = 3;
    float clock;

    bool startMomement = false;
    Rigidbody rb; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame

    public void StartMovement()
    {
        startMomement = true;
    }
    void Update()
    {
        //if (startMomement)
        //{
        //Quaternion rotation = Quaternion.LookRotation(target, Vector3.up);
        //transform.rotation = rotation;

        rb.AddForce(transform.forward * speed * 10, ForceMode.Force);


        //}

        clock += 1 * Time.deltaTime;

        if (clock >= lifeLength)
        {
            Destroy(gameObject);
        }

    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerLife.instance.AlterHealth(-damage);
            gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 4, ForceMode.Impulse);
            Destroy(gameObject);
        }
    }
}
