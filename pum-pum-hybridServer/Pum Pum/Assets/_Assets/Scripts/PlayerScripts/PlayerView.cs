using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerView : MonoBehaviourPun
{
    [Header("---- Object References ----")]
    [SerializeField] private GameObject _dashIndicator = null;
    [SerializeField] private ParticleSystem _winParticles = null;
    //[SerializeField] private SpriteRenderer _playerSprite = null;
    [SerializeField] private PlayerModel _playerModel = null;
    private Animator _myAnimator = null;

    public GameObject DashIndicator { get => _dashIndicator; set => _dashIndicator = value; }

    private void Awake()
    {
        if (!photonView.IsMine) Destroy(this);
        _myAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        //GameplayManager.OnPlayerWins += OnPlayerWinsHandler;
        _playerModel.OnPlayerWin += OnPlayerWinHandler;
    }

    public void ExecuteDamageAnimation() 
    {
        _myAnimator.SetBool("Damaged", true);
        StartCoroutine(DelayToResetBool("Damaged", false, 0.01f));
    }

    public void ExecuteDieAnimation() 
    {
        _myAnimator.SetBool("Dead", true);
        StartCoroutine(DelayToResetBool("Dead", false, 0.02f));
    }

    //public void MakePlayerGhost()
    //{
    //    _playerSprite.color = new Color(1f, 1f, 1f, 0.5f);
    //    transform.localScale = new Vector2(1, 1);
    //    print("Transformando al player en ghost desde el playerView");
    //}

    private void ExecuteWinAnimation()
    {
        if(_winParticles != null) _winParticles.Play();
    }

    private void OnPlayerWinHandler()
    {
        //GameplayManager.OnPlayerWin -= OnPlayerWinsHandler;
        ExecuteWinAnimation();
    }

    IEnumerator DelayToResetBool(string boolName, bool value, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        _myAnimator.SetBool(boolName, value);
    }
}
