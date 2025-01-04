using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    public bool isUsable;
    public GameObject piece;

    public Node(bool _isUsable,GameObject _piece)
    {
        isUsable = _isUsable;
        piece = _piece;

    }







}
