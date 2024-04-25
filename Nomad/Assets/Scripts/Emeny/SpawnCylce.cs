using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCylce : MonoBehaviour
{
    [SerializeField] int maxAreaSpawn = 10;
    public int dayCount = 2;
    public int nightCount = 0;
    [SerializeField] Transform[] daySpawnPoints;
    [SerializeField] Transform[] nightSpawnPoints;
    public GameObject dayPrefab;
    public GameObject nightPrefab;
    void Start()
    {
        LevelManager.instance.onSunSetCallback += nightSpawn;
        LevelManager.instance.onSunRiseCallback += daySpawn;

    }

    void daySpawn()
    {
        for (int i = 0; i < dayCount; i++)
        {
            int spawnLocation = 0;
            if (daySpawnPoints.Length > 1)
            {
                spawnLocation = Random.Range(0, daySpawnPoints.Length - 1);
            }

            Instantiate(dayPrefab, daySpawnPoints[spawnLocation].position, Quaternion.identity);
        }
    }

    void nightSpawn()
    {
        for (int i = 0; i < nightCount; i++)
        {
            int spawnLocation = 0;
            if (nightSpawnPoints.Length > 1)
            {
                spawnLocation = Random.Range(0, nightSpawnPoints.Length - 1);
            }

            Instantiate(nightPrefab, nightSpawnPoints[spawnLocation].position, Quaternion.identity);
        }
    }

}
