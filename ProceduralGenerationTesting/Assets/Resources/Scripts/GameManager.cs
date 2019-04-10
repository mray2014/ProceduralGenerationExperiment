using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class GameManager : MonoBehaviour
{
    public int generationLevel = 5;         // How far will the maze generator will go out from the center wall piece
    public int maxGoalGenLevel = 4;         // The max gen level a goal wall piece can appear on
    public int minGoalGenLevel = 4;         // The min gen level a goal wall piece can appear on      
    public float rayCastDistance = 10;      // Declaring this variable so I don't have to change the samme value 4 different times
    public GameObject wallParent;           // The parent object that will parent all the wall pieces
    public Material startMat;
    public Material goalMat;

    // These are just little control checks to see how exculding specific gates will effect the generation
    public bool excludeNorthDir = false;
    public bool excludeEastDir = false;
    public bool excludeSouthDir = false;
    public bool excludeWestDir = false;

    // List of wall piece prefabs that are possible to attach to specific gates
    public List<GameObject> possibleNorthWallAttachments;
    public List<GameObject> possibleEastWallAttachments;
    public List<GameObject> possibleSouthWallAttachments;
    public List<GameObject> possibleWestWallAttachments;
    //[HideInInspector]
    public bool generationFinished = false;

    Stack<GameObject> mazeTrace;

    GameObject startingPiece;
    GameObject goalPiece;
    WallScript prevWallPiece;

    int goalGenLevel;

    // Start is called before the first frame update
    void Start()
    {
        mazeTrace = new Stack<GameObject>();

        // Randomly choosing which wall piece to start off with
        int ranNum = Random.Range(0,400) % 4;

        switch (ranNum)
        {
            case 0:
                {
                    int ranWall = Random.Range(0, possibleNorthWallAttachments.Count);
                    startingPiece = Instantiate(possibleNorthWallAttachments[ranWall], wallParent.transform);
                    break;
                }
            case 1:
                {
                    int ranWall = Random.Range(0, possibleEastWallAttachments.Count);
                    startingPiece = Instantiate(possibleEastWallAttachments[ranWall], wallParent.transform);
                    break;
                }
            case 2:
                {
                    int ranWall = Random.Range(0, possibleSouthWallAttachments.Count);
                    startingPiece = Instantiate(possibleSouthWallAttachments[ranWall], wallParent.transform);
                    break;
                }
            case 3:
                {
                    int ranWall = Random.Range(0, possibleWestWallAttachments.Count);
                    startingPiece = Instantiate(possibleWestWallAttachments[ranWall], wallParent.transform);
                    break;
                }
        }

        // Randomly chose what generation level the goal will appear on
        goalGenLevel = Random.Range(minGoalGenLevel, maxGoalGenLevel);

        // Setting starting wall piece information and placing in the stack to start off the generation
        startingPiece.GetComponent<WallScript>().SetWallPieceMaterial(startMat);
        startingPiece.GetComponent<WallScript>().levelPos = new Vector2(0,0);
        mazeTrace.Push(startingPiece);

        prevWallPiece = startingPiece.GetComponent<WallScript>();
    }

    // Update is called once per frame
    void Update()
    {
        // Keep generating wall pieces until the stack has no more
        // Orginally had this in a while loop before we hit the update method
        // I ended up doing it this way becuase it was easier to debug and set through
        // exactly what was happening while generating instead of it being all generated at once
        // Also I think doing it this way os better because I use raycasting in the generation
        // So I'm assuming I need them to be physically in the world first for them to work
        if (mazeTrace.Count > 0)
        {
            GenerateMaze();
        }
        else if (!generationFinished)
        {
            generationFinished = true;

            // If we haven't hit a goal piece during generation set the goal piece to the last 
            // farthest wall piece tracked during generation
            if (goalPiece == null)
            {
                prevWallPiece.SetWallPieceMaterial(goalMat);
                goalPiece = prevWallPiece.gameObject;
            }
        }
    }

    void GenerateMaze()
    {
        // Grabbing a wall from the stack
        WallScript wall = mazeTrace.Pop().GetComponent<WallScript>();

        // If we haven't generated a goal wall piece to reach
        if (goalPiece == null)
        {
            // Setting the goal wall piece to the first wall piece to reach the goal gen level on its x or y
            if (wall.levelPos.x == goalGenLevel || wall.levelPos.y == goalGenLevel)
            {
                wall.SetWallPieceMaterial(goalMat);
                goalPiece = wall.gameObject;
            }

            // Also keeping track of the last farthest wall piece if no goal piece is determined
            if (Mathf.Abs(wall.levelPos.x) > Mathf.Abs(prevWallPiece.levelPos.x) || Mathf.Abs(wall.levelPos.y) > Mathf.Abs(prevWallPiece.levelPos.y))
            {
                prevWallPiece = wall;
            }
        }

        // Checking if the north side of the wall is open and if we didn't place a wall piece in this direction aleady
        if (wall.northOpen && (wall.curEntryPoint != EntryPoint.NorthSide))
        {
            //Calculating the new wall piece generation level
            Vector2 newLevelPos = wall.levelPos + new Vector2(0, 1);

            // Checking if there isn't a wall piece already placed in this direction that the wall doesn't know about
            Ray newRay = new Ray(wall.transform.position, new Vector3(0, 0, 1));
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(newRay, out hitInfo, rayCastDistance);

            // If we hit something we need to check what it was
            if (hit)
            {
                WallScript wallHit = hitInfo.collider.gameObject.GetComponent<WallScript>();

                // Making sure we don't have to close the gate on this particular border if that wall piece has an open gate there
                if ((wallHit == null) || !wallHit.southOpen)
                {
                    wall.northBlockOff.SetActive(true);
                }
            }
            // Checking if the wall piece about to be place is under the generation level limit
            else if (Mathf.Abs(newLevelPos.x) <= generationLevel && Mathf.Abs(newLevelPos.y) <= generationLevel && !excludeNorthDir)
            {
                // We pick a random wall that can connect to this side of the wall piece
                int ranWall = Random.Range(0, possibleNorthWallAttachments.Count);
                // Instatiating the new wall piece into the world
                GameObject newWall = Instantiate(possibleNorthWallAttachments[ranWall], wallParent.transform);
                // Moving the wall piece to the correct position
                newWall.transform.position = wall.transform.position + new Vector3(0, 0, 3);
                // Setting the wall piece new generation level
                newWall.GetComponent<WallScript>().levelPos = newLevelPos;
                // Setting where our entry point was so we don't generate a new wall or close it off by accident
                newWall.GetComponent<WallScript>().curEntryPoint = EntryPoint.SouthSide;

                // Pushing the wall onto the stack to generate it's next wall piece connections
                mazeTrace.Push(newWall);
            }
            else
            {
                // If we can't place a wall piece here, then we close it off
                wall.northBlockOff.SetActive(true);
            }

        }

        // The next 3 if statements are basically the exact same copies of the previous if statement
        // Just in a new direction

        // Checking if the east side of the wall
        if (wall.eastOpen && (wall.curEntryPoint != EntryPoint.EastSide))
        {
            Vector2 newLevelPos = wall.levelPos + new Vector2(1, 0);

            Ray newRay = new Ray(wall.transform.position, new Vector3(1, 0, 0));
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(newRay, out hitInfo, rayCastDistance);

            if (hit)
            {
                WallScript wallHit = hitInfo.collider.gameObject.GetComponent<WallScript>();

                if ((wallHit == null) || !wallHit.westOpen)
                {
                    wall.eastBlockOff.SetActive(true);
                }
            }
            else if (Mathf.Abs(newLevelPos.x) <= generationLevel && Mathf.Abs(newLevelPos.y) <= generationLevel && !excludeEastDir)
            {
                int ranWall = Random.Range(0, possibleEastWallAttachments.Count);
                GameObject newWall = Instantiate(possibleEastWallAttachments[ranWall], wallParent.transform);
                newWall.transform.position = wall.transform.position + new Vector3(3, 0, 0);
                newWall.GetComponent<WallScript>().levelPos = newLevelPos;
                newWall.GetComponent<WallScript>().curEntryPoint = EntryPoint.WestSide;

                mazeTrace.Push(newWall);
            }
            else
            {
                wall.eastBlockOff.SetActive(true);
            }

        }
        // Checking if the south side of the wall
        if (wall.southOpen && (wall.curEntryPoint != EntryPoint.SouthSide))
        {
            Vector2 newLevelPos = wall.levelPos + new Vector2(0, -1);

            Ray newRay = new Ray(wall.transform.position, new Vector3(0, 0, -1));
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(newRay, out hitInfo, rayCastDistance);

            if (hit)
            {
                WallScript wallHit = hitInfo.collider.gameObject.GetComponent<WallScript>();

                if ((wallHit == null) || !wallHit.northOpen)
                {
                    wall.southBlockOff.SetActive(true);
                }
            }
            else if (Mathf.Abs(newLevelPos.x) <= generationLevel && Mathf.Abs(newLevelPos.y) <= generationLevel && !excludeSouthDir)
            {
                int ranWall = Random.Range(0, possibleSouthWallAttachments.Count);
                GameObject newWall = Instantiate(possibleSouthWallAttachments[ranWall], wallParent.transform);
                newWall.transform.position = wall.transform.position + new Vector3(0, 0, -3);
                newWall.GetComponent<WallScript>().levelPos = newLevelPos;
                newWall.GetComponent<WallScript>().curEntryPoint = EntryPoint.NorthSide;

                mazeTrace.Push(newWall);
            }
            else
            {
                wall.southBlockOff.SetActive(true);
            }

        }
        // Checking if the west side of the wall
        if (wall.westOpen && (wall.curEntryPoint != EntryPoint.WestSide))
        {
            Vector2 newLevelPos = wall.levelPos + new Vector2(-1, 0);

            Ray newRay = new Ray(wall.transform.position, new Vector3(-1, 0, 0));
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(newRay, out hitInfo, rayCastDistance);

            if (hit)
            {
                 WallScript wallHit =  hitInfo.collider.gameObject.GetComponent<WallScript>();

                if ((wallHit == null) || !wallHit.eastOpen)
                {
                    wall.westBlockOff.SetActive(true);
                }
            }
            else if (Mathf.Abs(newLevelPos.x) <= generationLevel && Mathf.Abs(newLevelPos.y) <= generationLevel && !excludeWestDir)
            {
                int ranWall = Random.Range(0, possibleWestWallAttachments.Count);
                GameObject newWall = Instantiate(possibleWestWallAttachments[ranWall], wallParent.transform);
                newWall.transform.position = wall.transform.position + new Vector3(-3f, 0, 0);
                newWall.GetComponent<WallScript>().levelPos = newLevelPos;
                newWall.GetComponent<WallScript>().curEntryPoint = EntryPoint.EastSide;

                mazeTrace.Push(newWall);
            }
            else
            {
                wall.westBlockOff.SetActive(true);
            }

        }
    }
}
