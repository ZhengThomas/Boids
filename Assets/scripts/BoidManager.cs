using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

/// <summary>
/// a gameobject that handles the spawning and holding of information that all boids need
/// usually acts as the parent object of the boids
/// </summary>
public class BoidManager : MonoBehaviour
{
    // Start is called before the first frame update
    public int numRays;
    public int spawnNum;
    public GameObject boid; //boid to be spawned

    //screenwrap where
    public float xMax;
    public float yMax;
    public float zMax;

    [HideInInspector]
    public Transform[] boidArray;
    [HideInInspector]
    public Vector3[] boidPositions;
    [HideInInspector]
    public Vector3[] directions;
    [HideInInspector]
    public KDTree boidTree;

    void Start()
    {
        //create rays for obstacle avoidance
        //sort of complicated, but based on the number of specified rays we want to end up shooting out, we turn 
        //an angle, multiplied by the golden ratio
        //we want to multiply by an irrational number to cover the most amount of space or reduce the amount of
        //angles that we have alreaedy covered, and the golden ratio is apparently the most irrational number
        //we spin horizontally by a defined golden ratio * 2 Pi radians
        //we spin vertically by 1 / NumRays * 2Pi radians vertically
        //we then turn the angles into direction vector components
        directions = new Vector3[numRays];
        //first, we plot them onto a circle
        float goldenRatio = (1 + Mathf.Pow(5, 0.5f)) / 2;
        float angleIncrement = goldenRatio * Mathf.PI * 2; // 2 pi = 360, multiply by golden ratio
        for(int i = 0; i < numRays; i++)
        {
            float j = (float)i / numRays; // this is more or less the real iterator, a number between 0-1
            float vertiAngle = Mathf.Acos(1 - 2 * j); //also called inclination
            float horizAngle = angleIncrement * i; //also called azimuth

            //by multiplying the angles that have been sin/cos, you can turn them into direction vectors
            float x = Mathf.Sin(vertiAngle) * Mathf.Cos(horizAngle);
            float y = Mathf.Sin(vertiAngle) * Mathf.Sin(horizAngle);
            float z = Mathf.Cos(vertiAngle);

            directions[i] = new Vector3(x, y, z);
        }

        //create boids
        boidArray = new Transform[spawnNum];
        boidPositions = new Vector3[spawnNum];
        for(int i = 0; i < spawnNum; i++)
        {
            GameObject newBoid = Instantiate(boid, transform.position, transform.rotation);
            boidArray[i] = newBoid.transform;
            newBoid.transform.parent = transform; //set boids parent as self
        }

    }

    void Update()
    {
        for (int i = 0; i < boidArray.Length; i++)
        {
            boidPositions[i] = boidArray[i].position; //every frame, update the arryas positions
        }

        boidTree = KDTree.buildTree(boidPositions); //every frame, rebuild the tree
    }

}
