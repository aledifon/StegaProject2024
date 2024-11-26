using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    Vector3 offset; // initial distance between the camera and the player 
    public float smoothTargetTime;

    Vector3 smoothDampVelocity;
    PlayerHealth playerHealth;

    // Start is called before the first frame update
    void Start()
    {
        playerHealth = player.GetComponent<PlayerHealth>();
        offset = transform.position - player.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerHealth.currentHealth <= 0)
        {
            //GameOver
            return;
        }
        transform.position = Vector3.SmoothDamp(transform.position, player.position + offset, 
                                                ref smoothDampVelocity, smoothTargetTime);
    }
}
