using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;

    [Header("UI")]
    [SerializeField] private Image iconLife;
    [SerializeField] private float amountLife;               
    
    CameraFollow cameraFollow;

    #region Events & Delegates
    public event Action<Vector2, float> OnHitFXPlayer;            
    public event Action OnDeathPlayer;
    #endregion

    // GO Components        
    PlayerMovement playerMovement;    

    #region Unity API
    void Awake()
    {        
        playerMovement = GetComponent<PlayerMovement>();                        
        currentHealth = maxHealth;

        //GameManager.Instance.SubscribeEventsOfPlayerHealth(this);
        //StartCoroutine(nameof(WaitForCameraAndSubscribe));
        SendRefsToCamera();
    }
    private void OnEnable()
    {
        // Subscription to PLayerHealth Events from the GameManager
        // (Need to be OnEnable to assure the GameManager is ready)
        if (GameManager.Instance != null)
            GameManager.Instance.SubscribeEventsOfPlayerHealth(this);
    }
    private void OnDisable()
    {
        // Unsubscription from CameraFollow to playerHealth Events
        if (cameraFollow != null)
            cameraFollow.UnsubscribeEventsOfPlayerHealth();

        // Unsubscription to PLayerHealth Events from the GameManager
        // (Need to be OnDisable to assure clean the refs when switching Scenes)
        if (GameManager.Instance != null)
            GameManager.Instance.UnsubscribeEventsOfPlayerHealth();
    }
    #endregion    
    private void SendRefsToCamera()
    {
        cameraFollow = FindAnyObjectByType<CameraFollow>();                

        if (cameraFollow != null)
            cameraFollow.SubscribeEventsOfPlayerHealth(this);
        else
            Debug.LogError("CameraFollow component Not Found on the Scene!");
    }
    private IEnumerator WaitForCameraAndSubscribe()
    {
        yield return new WaitUntil(() => Camera.main != null);

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
            cameraFollow.SubscribeEventsOfPlayerHealth(this);
    }

    #region Damage
    // Player's Damage Handler 
    public void TakeDamage(int damageAmount, Vector2 thrustEnemyDir, float thrustEnemyForce)
    {
        // Avoid Executing the method if Player has already dead
        if (playerMovement.CurrentState == PlayerMovement.PlayerState.Hurting || 
            currentHealth <= 0)        
            return;

        // Update's Player's Life and Acorn Life's UI
        DecreaseHealth(damageAmount);

        // If Health <=0 --> Executes Death Method
        if (currentHealth <= 0)
        {
            Death();
            return;
        }

        // Play The Health VFX
        PlayerHealthVFX.Play(
            GameManager.Instance.HealthUIImage,
            1f, 60f, 220, 150f,
            1f, 2f);

        // Trigger the OnHitFXPlayer Event        
        OnHitFXPlayer?.Invoke(thrustEnemyDir, thrustEnemyForce);
    }        
    #endregion
    #region Health
    private void DecreaseHealth(int amount)
    {
        // Update's Player's Life and Acorn Life's UI
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth,0,maxHealth);
        iconLife.fillAmount = currentHealth / maxHealth;
    }
    private void IncreaseHealth(int amount)
    {
        // Update's Player's Life and Acorn Life's UI
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        iconLife.fillAmount = currentHealth / maxHealth;
    }
    public void SetMaxHealth()
    {
        currentHealth = maxHealth;
        iconLife.fillAmount = currentHealth / maxHealth;
    }
    #endregion
    #region Death
    private void Death()
    {
        // Stop the Camera Following
        cameraFollow.StopCameraFollow();

        // Play the Game Over SFX        
        //GameManager.Instance.PlayGameOverSFx();

        // Decrease the number of lifes
        playerMovement.DecreaseLifes();

        // Trigger the Death Player Event
        OnDeathPlayer?.Invoke();                
    }
    #endregion
}
