using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class StandardBullet : MonoBehaviourPun
{
    [Header("----------- Bullet Parameters -----------")]
    [SerializeField] private float _bulletDamage = 20f;
    [SerializeField] private float _bulletLife = 5f;

    public float BulletDamage { get => _bulletDamage; }

    private void Start()
    {
        if (photonView.IsMine) StartCoroutine(DestroyAfterDelay(_bulletLife, this.gameObject)); //Destroy bullet remotely after delay using CorrutinesExecutor because scriptables objects can not execute corrutines
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(photonView.IsMine) StartCoroutine(DestroyAfterDelay(0.005f, this.gameObject));
    }

    IEnumerator DestroyAfterDelay(float delayTime, GameObject objToDestroy)
    {
        yield return new WaitForSeconds(delayTime);
        PhotonNetwork.Destroy(objToDestroy);
    }
}
