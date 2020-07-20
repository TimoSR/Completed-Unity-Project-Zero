using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage;
    private int attackerId;
    private bool isMine;

    public Rigidbody rig;

    // called when the bullet is spawned
    public void Initialize (int damage, int attackerId, bool isMine)
    {
        this.damage = damage;
        this.attackerId = attackerId;
        this.isMine = isMine;

        // set a lifetime so it can't go on forever
        Destroy(gameObject, 5.0f);
    }

    void OnTriggerEnter (Collider other)
    {
        // did we hit a player?
        // if this is the local player's bullet, damage the hit player
        // we're using client side hit detection
        if(other.CompareTag("Player") && isMine)
        {
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if(player.id != attackerId)
                player.photonView.RPC("TakeDamage", player.photonPlayer, attackerId, damage);
        }

        Destroy(gameObject);
    }
}