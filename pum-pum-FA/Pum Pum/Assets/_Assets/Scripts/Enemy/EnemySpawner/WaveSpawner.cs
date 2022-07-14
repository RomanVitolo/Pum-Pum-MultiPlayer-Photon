using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class WaveSpawner : MonoBehaviourPun
{
    public int waveCount { get; set; } = 1;
    public float SpawnRate { get => spawnRate; }

    private bool waveIsDone = true;
    private int enemyCount;
    [SerializeField] private float spawnRate = 1.0f;
    [SerializeField] private float timeBetweenWaves = 3.0f;
    [SerializeField] private GameObject enemy;
    [SerializeField] private List<GameObject> spawnPoints = null;
    [SerializeField] private GameObject playerPrefab;
    
    
    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) Destroy(this.gameObject);

        if(spawnPoints.Count > 0)
        {
            foreach (var spawnPoint in spawnPoints)
            {
                SpawnEnemy(spawnPoint.transform.position);
            }
        }
        StartCoroutine(waveSpawner());
    }
    void Update()
    {
        if (waveIsDone == true)
        {
            StartCoroutine(waveSpawner());
        }
    }

    IEnumerator waveSpawner()
    {
        waveIsDone = false;

        for (int i = 0; i < enemyCount; i++)
        {
            yield return new WaitForSeconds(spawnRate);
            var randomIndex = Random.Range(0, spawnPoints.Count);

            var randomOffset = Random.insideUnitCircle;
            var spawnPoint = spawnPoints[randomIndex].transform.position + (Vector3)randomOffset;

            SpawnEnemy(spawnPoint);
        }
        
        //Ideal = Crear variables que se llamen "updateSpawnRate", "updateEnemyCount", "updateWaveCount". Lo dejo asi porque creo que se entiende mejor. 
        spawnRate -= 0.1f;
        enemyCount += 3;
        waveCount += 1;

        yield return new WaitForSeconds(timeBetweenWaves);
        waveIsDone = true;
    }
    
    private void SpawnEnemy(Vector3 spawnPoint)
    {
        PhotonNetwork.Instantiate("Prefabs/Enemy", spawnPoint, quaternion.identity);
        //Ya los dejo instanciados en el servidor. Funciona bien. 
    }
}