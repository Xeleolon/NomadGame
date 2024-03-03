using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseRespawn : MonoBehaviour
{
    [SerializeField] GameObject skellyPrefab;
    Vector3 startPosition;

    void Start()
    {
        LevelManager.instance.onSunSetCallback += NightCycleAwaken;
        startPosition = transform.position;
    }

    private void NightCycleAwaken()
    {
        Instantiate(skellyPrefab, startPosition, Quaternion.identity);
        LevelManager.instance.onSunSetCallback -= NightCycleAwaken;
        Destroy(gameObject);
    }
}
