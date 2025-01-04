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
            }
        }
    }

    void InitializeBoard()
    {
        Destroypieces();
        pieceBoard = new Node[width, height];

        spacingX = (float)(width - 1) / 2;
        spacingY = (float)((height - 1) / 2) + 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new Vector2(x - spacingX, y - spacingY);
                //
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
                    piecesToDestroy.Add(piece);
                }
            }
        }

        /*if (CheckBoard())
        {
            Debug.Log("We have matches let's re-create the board");
            InitializeBoard();
        }
        else
        {
            Debug.Log("There are no matches, it's time to start the game!");
        }*/
    }

    private void Destroypieces()
    {
        if (piecesToDestroy != null)
        {
            foreach (GameObject piece in piecesToDestroy)
            {
                Destroy(piece);
            }
            piecesToDestroy.Clear();
        }
    }

    /*public bool CheckBoard()
    {
      //  if (GameManager.Instance.isGameEnded)
         //   return false;
        Debug.Log("Checking Board");
        bool hasMatched = false;

        piecesToRemove.Clear();

        foreach (Node nodepiece in pieceBoard)
        {
            if (nodepiece.piece != null)
            {
                nodepiece.piece.GetComponent<Piece>().isMatched = false;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //checking if piece node is usable
                if (pieceBoard[x, y].isUsable)
                {
                    //then proceed to get piece class in node.
                    Piece piece = pieceBoard[x, y].piece.GetComponent<Piece>();

                    //ensure its not matched
                    if (!piece.isMatched)
                    {
                        //run some matching logic

                        MatchResult matchedpieces = IsConnected(piece);

                        if (matchedpieces.connectedpieces.Count >= 3)
                        {
                            MatchResult superMatchedpieces = SuperMatch(matchedpieces);

                            piecesToRemove.AddRange(superMatchedpieces.connectedpieces);

                            foreach (Piece pot in superMatchedpieces.connectedpieces)
                                pot.isMatched = true;

                            hasMatched = true;
                        }
                    }
                }
            }
        }

        return hasMatched;
    }*/

    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
        foreach (Piece pieceToRemove in piecesToRemove)
        {
            pieceToRemove.isMatched = false;
        }

        RemoveAndRefill(piecesToRemove);
       // GameManager.Instance.ProcessTurn(piecesToRemove.Count, _subtractMoves);
        yield return new WaitForSeconds(0.4f);

       /* if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }*/
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
                    Debug.Log("The location X: " + x + " Y: " + y + " is empty, attempting to refill it.");
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
            Debug.Log("The piece above me is null, but i'm not at the top of the board yet, so add to my yOffset and try again. Current Offset is: " + yOffset + " I'm about to add 1.");
            yOffset++;
        }

        //we've either hit the top of the board or we found a piece

        if (y + yOffset < height && pieceBoard[x, y + yOffset].piece != null)
        {
            //we've found a piece

            Piece pieceAbove = pieceBoard[x, y + yOffset].piece.GetComponent<Piece>();

            //Move it to the correct location
            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, pieceAbove.transform.position.z);
            Debug.Log("I've found a piece when refilling the board and it was in the location: [" + x + "," + (y + yOffset) + "] we have moved it to the location: [" + x + "," + y + "]");
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
        for (int y = 7; y >= 0; y--)
        {
            if (pieceBoard[x, y].piece == null)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
    }




    #region Cascading pieces


    //


    #endregion

    #region MatchingLogic
    private MatchResult SuperMatch(MatchResult _matchedResults)
    {
        //if we have a horizontal or long horizontal match
        if (_matchedResults.direction == MatchDirection.Horizontal || _matchedResults.direction == MatchDirection.LongHorizontal)
        {
            //for each piece...
            foreach (Piece pot in _matchedResults.connectedpieces)
            {
                List<Piece> extraConnectedpieces = new();
                //check up
                CheckDirection(pot, new Vector2Int(0, 1), extraConnectedpieces);
                //check down
                CheckDirection(pot, new Vector2Int(0, -1), extraConnectedpieces);

                //do we have 2 or more pieces that have been matched against this current piece.
                if (extraConnectedpieces.Count >= 2)
                {
                    Debug.Log("I have a super Horizontal Match");
                    extraConnectedpieces.AddRange(_matchedResults.connectedpieces);

                    //return our super match
                    return new MatchResult
                    {
                        connectedpieces = extraConnectedpieces,
                        direction = MatchDirection.Super
                    };
                }
            }
            //we didn't have a super match, so return our normal match
            return new MatchResult
            {
                connectedpieces = _matchedResults.connectedpieces,
                direction = _matchedResults.direction
            };
        }
        else if (_matchedResults.direction == MatchDirection.Vertical || _matchedResults.direction == MatchDirection.LongVertical)
        {
            //for each piece...
            foreach (Piece pot in _matchedResults.connectedpieces)
            {
                List<Piece> extraConnectedpieces = new();
                //check right
                CheckDirection(pot, new Vector2Int(1, 0), extraConnectedpieces);
                //check left
                CheckDirection(pot, new Vector2Int(-1, 0), extraConnectedpieces);

                //do we have 2 or more pieces that have been matched against this current piece.
                if (extraConnectedpieces.Count >= 2)
                {
                    Debug.Log("I have a super Vertical Match");
                    extraConnectedpieces.AddRange(_matchedResults.connectedpieces);
                    //return our super match
                    return new MatchResult
                    {
                        connectedpieces = extraConnectedpieces,
                        direction = MatchDirection.Super
                    };
                }
            }
            //we didn't have a super match, so return our normal match
            return new MatchResult
            {
                connectedpieces = _matchedResults.connectedpieces,
                direction = _matchedResults.direction
            };
        }
        //this shouldn't be possible, but a null return is required so the method is valid.
        return null;
    }

    MatchResult IsConnected(Piece piece)
    {
        List<Piece> connectedpieces = new();
        PieceType pieceType = piece.pieceType;

        connectedpieces.Add(piece);

        //check right
        CheckDirection(piece, new Vector2Int(1, 0), connectedpieces);
        //check left
        CheckDirection(piece, new Vector2Int(-1, 0), connectedpieces);
        //have we made a 3 match? (Horizontal Match)
        if (connectedpieces.Count == 3)
        {
            Debug.Log("I have a normal horizontal match, the color of my match is: " + connectedpieces[0].pieceType);

            return new MatchResult
            {
                connectedpieces = connectedpieces,
                direction = MatchDirection.Horizontal
            };
        }
        //checking for more than 3 (Long horizontal Match)
        else if (connectedpieces.Count > 3)
        {
            Debug.Log("I have a Long horizontal match, the color of my match is: " + connectedpieces[0].pieceType);

            return new MatchResult
            {
                connectedpieces = connectedpieces,
                direction = MatchDirection.LongHorizontal
            };
        }
        //clear out the connectedpieces
        connectedpieces.Clear();
        //readd our initial piece
        connectedpieces.Add(piece);

        //check up
        CheckDirection(piece, new Vector2Int(0, 1), connectedpieces);
        //check down
        CheckDirection(piece, new Vector2Int(0, -1), connectedpieces);

        //have we made a 3 match? (Vertical Match)
        if (connectedpieces.Count == 3)
        {
            Debug.Log("I have a normal vertical match, the color of my match is: " + connectedpieces[0].pieceType);

            return new MatchResult
            {
                connectedpieces = connectedpieces,
                direction = MatchDirection.Vertical
            };
        }
        //checking for more than 3 (Long Vertical Match)
        else if (connectedpieces.Count > 3)
        {
            Debug.Log("I have a Long vertical match, the color of my match is: " + connectedpieces[0].pieceType);

            return new MatchResult
            {
                connectedpieces = connectedpieces,
                direction = MatchDirection.LongVertical
            };
        }
        else
        {
            return new MatchResult
            {
                connectedpieces = connectedpieces,
                direction = MatchDirection.None
            };
        }
    }

    void CheckDirection(Piece pot, Vector2Int direction, List<Piece> connectedpieces)
    {
        PieceType pieceType = pot.pieceType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        //check that we're within the boundaries of the board
        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (pieceBoard[x, y].isUsable)
            {
                Piece neighbourpiece = pieceBoard[x, y].piece.GetComponent<Piece>();

                //does our pieceType Match? it must also not be matched
                if (!neighbourpiece.isMatched && neighbourpiece.pieceType == pieceType)
                {
                    connectedpieces.Add(neighbourpiece);

                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }

            }
            else
            {
                break;
            }
        }
    }
    #endregion

    #region Swapping pieces

    //select piece
    /* public void Selectpiece(Piece _piece)
     {
         // if we don't have a piece currently selected, then set the piece i just clicked to my selectedpiece
         if (selectedpiece == null)
         {
             Debug.Log(_piece);
             selectedpiece = _piece;
         }
         // if we select the same piece twice, then let's make selectedpiece null
         else if (selectedpiece == _piece)
         {
             selectedpiece = null;
         }
         //if selectedpiece is not null and is not the current piece, attempt a swap
         //selectedpiece back to null
         else if (selectedpiece != _piece)
         {
             Swappiece(selectedpiece, _piece);
             selectedpiece = null;
         }
     }*/
    public void Selectpiece(Piece _piece)
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
    }
    // Ayný renkten baðlý parçalarý bulma (yeni bir fonksiyon ekle)
    private List<Piece> GetConnectedPieces(Piece piece)
    {
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
        if (y + 1 < height && pieceBoard[x, y + 1].isUsable)
            neighbors.Add(pieceBoard[x, y + 1].piece.GetComponent<Piece>());

        // Aþaðý
        if (y - 1 >= 0 && pieceBoard[x, y - 1].isUsable)
            neighbors.Add(pieceBoard[x, y - 1].piece.GetComponent<Piece>());

        // Sað
        if (x + 1 < width && pieceBoard[x + 1, y].isUsable)
            neighbors.Add(pieceBoard[x + 1, y].piece.GetComponent<Piece>());

        // Sol
        if (x - 1 >= 0 && pieceBoard[x - 1, y].isUsable)
            neighbors.Add(pieceBoard[x - 1, y].piece.GetComponent<Piece>());

        return neighbors;
    }

    // Parçalarý yok eden bir yardýmcý fonksiyon ekle
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
    //swap piece - logic
  /*  private void Swappiece(Piece _currentpiece, Piece _targetpiece)
    {
        if (!IsAdjacent(_currentpiece, _targetpiece))
        {
            return;
        }

        DoSwap(_currentpiece, _targetpiece);

        isProcessingMove = true;

        StartCoroutine(ProcessMatches(_currentpiece, _targetpiece));
    }*/
    //do swap
  /*  private void DoSwap(Piece _currentpiece, Piece _targetpiece)
    {
        GameObject temp = pieceBoard[_currentpiece.xIndex, _currentpiece.yIndex].piece;

        pieceBoard[_currentpiece.xIndex, _currentpiece.yIndex].piece = pieceBoard[_targetpiece.xIndex, _targetpiece.yIndex].piece;
        pieceBoard[_targetpiece.xIndex, _targetpiece.yIndex].piece = temp;

        //update indicies.
        int tempXIndex = _currentpiece.xIndex;
        int tempYIndex = _currentpiece.yIndex;
        _currentpiece.xIndex = _targetpiece.xIndex;
        _currentpiece.yIndex = _targetpiece.yIndex;
        _targetpiece.xIndex = tempXIndex;
        _targetpiece.yIndex = tempYIndex;

        _currentpiece.MoveToTarget(pieceBoard[_targetpiece.xIndex, _targetpiece.yIndex].piece.transform.position);

        _targetpiece.MoveToTarget(pieceBoard[_currentpiece.xIndex, _currentpiece.yIndex].piece.transform.position);
    }*/

  /*  private IEnumerator ProcessMatches(Piece _currentpiece, Piece _targetpiece)
    {
        yield return new WaitForSeconds(0.2f);

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
        else
        {
            DoSwap(_currentpiece, _targetpiece);
        }
        isProcessingMove = false;
    }*/


    //IsAdjacent
   /* private bool IsAdjacent(Piece _currentpiece, Piece _targetpiece)
    {
        return Mathf.Abs(_currentpiece.xIndex - _targetpiece.xIndex) + Mathf.Abs(_currentpiece.yIndex - _targetpiece.yIndex) == 1;
    }*/

    //ProcessMatches

    #endregion

}


    public class MatchResult
    {
        public List<Piece> connectedpieces; 

        public MatchDirection direction;
    }

    public enum MatchDirection
    {
        Vertical,
        Horizontal,
        LongVertical,
        LongHorizontal,
        Super,
        None
    }


