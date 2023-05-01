using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfLife2 : MonoBehaviour
{
    private int count = 800;

    private void OnEnable()
    {
        count = 800;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        count--;
        if (count <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
