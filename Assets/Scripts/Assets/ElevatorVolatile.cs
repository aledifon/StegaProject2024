using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.GraphicsBuffer;

public class ElevatorVolatile : MonoBehaviour
{
    // Settings
    [Header("Vanishing Settings")]
    [SerializeField] float vanishingTime;
    [SerializeField] float reappearingTime;    
    private bool isVanished;
    public bool IsVanished => isVanished;

    [Header("VFX")]
    [SerializeField] private GameObject dustStonesVFX;    
    [SerializeField] Transform[] psPositions;
    private ParticleSystem[] dustStonesPS = new ParticleSystem[3];
    [SerializeField] private float vfxPlaybackTime;

    [Header("SFX")]    
    [SerializeField] AudioClip breakSFX;

    // GO Refs
    BoxCollider2D collider;
    TilemapRenderer tilemapRenderer;
    AudioSource audioSource;

    #region Unity API
    void Awake()
    {   
        // Get the Platform's Collider ref.
        collider = GetComponentInChildren<BoxCollider2D>();
        if (collider == null)
            Debug.LogError("Collider Not found on any child of the Platform");

        // Get the Tilemap Renderer ref.
        tilemapRenderer = GetComponentInChildren<TilemapRenderer>();
        if ((tilemapRenderer) == null)
            Debug.LogError("Tilemap Renderer Not found on any child of the Platform");

        // Setup VFX & SFX
        if (dustStonesPS != null && psPositions != null)
            SetupVFX();
        else
            Debug.LogError("The refs. to the PS and/or the PS positions are not properly initialised");

        audioSource = GetComponent<AudioSource>();
    }    
    private void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(transform, true);
            StartCoroutine(nameof(EnableVanishingTimer));
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(null, true);
        }
    }
    #endregion    
    #region Enabling Platform
    IEnumerator EnableVanishingTimer()
    {        
        // Start playing the FXs         
        StartCoroutine(PlayVFXForTime(vfxPlaybackTime));
        PLaySFX();

        // Vanish the platform after elapsed the vanishing Time
        yield return new WaitForSeconds(vanishingTime);
        EnablePlatform(false);
        isVanished = true;

        // Reappear the platform after a certain time
        yield return new WaitForSeconds(reappearingTime);
        EnablePlatform(true);
        isVanished = false;
    }
    private void EnablePlatform(bool enable)
    {
        // Enable/Disable the Collider & the Tilemap Renderer
        collider.enabled = enable;
        tilemapRenderer.enabled = enable;
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
}
