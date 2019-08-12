using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PathFinder : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    Vector3 startPos;
    public Queue<Vector3> q;
    float percentage = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        q = new Queue<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        moveTo(q, ref percentage);
    }

    
    void moveTo(Queue<Vector3> q, ref float percentage)
    {

            //Movement will calculate based on starting point and end of the list, total distance travelled
            //movement list will be of equal length segments
            //whenever percentage goes above 1, remove first element of queue and subtract 1 from percent //also set starting value
        if (q.Count == 0){
            percentage = 0.0f;
            startPos = transform.position;
        }
        else
        {
            //lerp position from startPos to q[0] based on percentage.
            Vector3 dist = (q.Peek() - startPos);
            //percentage is how much travelled distance over total distance, travelled distance = 0 + time.DeltaTime * moveSpeed;
            percentage += Time.deltaTime * moveSpeed / dist.magnitude;

            transform.position = startPos + dist * percentage;
        }

        if (percentage > 1f){
                q.Dequeue();
                startPos = transform.position;
                percentage--;
        }

        
            
    }
}
