using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntryPoint
{
    NONE,
    NorthSide,
    EastSide,
    SouthSide,
    WestSide
}

/// <summary>
/// This script is basically just a container to hold information
/// so much so that it is turned off in the editor just so the script 
/// itself isn't ran. Just holding information
/// </summary>
public class WallScript : MonoBehaviour
{
    public GameObject northBlockOff;
    public GameObject eastBlockOff;
    public GameObject southBlockOff;
    public GameObject westBlockOff;

    public bool northOpen = false;
    public bool eastOpen = false;
    public bool southOpen = false;
    public bool westOpen = false;

    public Vector2 levelPos;

    public EntryPoint curEntryPoint = EntryPoint.NONE;

    public void SetWallPieceMaterial(Material newMat)
    {
        for (int i = 2; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<Renderer>().material = newMat;
        }
    }
}
