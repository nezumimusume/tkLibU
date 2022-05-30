using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tkLibU
{
    public class tkVlit_RotateDemo : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            var addRotation = Quaternion.Euler(4.0f, 0.0f, 0.0f);

            transform.localRotation *= addRotation;
        }
    }
}
