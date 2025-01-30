using UnityEngine;
using System.Collections;

[System.Serializable]
public class ArrayLayout
{
    [System.Serializable]

    public struct rowData
    {
        public bool[] row;
    }

    //public rowData[] rows=new rowData[8];

    public rowData[] rows;
    public void InitializeRows(int width, int height)
    {
        rows = new rowData[height];
        for (int i = 0; i < height; i++)
        {
            rows[i].row = new bool[width];
        }
    }
}