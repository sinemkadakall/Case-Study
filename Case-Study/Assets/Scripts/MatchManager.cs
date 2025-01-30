using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{

   /* private Board board;
    public List<Piece> piecesToRemove = new();

    private void Awake()
    {
        board = GetComponent<Board>();
    }

    public bool CheckBoard()
    {
        Debug.Log("Checking Board");
        bool hasMatched = false;
        piecesToRemove.Clear();

        foreach (Node nodepiece in board.pieceBoard)
        {
            if (nodepiece.piece != null)
            {
                nodepiece.piece.GetComponent<Piece>().isMatched = false;
            }
        }

        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                if (board.pieceBoard[x, y].isUsable)
                {
                    Piece piece = board.pieceBoard[x, y].piece.GetComponent<Piece>();
                    if (!piece.isMatched)
                    {
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
    }

    public MatchResult IsConnected(Piece piece)
    {
        List<Piece> connectedpieces = new();
        PieceType pieceType = piece.pieceType;

        connectedpieces.Add(piece);

        CheckDirection(piece, new Vector2Int(1, 0), connectedpieces);
        CheckDirection(piece, new Vector2Int(-1, 0), connectedpieces);

        if (connectedpieces.Count == 3)
        {
            return new MatchResult { connectedpieces = connectedpieces, direction = MatchDirection.Horizontal };
        }
        else if (connectedpieces.Count > 3)
        {
            return new MatchResult { connectedpieces = connectedpieces, direction = MatchDirection.LongHorizontal };
        }

        connectedpieces.Clear();
        connectedpieces.Add(piece);

        CheckDirection(piece, new Vector2Int(0, 1), connectedpieces);
        CheckDirection(piece, new Vector2Int(0, -1), connectedpieces);

        if (connectedpieces.Count == 3)
        {
            return new MatchResult { connectedpieces = connectedpieces, direction = MatchDirection.Vertical };
        }
        else if (connectedpieces.Count > 3)
        {
            return new MatchResult { connectedpieces = connectedpieces, direction = MatchDirection.LongVertical };
        }

        return new MatchResult { connectedpieces = connectedpieces, direction = MatchDirection.None };
    }

    void CheckDirection(Piece pot, Vector2Int direction, List<Piece> connectedpieces)
    {
        PieceType pieceType = pot.pieceType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        while (x >= 0 && x < board.width && y >= 0 && y < board.height)
        {
            if (board.pieceBoard[x, y].isUsable)
            {
                Piece neighbourpiece = board.pieceBoard[x, y].piece.GetComponent<Piece>();
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

    private MatchResult SuperMatch(MatchResult matchedResults)
    {
        if (matchedResults.direction == MatchDirection.Horizontal || matchedResults.direction == MatchDirection.LongHorizontal)
        {
            foreach (Piece pot in matchedResults.connectedpieces)
            {
                List<Piece> extraConnectedpieces = new();
                CheckDirection(pot, new Vector2Int(0, 1), extraConnectedpieces);
                CheckDirection(pot, new Vector2Int(0, -1), extraConnectedpieces);

                if (extraConnectedpieces.Count >= 2)
                {
                    extraConnectedpieces.AddRange(matchedResults.connectedpieces);
                    return new MatchResult { connectedpieces = extraConnectedpieces, direction = MatchDirection.Super };
                }
            }
        }
        else if (matchedResults.direction == MatchDirection.Vertical || matchedResults.direction == MatchDirection.LongVertical)
        {
            foreach (Piece pot in matchedResults.connectedpieces)
            {
                List<Piece> extraConnectedpieces = new();
                CheckDirection(pot, new Vector2Int(1, 0), extraConnectedpieces);
                CheckDirection(pot, new Vector2Int(-1, 0), extraConnectedpieces);

                if (extraConnectedpieces.Count >= 2)
                {
                    extraConnectedpieces.AddRange(matchedResults.connectedpieces);
                    return new MatchResult { connectedpieces = extraConnectedpieces, direction = MatchDirection.Super };
                }
            }
        }
        return matchedResults;
    }*/

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
