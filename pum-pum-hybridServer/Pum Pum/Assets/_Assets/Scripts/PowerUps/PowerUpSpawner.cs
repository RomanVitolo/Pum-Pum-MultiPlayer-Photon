using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviourPun
{
    [Header("PowerUpSpawner Timing")]
    [SerializeField] private float _minSpawnTime = 10f;
    [SerializeField] private float _maxSpawnTime = 30f;

    [Header("PowerUpSpawner References")]
    [SerializeField] private GameObject _powerUpPrefab = null;
    private GameObject _currentInstantiatedPowerUp = null;

    private void Start()
    {
        SpawnPowerUp();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(_currentInstantiatedPowerUp != null)
        {
            _currentInstantiatedPowerUp = null;
            StartCoroutine(WaitRandomTimeToRespawn());
        }
    }

    private void SpawnPowerUp()
    {
        //_currentInstantiatedPowerUp = PhotonNetwork.Instantiate("Prefabs/PowerUpPrefabs/PowerUp", this.transform.position, Quaternion.identity); //--> NETWORKING
        _currentInstantiatedPowerUp = Instantiate(_powerUpPrefab, this.transform.position, Quaternion.identity);
    }

    IEnumerator WaitRandomTimeToRespawn()
    {
        var randomTimeToWait = Random.Range(_minSpawnTime, _maxSpawnTime);
        yield return new WaitForSeconds(randomTimeToWait);
        SpawnPowerUp();
    }
}
