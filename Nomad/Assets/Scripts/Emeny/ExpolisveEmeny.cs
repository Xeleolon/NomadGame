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
    [SerializeField] float clockStartPosition;
    [SerializeField] float clockHoldPosition;
    float explosionClock;
    [SerializeField] float minDistance;

    [Tooltip("size of obect head, x for finished valued, y for start value")]
    [SerializeField] Vector2 headSize;
    private float currentHeadSize;
    [SerializeField] GameObject headExplosive;

    GameObject player;
    PlayerLife playerLife;

    enum ExpolisveState {neurtal, charging, exploded}
    ExpolisveState explosionState = ExpolisveState.neurtal;
    void Start()
    {
        headExplosive.transform.localScale = new Vector3(headSize.y, headSize.y, headSize.y);

        LevelManager.instance.onResetRespawn += Reset;
    }

    void Update()
    {
        if (player != null && explosionState != ExpolisveState.neurtal && explosionState != ExpolisveState.exploded)
        {
            float distance = Vector3.Distance(player.transform.position, headExplosive.transform.position);

            float measurementClockToSize = explosionClock / clockStartPosition;
            if (measurementClockToSize <= 0)
            {
                measurementClockToSize = 0;
            }
            float sizeToScale = Mathf.Lerp(headSize.x, headSize.y, measurementClockToSize);
            headExplosive.transform.localScale = new Vector3(sizeToScale, sizeToScale, sizeToScale);




            //check player distance alter timer and alter head size hold if out side explosion range
            if (distance <= minDistance)
            {
                //Debug.Log("Activating explosion as player triggered min distance check at " + distance + " player position is beening seen at " + player.transform.position);
                Explode(distance);
                return;
            }
            else if (distance <= explosionRange)
            {
                //count down clock with distance from source increasing speed
                //if clock = 0 explode
                if (explosionClock > 0)
                {
                    explosionClock -= explosionRate.y * Time.deltaTime;
                }
                else
                {
                    Explode(distance);
                }

                return;
            }
            else if (distance <= dectectRange)
            {
                //count down clock till hold position only
                if (explosionClock > clockHoldPosition)
                {
                    explosionClock -= explosionRate.x * Time.deltaTime;
                }
                else
                {
                    explosionClock = clockHoldPosition;
                }

                return;
            }
            else
            {
                //Debug.Log("releasing explosive hold with player at " + distance);
                Reset();
            }
        }
    }

    void Explode(float distance)
    {
        explosionState = ExpolisveState.exploded;
        ParticleSystem particleSystem = gameObject.GetComponent<ParticleSystem>();

        if (headExplosive.activeSelf)
        {
            headExplosive.SetActive(false);
        }

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

        particleSystem.Play();
    }

    void Reset()
    {
        if (!headExplosive.activeSelf)
        {
            headExplosive.SetActive(true);
        }
        explosionClock = clockStartPosition;
        explosionState = ExpolisveState.neurtal;
        headExplosive.transform.localScale = new Vector3(headSize.y, headSize.y, headSize.y);
    }
    void OnTriggerEnter(Collider other) 
    {
        //Debug.Log("Dectecting clossion with " + other.gameObject);
        if (other.gameObject.tag == "Player" && explosionState == ExpolisveState.neurtal)
        {
            if (!headExplosive.activeSelf)
            {
                headExplosive.SetActive(true);
            }
            explosionClock = clockStartPosition;
            explosionState = ExpolisveState.charging;
            player = other.gameObject;
            playerLife = PlayerLife.instance;
        }
    }

    public virtual void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position , explosionRange);
    }


}
