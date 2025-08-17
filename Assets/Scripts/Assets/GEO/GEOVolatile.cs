using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.GraphicsBuffer;

public class GEOVolatile : MonoBehaviour
{
    // Settings
    [Header("Vanishing Settings")]
    [SerializeField] float vanishingTime;
    [SerializeField] float reappearingTime;
    [SerializeField] bool isDestructible;       // The GO will be destroyed or disabled after vanishing.
    [SerializeField] bool isDestroyedByTrigger; // In case the column will be destroyed by an Event
    [SerializeField] bool destroyTrigger;
    private bool isVanished;
    public bool IsVanished => isVanished;

    // TilemapRenderer Material
    Material instanceMaterial;

    [Header("VFX")]
    [SerializeField] private GameObject dustStonesVFX;    
    [SerializeField] Transform[] psPositions;
    private ParticleSystem[] dustStonesPS;
    //[SerializeField] private float vfxPlaybackTime;

    [Header("SFX")]    
    [SerializeField] AudioClip breakSFX;

    // GO Refs
    Collider2D playerCollider;
    BoxCollider2D colliderFloor;
    BoxCollider2D colliderWall;
    TilemapRenderer tilemapRenderer;
    AudioSource audioSource;
    
    private bool isCoroutineRunning;
    private bool isColumn;

    #region Unity API
    void Awake()
    {
        // For ColumnVolatile GO
        if (gameObject.CompareTag("Column"))
        {
            isColumn = true;

            Transform child = transform.Find("ColumnFloor");
            colliderFloor = child.GetComponent<BoxCollider2D>();

            child = transform.Find("ColumnWall");
            colliderWall = child.GetComponent<BoxCollider2D>();

            if (colliderFloor == null || colliderWall == null)
                Debug.LogError("Colliders werer Not found on any child of the Column GO");
        }
        // For PlatformVolatile GO
        else
        {
            isColumn = false;

            colliderFloor = GetComponentInChildren<BoxCollider2D>();
            if (colliderFloor == null)
                Debug.LogError("Collider Not found on any child of the Platform");
        }                    

        // Get the Tilemap Renderer ref.
        tilemapRenderer = GetComponentInChildren<TilemapRenderer>();
        if ((tilemapRenderer) == null)
            Debug.LogError("Tilemap Renderer Not found on any child of the Platform GO");

        if (tilemapRenderer.material == null)
        {
            Debug.LogError("It was not found any default material assigned to the Tilemap Renderer!");
        }
        else
        {
            instanceMaterial = new Material(tilemapRenderer.material);
            tilemapRenderer.material = instanceMaterial;
        } 
        
        // Setup VFX & SFX
        if (psPositions != null)
            dustStonesPS = new ParticleSystem[psPositions.Length];
        else
            Debug.LogError("The refs. to the PS posititons are not properly initialised through the Inspector");
        if (dustStonesPS != null)
            SetupVFX();
        else
            Debug.LogError("The refs. to the PS and/or the PS positions are not properly initialised");

        audioSource = GetComponent<AudioSource>();

        // Force to be autodestructible in case is configure as destructible By Trigger
        if (isDestroyedByTrigger)
            isDestructible = true;
    }    
    private void Update()
    {
        if (isDestroyedByTrigger && destroyTrigger && !isCoroutineRunning && !isVanished)
        {
            destroyTrigger = false;
            StartCoroutine(nameof(VanishPlatform));
        }                                
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((isColumn && collision.otherCollider == colliderFloor) || !isColumn)
        {
            if (collision.collider.CompareTag("Player"))
            {
                playerCollider = collision.collider;
                playerCollider.transform.SetParent(transform, true);

                if (!isDestroyedByTrigger)
                {
                    if (!isCoroutineRunning)                        
                        StartCoroutine(nameof(VanishPlatform));
                }
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if ((isColumn && collision.otherCollider == colliderFloor) || !isColumn)
        {
            if (collision.collider.CompareTag("Player"))
            {
                playerCollider = collision.collider;
                HandleExitPlatform();
            }
        }
    }
    private void OnDestroy()
    {
        HandleExitPlatform();
    }
    private void HandleExitPlatform()
    {
        if (playerCollider != null)
        {
            playerCollider.transform.SetParent(null, true);
            playerCollider = null;
        }
    }
    #endregion    
    #region Enabling Platform
    IEnumerator VanishPlatform()
    {                      
        isCoroutineRunning = true;

        // Start playing the FXs         
        StartCoroutine(PlayVFXForTime(vanishingTime));        
        PLaySFX();

        // Wait for an 60% of the Vanishing Time has elapsed and the start the Fade In
        yield return new WaitForSeconds(vanishingTime*0.6f);
        yield return StartCoroutine(SpriteFadeInOut(false, vanishingTime*0.4f));

        // Disable the platform's collider once the vanishing Time has elapsed
        EnablePlatform(false);
        isVanished = true;

        if (isDestructible)
        {
            HandleExitPlatform();
            isCoroutineRunning = false;
            Destroy(gameObject,1f);
            yield break;
        }
        else
        {
            // Start the fade in of the platform once a 90% of the reappearing Time has elapsed
            yield return new WaitForSeconds(reappearingTime * 0.9f);
            yield return StartCoroutine(SpriteFadeInOut(true, reappearingTime * 0.1f));
            // Re-Enable the platform's collider once the Reappearing time has completely elapsed       
            EnablePlatform(true);
            isVanished = false;
        }

        isCoroutineRunning = false;
    }
    private void EnablePlatform(bool enable)
    {
        // Enable/Disable the Colliders (also the Wall's collider in case of Column)       
        colliderFloor.enabled = enable;
        if (isColumn)
            colliderWall.enabled = enable;
    }
    #endregion
    #region Sprite Fading
    // fadeInOut = true --> Fade In | fadeInOut = false --> Fade Out
    private IEnumerator SpriteFadeInOut(bool fadeIn, float fadingTime)
    {
        float fadingTimer = fadingTime;

        Color newColor = tilemapRenderer.material.color;        

        while (fadingTimer > 0f)
        {            
            newColor.a = fadeIn ? 
                        (1 - (fadingTimer / fadingTime)) :
                        (fadingTimer / fadingTime);            
            tilemapRenderer.material.color = newColor;

            fadingTimer -= Time.deltaTime;
            yield return null;
        }

        newColor.a = fadeIn ? 1f : 0f;
        tilemapRenderer.material.color = newColor;
    }
    #endregion
    #region SFX
    private void PLaySFX()
    {
        audioSource.PlayOneShot(breakSFX);
    }
    #endregion
    #region VFX
    private void SetupVFX()
    {
        for(int i = 0; i<dustStonesPS.Length; i++)
        {
            dustStonesPS[i] = Instantiate(dustStonesVFX, transform).GetComponent<ParticleSystem>();
            dustStonesPS[i].transform.localPosition = psPositions[i].localPosition;
            dustStonesPS[i].transform.localRotation = psPositions[i].localRotation;
        }
    }
    private IEnumerator PlayVFXForTime(float playbackTime)
    {
        foreach(ParticleSystem ps in dustStonesPS)
            PlayVFX(ps);
        yield return new WaitForSeconds(playbackTime);
        foreach (ParticleSystem ps in dustStonesPS)
            StopVFX(ps);
    }
    private void PlayVFX(ParticleSystem ps)
    {
        //if (!ps.isPlaying)
        //    ps.Play();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();
    }
    private void StopVFX(ParticleSystem ps)
    {
        if (ps.isPlaying)
            ps.Stop();
    }
    #endregion
    #region Destroy Trigger
    public void DestroyTrigger()
    {
        destroyTrigger = true;
    }
    #endregion
}
