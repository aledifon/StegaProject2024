using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class Door : MonoBehaviour
{
    Animator animator;
    AudioSource audioSource;
    bool isOpened = false;
    bool isTriedOpenOnce = false;

    [SerializeField] AudioClip unlockDoorClip;
    [SerializeField] AudioClip openDoorClip;
    [SerializeField] AudioClip doorLockedClip;

    [Header("Text")]    
    TextMeshProUGUI textBox;
    private Color colorTextBox;
    [SerializeField] string text;
    Transform textPos;

    BoxCollider2D openDoorCollider;
    BoxCollider2D EnterDoorCollider;

    [Header("Fading")]
    [SerializeField, Range(0f, 5f)] float fadingTime;
    private float fadingTimer;    
    [SerializeField, Range(0f, 2f)] float targetPosY;

    //[Header("VFX")]
    //[SerializeField] private GameObject dustVFX;
    //private ParticleSystem dustPS;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Get components on the parent GO
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        foreach(var collider in GetComponents<BoxCollider2D>())
        {
            if(collider.isTrigger)
                openDoorCollider = collider;
            else
                EnterDoorCollider = collider;
        }
        //openDoorCollider = GetComponent<BoxCollider2D>();

        // Get the SpriteRenderer Component on the Item Child
        textPos = transform.Find("Item");                

        // Get the TextMeshProUGUI component on the Text Child
        textBox = GetComponentInChildren<TextMeshProUGUI>(true);
        textBox.text = "";
        colorTextBox = textBox.color;
        colorTextBox.a = 0f;
        textBox.color = colorTextBox;

        // VFX
        //if (dustVFX == null)
        //    Debug.LogError("No Sparks VFX were added on the Inspector");
        //else
        //{
        //    dustPS = InstantiateVFXPrefabs(dustVFX, transform, transform);                        
        //}            
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isOpened)
        {
            if (collision.GetComponent<PlayerMovement>().IsKeyUnlocked)
            {
                openDoorCollider.enabled = false;
                isOpened = true;

                // Register the checkpoint pos. as the player Respawn position.          
                // GameManager.Instance.RegisterCheckPoint();

                StartCoroutine(nameof(CaptureCheckpoint));

                GameManager.Instance.DisableAllInputs();
            }            

            if (!collision.GetComponent<PlayerMovement>().IsKeyUnlocked && !isTriedOpenOnce)
            {                
                isTriedOpenOnce = true;               
                StartCoroutine(nameof(TryOpenDoorWithoutKey));
            }            
        }
    }

    private IEnumerator TryOpenDoorWithoutKey()
    {        
        audioSource.PlayOneShot(doorLockedClip);        

        yield return StartCoroutine(nameof(FadingAndMoving));
        yield return new WaitForSeconds(2f);

        HideText();
    }
    private IEnumerator CaptureCheckpoint()
    {        
        audioSource.PlayOneShot(unlockDoorClip);

        //PlayVFX(dustPS);        

        yield return new WaitUntil(() => !audioSource.isPlaying);

        animator.SetTrigger("Open");
        audioSource.PlayOneShot(openDoorClip);

        //yield return StartCoroutine(nameof(FadingAndMoving));
        
        //StopVFX(dustPS);

        yield return new WaitForSeconds(2f);

        //HideText();

        // Trigger The End Scene
        GameManager.Instance.SetEndCreditsSceneFlag(true);
    }

    private IEnumerator FadingAndMoving()
    {
        fadingTimer = 0f;        

        colorTextBox.a = 0f;
        textBox.color = colorTextBox;
        textBox.text = text;

        Transform textTransform = textPos.transform;
        Vector2 targetPos = textTransform.localPosition;
        targetPos.y = targetPos.y + targetPosY;

        float speed = Vector2.Distance(textTransform.localPosition, targetPos) / fadingTime;

        while (fadingTimer < fadingTime)
        {           
            colorTextBox.a = fadingTimer / fadingTime;
            textBox.color = colorTextBox;

            textTransform.localPosition = Vector2.MoveTowards(textTransform.localPosition, targetPos, speed * Time.deltaTime);

            fadingTimer += Time.deltaTime;
            yield return null;
        }        

        colorTextBox.a = 1f;
        textBox.color = colorTextBox;

        textTransform.localPosition = targetPos;
    }
    private void HideText()
    {
        colorTextBox.a = 0f;
        textBox.color = colorTextBox;
    }

    private ParticleSystem InstantiateVFXPrefabs(GameObject prefab, Transform originTransform, Transform parentTransform)
    {
        ParticleSystem ps = Instantiate(prefab, parentTransform).GetComponent<ParticleSystem>();

        ps.transform.localPosition = Vector3.zero;
        ps.transform.localRotation = prefab.transform.rotation;
        //ps.transform.localRotation *= Quaternion.Euler(30f,0f,0f);

        return ps;

        //return Instantiate(prefab, originTransform.position, originTransform.rotation, parentTransform).
        //                GetComponent<ParticleSystem>();        
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
}
