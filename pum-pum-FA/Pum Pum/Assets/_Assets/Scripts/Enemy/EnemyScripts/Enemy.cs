using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;


public class Enemy : MonoBehaviour, IHittable, IAgent
{
    private ServerManager _serverManager = null;
    private float _hitDamage = 30f;
    private float _maxHealth = 100;
    private float _currentHealth = 100;
    
    public UnityEvent OnGetHit { get; set; }
    public UnityEvent OnDie { get; set; }

    private void Awake()
    {
        _serverManager = GameObject.FindObjectOfType<ServerManager>();
        _currentHealth = _maxHealth;
    }

    private void Start()
    {
        //_serverManager.EnemySpawned(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (collision.gameObject.tag == "AlliedBullet")
        {
            print("Bullet collide with enemy on server");
            var bulletDamage = collision.gameObject.GetComponent<StandardBullet>().BulletDamage;
            GetHit(bulletDamage);
        }

        if(collision.gameObject.tag == "Player")
        {
            var playerModel = collision.gameObject.GetComponent<PlayerModel>();
            if (playerModel == null) return;

            playerModel.GetDamage(_hitDamage);
        }
    }


    public void GetHit(float damage)
    {
        OnGetHit?.Invoke();

        if (_currentHealth - damage > 0)
        {
            _currentHealth -= damage;
        }
        else
        {
            OnDie?.Invoke();
            Die();
        }
    }
    
    public void Die()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        //_serverManager.EnemyDie(this.gameObject);
        PhotonNetwork.Destroy(gameObject);
    }

}
