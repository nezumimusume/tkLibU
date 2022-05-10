using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tkVlit_RotateDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var addRotation = Quaternion.Euler(-120.0f * Time.deltaTime, 0.0f, 0.0f);

        transform.localRotation *= addRotation;
    }
}
