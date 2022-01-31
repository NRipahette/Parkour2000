using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnColliderScript : MonoBehaviour
{
    GameObject Player;
    private void Awake()
    {
        Player = GameObject.Find("Player");
    }
    private void OnTriggerEnter(Collider other)
    {
        Player.GetComponent<PlayerController>().Respawn();
    }
}
