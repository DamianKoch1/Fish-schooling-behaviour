using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [SerializeField]
    private Fish fishPrefab;

    [SerializeField]
    private float spawnRadius;

    [SerializeField, Range(0, MAX_SPAWN_COUNT)]
    private int initialSpawns;

    [SerializeField, Range(0, 10)]
    private float spawnRate;

    [SerializeField, Range(0, 0.9f)]
    private float behaviourRandomness = 0;

    [SerializeField, Range(0, 0.9f)]
    private float scaleRandomness = 0;

    public List<Fish> spawnedFish;

    public const int MAX_SPAWN_COUNT = 300;


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
            if (i >= MAX_SPAWN_COUNT) break;
            SpawnFish();
        }
    }

    private void Update()
    {
        UpdateSpawnTimer();
    }

    private void UpdateSpawnTimer()
    {
        if (spawnedFish.Count > MAX_SPAWN_COUNT) return;
        if (spawnRate == 0) return;
        for (int i = 0; i < spawnRate * Time.deltaTime; i++)
        {
            SpawnFish();
        }
    }

    private void SpawnFish()
    {
        var fish = Instantiate(fishPrefab.gameObject, Random.insideUnitSphere * spawnRadius, Random.rotation, transform).GetComponent<Fish>();
        fish.Initialize(this);
        fish.RandomizeStats(behaviourRandomness);
        fish.RandomizeScale(scaleRandomness);
        spawnedFish.Add(fish);
    }

  
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red - new Color(0, 0, 0, 0.7f);
        Gizmos.DrawSphere(transform.position, spawnRadius);

       
    }
}
