using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class LifeCollectible : MonoBehaviour
{
    //Animator animator;
    AudioSource audioSource;
    bool isCaptured = false;

    [SerializeField] AudioClip clip;

    [Header("Text")]    
    TextMeshProUGUI textBox;
    private Color colorTextBox;
    [SerializeField] string text;
    Transform itemPos;
    Transform canvasPos;

    Collider2D myCollider;

    [Header("Fading")]
    [SerializeField, Range(0f, 10f)] float fadingTime;
    private float fadingTimer;    
    [SerializeField, Range(0f, 5f)] float targetPosY;
    private SpriteRenderer itemSprite;
    private Color itemSpriteColor;

    [Header("Movement")]
    [SerializeField] float vertMoveTime;
    private float vertMoveTimer = 0f;
    [SerializeField, Range(-2f,2f)] float DeltaPosY;
    Vector3[] movePos = new Vector3[2];        
    int idxPos;
    [SerializeField] float rotationSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Get components on the parent GO        
        audioSource = GetComponent<AudioSource>();
        myCollider = GetComponent<Collider2D>();

        // Get the SpriteRenderer Component on the Item Child & the textPos
        itemPos = transform.Find("Item");
        itemSprite = itemPos.GetComponent<SpriteRenderer>();
        itemSpriteColor = itemSprite.color;

        // Get the TextMeshProUGUI component on the Text Child
        canvasPos = transform.Find("Canvas");
        textBox = canvasPos.GetComponentInChildren<TextMeshProUGUI>(true);
        textBox.text = "";
        colorTextBox = textBox.color;
        colorTextBox.a = 0f;
        textBox.color = colorTextBox;

        // Get the Sprite starting pos. & set the target pos.
        movePos[0] = itemPos.localPosition;
        movePos[1] = itemPos.localPosition;
        movePos[1].y += DeltaPosY;
    }
    private void Update()
    {
        Movement();        
        Rotation();
    }    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isCaptured)
        {
            isCaptured = true;

            // Save the sprite before disabling it
            Sprite lifeSprite = itemSprite.sprite;

            myCollider.enabled = false;            

            // Increase Lifes counter
            collision.gameObject.GetComponent<PlayerMovement>().IncreaseLifes();

            // Play Life Fly VFX
            CollectItemFlyFX.Play(lifeSprite, transform.position, GameManager.Instance.LifesUIImage);

            StartCoroutine(nameof(CaptureLife));            
        }
    }

    private void Movement()
    {
        float distSqr = (itemPos.localPosition - movePos[idxPos]).sqrMagnitude;
        vertMoveTimer += Time.deltaTime;
        float t = vertMoveTimer / vertMoveTime;

        if (distSqr <= 0.0001f)
        {
            vertMoveTimer = 0f;

            if (idxPos == 1)
                idxPos = 0;
            else
                idxPos++;
        }
        else
            itemPos.localPosition = Vector3.Lerp(itemPos.localPosition, movePos[idxPos], t);
    }
    private void Rotation()
    {
        itemPos.Rotate(Vector3.up*rotationSpeed*Time.deltaTime); 
    }

    private IEnumerator CaptureLife()
    {        
        audioSource.PlayOneShot(clip);
        rotationSpeed *= 10f; 

        yield return StartCoroutine(nameof(FadingAndMoving));

        yield return new WaitUntil(() => !audioSource.isPlaying);
        
        Destroy(gameObject);
    }
    private IEnumerator FadingAndMoving()
    {
        fadingTimer = 0f;        

        colorTextBox.a = 0f;
        textBox.color = colorTextBox;
        textBox.text = text;

        itemSpriteColor.a = 1f;
        itemSprite.color = itemSpriteColor;

        Transform textTransform = canvasPos.transform;
        Vector2 targetPos = textTransform.localPosition;
        targetPos.y = targetPos.y + targetPosY;

        float speed = Vector2.Distance(textTransform.localPosition, targetPos) / fadingTime;

        while (fadingTimer < fadingTime)
        {           
            colorTextBox.a = fadingTimer / fadingTime;
            textBox.color = colorTextBox;

            // Sprite Scale fading from 1 to 0
            itemPos.localScale = new Vector3(Mathf.Clamp(1 - colorTextBox.a,0.5f,1),
                                                itemPos.localScale.y,
                                                itemPos.localScale.z);

            itemSpriteColor.a = 1f - (fadingTimer / fadingTime);
            itemSprite.color = itemSpriteColor;


            textTransform.localPosition = Vector2.MoveTowards(textTransform.localPosition, targetPos, speed * Time.deltaTime);

            fadingTimer += Time.deltaTime;
            yield return null;
        }        

        colorTextBox.a = 1f;
        textBox.color = colorTextBox;

        itemSpriteColor.a = 0f;
        itemSprite.color = itemSpriteColor;

        textTransform.localPosition = targetPos;
    }
    private void HideText()
    {
        colorTextBox.a = 0f;
        textBox.color = colorTextBox;
    }    
}
