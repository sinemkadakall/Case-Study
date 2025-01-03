using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPieceConfig
{
    int Index { get; set; }
    string Name { get; set; }
    PieceTypes Type { get; set; }
    GameObject GameObject { get; set; }
}
