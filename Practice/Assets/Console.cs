using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Console : MonoBehaviour
{
    public void Print(string message)
    {
        Debug.Log(message);
    }

    public void Print(float num)
    {
        Debug.Log(num);
    }
}
