using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EpicPowerUp : BasicPowerUp
{
    private Rigidbody2D _myRb = null;
    private Vector2 _movementDir = Vector3.zero;
    private float _speed = 0f;

    private void Awake()
    {
        _myRb = this.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
    }

    public void ChangeMovementDir(Vector3 newDir, float speed)
    {
        _movementDir = (Vector2)newDir;
        _speed = speed;
        _myRb.AddForce(_movementDir * _speed, ForceMode2D.Impulse);
    }
}
