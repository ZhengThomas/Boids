using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a gameobject that handles the movement and input for camera movement
/// </summary>
public class CameraController : MonoBehaviour
{
    public float moveSpeed = 20f;
    Vector3 velocity;

    float xRotation = 0;
    float yRotation = 0;
    float rotSpeed = 1000; 
    void Start()
    {
        //lock cursor into center of screen
        //press esc to unlock
        Cursor.lockState = CursorLockMode.Locked; 
    }

    void Update()
    {
        RotateCam();
        MoveCam();
    }

    /// <summary>
    /// a helper function that causes this gameobject to move based on input
    /// </summary>
    void MoveCam()
    {
        //movement of camera is based on minecraft
        //wasd only moves you horizontally
        //shift or space moves you vertically
        //ctrl to move faster

        //get input
        float leftRight = Input.GetAxisRaw("Horizontal");
        float forwardBack = Input.GetAxisRaw("Vertical");
        float upDown = Input.GetAxisRaw("Upwards");

        float camSpeed = moveSpeed; //i dont want to rename my stuff

        camSpeed *= 1 + (2 * Input.GetAxisRaw("Sprint"));

        

        //im completly aware that rigidbody exists and already does a lot of physics for me
        //but i want to make it look like i did some cool code
        float xRot = Mathf.Deg2Rad * transform.localRotation.eulerAngles.y;
        
        //get direction based on your rotation when you press forwards and back
        Vector3 forwardDir = new Vector3(Mathf.Sin(xRot), 0, Mathf.Cos(xRot));
        Vector3 sideDir = new Vector3(Mathf.Cos(xRot), 0, -Mathf.Sin(xRot));
        
        velocity = (forwardDir * forwardBack + (sideDir * leftRight));
        //normalize direction before you go forward to make sure you arent moving faster diagonally
        velocity = Vector3.Normalize(velocity) * camSpeed;

        //this is for vertical movement
        velocity.y += upDown * camSpeed;

        Vector3 toAdd = new Vector3(velocity.x * Time.deltaTime, velocity.y * Time.deltaTime, velocity.z * Time.deltaTime);
        transform.position += toAdd;
        
    }

    /// <summary>
    /// a helper function that rotates the camera based on mouse movement
    /// uses Time.deltatime to assume linear mouse movement between frames, meaning it
    /// makes the mouse move very faster on frame drops
    /// </summary>
    void RotateCam()
    {
        //get mouse input
        float mouseX = Input.GetAxis("Mouse X") * rotSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * rotSpeed * Time.deltaTime;

        yRotation -= mouseY;
        xRotation += mouseX; //adding to make it follow mouse rather then invert

        //clamps y rotation so you dont rotate and break neck
        yRotation = Mathf.Clamp(yRotation, -89f, 89f); 

        //the "x" value in euler dicatates up and down looking
        //the "y" value in the euler dictaties left and right
        //the "z" value is the barrel roll rotation, completly useless and should never be changed in this case
        transform.localRotation = Quaternion.Euler(yRotation, xRotation, 0f);
    }
}
