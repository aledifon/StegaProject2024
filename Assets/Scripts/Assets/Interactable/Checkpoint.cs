using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    Animator animator;
    AudioSource audiouSource;
    bool isCaptured = false;    

    [SerializeField] AudioClip clip;

    [Header("Text")]    
    TextMeshProUGUI textBox;
    private Color colorTextBox;
    [SerializeField] string text;
    Transform textPos;

    BoxCollider2D myCollider;

    [Header("Fading")]
    [SerializeField, Range(0f, 5f)] float fadingTime;
    private float fadingTimer;    
    [SerializeField, Range(0f, 2f)] float targetPosY;

    [Header("VFX")]
    [SerializeField] private GameObject sparksVFX;
    private ParticleSystem[] sparksPS = new ParticleSystem[3];

    [Header("Camera Confiner Data")]    
    [SerializeField] private CamTriggerAreaData checkPointAreaData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Get components on the parent GO
        animator = GetComponent<Animator>();
        audiouSource = GetComponent<AudioSource>();
        myCollider = GetComponent<BoxCollider2D>();

        // Get the SpriteRenderer Component on the Item Child
        textPos = transform.Find("Item");                

        // Get the TextMeshProUGUI component on the Text Child
        textBox = GetComponentInChildren<TextMeshProUGUI>(true);
        textBox.text = "";
        colorTextBox = textBox.color;
        colorTextBox.a = 0f;
        textBox.color = colorTextBox;

        // VFX
        if (sparksVFX == null)
            Debug.LogError("No Sparks VFX were added on the Inspector");
        else
        {
            sparksPS[0] = InstantiateVFXPrefabs(sparksVFX, transform, transform);            

            sparksPS[1] = InstantiateVFXPrefabs(sparksVFX, transform, transform);
            Quaternion rotationX = Quaternion.AngleAxis(30f,Vector3.up);
            sparksPS[1].transform.localRotation *= rotationX;

            sparksPS[2] = InstantiateVFXPrefabs(sparksVFX, transform, transform);
            rotationX = Quaternion.AngleAxis(-30f, Vector3.up);
            sparksPS[2].transform.localRotation *= rotationX;            
        }

        // Check if the Camera Confiner Data are correctly set
        if(checkPointAreaData.respawnPos == null)
        {
            Debug.LogError("No Respawn Pos assigned!");
            checkPointAreaData.respawnPos = transform;
        }            

        if (checkPointAreaData.camTriggerAreaId == CamTriggerAreaEnum.CamTriggerArea.Init)
            Debug.LogError("The Area Id position set is not correct!");

        if (checkPointAreaData.respawnCamBoundTriggerArea == null)
            Debug.LogError("No Cam Bound Trigger Area is assigned to this Checkpoint GO!");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isCaptured)
        {
            if (collision.gameObject.name != "Player")
                return;

            myCollider.enabled = false;
            isCaptured = true;

            // Register the checkpoint pos. as the player Respawn position.          
            GameManager.Instance.SetLastCheckPointData(checkPointAreaData);

            StartCoroutine(nameof(CaptureCheckpoint));
        }
    }

    private IEnumerator CaptureCheckpoint()
    {
        animator.SetTrigger("Capture");
        audiouSource.PlayOneShot(clip);

        foreach (ParticleSystem ps in sparksPS)
            PlayVFX(ps);        

        yield return StartCoroutine(nameof(FadingAndMoving));

        foreach (ParticleSystem ps in sparksPS)
            StopVFX(ps);

        yield return new WaitForSeconds(5f);

        HideText();
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
