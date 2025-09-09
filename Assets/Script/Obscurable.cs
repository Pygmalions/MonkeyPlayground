using UnityEngine;
using System.Collections.Generic;

public class Obscurable : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("猴子需要进入这个半径才能识别物体")]
    [SerializeField] private float identificationRadius = 3f;

    [Tooltip("代表'未知'状态的Sprite (例如一个问号)")]
    [SerializeField] private Sprite unknownSprite;

    private Transform playerTransform;
    private GameObject unknownVisualObject;

    private List<Behaviour> behavioursToManage = new List<Behaviour>();
    private List<Renderer> renderersToManage = new List<Renderer>();
    private List<Collider2D> collidersToManage = new List<Collider2D>();
    
    private Rigidbody2D rb;
    private RigidbodyType2D originalBodyType;

    private void Awake()
    {
        playerTransform = FindObjectOfType<Monkey>().transform;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            originalBodyType = rb.bodyType;
        }

        behavioursToManage.AddRange(GetComponents<Behaviour>());
        renderersToManage.AddRange(GetComponents<Renderer>());
        collidersToManage.AddRange(GetComponents<Collider2D>());

        behavioursToManage.Remove(this);

        // 2. 禁用它们
        foreach (var behaviour in behavioursToManage)
        {
            behaviour.enabled = false;
        }
        foreach (var renderer in renderersToManage)
        {
            renderer.enabled = false;
        }
        foreach (var collider in collidersToManage)
        {
            collider.enabled = false;
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero; // 同时清除速度，以防万一
        }
        
        CreateUnknownVisual();
    }

    private void CreateUnknownVisual()
    {
        unknownVisualObject = new GameObject("Unknown Visual");
        unknownVisualObject.transform.position = transform.position;
        unknownVisualObject.transform.SetParent(transform);

        var spriteRenderer = unknownVisualObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = unknownSprite;
        spriteRenderer.sortingOrder = 10;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= identificationRadius)
        {
            Reveal();
        }
    }

    private void Reveal()
    {
        if (unknownVisualObject != null)
        {
            Destroy(unknownVisualObject);
        }

        foreach (var behaviour in behavioursToManage)
        {
            if (behaviour != null) behaviour.enabled = true;
        }
        foreach (var renderer in renderersToManage)
        {
            if (renderer != null) renderer.enabled = true;
        }
        foreach (var collider in collidersToManage)
        {
            if (collider != null) collider.enabled = true;
        }

        this.enabled = false;
        
        if (rb != null)
        {
            rb.bodyType =originalBodyType;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, identificationRadius);
    }
}