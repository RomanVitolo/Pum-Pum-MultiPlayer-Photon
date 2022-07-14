using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BasicPowerUp : MonoBehaviourPun
{
    [Header("----------- PowerUpStats -----------")]
    [SerializeField] private int _maxBullets = 20;
    private int _dropBullets = 0;

    [Header("----------- VFX -----------")]
    [SerializeField] private ParticleSystem _particleSystemExplosion = null;

    public int DropBullets { get => _dropBullets; set => _dropBullets = value; }

    private void Start()
    {
        ChooseRandomBullets();
    }

    private void ChooseRandomBullets()
    {
        var randomBullets = Random.Range(0, _maxBullets);
        _dropBullets = randomBullets;
    }

    private void OnTriggerEnter2D(Collider2D collision) //This will be fixed on hibrid system where you ask the server a DestroyPowerUpRequest()
    {
        PlayerModel playerModel = collision.gameObject.GetComponent<PlayerModel>();
        if (playerModel != null)
        {
            playerModel.CollisionWithPowerUp(this);
            Destroy(this.gameObject);
        }
    }
}

