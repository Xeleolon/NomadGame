using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryTimer : MonoBehaviour
{
    [SerializeField] float timeToDestory = 1;
    void Update()
    {
        if (timeToDestory > 0)
        {
            Destroy(gameObject);
        }
        else
        {
            timeToDestory -= 1 * Time.deltaTime;
        }
    }
}
