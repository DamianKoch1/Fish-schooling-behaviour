using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [SerializeField]
    private Fish fishPrefab;

    [SerializeField]
    private float spawnRadius;

    [SerializeField, Range(0, 200)]
    private int initialSpawns;

    [SerializeField, Range(0, 10)]
    private float spawnRate;

    [SerializeField]
    private int maxSpawnCount;

    public List<Fish> spawnedFish;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        spawnedFish = new List<Fish>();
        SpawnInitialFish();
    }

    private void SpawnInitialFish()
    {
        for (int i = 0; i < initialSpawns; i++)
        {
            if (i >= maxSpawnCount) break;
            SpawnFish();
        }
    }

    private void Update()
    {
        UpdateSpawnTimer();
    }

    private void UpdateSpawnTimer()
    {
        if (spawnedFish.Count > maxSpawnCount) return;
        if (spawnRate == 0) return;
        for (int i = 0; i < spawnRate * Time.deltaTime; i++)
        {
            SpawnFish();
        }
    }

    private void SpawnFish()
    {
        var fish = Instantiate(fishPrefab.gameObject, Random.insideUnitSphere * spawnRadius, Random.rotation, transform).GetComponent<Fish>();
        spawnedFish.Add(fish);
        fish.Initialize(this);
    }

  
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red - new Color(0, 0, 0, 0.7f);
        Gizmos.DrawSphere(transform.position, spawnRadius);

       
    }
}
