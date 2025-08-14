using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Chest : MonoBehaviour
{    
    Animator animator;
    AudioSource audiouSource;
    [SerializeField] AudioClip clip;

    [Header("Item")]
    [SerializeField] Sprite itemSprite;
    TextMeshProUGUI itemTextBox;
    private Color colorItemTextBox;
    [SerializeField] string itemText;
    SpriteRenderer itemSpriteRenderer;
    BoxCollider2D collider;
    
    [SerializeField] private ItemTypeEnum.ItemType itemType;

    [Header("Fading")]
    [SerializeField, Range(0f,5f)] float fadingTime;
    private float fadingTimer;
    private Color colorSprite;
    [SerializeField, Range(0f, 2f)] float targetPosY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Get components on the parent GO
        animator = GetComponent<Animator>();
        audiouSource = GetComponent<AudioSource>();
        collider = GetComponent<BoxCollider2D>();

        // Get the SpriteRenderer Component on the Item Child
        itemSpriteRenderer = transform.Find("Item").GetComponent<SpriteRenderer>();        
        itemSpriteRenderer.sprite = itemSprite;
        colorSprite = itemSpriteRenderer.color;
        colorSprite.a = 0f;
        itemSpriteRenderer.color = colorSprite;

        // Get the TextMeshProUGUI component on the Text Child
        itemTextBox = GetComponentInChildren<TextMeshProUGUI>(true);
        itemTextBox.text = "";
        colorItemTextBox = itemTextBox.color;
        colorItemTextBox.a = 0f;
        itemTextBox.color = colorItemTextBox;

        if (itemType == 0)
            itemType = ItemTypeEnum.ItemType.ClimbingBoots;        
    }    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collider.enabled = false;
            collision.gameObject.GetComponent<PlayerMovement>().UnlockPowerUp(itemType);
            StartCoroutine(nameof(OpenChest));            
        }
    }

    private IEnumerator OpenChest()
    {        
        animator.SetTrigger("Open");
        audiouSource.PlayOneShot(clip);

        yield return StartCoroutine(nameof(FadingAndMoving));

        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    private IEnumerator FadingAndMoving()
    {   
        fadingTimer = 0f;

        colorSprite.a = 0f;        
        itemSpriteRenderer.color = colorSprite;
        
        colorItemTextBox.a = 0f;
        itemTextBox.color = colorItemTextBox;
        itemTextBox.text = itemText;

        Transform itemTransform = itemSpriteRenderer.transform;
        Vector2 targetPos = itemTransform.localPosition;
        targetPos.y = targetPos.y + targetPosY;

        float speed = Vector2.Distance(itemTransform.localPosition, targetPos)/fadingTime;

        while (fadingTimer < fadingTime)
        {
            colorSprite.a = fadingTimer / fadingTime;
            itemSpriteRenderer.color = colorSprite;

            colorItemTextBox.a = fadingTimer / fadingTime;
            itemTextBox.color = colorItemTextBox;

            itemTransform.localPosition = Vector2.MoveTowards(itemTransform.localPosition, targetPos, speed*Time.deltaTime);

            fadingTimer += Time.deltaTime;
            yield return null;
        }

        colorSprite.a = 1f;
        itemSpriteRenderer.color = colorSprite;

        colorItemTextBox.a = 1f;
        itemTextBox.color = colorItemTextBox;

        itemTransform.localPosition = targetPos;
    }    
}
