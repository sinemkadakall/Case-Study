using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public PieceType pieceType;
    

    public int xIndex;
    public int yIndex;

    public bool isMatched;
    public bool isMoving;

    private Vector2 currentPos;
    private Vector2 targetPos;

    private SpriteRenderer spriteRenderer;
    public Sprite defaultSprite;
    public Sprite bombSprite;    // 5-7 eþleþme için
    public Sprite rocketSprite;  // 8-9 eþleþme için
    public Sprite specialSprite; // 10+ eþleþme için


    private float checkInterval = 0.2f;
    private float nextCheckTime = 0f;

    public Piece(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;

    }
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Mevcut sprite'ý default olarak kaydet

        //BUNU TEST ÝÇÝN KAPADIM
        //defaultSprite = spriteRenderer.sprite;

        if (spriteRenderer == null)
        {
            Debug.LogError($"SpriteRenderer is null on {gameObject.name}. Make sure this GameObject has a SpriteRenderer component.");
        }
    }

    private void Update()
    {
        if (!isMoving && Time.time >= nextCheckTime)
        {
            CheckAndUpdateSprite();
            nextCheckTime = Time.time + checkInterval;
        }
    }

    private void CheckAndUpdateSprite()
    {
        if (Board.Instance != null)
        {
            List<Piece> connectedPieces = Board.Instance.GetConnectedPieces(this);
            if (spriteRenderer != null)
            {
                UpdateSprite(connectedPieces.Count);
            }
        }
    }
    public void SetIndicies(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;

    }

    public void MoveToTarget(Vector2 _targetPos)
    {
        StartCoroutine(MoveCoroutine(_targetPos));
    }

    private IEnumerator MoveCoroutine(Vector2 _targetPos)
    {
        isMoving = true;
        float duration = 0.2f;
        
        Vector2 startPosition = transform.position;
        float elaspedTime = 0f;

        while (elaspedTime<duration)
        {
            float t = elaspedTime / duration;

            transform.position = Vector2.Lerp(startPosition, _targetPos, t);

            elaspedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _targetPos;
        isMoving = false;
    }
    public void UpdateSprite(int matchCount)
    {

     
        if (matchCount >= 10 && specialSprite != null)
        {
            spriteRenderer.sprite = specialSprite;
          
            Debug.Log($"10+ eþleþme (C ikonu): {matchCount} parça");
            Debug.Log($"MatchCount for {pieceType} ({gameObject.name}): {matchCount}");

        }
        else if (matchCount >= 8 && rocketSprite != null)
        {
            spriteRenderer.sprite = rocketSprite;
           
            Debug.Log($"8-9 eþleþme (B ikonu): {matchCount} parça");
            Debug.Log($"MatchCount for {pieceType} ({gameObject.name}): {matchCount}");

        }
        else if (matchCount >= 5 && bombSprite != null)
        {
            spriteRenderer.sprite = bombSprite;
          
            Debug.Log($"5-7 eþleþme (A ikonu): {matchCount} parça");
            Debug.Log($"MatchCount for {pieceType} ({gameObject.name}): {matchCount}");

        }
        else
        {
            spriteRenderer.sprite = defaultSprite;
           
            Debug.Log($"Normal eþleþme: {matchCount} parça");
            Debug.Log($"Sprite set to: {spriteRenderer.sprite.name} for {pieceType} ({gameObject.name})");

        }
    }

    // Sprite'ý default haline döndürme metodu
    public void ResetSprite()
    {
        spriteRenderer.sprite = defaultSprite;
    }
}
    


