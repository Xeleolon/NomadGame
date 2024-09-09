using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakablePlatform : MonoBehaviour
{
    // Start is called before the first frame update
    Renderer renderer;
    Collider collider;
    void Start()
    {
        //unblock line of code if want to reset on respawn from checkpoints
        //LevelManager.instance.onResetCheckPoint += Reset;
        

        LevelManager.instance.onResetRespawn += Reset;
    }
    private void Reset()
    {
        CheckRendererBoxCollider();
        renderer.enabled = true;
        collider.enabled = true;
    }

    private void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.tag == "Player" || other.gameObject.tag == "Emeny")
        {
            CheckRendererBoxCollider();
            renderer.enabled = false;
            collider.enabled = false;
        }
    }

    void CheckRendererBoxCollider()
    {
        if (renderer != null && collider != null)
        {
            return;
        }
        renderer = gameObject.GetComponent<Renderer>();
        collider = gameObject.GetComponent<Collider>();
    }
}
