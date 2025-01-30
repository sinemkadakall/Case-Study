using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    //define the size of the board
    public int width = 6;
    public int height = 8;
    //define some spacing for the board
    public float spacingX;
    public float spacingY;
    //get a reference to our piece prefabs
    public GameObject[] piecePrefabs;
    //get a reference to the collection nodes pieceBoard + GO
    public Node[,] pieceBoard;
    public GameObject pieceBoardGO;

    public List<GameObject> piecesToDestroy = new();
    public GameObject pieceParent;

    [SerializeField]
    private Piece selectedpiece;

    [SerializeField]
    private bool isProcessingMove;

    [SerializeField]
    List<Piece> piecesToRemove = new();

   
    //layoutArray
    public ArrayLayout arrayLayout;
    //public static of pieceboard
    public static Board Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeBoard();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<Piece>())
            {
                if (isProcessingMove)
                    return;

                Piece piece = hit.collider.gameObject.GetComponent<Piece>();
                Debug.Log("I have a clicked a piece it is: " + piece.gameObject);

                Selectpiece(piece);

                StartCoroutine(CheckForNoMatches());
            }
        }
    }

     void InitializeBoard()
     {

        arrayLayout.InitializeRows(width, height);
        // Destroypieces();
        pieceBoard = new Node[width, height];

         spacingX = (float)(width - 1) / 2;
         spacingY = (float)((height - 1) / 2) + 1;

         for (int y = 0; y < height; y++)
         {
             for (int x = 0; x < width; x++)
             {
                 Vector2 position = new Vector2(x - spacingX, y - spacingY);

                 //Vector2 position = new Vector2((x * 1.3f) - spacingX, (y * 1.3f) - spacingY);
                 if (arrayLayout.rows[y].row[x])
                 {
                     pieceBoard[x, y] = new Node(false, null);
                 }
                 else
                 {
                     int randomIndex = Random.Range(0, piecePrefabs.Length);

                     GameObject piece = Instantiate(piecePrefabs[randomIndex], position, Quaternion.identity);
                     piece.transform.SetParent(pieceParent.transform);
                     piece.GetComponent<Piece>().SetIndicies(x, y);
                     pieceBoard[x, y] = new Node(true, piece);
                   //  piecesToDestroy.Add(piece);
                 }
             }
         }

     }
    


    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
        foreach (Piece pieceToRemove in piecesToRemove)
        {
            pieceToRemove.isMatched = false;
        }

        RemoveAndRefill(piecesToRemove);
        yield return new WaitForSeconds(0.4f);

    }

    private void RemoveAndRefill(List<Piece> _piecesToRemove)
    {
        //Removing the piece and clearing the board at that location
        foreach (Piece piece in _piecesToRemove)
        {
            //getting it's x and y indicies and storing them
            int _xIndex = piece.xIndex;
            int _yIndex = piece.yIndex;

            //Destroy the piece
            Destroy(piece.gameObject);

            //Create a blank node on the piece board.
            pieceBoard[_xIndex, _yIndex] = new Node(true, null);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieceBoard[x, y].piece == null)
                {
            //        Debug.Log("The location X: " + x + " Y: " + y + " is empty, attempting to refill it.");
                    Refillpiece(x, y);
                }
            }
        }
    }

    private void Refillpiece(int x, int y)
    {
        //y offset
        int yOffset = 1;

        //while the cell above our current cell is null and we're below the height of the board
        while (y + yOffset < height && pieceBoard[x, y + yOffset].piece == null)
        {
            //increment y offset
          //  Debug.Log("The piece above me is null, but i'm not at the top of the board yet, so add to my yOffset and try again. Current Offset is: " + yOffset + " I'm about to add 1.");
            yOffset++;
        }

        //we've either hit the top of the board or we found a piece

        if (y + yOffset < height && pieceBoard[x, y + yOffset].piece != null)
        {
            //we've found a piece

            Piece pieceAbove = pieceBoard[x, y + yOffset].piece.GetComponent<Piece>();

            //Move it to the correct location
            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, pieceAbove.transform.position.z);
         //   Debug.Log("I've found a piece when refilling the board and it was in the location: [" + x + "," + (y + yOffset) + "] we have moved it to the location: [" + x + "," + y + "]");
            //Move to location
            pieceAbove.MoveToTarget(targetPos);
            //update incidices
            pieceAbove.SetIndicies(x, y);
            //update our pieceBoard
            pieceBoard[x, y] = pieceBoard[x, y + yOffset];
            //set the location the piece came from to null
            pieceBoard[x, y + yOffset] = new Node(true, null);
        }

        //if we've hit the top of the board without finding a piece
        if (y + yOffset == height)
        {
            Debug.Log("I've reached the top of the board without finding a piece");
            SpawnpieceAtTop(x);
        }

    }

    private void SpawnpieceAtTop(int x)
    {
        int index = FindIndexOfLowestNull(x);
        int locationToMoveTo = 8 - index;
        Debug.Log("About to spawn a piece, ideally i'd like to put it in the index of: " + index);
        //get a random piece
        int randomIndex = Random.Range(0, piecePrefabs.Length);
        GameObject newpiece = Instantiate(piecePrefabs[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
        newpiece.transform.SetParent(pieceParent.transform);
        //set indicies
        newpiece.GetComponent<Piece>().SetIndicies(x, index);
        //set it on the piece board
        pieceBoard[x, index] = new Node(true, newpiece);
        //move it to that location
        Vector3 targetPosition = new Vector3(newpiece.transform.position.x, newpiece.transform.position.y - locationToMoveTo, newpiece.transform.position.z);
        newpiece.GetComponent<Piece>().MoveToTarget(targetPosition);
    }

    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;
        for (int y = height - 1; y >= 0; y--)
        {
            if (pieceBoard[x, y].piece == null)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
    }


    /* public void Selectpiece(Piece _piece)
     {
         // Eðer seçili bir parça yoksa ve týklanan parçanýn en az 2 komþusu ayný renkteyse yok etme iþlemini baþlat
         if (_piece != null)
         {
             List<Piece> connectedPieces = GetConnectedPieces(_piece);

             if (connectedPieces.Count >= 2) // En az 2 benzer renkten blok varsa
             {
                 Debug.Log("En az 2 ayný renkten blok bulundu, yok ediliyor...");
                 DestroyPieces(connectedPieces);
             }
             else
             {
                 Debug.Log("Yeterli eþleþme bulunamadý.");
             }
         }
     }*/
    public void Selectpiece(Piece _piece)
    {
        if (_piece != null)
        {
            List<Piece> connectedPieces = GetConnectedPieces(_piece);

            if (connectedPieces.Count >= 2)
            {
                // Eþleþen parçalarýn sprite'larýný güncelle
                foreach (Piece piece in connectedPieces)
                {
                    piece.UpdateSprite(connectedPieces.Count);
                }

                // Kýsa bir gecikme ekleyelim ki yeni ikonlarý görebilelim
                StartCoroutine(DestroyPiecesAfterDelay(connectedPieces));
            }
            else
            {
                Debug.Log("Yeterli eþleþme bulunamadý.");
            }
        }
    }

    /*  private IEnumerator DestroyPiecesAfterDelay(List<Piece> pieces)
      {
          // Yeni ikonlarý görebilmek için bekle
            yield return new WaitForSeconds(0.5f);

          List<(int x, int y)> positions = new List<(int x, int y)>();
          foreach (Piece piece in pieces)
          {
              if (piece != null)
              {
                  positions.Add((piece.xIndex, piece.yIndex));
              }
          }


          // Sonra parçalarý yok et ve board'u güncelle
          foreach (var position in positions)
          {
              if (pieceBoard[position.x, position.y].piece != null)
              {
                  Destroy(pieceBoard[position.x, position.y].piece.gameObject);
                  pieceBoard[position.x, position.y] = new Node(true, null);
              }
          }

          // Boþluklarý doldur
          StartCoroutine(ProcessTurnOnMatchedBoard(false));
      }

      */


    /* private IEnumerator DestroyPiecesAfterDelay(List<Piece> pieces)
     {
         // Yeni ikonlarý görebilmek için bekle
         yield return new WaitForSeconds(0.5f);

         foreach (Piece piece in pieces)
         {
             if (piece != null)
             {
                 // ExploitPaths objesini bul
                 Transform exploitPaths = piece.transform.Find("ExploitPaths");
                 if (exploitPaths != null)
                 {
                     // ExploitPaths'i aktifleþtir ve parçadan ayýr
                     exploitPaths.gameObject.SetActive(true);
                     exploitPaths.SetParent(null); // Parçadan ayýr

                     // Patlama efektini baþlat
                     ExploitAnimation.TriggerExplosionAtGrid(exploitPaths.gameObject);
                 }
             }
         }

         // Board'u güncelle
         List<(int x, int y)> positions = new List<(int x, int y)>();
         foreach (Piece piece in pieces)
         {
             if (piece != null)
             {
                 positions.Add((piece.xIndex, piece.yIndex));
             }
         }

         foreach (var position in positions)
         {
             if (pieceBoard[position.x, position.y].piece != null)
             {
                 Destroy(pieceBoard[position.x, position.y].piece.gameObject);
                 pieceBoard[position.x, position.y] = new Node(true, null);
             }
         }

         // Boþluklarý doldur
         StartCoroutine(ProcessTurnOnMatchedBoard(false));
     }*/

    private IEnumerator DestroyPiecesAfterDelay(List<Piece> pieces)
    {
        // Yeni ikonlarý görebilmek için bekle
        yield return new WaitForSeconds(0.5f);

        List<GameObject> exploitPathsToDestroy = new List<GameObject>();

        foreach (Piece piece in pieces)
        {
            if (piece != null)
            {
                // ExploitPaths objesini bul
                Transform exploitPaths = piece.transform.Find("ExploitPaths");
                if (exploitPaths != null)
                {
                    // ExploitPaths'i listeye ekle (daha sonra yok etmek için)
                    exploitPathsToDestroy.Add(exploitPaths.gameObject);

                    // Orijinal sprite'ý gizle
                    piece.GetComponent<SpriteRenderer>().enabled = false;

                    // ExploitPaths'i aktifleþtir
                    exploitPaths.gameObject.SetActive(true);
                    // Her bir parçayý hareket ettir
                    foreach (Transform child in exploitPaths)
                    {
                      //  StartCoroutine(AnimateExploitPiece(child.gameObject));
                    }
                }
            }
        }
        yield return new WaitForSeconds(.75f);

        // ExploitPaths'leri yok et
        foreach (GameObject exploitPath in exploitPathsToDestroy)
        {
            Destroy(exploitPath);
        }
        // Board'u güncelle
        List<(int x, int y)> positions = new List<(int x, int y)>();
        foreach (Piece piece in pieces)
        {
            if (piece != null)
            {
                positions.Add((piece.xIndex, piece.yIndex));
            }
        }

        foreach (var position in positions)
        {
            if (pieceBoard[position.x, position.y].piece != null)
            {
                Destroy(pieceBoard[position.x, position.y].piece.gameObject);
                pieceBoard[position.x, position.y] = new Node(true, null);
            }
        }

        // Boþluklarý doldur
        StartCoroutine(ProcessTurnOnMatchedBoard(false));
    }

    // Ayný renkten baðlý parçalarý bulma (yeni bir fonksiyon ekle)
    public List<Piece> GetConnectedPieces(Piece piece)
    {
        //Baþlangýçta liste boþ 
        List<Piece> connectedPieces = new List<Piece>();
        PieceType pieceType = piece.pieceType;

        // Baðlantýlý parçalarý bulmak için bir Queue mekanizmasý kullan
        Queue<Piece> toCheck = new Queue<Piece>();
        toCheck.Enqueue(piece);

        while (toCheck.Count > 0)
        {
            Piece currentPiece = toCheck.Dequeue();

            if (!connectedPieces.Contains(currentPiece) && currentPiece.pieceType == pieceType)
            {
                connectedPieces.Add(currentPiece);

                // Komþularý sýraya ekle
                foreach (Piece neighbor in GetNeighbors(currentPiece))
                {
                    if (!connectedPieces.Contains(neighbor) && neighbor.pieceType == pieceType)
                    {
                        toCheck.Enqueue(neighbor);
                    }
                }
            }
        }

        return connectedPieces;
    }

    // Komþularý döndüren bir yardýmcý fonksiyon ekle
    private List<Piece> GetNeighbors(Piece piece)
    {
        List<Piece> neighbors = new List<Piece>();
        int x = piece.xIndex;
        int y = piece.yIndex;

        // Yukarý
        if (y + 1 < height && pieceBoard[x, y + 1].isUsable && pieceBoard[x, y + 1].piece != null)
            neighbors.Add(pieceBoard[x, y + 1].piece.GetComponent<Piece>());

        // Aþaðý
        if (y - 1 >= 0 && pieceBoard[x, y - 1].isUsable && pieceBoard[x, y - 1].piece != null)
            neighbors.Add(pieceBoard[x, y - 1].piece.GetComponent<Piece>());

        // Sað
        if (x + 1 < width && pieceBoard[x + 1, y].isUsable && pieceBoard[x + 1, y].piece != null)
            neighbors.Add(pieceBoard[x + 1, y].piece.GetComponent<Piece>());

        // Sol
        if (x - 1 >= 0 && pieceBoard[x - 1, y].isUsable && pieceBoard[x - 1, y].piece != null)
            neighbors.Add(pieceBoard[x - 1, y].piece.GetComponent<Piece>());

        return neighbors;
    }

    // Parçalarý yok eden bir yardýmcý fonksiyon
    private void DestroyPieces(List<Piece> pieces)
    {
        foreach (Piece piece in pieces)
        {
            int x = piece.xIndex;
            int y = piece.yIndex;

            // Parçayý yok et
            Destroy(piece.gameObject);

            // Parça tahtasýný güncelle
            pieceBoard[x, y] = new Node(true, null);
        }

        // Parçalarý yok ettikten sonra boþluklarý doldur
        StartCoroutine(ProcessTurnOnMatchedBoard(false));
    }
    private bool HasAnyPossibleMatches()
    {
        // Tüm tahtayý kontrol et
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieceBoard[x, y].piece != null)
                {
                    Piece currentPiece = pieceBoard[x, y].piece.GetComponent<Piece>();
                    List<Piece> connectedPieces = GetConnectedPieces(currentPiece);

                    // Eðer 2 veya daha fazla baðlantýlý parça varsa, eþleþme mümkün
                    if (connectedPieces.Count >= 2)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void ShuffleBoard()
    {
        List<PieceType> pieceTypes = new List<PieceType>();

        // Mevcut parçalarýn tiplerini topla
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieceBoard[x, y].piece != null)
                {
                    Piece piece = pieceBoard[x, y].piece.GetComponent<Piece>();
                    pieceTypes.Add(piece.pieceType);
                    Destroy(piece.gameObject);
                }
            }
        }

        // Parçalarý karýþtýr
        for (int i = pieceTypes.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            PieceType temp = pieceTypes[i];
            pieceTypes[i] = pieceTypes[randomIndex];
            pieceTypes[randomIndex] = temp;
        }

        // Yeni karýþtýrýlmýþ parçalarý yerleþtir
        int pieceIndex = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieceBoard[x, y].isUsable)
                {
                    Vector2 position = new Vector2(x - spacingX, y - spacingY);

                    // Parça tipine göre prefab seç
                    GameObject prefab = piecePrefabs[(int)pieceTypes[pieceIndex]];
                    GameObject newPiece = Instantiate(prefab, position, Quaternion.identity);
                    newPiece.transform.SetParent(pieceParent.transform);
                    newPiece.GetComponent<Piece>().SetIndicies(x, y);
                    pieceBoard[x, y] = new Node(true, newPiece);

                    pieceIndex++;
                }
            }
        }

        Debug.Log("Parçalar bitti tahta karýþýyor");
    }
    private IEnumerator CheckForNoMatches()
    {
        // Mevcut hamlenin ve animasyonlarýn tamamlanmasýný bekle
        yield return new WaitForSeconds(0.5f);

        // Eþleþme kontrolü
        if (!HasAnyPossibleMatches())
        {

            yield return new WaitForSeconds(5f);
            ShuffleBoard();
        }
    }
}



