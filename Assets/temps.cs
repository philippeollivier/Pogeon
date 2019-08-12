    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum BrainState
{
    Idle, Hungry, Socialize, Pecking, Scared, Blank
}
enum ColliderType
{
    Food, Player, Nothing
}

public class temps : MonoBehaviour
{
    public float temp;

    public Camera cam;
    public NavMeshAgent agent;
    public Animator animator;
    public GameObject hat;
    private GameObject realHat;
    public GameObject headBone;
    private Guidance guidance;

    //brain
    public float boredom = 0;
    public  float social = 0;
    float interest = 0;
    private float peckRange = 0.7f;
    private float peckTimer = 0f;
    private float peckTime = 0.5f;
    public BrainState brainState = BrainState.Idle;
    public GameObject focus = null;
    bool waited = false;

    //flight
    Vector3[] vectors = new Vector3[3];
    bool flying = false;
    List<Vector3> tempList = new List<Vector3>();
    Vector3 startPos;
    public float moveSpeed = 4f;
    float percentage = 0.0f;

    //pigeon traits
    private float locality = 10f;
    private float maxBoredom = 10f;
    public float idleSpeed = 1f;
    private float maxSpeed = 4f;
    private float size = 1f;
    private float competitiveness = 1f;
    private float extroversion = 1f;
    private float impatience = 1f;
    private float maxSocial = 2f;


    void RandomStats()
    {
        extroversion = Random.Range(-0.2f, 1f);
        competitiveness = Random.Range(0.5f, 2.5f);
        locality = Random.Range(8f, 12f);
        maxBoredom = Random.Range(2f, 5f);
        idleSpeed = Random.Range(1.6f, 2.6f);
        maxSpeed = Random.Range(3.5f, 4.5f);
        maxSocial = Random.Range(1.5f, 5.5f);
        size = Random.Range(0.87f, 1.15f);

        if(Random.Range(0f, 1f) > 0.99f)
        {
            realHat = Instantiate(hat, headBone.transform);
            realHat.transform.localPosition = new Vector3(0, 0, 0.01f);
            realHat.transform.localScale = new Vector3(2f, 2f, 0.2f);
            realHat.transform.localEulerAngles = new Vector3(-42f, 0f, 0f);
            size = 1.5f;
        }

        //introverts are slower
        if(extroversion < 0) { idleSpeed *= 0.8f; maxSpeed *= 0.8f; }
    }

    private void Start()
    {
        //RANDOMLY GENERATE STATS
        animator = GetComponent<Animator>();
        RandomStats();
        transform.localScale *= size;
        guidance = GetComponent<Guidance>();

        AddEvent(2, 0.18f, "Peck", 0);
        AddEvent(2, 0.37f, "ResetBrain", 0);

        guidance.GenerateGrid();

        boredom = maxBoredom;
       // Debug.Log(gameObject.name + ": E" + extroversion + ", C" + competitiveness + ", L" + locality + ", MB" + maxBoredom + ", IS" + idleSpeed + ", MS" + maxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        //================================================================================================================================================================================
        //DECISION
        //================================================================================================================================================================================
        //Take in all there is around him

        if (agent.enabled)
        {
            if(brainState != BrainState.Scared)
            {
                SphereBrain();
            }

            if (brainState == BrainState.Hungry && focus != null)
            {
                agent.SetDestination(focus.transform.position);
            }

            if (brainState == BrainState.Idle && boredom > maxBoredom)
            {
                //Randomly walk somewhere
                agent.SetDestination(transform.position + Vector3.right * Random.Range(-5, 5) + Vector3.forward * Random.Range(-5, 5));
                boredom = 0;

            }

            //================================================================================================================================================================================
            //UPDATE
            //================================================================================================================================================================================
            if (brainState == BrainState.Hungry && focus != null)
            {
                /*
                Collider[] nearby = Physics.OverlapSphere(transform.position, 2f);
                int num = 0;
                foreach (Collider x in nearby)
                {
                    if (x.gameObject.tag == "Player" && x.gameObject != gameObject)
                    {
                        num++;
                    }
                }
                //Set interest from 0-1
                interest = Mathf.Clamp(num * 0.05f * competitiveness, 0, 1f);
                */
                //When in range, peck, after pecking then idle, also need some way to prevent getting stuck after pecking and finding food nearby still
                agent.speed = idleSpeed + (maxSpeed - idleSpeed); //* interest;

                //Wait one frame
                if (waited)
                {
                    if (Vector3.Magnitude(focus.transform.position - transform.position) < peckRange)
                    {
                        animator.SetTrigger("Peck");
                        agent.SetDestination(transform.position);
                    }
                }
                else { waited = true; }
                
            }
            else if (brainState == BrainState.Hungry && focus == null)
            {
                ResetBrain();
            }
            else if (brainState == BrainState.Socialize && focus != null && social < maxSocial)
            {
                agent.speed = idleSpeed + (maxSpeed - idleSpeed) * 0.5f;
                social += Time.deltaTime;
                animator.SetBool("Flapping", false);
                if (Vector3.Magnitude(focus.transform.position - transform.position) < locality)
                {
                    if (extroversion > 0)
                    {
                        agent.SetDestination(focus.transform.position);
                    }
                    else
                    {
                        agent.SetDestination(2 * transform.position - focus.transform.position);

                        if (Vector3.Magnitude(focus.transform.position - transform.position) < 2f)
                        {
                            animator.SetBool("Flapping", true);
                            agent.speed = maxSpeed * 1.2f;
                        }
                    }
                }
                else
                {
                    ResetBrain();
                }
            }
            else if(brainState == BrainState.Scared)
            {
                agent.SetDestination(2 * transform.position - focus.transform.position);
                agent.speed = maxSpeed * 1.2f;

                if (Vector3.Magnitude(focus.transform.position - transform.position) < 2f)
                {
                    animator.SetBool("Flapping", true);
                    agent.speed = maxSpeed * 1.4f;
                }
            }
            else
            {
                agent.speed = idleSpeed;
                social -= Time.deltaTime;
                boredom += Time.deltaTime * impatience;
            }
            

            //If can't walk to destination then should fly
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(agent.destination, path);
            if (path.status == NavMeshPathStatus.PathPartial)
            {
                //Enable Flight and set flight destination
                //vectors[0] = transform.position;
                //vectors[1] = ;
                //vectors[2] = ;
                //vectors[3] = ;
                //vectors[4] = agent.destination;

                //tempList = bezier(vectors);
                

                agent.enabled = false;
                flying = true;
                percentage = 0;
            
                vectors[0] = transform.position;

                /*
                //do 5 raycasts in direction of destination, forward, and the 45s cardinals, if they don't hit anything then set that to be the next position. IF ALL 5 hit, then 
                for (int i = 1; i < vectors.Length - 1; i++)
                {
                    Vector3 startPos;
                    Vector3 direction;

                    startPos = vectors[i-1];
                    direction = agent.destination - startPos;

                    Vector3[] directions = { direction.normalized, (direction + Vector3.up).normalized, (direction - Vector3.up).normalized, (direction + Vector3.right).normalized, (direction - Vector3.right).normalized };
                    
                    for(int j = 0; j < directions.Length; j++)
                    {
                        Ray ray = new Ray(vectors[i - 1], directions[j] + vectors[i-1]);
                        if(!Physics.Raycast(ray, 3f))
                        {
                            vectors[i] = directions[j] + vectors[i - 1] * 3f;
                        }
                    }

                    if(vectors[i] == null)
                    {
                        //Pigeon can't reach
                    }
                }
                */

                vectors[1] = Vector3.one;
                vectors[2] = agent.destination;
                
                tempList = bezier(vectors);
                Debug.Log(tempList.Count);

            }

        }
        

        if (flying)
        {
            animator.SetBool("Flapping", true);
            DrawList(tempList);
            moveTo(tempList, ref percentage);
            if(tempList.Count < 2)
            {
                animator.SetBool("Flapping", false);
                agent.enabled = true;
                flying = false;
            }
        }

        
        //Animator speed stuff
        animator.speed = agent.speed * 16 / 25f + 28 / 50f;
        AnimationClip clip = animator.runtimeAnimatorController.animationClips[1];


        animator.SetFloat("Velocity", Mathf.Clamp(agent.velocity.magnitude / 3.5f, 0, 0.9f));
    }

















    void Peck()
    {
        Vector3 relativeDist = transform.forward * 0.5f;
        float peckSize = 0.65f;
        Collider[] cs = Physics.OverlapSphere(relativeDist + transform.position, peckSize);

        foreach (Collider c in cs)
        {
            if (c.tag == "Food")
            {
                Destroy(c.gameObject);
            }
        }
        
    }

    void SphereBrain()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, locality);
        //temporary interest set to null
        GameObject interestObject = null;
        //temp collider set to largest at nothing
        ColliderType ct = ColliderType.Nothing;
        //Cycle through colliders and find highest priority and save that 
        if(colliders.Length > 2)
        {

            foreach (Collider c in colliders)
            {
                //Second temp is also nothing
                ColliderType ct2 = ColliderType.Nothing;
                if (c.gameObject.tag == "Food")
                {
                    ct2 = ColliderType.Food;
                }
                else if (c.gameObject.tag == "Player" && c.gameObject != gameObject)
                {
                    ct2 = ColliderType.Player;
                }
                else
                {
                    ct2 = ColliderType.Nothing;
                }

                if (ct2 <= ct)
                {
                    interestObject = c.gameObject;
                    switch ((int)ct2)
                    {
                        case (int)ColliderType.Food:
                            brainState = BrainState.Hungry;
                            if(focus == null || interestObject.tag != "Food")
                            {
                                focus = interestObject;
                                ct = ct2;
                            }
                            else if((interestObject.transform.position - transform.position).magnitude < (focus.transform.position - transform.position).magnitude){
                                focus = interestObject;
                                ct = ct2;
                            }
                            break;
                        case (int)ColliderType.Player:
                            //if current focus is closer than new focus then update 
                            if (focus != null && Vector3.Magnitude(focus.transform.position - transform.position) > locality * 0.9f)
                            {
                                focus = null;
                            }
                            else if(focus != null)
                            {
                                
                                if (extroversion > 0)
                                {
                                    if (Vector3.Magnitude(focus.transform.position - transform.position) < Vector3.Magnitude(interestObject.transform.position - transform.position))
                                    {
                                        focus = interestObject;
                                        ct = ct2;
                                    }
                                }
                                else
                                {
                                    if (Vector3.Magnitude(focus.transform.position - transform.position) > Vector3.Magnitude(interestObject.transform.position - transform.position))
                                    {
                                        focus = interestObject;
                                        ct = ct2;
                                    }
                                }
                            }
                            
                            if(focus == null)
                            {
                                focus = interestObject;
                                ct = ct2;
                            }
                            if(extroversion > 0)
                            {
                                Debug.DrawLine(transform.position, focus.transform.position, Color.green);
                            }
                            else
                            {
                                Debug.DrawLine(transform.position, focus.transform.position, Color.red);
                            }
                            brainState = BrainState.Socialize;

                            if(focus != null && Vector3.Magnitude(focus.transform.position - transform.position) > locality * 0.6f)
                            {
                                brainState = BrainState.Idle;
                                focus = null;
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    int[] Pascal(int row)
    {
        if(row < 0)
        {
            return null;
        }
        else
        {
            int[] ints = new int[row+1];

            ints[0] = 1; ints[row] = 1;

            for(int i = 0; i < (int)row/2; i++)
            {
                int x = ints[i] * (row - i) / (i + 1);
                ints[i + 1] = x;
                ints[row-1-i] = x;
            }

            return ints;
        }

    }

    
    List<Vector3> bezier(Vector3[] vectors)
    {
        if(vectors.Length > 0)
        {
            //Resolution
            int resolution = 500;
            
            //Calculate nth order bezier
            int order = vectors.Length - 1;

            //Pascals
            int[] pascal = Pascal(order);

            //Create list
            List<Vector3> pathList = new List<Vector3>();

            //Create t (t/i)
            float tscale = 1f / resolution;

            for (int i = 0; i < resolution; i++)
            {
                Vector3 newPoint = Vector3.zero;
                for(int j = 0; j <= order; j++)
                {
                    float t = i * tscale;
                    newPoint += pascal[j] * Mathf.Pow((1 - t), order - j) * Mathf.Pow(t, j) * vectors[j];
                }

                pathList.Add(newPoint);
            }

            return pathList;

        }
        else
        {
            return null;
        }
    }
    

    void DrawList(List<Vector3> inList)
    {
        if(inList.Count > 0)
        {
            Vector3 v1;
            Vector3 v2;

            for (int i = 1; i < inList.Count; i++)
            {
                v1 = inList[i - 1];
                v2 = inList[i];
                Debug.DrawLine(v1, v2, Color.red);
            }
        }
    }

    void moveTo(List<Vector3> list, ref float percentage)
    {
        //When you call it, snap yourself to first point on the list, remove first index on list.
        //lerp to index one, when near index one snap to it and remove it from the list.
        
        //Movement will calculate based on starting point and end of the list, total distance travelled
        //movement list will be of equal length segments
        //whenever percentage goes above 1, remove first element of queue and subtract 1 from percent //also set starting value
        if (list.Count > 1)
        {
            percentage += Time.deltaTime * moveSpeed / (list[1] - list[0]).magnitude;
            while (percentage > 0.99f)
            {
                if(list.Count == 1)
                {
                    list.Clear();
                    break;
                }
                else
                {
                    list.RemoveAt(0);
                    percentage--;
                }
            }
            if(list.Count > 1)
            {
                transform.position = list[0] + (list[1] - list[0]) * percentage;
                transform.forward = list[1] - list[0];
            }
        }
    }

    void ResetBrain()
    {
        agent.enabled = true;
        boredom = 0;
        agent.SetDestination(transform.position);
        brainState = BrainState.Idle;
        animator.SetBool("Flapping", false);
    }

    void AddEvent(int Clip, float time, string functionName, float floatParameter)
    {
        animator = GetComponent<Animator>();
        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.functionName = functionName;
        animationEvent.floatParameter = floatParameter;
        animationEvent.time = time;
        AnimationClip clip = animator.runtimeAnimatorController.animationClips[Clip];
        clip.AddEvent(animationEvent);
    }
}
