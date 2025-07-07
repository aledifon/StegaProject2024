using Demo_Project;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    // Singleton instance of GameManager
    private static GameManager instance;
    public static GameManager Instance
    {
        get 
        { 
            if (instance == null)
            {
                instance = FindAnyObjectByType<GameManager>();

                if (instance == null)
                {
                    Debug.LogWarning("No GameManager found in scene. Creating a new one.");
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance; 
        }
    }

    [SerializeField] AudioClip gameOverClip;    

    [Header("Surface Type")]
    [SerializeField] private bool isWetSurface;
    public bool IsWetSurface => isWetSurface;


    [Header("Slow Hit Time")]
    [SerializeField] float elapsedSlowHitTime;
    [SerializeField] float durationSlowHitTime;     // 1f;

    [Header("Slow Hit Time DOTWeen")]
    [SerializeField] float slowDuration;            // 0.3f
    [SerializeField] float returnDuration;          // 1f;

    // GO Refs.
    AudioSource generalAudioSource;
    PlayerHealth playerHealth;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;

        DontDestroyOnLoad(gameObject);

        generalAudioSource = GetComponent<AudioSource>();
    }    
    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDamagePlayer -= SlowMotionOnHit;
    }
    public void SubscribeEventsOfPlayerHealth(PlayerHealth pH)
    {
        playerHealth = pH;
        playerHealth.OnDamagePlayer += SlowMotionOnHit;
    }
    public void PlayGameOverSFx()
    {
        if(generalAudioSource.isPlaying)
            generalAudioSource.Stop();

        generalAudioSource.PlayOneShot(gameOverClip);
    }
    public void EnableSlowMotion()
    {
        Time.timeScale = 0.2f; // Velocidad al 20%
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Ajustar física
    }
    private void SlowMotionOnHit()
    {
        // 1. Mata cualquier tween previo con el ID "SlowTime"
        DOTween.Kill("SlowTime");

        // 2. Pone el tiempo a 0 instantáneamente
        Time.timeScale = 0.2f;

        // 3. Usa DOVirtual.DelayedCall para esperar slowDuration segundos en tiempo real
        DOVirtual.DelayedCall(slowDuration, () =>
        {
            // 4. Cuando termina la espera, comienza el tween para interpolar Time.timeScale de 0 a 1
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, returnDuration)
                //.SetEase(Ease.OutCubic)
                .SetEase(Ease.InQuad)
                // 5. Le da un ID para poder controlar o cancelar este tween luego
                .SetId("SlowTime")
                // 6. Hace que el tween corra usando tiempo real, ignorando Time.timeScale
                .SetUpdate(true);
        })
        // 7. Hace que la llamada retrasada también use tiempo real para contar correctamente
        .SetUpdate(true);
    }
    private void SlowMotionOnHit_()
    {
        StartCoroutine(nameof(SlowMotionOnHitCoroutine));
    }    
    IEnumerator SlowMotionOnHitCoroutine()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.3f);

        elapsedSlowHitTime = 0f;

        while (elapsedSlowHitTime < durationSlowHitTime)
        {
            float t = elapsedSlowHitTime / durationSlowHitTime;
            Time.timeScale = Mathf.Lerp(0f,1f,t);
                        
            yield return null;
            elapsedSlowHitTime += Time.unscaledDeltaTime;
        }        

        Time.timeScale = 1f;
    }

}
