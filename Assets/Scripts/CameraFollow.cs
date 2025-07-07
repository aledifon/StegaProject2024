using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraFollow : MonoBehaviour
{        
    [SerializeField] Transform player;
    Vector3 offset;    // Initial distance between the camera and the player
    [SerializeField] float smoothTargetTime;
    
    PlayerHealth playerHealth;

    Vector3 smoothDampVelocity;            // Player's current speed storage (Velocity Movement type through r        

    private bool cameraFollowEnabled = true;

    [Header ("Camera Shaking")]
    [SerializeField] float shakeDuration;   // 2f
    [SerializeField] float shakeStrength;   // 0.5f
    [SerializeField] int shakeVibrato;    // 100
    [SerializeField] float shakeRandomness; // 90f

    
    void Start()
    {
        offset = transform.position - player.position;  // Calculate the initial distance between the camera and the player
    }    
    void FixedUpdate()
    {
        if (cameraFollowEnabled) 
            transform.position = Vector3.SmoothDamp(transform.position, player.position + offset, 
                                                    ref smoothDampVelocity, smoothTargetTime);
    }
    public void StopCameraFollow()
    {
        cameraFollowEnabled = false;
    }
    public void SubscribeEventsOfPlayerHealth(PlayerHealth pH)
    {
        playerHealth = pH;
        playerHealth.OnDamagePlayer += CameraShaking;
    }    
    private void CameraShaking()
    {
        Camera.main.transform.DOShakePosition(
            duration: shakeDuration,           // Duración total del shake
            strength: shakeStrength,         // Magnitud del movimiento (puede ser Vector3)
            vibrato: shakeVibrato,            // Cuántas veces vibra en ese tiempo
            randomness: 90f,        // Aleatoriedad del movimiento
            snapping: false,        // Si debe redondear los valores a enteros (tiles, píxeles, etc.)
            fadeOut: true           // Si el shake se va desvaneciendo hacia el final
    );
    }
}
