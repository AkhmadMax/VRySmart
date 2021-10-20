using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAwake : MonoBehaviour
{
    public string testString;

    private void Awake()
    {
        Debug.Log("Awake");
        Debug.Log(testString);
    }
    void Start()
    {
        Debug.Log("Start");
        Debug.Log(testString);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
