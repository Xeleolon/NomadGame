using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpolisveEmeny : MonoBehaviour
{
    [SerializeField] float explosionRange;
    [SerializeField] float dectectRange;

    [SerializeField] float damage;
    [SerializeField] float fireDamage;


    [Tooltip("Rate at wich explosion charge up")]
    [SerializeField] Vector2 explosionRate;
    [SerializeField] float minDistance;

    [Tooltip("size of obect head")]
    [SerializeField] Vector2 headSize;
    private float currentHeadSize;
    [SerializeField] GameObject headExplosive;
    float explosionClock;

    GameObject player;
    PlayerLife playerLife;

    enum ExpolisveState {neurtal, warning,charging, releasing, exploded}
    ExpolisveState explosionState = ExpolisveState.neurtal;

    void Update()
    {
        if (explosionState != ExpolisveState.neurtal || explosionState != ExpolisveState.exploded)
        {
            //check player distance alter timer and alter head size hold if out side explosion range
        }
    }

    void Explode()
    {
        explosionState = ExpolisveState.exploded;

        float distance = Vector3.Distance(player.transform.position, headExplosive.transform.position);

        if (playerLife.torchState == PlayerLife.TorchStates.lit)
        {
            if (distance <= explosionRange)
            {
                //play fire explosion
                playerLife.AlterHealth(-fireDamage);
            }
            else if (distance <= dectectRange)
            {
                //play fire explosion
                playerLife.AlterHealth(-damage);
            }
        }
        else
        {
            if (distance <= explosionRange)
            {
                playerLife.AlterHealth(-damage);
            }
        }
    }
    void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.tag == "Player" && (explosionState == ExpolisveState.neurtal || explosionState == ExpolisveState.releasing))
        {
            if (!headExplosive.activeSelf)
            {
                headExplosive.SetActive(true);
            }
            player = other.gameObject;
            playerLife = PlayerLife.instance;
        }
    }


}
