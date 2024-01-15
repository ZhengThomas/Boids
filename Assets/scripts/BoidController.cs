using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a gameobject that handles the movement and physics of the boid object
/// </summary>
public class BoidController : MonoBehaviour
{
    public float speedMult;
    public float maxSpeed;
    public float minSpeed;

    BoidManager boidMan;
    float xMax;
    float yMax;
    float zMax;

    public Vector3 velocity;
    void Start()
    {
        //speedMult *= Random.value;
        //speedMult += 2f;
        velocity = new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f);
        velocity = velocity.normalized * 5;

        boidMan = transform.parent.GetComponent<BoidManager>(); //get info from boid manager

        //bounds for screenwrapping
        xMax = boidMan.xMax;
        yMax = boidMan.yMax;
        zMax = boidMan.zMax;
    }

    // Update is called once per frame
    void Update()
    {
        //screenwraps if youre too far off
        screenWrap(xMax, yMax, zMax);

        Vector3 direction = velocity.normalized; //which way are we going in
        float speed = Mathf.Clamp(velocity.magnitude, minSpeed, maxSpeed); //clamp speed to a max and min
        velocity = direction * speed; //clamp velocity to a max and a min based on the speed

        transform.position += velocity * Time.deltaTime * speedMult; //actually move
        transform.forward = direction; // turn where were moving
    }

    /// <summary>
    /// a helper function that causes this gameobject to teleport and "screenwrap" when it leaves
    /// given boundaries
    /// <param name="xMax">the maximum x position</param>
    /// <param name="yMax">the maximum y position</param>
    /// <param name="zMax">the maximum z position</param>
    /// </summary>
    void screenWrap(float xMax, float yMax, float zMax)
    {
        //max defines the max height, the boid can exist from 0 to max

        //screen wrap sorta, teleport to ground if you hit the roof, left if you hit right
        Vector3 newPos = new Vector3(0, 0, 0);
        newPos.y = mod(transform.position.y, yMax); // wrap between 0 and max
        newPos.x = mod(transform.position.x + xMax/2, xMax) - (xMax/2); //wrap between -max/2 and max/2
        newPos.z = mod(transform.position.z + zMax/2, zMax) - (zMax/2); //wrap between -max/2 and max/2
        transform.position = newPos;
    }

    /// <summary>
    /// a function that properly modulo's data
    /// C#'s modulo is closer to remainder after division, and so something like -5%99 = -5 rather then 94
    /// <param name="x">the number to be modded</param>
    /// <param name="m">the number to mod by</param>
    /// </summary>
    float mod(float x, float m)
    {
        return (x % m + m) % m; 
    }


}
