using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using System;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Voice.PUN;

public class PlayerModel : MonoBehaviourPun
{
    [Header("---- Gameobjects References ----")]
    [SerializeField] private Rigidbody2D _myPlayerRB = null;
    [SerializeField] private SpriteRenderer _myPlayerSR = null;
    [SerializeField] private TextMeshPro _lifeText = null;
    
    [Header("---- Mic params ----")]
    [SerializeField] private string _folderPrefabsPath = "";
    [SerializeField] private GameObject _photonVoiceViewPrefab = null;
    private GameObject _recorderObj = null;

    private PlayerInGameHudsData _playerHudData = null;
    private ServerManager _server = null;
    private Player _localPlayer = null;
    private PlayerView _myPlayerView = null;

    [Header("---- Player Gameplay Params ----")]
    [SerializeField] private float _maxLife = 100;
    private float _currentLife = 0;
    [SerializeField]private float _movementSpeed = 5;

    public float CurrentLife { get => _currentLife; set => _currentLife = value; }
    public SpriteRenderer MyPlayerSR { get => _myPlayerSR; set => _myPlayerSR = value; }
    public PlayerInGameHudsData PlayerHudData { get => _playerHudData; set => _playerHudData = value; }
    public Player LocalPlayer { get => _localPlayer; set => _localPlayer = value; }
    public float MovementSpeed { get => _movementSpeed; set => _movementSpeed = value; }

    public bool _isDead = false;
    public event Action<int> OnPickedPowerUp; //Executed by player controller
    #region Private Methods

    private void Awake()
    {
        _server = GameObject.FindObjectOfType<ServerManager>();
        _localPlayer = PhotonNetwork.LocalPlayer;
        _playerHudData = GameObject.FindObjectOfType<PlayerInGameHudsData>();

        _currentLife = _maxLife;
        _myPlayerView = GetComponent<PlayerView>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            var collisionPlayerModel = collision.gameObject.GetComponent<PlayerModel>();
            if (collisionPlayerModel.CurrentLife > _currentLife) return; //Avoid sending request without purpose
            Player collisionPlayer = _server.PlayersModelsDic[collisionPlayerModel];
            _server.photonView.RPC("RequestEatPlayer", _server.GetPlayerServer, _localPlayer, collisionPlayer);
        }
    }

    public void GetDamage(float damage)
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

    #region Public RPC Methods
    public void Move(Vector3 dir) //Player Movement behaviour
    {
        _myPlayerRB.AddForce(dir * _movementSpeed);
    }

    [PunRPC]
    public void CollisionWithLifePowerUp() //Called by basic Power Up
    {
        _server.photonView.RPC("RequestLife", _server.GetPlayerServer, _localPlayer);
    }

    [PunRPC]
    public void CollisionWithSpeedPowerUp() //Called by basic Power Up
    {
        _server.photonView.RPC("RequestSpeed", _server.GetPlayerServer, _localPlayer, false);
    }

    [PunRPC]
    public void CollisionWithMixedPowerUp() //Called by basic Power Up
    {
        _server.photonView.RPC("RequestLife", _server.GetPlayerServer, _localPlayer);
        _server.photonView.RPC("RequestSpeed", _server.GetPlayerServer, _localPlayer, false);
    }

    [PunRPC]
    public void CollisionWithEpicPowerUp() //Called by basic Power Up
    {
        _server.photonView.RPC("RequestLife", _server.GetPlayerServer, _localPlayer);
        _server.photonView.RPC("RequestSpeed", _server.GetPlayerServer, _localPlayer, true);
    }

    [PunRPC]
    public void MakePlayerWinner() //Called by Server Manager
    {
        _myPlayerView.OnPlayerWinHandler();
    }

    [PunRPC]
    public void MakePlayerLooser() //Called by Server Manager
    {
        _myPlayerView.OnPlayerLooseHandler();
    }

    [PunRPC]
    public void MakePlayerGhost() //Called by Server Manager
    {
        this.gameObject.GetComponent<Collider2D>().enabled = false;
        _currentLife = 100000;
        _myPlayerSR.color = new Color(1, 1, 1, 0.5f);
    }

    [PunRPC]
    public void ChangePlayerMagnitude(float newLifeAmount, Vector3 newScaleAmount) //Called by Server Manager
    {
        _currentLife = (int)newLifeAmount;
        ScalePlayer(newScaleAmount);
        _server.photonView.RPC("SyncGUIRequest", _server.GetPlayerServer, _localPlayer);
    }

    [PunRPC]
    public void ChangePlayerInWorldLifeText(float lifeAmount) //Called by Server Manager
    {
        _lifeText.text = ((int)lifeAmount).ToString();
    }

    [PunRPC]
    public void ChangePlayerSpeed(float newSpeed) //Called by Server Manager
    {
        _movementSpeed = newSpeed;
    }

    [PunRPC]
    public void ScalePlayer(Vector3 newScaleAmount)
    {
        transform.localScale = newScaleAmount;
        //print($"new localScale of gameobject {gameObject.transform.name} = {transform.localScale} . newScaleAmount = {newScaleAmount}");
    }

    public void ShowClientUI()
    {
        _playerHudData.ShowUi();
    }

    [PunRPC]
    public void DisconnectPlayer(string sceneName) //RPC Called by serverManager
    {
        SceneManager.LoadScene(sceneName);
        //PhotonNetwork.Disconnect();
    }

    [PunRPC]
    public void SetCanvas() //RPC Called by serverManager
    {
        ShowClientUI();
    }

    [PunRPC]
    public void UpdateGUIData(float life, float time, float startCountdownTime, float targetTime) //RPC Called by serverManager
    {
        _playerHudData.UpdateAllGUI(life, time, startCountdownTime, targetTime);
    }

    [PunRPC]
    public void InitClientMic() //RPC Called by serverManager
    {
        _recorderObj = PhotonNetwork.Instantiate(_folderPrefabsPath + "/" + _photonVoiceViewPrefab.name, transform.position, Quaternion.identity);
        _recorderObj.GetComponent<PhotonVoiceView>().RecorderInUse = _server.Recorder;
    }

    [PunRPC]
    public void KillPlayer() //RPC Called by serverManager
    {
        if (!_isDead)
        {
            StartCoroutine(Die());
        }
    }
    #endregion

    IEnumerator Die()
    {
        _isDead = true;
        _myPlayerView.ExecuteDieAnimation();
        yield return new WaitForSeconds(0.1f);

        PhotonNetwork.Destroy(_recorderObj);
        _server.photonView.RPC("OnPlayerDieNotification", _server.GetPlayerServer, this);
    }
}
