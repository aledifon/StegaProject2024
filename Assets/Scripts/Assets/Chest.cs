using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Chest : MonoBehaviour
{
    Animator animator;
    AudioSource audiouSource;
    [SerializeField] AudioClip clip;
    [SerializeField] Sprite itemSprite;
    SpriteRenderer itemSpriteRenderer;
    BoxCollider2D collider;

    [Header("Fading")]
    [SerializeField, Range(0f,5f)] float fadingTime;
    private float fadingTimer;
    private Color colorSprite;
    [SerializeField, Range(0f, 2f)] float targetPosY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        animator = GetComponent<Animator>();
        audiouSource = GetComponent<AudioSource>();
        collider = GetComponent<BoxCollider2D>();
        itemSpriteRenderer = transform.Find("Item").GetComponent<SpriteRenderer>();        
        itemSpriteRenderer.sprite = itemSprite;
        colorSprite = itemSpriteRenderer.color;
        colorSprite.a = 0f;
        itemSpriteRenderer.color = colorSprite;
    }    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collider.enabled = false;
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

        Transform itemTransform = itemSpriteRenderer.transform;
        Vector2 targetPos = itemTransform.localPosition;
        targetPos.y = targetPos.y + targetPosY;

        float speed = Vector2.Distance(itemTransform.localPosition, targetPos)/fadingTime;

        while (fadingTimer < fadingTime)
        {
            colorSprite.a = fadingTimer/fadingTime;
            itemSpriteRenderer.color = colorSprite;

            itemTransform.localPosition = Vector2.MoveTowards(itemTransform.localPosition, targetPos, speed*Time.deltaTime);

            fadingTimer += Time.deltaTime;
            yield return null;
        }

        colorSprite.a = 1f;
        itemSpriteRenderer.color = colorSprite;

        itemTransform.localPosition = targetPos;
    }    
}
