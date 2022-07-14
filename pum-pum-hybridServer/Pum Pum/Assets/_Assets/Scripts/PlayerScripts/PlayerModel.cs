using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine.SceneManagement;

public class PlayerModel : MonoBehaviourPun
{
    [Header("---- Gameobjects References ----")]
    [SerializeField] private Camera _myCamera = null;
    [SerializeField] private Transform _shootTransform = null;
    [SerializeField] private Rigidbody2D _myPlayerRB = null;
    [SerializeField] private SpriteRenderer _myPlayerSR = null;
    private ServerManager _server = null;
    private PlayerView _myPlayerView = null;

    [Header("---- Player Gameplay Params ----")]
    [SerializeField] private float _maxLife = 100;
    private float _currentLife = 0;
    [SerializeField]private float _movementSpeed = 5;

    [SerializeField]private float _dashImpulse = 10;
    [SerializeField]private float _dashDelay = 3f;
    private bool _isDashing = false;

    [Header("---- Weapon ----")]
    [SerializeField] private Weapon _currentWeapon = null;

    public float CurrentLife { get => _currentLife; }
    public Weapon CurrentWeapon { get => _currentWeapon; set => _currentWeapon = value; }
    public SpriteRenderer MyPlayerSR { get => _myPlayerSR; set => _myPlayerSR = value; }

    public event Action OnPlayerDie;
    public bool _isDead = false;
    public event Action OnPlayerWin;
    public event Action<int> OnPickedPowerUp; //Executed by player controller
    #region Private Methods

    private void Awake()
    {
        if (!photonView.IsMine)
        {
            Destroy(_myCamera.gameObject);
            Destroy(this);
        }

        _server = GameObject.FindObjectOfType<ServerManager>();
        _currentLife = _maxLife;
        _myPlayerView = GetComponent<PlayerView>();

    }
    
    private void Update()
    {
        //if (_currentWeapon != null) _currentWeapon.ExecuteWeaponCoolDownTimer();//Check if i already have picked a weapon to start cooldown timer
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Bullet")//Future fix --> Create Bullet class that calls "PlayerModel.GetDamage()"
        {
            var bulletDamage = collision.gameObject.GetComponent<StandardBullet>().BulletDamage;
            GetDamage(bulletDamage);
        }
    }

    private void GetDamage(float damage)
    {
        if (_currentLife - damage > 0)
        {
            _currentLife -= damage;
            _myPlayerView.ExecuteDamageAnimation();
        }
        else
        {
            if (!_isDead)
            {
                StartCoroutine(Die());
            }
        }
    }

    #endregion 

    #region Public Methods
    public void Move() //Player Movement behaviour
    {
        float xMovement = Input.GetAxisRaw("Horizontal");
        float yMovement = Input.GetAxisRaw("Vertical");
        var newDir = new Vector2(xMovement, yMovement).normalized;

        _myPlayerRB.AddForce(newDir * _movementSpeed);
    }

    public void LookAtMouse() //Player look at mouse behaviour
    {
        Vector2 mousePos = _myCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _myCamera.nearClipPlane));
        Vector2 direction = (mousePos - (Vector2)this.transform.position).normalized;
        this.transform.up = direction;
    }

    public void Shoot() 
    {
        _currentWeapon.Shoot(_shootTransform);
    }

    public void Dash()
    {
        if (!_isDashing)
        {
            _isDashing = true;

            Vector2 mousePos = _myCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - (Vector2)this.transform.position).normalized;

            _myPlayerRB.AddForce(direction * _dashImpulse, ForceMode2D.Impulse);

            StartCoroutine(DashDelay());
        }
    }

    public void CollisionWithPowerUp(BasicPowerUp powerUp)
    {
        OnPickedPowerUp?.Invoke(powerUp.DropBullets);
        _currentWeapon.CurrentAvailableBullets = powerUp.DropBullets;

    }

    public void MakePlayerGhost()
    {
        _maxLife = 1000;
        print("Transformando al player en ghost desde el playerModel");
    }

    public void MakePlayerWinner()
    {
        if(_currentLife > 0)
        {
            OnPlayerWin?.Invoke();
        }
    }
    #endregion

    public void ChangePlayerLayer(int layer)
    {
        this.gameObject.layer = layer;
    }

    [PunRPC]
    public void DisconnectPlayer(string sceneName)
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator Die()
    {
        _isDead = true;
        _myPlayerView.ExecuteDieAnimation();
        yield return new WaitForSeconds(0.1f);
        OnPlayerDie?.Invoke();

        //_server.photonView.RPC("OnPlayerDieNotification", _server.GetPlayerServer, PhotonNetwork.LocalPlayer);
        //MakePlayerGhost();
    }

    IEnumerator DashDelay() //No funca
    {
        _myPlayerView.DashIndicator.SetActive(false);
        yield return new WaitForSeconds(_dashDelay);
        _isDashing = false;
        _myPlayerView.DashIndicator.SetActive(true);
    }
}
