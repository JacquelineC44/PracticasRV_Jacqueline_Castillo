using UnityEngine;

public class Reinicio : MonoBehaviour
{
    public Transform player;
    public Transform playerStartPoint;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entró al trigger: " + other.name);

        if (other.transform == player || other.transform.IsChildOf(player))
        {
            RespawnPlayer();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform == player || other.transform.IsChildOf(player))
        {
            RespawnPlayer();
        }
    }

    void RespawnPlayer()
    {
        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        player.position = playerStartPoint.position;
        player.rotation = playerStartPoint.rotation;

        if (cc != null)
            cc.enabled = true;
    }
}