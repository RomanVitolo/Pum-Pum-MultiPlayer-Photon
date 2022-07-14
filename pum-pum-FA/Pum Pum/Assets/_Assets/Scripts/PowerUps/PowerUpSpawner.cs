using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviourPun
{
    [Header("PowerUpSpawner Params")]
    [SerializeField] private float _spawnRadius = 30f;
    [SerializeField] private int _initSpawnAmount = 30;
    [SerializeField] private float _standardSpawnRate = 5;

    [Header("Quidditch Ball Params")]
    [SerializeField] private float _epicPowerUpSpawnSpeed = 5;
    [SerializeField] private float _epicPowerUpChangeDirFrecuency = 2f;
    private EpicPowerUp _epicPowerUpRef = null;
    private bool _reasignNewDirQuidditchBall = true;

    [Header("Power up prefab root")]
    [SerializeField] private string _folderPrefabsPath = "";
    [SerializeField] private GameObject[] _powerUpPrefabs = null;
    [SerializeField] private GameObject _quidditchPowerUpPrefab = null;

    private bool _sleep = false;

    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient) Destroy(this.gameObject);
    }

    private void Start()
    {
        SpawnPowerUp(_initSpawnAmount);
        SpawnEpicPowerUp();
    }

    private void Update()
    {
        if(_reasignNewDirQuidditchBall && _epicPowerUpRef != null) StartCoroutine(WaitToReassignDir());
        if (!_sleep) StartCoroutine(WaitRandomTimeToRespawn());
    }

    private void SpawnPowerUp(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var randomPowerUpIndex = Random.Range(0, _powerUpPrefabs.Length);
            var randomPowerUp = _powerUpPrefabs[randomPowerUpIndex];

            PhotonNetwork.Instantiate(_folderPrefabsPath + "/" + randomPowerUp.name, Random.insideUnitCircle * _spawnRadius, Quaternion.identity);
        }
    }

    private void SpawnEpicPowerUp()
    {
        var epicPowerUp = PhotonNetwork.Instantiate(_folderPrefabsPath + "/" + _quidditchPowerUpPrefab.name, Random.insideUnitCircle * _spawnRadius, Quaternion.identity);
        _epicPowerUpRef = epicPowerUp.GetComponent<EpicPowerUp>();
        //GetNewDir();
    }

    private void GetNewDir()
    {
        var randomPos = (Vector3)Random.insideUnitCircle * _spawnRadius;
        var randomDir = randomPos - _epicPowerUpRef.transform.position;

        _epicPowerUpRef.ChangeMovementDir(randomDir, _epicPowerUpSpawnSpeed);
    }

    public void BoostPowerUpInterval(float newSpawnRate)
    {
        _standardSpawnRate = newSpawnRate;
    }

    IEnumerator WaitRandomTimeToRespawn()
    {
        _sleep = true;
        yield return new WaitForSeconds(_standardSpawnRate);
        SpawnPowerUp(5);
        _sleep = false;
    }

    IEnumerator WaitToReassignDir()
    {
        _reasignNewDirQuidditchBall = false;
        yield return new WaitForSeconds(_epicPowerUpChangeDirFrecuency);
        GetNewDir();
        _reasignNewDirQuidditchBall = true;
    }
}
