using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    //Defining size of the board
    public int width = 6;
    public int height = 8;

    //Defining spacing for the board
    public float spacingX;
    public float spacingY;

    //Referring potion prefabs
    public GameObject[] potionPrefabs;

    //Get a reference to the nodes of potionBoard + GameObject
    private Node[,] potionBoard;
    public GameObject potionBoardGO;

    //Layout Array
    public ArrayLayout arrayLayout;

    public static PotionBoard Instance;
    private List<GameObject> potionsToDestroy = new();
    public GameObject potionsParent;

    [SerializeField]
    private Potion selectedPotion;
    [SerializeField]
    private bool isProcessingMove;

    public PuzzleManager puzzleManager;

    public int points;

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
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if((hit.collider != null) && hit.collider.gameObject.GetComponent<Potion>())
            {
                if (isProcessingMove)
                    return;

                Potion potion = hit.collider.gameObject.GetComponent<Potion>();
                //Debug.Log("A potion clicked which:" + potion);
                SelectPotion(potion);
            }
        }
    }

    void InitializeBoard()
    {
        DestroyPotions();
        potionBoard = new Node[width, height];
        spacingX = (float)(2.6);
        spacingY = (float)(4);

        for(int y=0; y < height; y++)
        {
            for(int x=0; x < width; x++)
            {
                Vector2 position = new Vector2(x-spacingX, y-spacingY);

                if (arrayLayout.rows[y].row[x])
                {
                    potionBoard[x, y] = new Node(false, null);
                }
                else
                {
                    int randomIndex = Random.Range(0, potionPrefabs.Length);

                    GameObject potion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
                    potion.transform.SetParent(potionsParent.transform);
                    potion.GetComponent<Potion>().SetIndicies(x, y);
                    potionBoard[x, y] = new Node(true, potion);
                    potionsToDestroy.Add(potion);
                }
            }
        }

        if (CheckBoard(false))
        {
            InitializeBoard();
        }
        else
            Debug.Log("No matches starting the game!");
    }

    public bool CheckBoard(bool inTakeAction)
    {
        //Debug.Log("Checking Board");
        bool hasMatched = false;

        List<Potion> potionsToRemove = new();
        foreach(Node nodePot in potionBoard)
        {
            if(nodePot.potion != null)
                nodePot.potion.GetComponent<Potion>().matched = false;
        }

        for(int x=0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x,y].isUsable)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
                    
                    if(!potion.matched)
                    {
                        //Running the matching logic
                        MatchResult matchedPotions = IsConnected(potion);
                        if(matchedPotions.connectedPotions.Count >= 3)
                        {
                            //Complex Matching (will be added)
                            potionsToRemove.AddRange(matchedPotions.connectedPotions);

                            //Marking potions as matched
                            foreach (Potion pot in matchedPotions.connectedPotions)
                                pot.matched = true;

                            hasMatched = true;
                        }
                    }
                }
            }
        }

        if(inTakeAction)
        {
            foreach(Potion potToRemove in potionsToRemove)
                potToRemove.matched = false;

            RemoveAndRefill(potionsToRemove);

            if (CheckBoard(false)) 
                CheckBoard(true);
        }

        return hasMatched;
    }

    private void RemoveAndRefill(List<Potion> potionsToRemove)
    {
        // Removing potion and clearing the board at that location
        foreach(Potion potion in potionsToRemove)
        {
            //Getting x and y indicies for storing

            int tempxIndex = potion.xIndex;
            int tempyIndex = potion.yIndex;

            //Destroy that potion
            Destroy(potion.gameObject);

            potionBoard[tempxIndex, tempyIndex] = new Node(true, null); 
        }

        points += potionsToRemove.Count * 10;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x,y].potion == null)
                {
                    //Debug.Log("The location X:" + x + "Y:" + y + "is empty, refilling!");
                    RefillPotion(x, y);
                }
            }
        }

    }

    private void RefillPotion(int x, int y)
    {
        int yOffset = 1;

        // Cell above current one is null
        while (y+yOffset<height && potionBoard[x,y+yOffset].potion == null)
        { 
            yOffset++;
        }

        // another potion on top
        if(y + yOffset < height && potionBoard[x, y + yOffset].potion != null)
        {
            Potion potionAbove = potionBoard[x, y + yOffset].potion.GetComponent<Potion>();

            Vector3 targetPos = new Vector3(x-spacingX, y - spacingY,potionAbove.transform.position.z);

            potionAbove.MoveToTarget(targetPos);

            potionAbove.SetIndicies(x, y);
            potionBoard[x, y] = potionBoard[x, y + yOffset];
            potionBoard[x, y + yOffset] = new Node(true, null);
        }

        //hitting the top
        if (y + yOffset == height)
            SpawnPotionAtTop(x);
        
    }

    private void SpawnPotionAtTop(int x)
    {
        int index = FindIndexOfLowestNull(x);
        int locationToMove = 8 - index;

        //Get random pot to spawn
        int randIndex = Random.Range(0,potionPrefabs.Length);
        GameObject newPotion = Instantiate(potionPrefabs[randIndex],new Vector2(x-spacingX,height-spacingY),Quaternion.identity);
        newPotion.transform.SetParent(potionsParent.transform);
        //Movig pot with indicies
        newPotion.GetComponent<Potion>().SetIndicies(x, index);
        potionBoard[x,index] = new Node(true,newPotion);
        Vector3 targetPosition = new Vector3(newPotion.transform.position.x,
            newPotion.transform.position.y-locationToMove,newPotion.transform.position.z);

        newPotion.GetComponent<Potion>().MoveToTarget(targetPosition);
    }

    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;

        for(int y=(height-1); y >= 0; y--)
        {
            if (potionBoard[x, y].potion == null)
                lowestNull = y;
        }

        return lowestNull;
    }
    

    #region Cascading Potions 

    private MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new();
        PotionType potionType = potion.potionType;

        connectedPotions.Add(potion);

        //Checking righten side
        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);
        //Checking leften side
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);

        //Checking triple(H) match
        if(connectedPotions.Count == 3)
        {
            /*
            Debug.Log("Found Horizontal match! The color is:" + connectedPotions[0].potionType);
            Debug.Log("Its pos:(" + connectedPotions[0].xIndex + "," + connectedPotions[0].yIndex + ")");
            */
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };
        }
        //Checking for more than triple (Long Matches)
        else if(connectedPotions.Count > 3)
        {
            /*
            Debug.Log("Found LongHorizontal match! The color is:" + connectedPotions[0].potionType);
            Debug.Log("Its pos:(" + connectedPotions[0].xIndex + "," + connectedPotions[0].yIndex + ")");
            */

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongHorizontal
            };

        }
        //No match > clear connectedPotions list and preparing for other checks
        connectedPotions.Clear();
        connectedPotions.Add(potion);


        //Checking up side
        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);
        //Checking down side
        CheckDirection(potion, new Vector2Int(0, -1), connectedPotions);

        //Checking triple(V) match
        if (connectedPotions.Count == 3)
        {
            /*
            Debug.Log("Found Vertical match! The color is:" + connectedPotions[0].potionType);
            Debug.Log("Its pos:(" + connectedPotions[0].xIndex + "," + connectedPotions[0].yIndex + ")");
            */
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical
            };
        }
        //Checking for more than triple (Long Matches)
        else if (connectedPotions.Count > 3)
        {
            /*
            Debug.Log("Found LongVertical match! The color is:" + connectedPotions[0].potionType);
            Debug.Log("Its pos:(" + connectedPotions[0].xIndex + "," + connectedPotions[0].yIndex + ")");
            */
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongVertical
            };

        }
        else
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.None
            };
        }

    }


    //Checking by direction
    private void CheckDirection(Potion pot, Vector2Int direction, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        //Checking if pos within boundaries
        while((x >= 0 && x < width) && (y >= 0 && y < height))
        {
            if (potionBoard[x, y].isUsable)
            {
                Potion neighbour = potionBoard[x, y].potion.GetComponent<Potion>();
                if (!neighbour.matched && (neighbour.potionType == potionType))
                {
                    connectedPotions.Add(neighbour);
                    x += direction.x;
                    y += direction.y;
                }
                else
                    break;
            }
            else
                break;
        }
    }

    private void DestroyPotions()
    {
        if(potionsToDestroy != null)
        {
            foreach(GameObject potion in potionsToDestroy)
            {
                Destroy(potion);
            }
            potionsToDestroy.Clear();
        }
    }

    #endregion

    #region Swapping Potions

    // Selecting potion
    public void SelectPotion(Potion inPotion)
    {

        // No selected potion, select clicked
        if(selectedPotion == null)
        {
            selectedPotion = inPotion;
        }
        // Selected Potion, for same potion click unselect(null)
        else if(selectedPotion == inPotion)
        {
            selectedPotion = null;
        }
        // Selected Potion, for different potion click attempt swap
        else if(selectedPotion != inPotion)
        {
            SwapPotion(selectedPotion, inPotion);
            selectedPotion = null;
        }
    }

    // Potion Swap Logic
    private void SwapPotion(Potion currentPot, Potion targetPot)
    {
        // Not adjacent > do nothing
        if(!IsAdjacent(currentPot,targetPot))
        {
            return;
        }

        DoSwap(currentPot, targetPot);
        isProcessingMove = true;

        StartCoroutine(ProcessMatches(currentPot, targetPot));
    }

    // Swapping Potions
    private void DoSwap(Potion currentPot,Potion targetPot)
    {
        GameObject temp = potionBoard[currentPot.xIndex, currentPot.yIndex].potion;

        potionBoard[currentPot.xIndex, currentPot.yIndex].potion =
            potionBoard[targetPot.xIndex, targetPot.yIndex].potion;

        potionBoard[targetPot.xIndex, targetPot.yIndex].potion = temp;

        //Updating indicies
        int tempXIndex = currentPot.xIndex;
        int tempYIndex = currentPot.yIndex;

        currentPot.xIndex = targetPot.xIndex;
        currentPot.yIndex = targetPot.yIndex;

        targetPot.xIndex = tempXIndex;
        targetPot.yIndex = tempYIndex;

        currentPot.MoveToTarget(potionBoard[targetPot.xIndex, targetPot.yIndex].potion.transform.position);
        targetPot.MoveToTarget(potionBoard[currentPot.xIndex, currentPot.yIndex].potion.transform.position);
    }

    private IEnumerator ProcessMatches(Potion currentPot,Potion targetPot)
    {
        yield return new WaitForSeconds(0.4f);
        bool hasMatch = CheckBoard(true);

        //Debug.Log("hasMatch:"+hasMatch);


        if (!hasMatch)
        {
            DoSwap(targetPot, currentPot);
        }
        else
        {
            puzzleManager.ProcessTurn(points, true);
            yield return new WaitForSeconds(0.8f);
        }
        isProcessingMove = false;
    }

    // Check for adjacents
    private bool IsAdjacent(Potion currentPot, Potion targetPot)
    {
        return Mathf.Abs(currentPot.xIndex - targetPot.xIndex) +
                Mathf.Abs(currentPot.yIndex - targetPot.yIndex) == 1;
    }

    #endregion

    public void ClearAllPotions()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Check if the node is usable and contains a potion
                if (potionBoard[x, y] != null && potionBoard[x, y].potion != null)
                {
                    Destroy(potionBoard[x, y].potion);
                    potionBoard[x, y].potion = null; // Clear the logical reference
                }
            }
        }
    }
}

public class MatchResult
{
    public List<Potion> connectedPotions;
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