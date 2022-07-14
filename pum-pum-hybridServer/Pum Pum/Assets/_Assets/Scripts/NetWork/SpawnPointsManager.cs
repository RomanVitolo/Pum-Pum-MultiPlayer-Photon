using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class SpawnPointsManager : MonoBehaviour
{
    [SerializeField] private Transform[] _initialpawnPoints;

    private void Awake()
    {
        GetSpawnPosition();
    }

    public Vector3 GetSpawnPosition()
    {
        var randomIndex = Random.Range(0, _initialpawnPoints.Length);
        var newPos = _initialpawnPoints[randomIndex].position;

        return newPos;
    }
}


