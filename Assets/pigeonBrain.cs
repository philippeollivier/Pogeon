using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class pigeonBrain : MonoBehaviour
{
    [Header("Armature")]
    [SerializeField] GameObject armBase;
    [SerializeField] GameObject armNeck, armHead, armLWingBase, armLWingTip, armRWingBase, armRWingTip, armHeadEnd;
    private Vector3 normNeck;

    private float headHorzAngle = 0, headVertAngle = 0;
    private Vector3 neckDefaultAngle, headDefaultAngle;
    private Quaternion neckDefaultAngleQ, headDefaultAngleQ;
    private float ratio = 0.3f;
    private float peckSize = 0.5f;
    private float peckTimer = 0.0f;
    private float peckLength = 0.2f;

    private float locality = 10f;


    private BrainState brainState = BrainState.Idle;
    
    Vector3 startDir;
    public GameObject bread;
    GameObject lookMe;
    PathFinder pf;
    Guidance guidance;

    private float patience = 1f;
    private float boredom = 0f;
    private float speed = 2.5f;
    private Rigidbody rb;

    float t = 0;
    int lastTick = 4;

    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        guidance = gameObject.GetComponent<Guidance>();
        pf = gameObject.GetComponent<PathFinder>();

        neckDefaultAngle = armNeck.transform.eulerAngles;
        headDefaultAngle = armHead.transform.eulerAngles;

        neckDefaultAngleQ = armNeck.transform.localRotation;
        headDefaultAngleQ = armHead.transform.localRotation;

        normNeck = armHeadEnd.transform.position - armHead.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //Create trigger zone, if pigeon is in the zone then he jumps



        



        //pigeons have an innate random bor

        //if there is food nearby then walk in that direction


        //when you reach a ledge, hop over it. a ledge is defined by having your forward raycast blocked but the forward + jump height not blocked;





    }

    void Ledge()
    {
        //Debug.DrawLine(transform.position, transform.right)
        //if (Physics.Raycast(transform.position, transform.right, 1f, 10))
        {

        }
    }

    void rotate(GameObject go)
    {
        transform.eulerAngles = (Vector3.SignedAngle(transform.right, go.transform.position - transform.position, Vector3.up) * Vector3.up) + new Vector3(0, 270f, 0);
    }

    void peck()
    {
        Collider[] cs = Physics.OverlapSphere(armHeadEnd.transform.position, peckSize);

        foreach(Collider c in cs)
        {
            if(c.tag == "Food")
            {
                Destroy(c.gameObject);
            }
        }

    }

    void returnToNorm()
    {
        //armBase.transform.localEulerAngles = new Vector3(armBase.transform.localEulerAngles.x, 0f, -54.245f);
        //armNeck.transform.localEulerAngles = new Vector3(45.474f, armNeck.transform.localEulerAngles.y, armNeck.transform.localEulerAngles.z);
        //armHead.transform.localEulerAngles = new Vector3(-30.571f, armHead.transform.localEulerAngles.y, armHead.transform.localEulerAngles.z);
        Quaternion q = Quaternion.Euler(armBase.transform.localEulerAngles.x, 0f, -54.245f);
        armBase.transform.localRotation = q;
        Quaternion w = Quaternion.Euler(45.474f, armNeck.transform.localEulerAngles.y, armNeck.transform.localEulerAngles.z);
        armNeck.transform.localRotation = w;
        Quaternion e = Quaternion.Euler(-30.571f, armHead.transform.localEulerAngles.y, armHead.transform.localEulerAngles.z);
        armHead.transform.localRotation = e;
    }

    //Look at something taking in direction, if the object is at too high of an angle, just look straight forwards
    void lookAt(Vector3 direction)
    {
        //Convert direction in to a horizontal angle and vertical angle and clamp them
        float tempHorz = AngleAroundAxis(Vector3.forward, direction, Vector3.up);
        //Debug.Log(direction + ", " + tempHorz);
        if (direction.x * direction.x + direction.z * direction.z < 0.05f) { tempHorz = 0; }

        float tempVert = Mathf.Acos(Vector3.ProjectOnPlane(direction, Vector3.up).magnitude / direction.magnitude) * 180f / Mathf.PI * Mathf.Sign(direction.y);

        //Either have clamp or forward looks
        //if (tempHorz > 85f || tempHorz < -85f || tempVert > 85f || tempVert < -55f)
        //{
        //    tempHorz = 0;
        //    tempVert = 0;
        //}

       
        tempHorz = Mathf.Clamp(tempHorz, -85f, 85f);
        tempVert = Mathf.Clamp(tempVert, -55f, 85f);


        //Convert angles into two seperate vector3 euler angles, 1 vector3 for neck and 1 for head
        
        armHead.transform.rotation = Quaternion.Euler(headDefaultAngle + new Vector3(tempVert, tempHorz));
        armNeck.transform.rotation = Quaternion.Euler(neckDefaultAngle + new Vector3(tempVert * ratio, tempVert * ratio));

        //armHead.transform.eulerAngles = headDefaultAngle + new Vector3(tempVert, tempHorz);
        //armNeck.transform.eulerAngles = neckDefaultAngle + new Vector3(tempVert * ratio, tempVert * ratio);
    }

    // The angle between dirA and dirB around axis
    public static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
    {
        // Project A and B onto the plane orthogonal target axis
        dirA = dirA - Vector3.Project(dirA, axis);
        dirB = dirB - Vector3.Project(dirB, axis);

        // Find (positive) angle between A and B
        float angle = Vector3.Angle(dirA, dirB);

        // Return angle multiplied with 1 or -1
        return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
    }
}
