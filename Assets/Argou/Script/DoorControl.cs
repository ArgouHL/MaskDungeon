using System;
using UnityEngine;

public class DoorControl : MonoBehaviour
{
    internal void Close()
    {
       gameObject.SetActive(true);
    }

    internal void Open()
    {
        gameObject.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
