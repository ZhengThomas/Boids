using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

/// <summary>
/// a gameobject that handles the logic and direction the boid will move in
/// </summary>
public class BoidBrain : MonoBehaviour
{
    // Start is called before the first frame update
    BoidController boid;
    BoidManager boidMan;

    Transform[] boidArray;

    [Header("Boid Vision")]
    public float visionRadius;
    public float seperationRadius;
    public float obstacleVisionRadius;
    public int maxPerceiveable;
    public LayerMask obstacleLayer;

    [Header("Boid Weights and Multipliers")]
    public float cohesionWeight;
    public float alignmentWeight;
    public float seperationWeight;
    public float avoidObstacleWeight;
    public float maxSteerForce;
    public float accelMult;
    


    
    void Start()
    {
        boid = GetComponent<BoidController>();
        boidMan = transform.parent.GetComponent<BoidManager>();
        boidArray = boidMan.boidArray;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 avgCohesionPos = Vector3.zero;
        Vector3 avgHeadingDir = Vector3.zero;
        Vector3 avgSeperationDir = Vector3.zero;

        Vector3 acceleration = Vector3.zero;
        int othersFound = 0;

        
        //comment this code if you want to use the brute force method
        //use priority queue since it order itself without mne needing to care
        //this way if theres 8 items and i want to get 5 closest
        //i can just pop 5 times
        SimplePriorityQueue<float[]> closest = boidMan.boidTree.findNearest(transform.position, maxPerceiveable);
        //if they didnt find more then the max perceiveable, then the first one is a bad one
        //almost never happens
        if(closest.First[0] == -1)
        {
            closest.Dequeue();
        }

        int startAmount = closest.Count;
        //the amount perceived can not be more then max perceivable
        int smallerVal = (int)Mathf.Min(startAmount, maxPerceiveable); 
        Transform[] closestArray = new Transform[smallerVal]; //array of close by people

        for (int i = 0; i < smallerVal; i++)
        {
            int indCurrent = (int)closest.Dequeue()[0];
            closestArray[i] = boidArray[indCurrent];
        }
        
        
        foreach (Transform other in closestArray)
        {
            if (other != transform)
            {
                Vector3 diff = other.transform.position - transform.position;
                float dist = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;

                //if other is within my vision radius
                if (dist < visionRadius * visionRadius)
                {
                    //cohesion logic
                    //average position of all boids iwthin my vision is where i wanna go
                    avgCohesionPos += other.transform.position;
                    othersFound += 1;

                    //alignment logic
                    avgHeadingDir += other.forward;

                    //seperation logic
                    // 1/dist relationship, become incredibly unnattracted to nearby boid if they are really close
                    if (dist < seperationRadius * seperationRadius)
                    {
                        avgSeperationDir -= diff / dist;
                    }
                }
            }
        }
        if (othersFound > 0)
        {
            avgCohesionPos /= othersFound; //divide to find average
        }
        else
        {
            //if we didnt find any bvoids around us, the average of all boids around us is ourself
            avgCohesionPos = transform.position; 
        }

        //avoidance logic for obstacles
        if (isHeadingForCollision())
        {
            Vector3 dir = findOpenDirection();
            
            //change acceleration based on my own rules
            acceleration += steerTowards(dir) * avoidObstacleWeight;
        }

        //change accelration based on the boid rules
        acceleration += steerTowards(avgHeadingDir) * alignmentWeight;
        acceleration += steerTowards(avgSeperationDir) * seperationWeight;
        acceleration += steerTowards(avgCohesionPos - transform.position) * cohesionWeight;

        //apply accerlation, this makes sure its smoothly turning rather then immediatly
        boid.velocity += acceleration * Time.deltaTime * accelMult;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }

    /// <summary>
    /// a helper function that checks if the current boid is moving towards a wall withing a vision radius
    /// uses raycasts
    /// </summary>
    bool isHeadingForCollision(){
        RaycastHit ray;
        //start, radius, direction, where ray info goes, distance, which layers
        if (Physics.SphereCast(transform.position, 0.75f, transform.forward, out ray, obstacleVisionRadius * 1.25f, obstacleLayer))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// a helper function that helps clamp a vector3 to a maxForce that the boid can be allowed to handle
    /// </summary>
    Vector3 steerTowards(Vector3 where)
    {
        Vector3 v = where.normalized * boid.maxSpeed;
        return Vector3.ClampMagnitude(v, maxSteerForce);
    }

    /// <summary>
    /// a helper function that continously shoots rays out in a circle around the boid, until a direction is found that is empty
    /// ran whenever the boid is heading for collision (the above function)
    /// uses precomputed directional vectors for directions to shoot in
    /// </summary>
    Vector3 findOpenDirection()
    {
        RaycastHit ray;
        for (int i = 0; i < boidMan.numRays; i++)
        {
            //realdir means the direction relative to the boids current direction
            //a direction of (0,0,1) would be whaterver forwards for the boid is at a specfic time
            Vector3 realDir = transform.TransformDirection(boidMan.directions[i]);
            if (!Physics.SphereCast(transform.position, 0.75f, realDir, out ray, obstacleVisionRadius, obstacleLayer))
            {
                return realDir;
            }
        }
        return transform.forward;
    }
}