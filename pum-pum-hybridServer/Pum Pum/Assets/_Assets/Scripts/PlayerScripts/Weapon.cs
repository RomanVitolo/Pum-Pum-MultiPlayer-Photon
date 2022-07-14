using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    //Weapon Shooting Timing 
    [Header("----------- Weapon Parameters -----------")]
    [SerializeField] private float _weaponShootRate = 1f;
    private float _currentAvailableBullets = 0;
    private float _weaponCurrentShootTime = 0f;
    private bool _isCoolingDown = false;
    private bool _executeCoolDown = false;

    [Header("----------- Bullet Parameters -----------")]
    [SerializeField] private float _bulletSpeed = 10f;

    private GameObject _weaponRef;
    public float CurrentAvailableBullets { get => _currentAvailableBullets; set => _currentAvailableBullets = value; }
    public bool ExecuteCoolDown { get => _executeCoolDown; set => _executeCoolDown = value; }

    private void Update()
    {
        ExecuteWeaponCoolDownTimer();
    }

    private void ExecuteWeaponCoolDownTimer()
    {
        if (_executeCoolDown) //Just execute cool down when gun has been shot
        {
            if (_weaponCurrentShootTime >= _weaponShootRate)
            {
                _weaponCurrentShootTime = 0;
                _isCoolingDown = false;
                _executeCoolDown = false;
            }
            else
            {
                _weaponCurrentShootTime += Time.deltaTime;
                _isCoolingDown = true;
            }
        }
    }

    public void Shoot(Transform shootTransform) //HIBRID = Change this to "shootRequest" on server. 
    {
        if (!_isCoolingDown) //If the weapon is already cooled down, shoot. Otherwise, dont shoot.
        {
            if (_currentAvailableBullets > 0) //Check if i have enough bullets on my weapon for every cannon
            {
                var newBullet = PhotonNetwork.Instantiate("Prefabs/BulletsPrefabs/Bullet", shootTransform.position, Quaternion.identity);
                newBullet.GetComponent<Rigidbody2D>().velocity = shootTransform.up * _bulletSpeed;

                _currentAvailableBullets -= 1;
                _isCoolingDown = true;
            }
        }
    }
}
