using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartTrack : MonoBehaviour
{
    public Vector3[] line;
    void Start()
    {
        line = new Vector3[transform.childCount];

        for (int i = 0; i < line.Length; i++)
        {
            Transform child = transform.GetChild(i);
            line[i] = child.position;
            //Destroy(child.gameObject);
        }
    }

}
