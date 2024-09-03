using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorDamage : MonoBehaviour
{
    public float damage = 1;

    private void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerLife playerLife = PlayerLife.instance;
            playerLife.AlterHealth(-damage);
            playerLife.CheckPointRespawn();
            return;
        }

        EmenyHealth emenyHealth = other.gameObject.GetComponent<EmenyHealth>();
        if (emenyHealth != null)
        {
            emenyHealth.ForceDeath();
            return;
        }
    }
}
