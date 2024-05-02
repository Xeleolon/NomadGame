using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCylce : MonoBehaviour
{
    public static SpawnCylce instance;
    void Awake ()
    {
        if (instance != null)
        {
            Debug.LogWarning("More Than One instance of LevelManager found!");
        }
        instance = this;
    }
    [SerializeField] int maxAreaSpawnDay = 10;
    [SerializeField] int maxAreaSpawnNight = 20;
    public int dayCount = 2;
    public int nightCount = 0;
    [SerializeField] Transform[] daySpawnPoints;
    [SerializeField] Transform[] nightSpawnPoints;
    public AIZoner premiter;
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
        if (dayCount >= maxAreaSpawnDay)
        {
            dayCount += 1;
        }
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

    public bool makeSpace(bool emenyTpye)
    {
        if (emenyTpye)
        {
            if (nightSpawnPoints.Length < maxAreaSpawnNight)
            {
                Transform[] tempPoints = new Transform[nightSpawnPoints.Length];
                for (int i = 0; i < nightSpawnPoints.Length; i ++)
                {
                    tempPoints[i] = nightSpawnPoints[i];
                }

                nightSpawnPoints = new Transform[tempPoints.Length + 1];
                for (int i = 0; i < tempPoints.Length; i ++)
                {
                    nightSpawnPoints[i] = tempPoints[i];
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public void AddSpawnPoint(GameObject newAddition, bool emenyTpye)
    {
        if (emenyTpye)
        {
            if (nightCount >= maxAreaSpawnNight)
            {
                nightCount += 1;
            }
            nightSpawnPoints[nightSpawnPoints.Length - 1] = newAddition.transform;
        }

    }

}
