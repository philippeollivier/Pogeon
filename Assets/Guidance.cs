using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    empty, explored, wall, objective, player, parent
}

public class Guidance : MonoBehaviour
{
    public Queue<Vector3> guideQueue = new Queue<Vector3>();

    //must be odd numbers
    public int ROW = 10, COL = 10, SLICE = 10;

    public float edgeLength = 1.1f;
    public GameObject cube;
    public bool foundObjective = false;
    Vector3 startPos = Vector3.zero;

    //Grid is made up of cells
    public Vector3 gridCenter;
    public Vector3 gridCenterU;
    public cell objective = new cell();
    public cell[,,] grid;
    public List<cell> freeCells = new List<cell>();

    public struct cell
    {
        public Vector3 gridPos;
        public Vector3 pos;
        public State state;
        public List<cell> neighbors;
        public Vector3 parent;
        public float distToSeed;

        public cell(Vector3 inGridPos, Vector3 inPos)
        {
            gridPos = inGridPos;
            pos = inPos;
            state = State.empty;
            neighbors = new List<cell>();
            parent = Vector3.zero;
            distToSeed = 0f;
        }

        public void generateList(int ROW, int COL, int SLICE, ref cell[,,] grid)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if (InRange(gridPos - new Vector3(i, j, k), ROW, COL, SLICE) && new Vector3(i, j, k) != Vector3.zero) // if grid pos is within range of grid and also not zero
                        {
                            //Add the cell at that location to the list
                            Vector3 v = gridPos - new Vector3(i, j, k);

                            neighbors.Add(grid[(int)v.x, (int)v.y, (int)v.z]);
                        }
                    }
                }
            }
        }

        bool InRange(Vector3 inVec, int x, int y, int z)
        {
            if (inVec.x <= x-1 && inVec.x >= 0
                && inVec.y <= y-1 && inVec.y >= 0
                && inVec.z <= z-1 && inVec.z >= 0)
            {
                return true;
            }

            return false;
        }
    }

    public void GenerateGrid()
    {
        //IF ROW COL OR SLICE ARE EVEN, SET MIDDLE TO INT HALF
        //ELSE SET MIDDLE TO HALF+1
        Vector3 offset;
        offset.x = (ROW % 2 == 0) ? (ROW / 2) : ((ROW + 1) / 2 - 1);
        offset.y = (COL % 2 == 0) ? (COL / 2) : ((COL + 1) / 2 - 1);
        offset.z = (SLICE % 2 == 0) ? (SLICE / 2) : ((SLICE + 1) / 2 - 1);

        gridCenterU = offset;

        offset *= edgeLength;
        
        grid = new cell[ROW, COL, SLICE];

        for (int i = 0; i < ROW; i++)
        {
            for (int j = 0; j < COL; j++)
            {
                for (int k = 0; k < SLICE; k++)
                {
                    grid[i, j, k].gridPos = new Vector3(i, j, k);
                    grid[i, j, k].pos = new Vector3(i, j, k) * edgeLength - offset;
                    grid[i, j, k].neighbors = new List<cell>();
                }
            }
        }
        for (int i = 0; i < ROW; i++)
        {
            for (int j = 0; j < COL; j++)
            {
                for (int k = 0; k < SLICE; k++)
                {
                    grid[i, j, k].generateList(ROW, COL, SLICE, ref grid);
                }
            }
        }


        Vector3 gridCenter = Vector3.zero;
    }

    public void CenterGrid(Vector3 position)
    {
        Vector3 relativePos = gridCenter - position;

        for (int i = 0; i < ROW; i++)
        {
            for (int j = 0; j < COL; j++)
            {
                for (int k = 0; k < SLICE; k++)
                {
                    grid[i, j, k].pos -= relativePos;
                }
            }
        }
        gridCenter = position;
    }

    
    public void massColliderCheck(ref cell[,,] grid)
    {
        for (int i = 0; i < ROW; i++){
            for (int j = 0; j < COL; j++){
                for (int k = 0; k < SLICE; k++){
                    //if (Physics.BoxCast(grid[i, j, k].pos, edgeLength / 2f * Vector3.one, Vector3.forward, Quaternion.Euler(0, 0, 0), edgeLength/2f) && grid[i,j,k].state != State.objective)
                    Collider[] c = Physics.OverlapBox(grid[i, j, k].pos, Vector3.one * edgeLength / 2f, Quaternion.Euler(0, 0, 0));
                    if (c.Length != 0 && grid[i, j, k].state != State.objective)
                    {
                        bool temp = false;
                        //if any of the colliders are not tagged: player, floor, food then it is a wall
                        foreach(Collider s in c)
                        {
                            if(s.gameObject.tag == "Floor" || s.gameObject.tag == "Food"|| s.gameObject == gameObject)
                            {
                                temp = true;
                            }
                        }
                        if(temp)
                        {
                            grid[i, j, k].state = State.empty;
                            freeCells.Add(grid[i, j, k]);
                        }
                        else
                        {
                            grid[i, j, k].state = State.wall;
                        }
                    }
                    else if (grid[i, j, k].state != State.objective)
                    {
                        grid[i, j, k].state = State.empty;
                        freeCells.Add(grid[i, j, k]);
                    }
                }
            }
        }
    }


}












//BRAIN SCRIPT



/* 
 * 
 * 
 * 
 *     public void DrawGrid()
    {
        for(int i = 0; i < ROW; i++)
        {
            for (int j = 0; j < COL; j++)
            {
                for (int k = 0; k < SLICE; k++)
                {
                    if(grid[i,j,k].state == State.wall)
                    {
                        DrawCube(grid[i, j, k].pos, edgeLength, Color.red);
                    }
                    else
                    {
                        DrawCube(grid[i, j, k].pos, edgeLength, Color.white);
                    }
                }
            }
        }
    }
 * 
 * 
 * public void FindObjective(Vector3 target)
    {
        //2 cases target is in range or target is out of range, first check whether in range
        //Check x, y, and z
        Vector3 targetPos = target;
        Vector3 relativePos = target - gridCenter;

        Vector3 xyzMax;
        Vector3 xyzMin;
        
        xyzMax.x = (ROW % 2 == 0) ? (ROW / 2 - 0.5f) : (ROW / 2 + 0.5f);
        xyzMax.y = (COL % 2 == 0) ? (COL / 2 - 0.5f) : (COL / 2 + 0.5f);
        xyzMax.z = (SLICE % 2 == 0) ? (SLICE / 2 - 0.5f) : (SLICE / 2 + 0.5f);
        xyzMax *= edgeLength;
        xyzMax += gridCenter;

        xyzMin = new Vector3((ROW / 2 + 0.5f) * edgeLength, (COL / 2 + 0.5f) * edgeLength, (SLICE / 2 + 0.5f) * edgeLength);
        xyzMin = gridCenter - xyzMin;

        if (targetPos.x <= xyzMax.x && targetPos.x >= xyzMin.x &&
            targetPos.y <= xyzMax.y && targetPos.y >= xyzMin.y &&
            targetPos.z <= xyzMax.z && targetPos.z >= xyzMin.z)
        {
            //Draw a blue cube at its location.
            //find closest point to it by snappings its position values to the voxel grid value.
            //snap x, y, z, this is relative to the grid position of the center
            Vector3 gridPos;

            gridPos.x = (Mathf.Abs(relativePos.x % edgeLength) > 0.5 * edgeLength)?((int)(relativePos.x /edgeLength) + Mathf.Sign(relativePos.x)):((int)((targetPos.x - gridCenter.x) / edgeLength));
            gridPos.y = (Mathf.Abs(relativePos.y % edgeLength) > 0.5 * edgeLength) ? ((int)(relativePos.y / edgeLength) + Mathf.Sign(relativePos.y)) : ((int)((targetPos.y - gridCenter.y) / edgeLength));
            gridPos.z = (Mathf.Abs(relativePos.z % edgeLength) > 0.5 * edgeLength) ? ((int)(relativePos.z / edgeLength) + Mathf.Sign(relativePos.z)) : ((int)((targetPos.z - gridCenter.z) / edgeLength));

            //Grid 
            cell objectiveCell = grid[(int)(gridCenterU + gridPos).x, (int)(gridCenterU + gridPos).y, (int)(gridCenterU + gridPos).z];

            DrawCube(objectiveCell.pos, 1f, Color.green);

            objective = objectiveCell;
            objectiveCell.state = State.objective;
            foundObjective = true;
        }
        else
        {
            foundObjective = false;
        }
    }
 * public void DrawCube(Vector3 position, float size, Color color)
    {
        //1
        Vector3 start = position - new Vector3(size / 2, size / 2, size / 2);
        Debug.DrawLine(start, start + Vector3.right * size, color);
        Debug.DrawLine(start, start + Vector3.up * size, color);
        Debug.DrawLine(start, start + Vector3.forward * size, color);
        //2
        start = position + new Vector3(size / 2, -size / 2, size / 2);
        Debug.DrawLine(start, start + Vector3.left * size, color);
        Debug.DrawLine(start, start + Vector3.up * size, color);
        Debug.DrawLine(start, start + Vector3.back * size, color);
        //3
        start = position + new Vector3(-size / 2, size / 2, size / 2);
        Debug.DrawLine(start, start + Vector3.down * size, color);
        Debug.DrawLine(start, start + Vector3.back * size, color);
        Debug.DrawLine(start, start + Vector3.right * size, color);
        //4
        start = position + new Vector3(size / 2, size / 2, -size / 2);
        Debug.DrawLine(start, start + Vector3.down * size, color);
        Debug.DrawLine(start, start + Vector3.left * size, color);
        Debug.DrawLine(start, start + Vector3.forward * size, color);
    }
    
   
    
    public Vector3 BestDirection(Vector3 path, ref List<cell> cells, ref cell currCell)
    {
        //path.Normalize();
        //each cell has a position that is some variation of +-1 x,y,z
        cell tempDir = new cell();
        float tempMag = 10f;
        if(cells.Count > 0)
        {
            foreach (cell c in cells)
            {
                if ((path - (c.pos - currCell.pos)).magnitude < tempMag)
                {
                    tempDir = c;
                    tempMag = (path - (c.pos - currCell.pos)).magnitude;
                }
            }
        }
        else
        {
            //ERROR code here
        }
        
        return tempDir.gridPos;
    }
 *     public void DrawQueue(Queue<Vector3> v)
    {
        Vector3 start = transform.position;
        Vector3 end;
        if(v.Count > 0)
        {
            for (int i = 0; i < v.Count; i++)
            {
                end = v.Peek();
                Debug.DrawLine(start, end);
                start = v.Dequeue();
            }
        }

    }

    void Start()
    {
        
    }

    void Update()
    {
        DrawQueue(guideQueue);
        if(cube != null)
        {
            DrawGrid();
            

            massColliderCheck(ref grid);
            FindObjective(cube.transform.position);
            findOptimalPath(ref grid[(int)(gridCenterU).x, (int)(gridCenterU).y, (int)(gridCenterU).z]);
        }
        else
        {
            
            CenterGrid(transform.position);
            guideQueue = new Queue<Vector3>();
        }
    }
 *  public void findOptimalPath(ref cell startCell)
    {
        //if path is too long then break the function
        if (guideQueue.Count > 20)
        {
            return;
        }

        if(startCell.neighbors.Count == 0)
        {
            return;
        }

        Vector3 t = BestDirection(objective.pos - startCell.pos, ref startCell.neighbors, ref startCell);
        cell direction = grid[(int)t.x,(int)t.y,(int)t.z];
        
        if(grid[(int)t.x, (int)t.y, (int)t.z].gridPos == objective.gridPos)
        {
            guideQueue.Enqueue(objective.gridPos);
            Debug.DrawLine(startCell.pos, grid[(int)t.x, (int)t.y, (int)t.z].pos, Color.cyan, 1f);
            Debug.Log("Found!");
            return;
        }
        else
        {
            //check if it is blocked at direction,
            if(grid[(int)t.x, (int)t.y, (int)t.z].state == State.wall || grid[(int)t.x, (int)t.y, (int)t.z].state == State.parent)
            {

                for(int i = 0; i < startCell.neighbors.Count; i++)
                {
                    if(startCell.neighbors[i].gridPos == grid[(int)t.x, (int)t.y, (int)t.z].gridPos)
                    {
                        startCell.neighbors.RemoveAt(i);
                        i = 50;
                    }
                }

                findOptimalPath(ref startCell);
            }
            //else if (startCell.distToSeed + (grid[(int)t.x, (int)t.y, (int)t.z].pos - startCell.pos).magnitude > grid[(int)t.x, (int)t.y, (int)t.z].distToSeed && grid[(int)t.x, (int)t.y, (int)t.z].distToSeed != 0f) //less optimal than what it currently has
            //{
            //    startCell.neighbors.Remove(grid[(int)t.x, (int)t.y, (int)t.z]);
            //    grid[(int)t.x, (int)t.y, (int)t.z] = startCell;
            //}
            else
            {
                //calculate distance and add it to tot dist
                Debug.DrawLine(startCell.pos, grid[(int)t.x, (int)t.y, (int)t.z].pos, Color.cyan, 1f);
                
                guideQueue.Enqueue(grid[(int)t.x, (int)t.y, (int)t.z].pos);

                grid[(int)t.x, (int)t.y, (int)t.z].distToSeed = startCell.distToSeed + (grid[(int)t.x, (int)t.y, (int)t.z].pos - startCell.pos).magnitude;
                grid[(int)t.x, (int)t.y, (int)t.z].parent = startCell.gridPos;
                startCell.state = State.parent;
                findOptimalPath(ref grid[(int)t.x, (int)t.y, (int)t.z]);
                
            }
        }
    }



    if(boredom > 5f)
    {
        //look for something to do
        Collider[] cs = Physics.OverlapSphere(armBase.transform.position, 10f);

        //make decision on what to do based on cs
        foreach(Collider c in cs)
        {
            if(c.tag == "Food")
            {
                //Set task to get food;
                lookMe = c.gameObject;
                //guidance.cube = lookMe;
                brainState = BrainState.Hungry;
                boredom = 0.0f;
                break;
            }
            else
            {
                brainState = BrainState.Idle;
            }
        }

    }

    //Based on brainstate do something
    if (brainState == BrainState.Hungry)
    {
        if (lookMe == null)
        {
            boredom = 5f;
            returnToNorm();
            pf.q.Clear();
            brainState = BrainState.Idle;

        }
        else
        {
            //Walk towards food, when head is close enough to food set state to pecking
            //Rotate body to face the food
            Vector3 t = new Vector3(lookMe.transform.position.x - transform.position.x, 0f, lookMe.transform.position.z - transform.position.z);
            transform.right = t;
            //lookAt(lookMe.transform.position - transform.position);

            //pf.q = guidance.guideQueue;

            //move //direction of the bread
            if (pf.q.Count == 0)
            {
                Vector3 p = (lookMe.transform.position);
                p.y = transform.position.y;
            }



            //if the head is within 0.4 x, z of the food
            Vector3 j = armHead.transform.position - lookMe.transform.position;
            j.y = 0;

            if (j.magnitude < 0.1f)
            {
                returnToNorm();
                peckTimer = 0.0f;
                pf.q.Clear();
                brainState = BrainState.Pecking;

            }
        }
    }
    else if(brainState == BrainState.Pecking)
    {
        //while pecking run a count down to how long the peck will last, at the end of the peck set state to idle and call returntonormal, also check if there are more seeds nearby and call hungry on them
        peckTimer += Time.deltaTime;
        peck();
        if(peckTimer > peckLength)
        {
            returnToNorm();
            boredom = 5f;
            brainState = BrainState.Idle;
        }
    }
    else if(brainState == BrainState.Idle)
    {
        boredom += Time.deltaTime;
        //increase boredom
    }

*/
