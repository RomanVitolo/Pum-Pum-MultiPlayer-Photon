using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum PowerUpsTypes
{
    LIFE_POWERUP,
    SPEED_POWERUP,
    MIXED_POWERUP,
    EPIC_POWERUP,
}

public class BasicPowerUp : MonoBehaviourPun
{
    [Header("----------- PowerUpStats -----------")]
    [SerializeField] private PowerUpsTypes _type;
    private ServerManager _server = null;

    [Header("----------- VFX -----------")]
    [SerializeField] private ParticleSystem _particleSystemExplosion = null;

    private void Start()
    {
        _server = GameObject.FindGameObjectWithTag("ServerManager").GetComponent<ServerManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;

        PlayerModel playerModel = collision.gameObject.GetComponent<PlayerModel>();
        //print($"Collide with player id = {playerModel.photonView.ViewID}");
        if (playerModel != null)
        {
            GetPowerUpBehaviour(playerModel);
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

    private void GetPowerUpBehaviour(PlayerModel playerModel)
    {
        if (_server == null) return;
        Player player = _server?.PlayersModelsDic[playerModel];

        switch (_type)
        {
            case PowerUpsTypes.LIFE_POWERUP:
                //Debug.Log($"PowerUpsTypes.LIFE_POWERUP -- Player == {player.UserId} playerModel.RPC(CollisionWithLifePowerUp)");
                playerModel.photonView.RPC("CollisionWithLifePowerUp", player);
                break;
            case PowerUpsTypes.SPEED_POWERUP:
                //Debug.Log($"PowerUpsTypes.SPEED_POWERUP -- Player == {player.UserId} playerModel.RPC(CollisionWithSpeedPowerUp)");
                playerModel.photonView.RPC("CollisionWithSpeedPowerUp", player);
                break;
            case PowerUpsTypes.MIXED_POWERUP:
                //Debug.Log($"PowerUpsTypes.MIXED_POWERUP -- Player == {player.UserId} playerModel.RPC(CollisionWithMixedPowerUp)");
                playerModel.photonView.RPC("CollisionWithMixedPowerUp", player);
                break;
            case PowerUpsTypes.EPIC_POWERUP:
                //Debug.Log($"PowerUpsTypes.EPIC_POWERUP -- Player == {player.UserId} playerModel.RPC(CollisionWithEpicPowerUp)");
                playerModel.photonView.RPC("CollisionWithEpicPowerUp", player);
                break;
            default:
                break;
        }
    }
}

