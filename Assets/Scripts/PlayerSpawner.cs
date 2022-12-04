using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;
    public GameObject playerPrefab;
    private GameObject player;
    public GameObject deathEffect;
    public float respawnTime=5f;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }
     public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }
    public void Die(string damager)
    {
        UIController.instance.dieMessage.text = "You were killed by " + damager;
        // SpawnPlayer();
        MatchManager.instance.UpdateStatSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
       if(player!=null)
        {
            StartCoroutine(DieCo(respawnTime));
        }
    }
    public IEnumerator DieCo(float duration)
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        player = null;
        UIController.instance.dieScreen.SetActive(true);
        yield return new WaitForSeconds(duration);
        UIController.instance.dieScreen.SetActive(false);
        if(MatchManager.instance.state == MatchManager.GameState.Playing && player == null)
        {
            SpawnPlayer();
        }
    }
}
