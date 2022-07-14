using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerView : MonoBehaviourPun
{
    [Header("---- Object References ----")]
    [SerializeField] private PlayerModel _playerModel = null;

    [Header("---- Victory & Defeat visuals ----")]
    [SerializeField] private ParticleSystem _winParticles = null;
    [SerializeField] private GameObject _winOverlayScreen = null;
    [SerializeField] private ParticleSystem _looseParticles = null;
    [SerializeField] private GameObject _looseOverlayScreen = null;

    private Animator _myAnimator = null;

    private void Awake()
    {
        if (!photonView.IsMine) Destroy(this);
        _myAnimator = GetComponent<Animator>();
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

    public void OnPlayerWinHandler()
    {
        ExecuteWinAnimation();
    }

    public void OnPlayerLooseHandler()
    {
        ExecuteLooseAnimation();
    }

    private void ExecuteWinAnimation()
    {
        if (_winParticles != null)
        {
            _winOverlayScreen.SetActive(true);
            _winParticles.Play();
        }
    }

    private void ExecuteLooseAnimation()
    {
        if (_looseParticles != null)
        {
            _looseOverlayScreen.SetActive(true);
            _looseParticles.Play();
        }
    }

    IEnumerator DelayToResetBool(string boolName, bool value, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        _myAnimator.SetBool(boolName, value);
    }
}
