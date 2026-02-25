using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RotateObject : MonoBehaviour
{

    public float rotationSpeed;
    public string axis = "y";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (axis == "y")
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        }
        else if (axis == "x")
        {
            transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f);
        }
        else if (axis == "z")
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
        else
        {
            Debug.Log("Invalid Axis");
        }

            
    }
}
