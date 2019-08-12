using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    Ray ray;
    RaycastHit hit;
    public GameObject seed;
    public float circleSize = 0.3f;
    GameObject[] pigeons;
    temps[] pigeonbrains;
    public float scareRadius = 5f;
    public GameObject scareBox;
    private void Start()
    {
        pigeons = GameObject.FindGameObjectsWithTag("Player");
        pigeonbrains = new temps[pigeons.Length];
        for(int i = 0; i < pigeons.Length; i++)
        {
            pigeonbrains[i] = pigeons[i].GetComponent<temps>();
        }
    }


    // Update is called once per frame
    void Update()
    { 
        if (Input.GetMouseButtonDown(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 500f, LayerMask.GetMask("Floor")))
            {
                seeds();
            }
        }

        if (Input.GetKey(KeyCode.V))
        {
            ScarePigeons();
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            foreach (temps t in pigeonbrains)
            {
                t.brainState = BrainState.Idle;
                t.animator.SetBool("Flapping", false);
                t.agent.speed = t.idleSpeed;
            }
        }
    }

    void seeds()
    {
        //random value from 3 to 8 seeds

        int rand = (int)Random.Range(1, 3f);
        for(int i = 0; i <= rand; i++)
        {
            //in a spherical area around the hit, cast a raycast from the sky downwards, if it touches the floor, spawn a food there
            //to generate a random sphere area
            Vector3 p = new Vector3(Random.Range(-circleSize, circleSize), 100f, Random.Range(-circleSize, circleSize));
            RaycastHit tempHit;
            if (Physics.Raycast(hit.point + p, Vector3.down, out tempHit, LayerMask.GetMask("Floor")))
            {
                Instantiate(seed, tempHit.point, Random.rotationUniform);
            }
        }
    }

    void ScarePigeons()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 5000f, LayerMask.GetMask("Floor")))
        {
            scareBox.transform.position = hit.point;
            foreach (temps t in pigeonbrains)
            {
                if ((t.gameObject.transform.position - hit.point).sqrMagnitude < scareRadius * scareRadius)
                {
                    t.brainState = BrainState.Scared;
                    t.focus = scareBox;
                }
                else
                {
                    t.brainState = BrainState.Idle;
                }
            }
        }
    }
}
