using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfLife : MonoBehaviour
{
    int count = 150;

    private void OnEnable()
    {
        count = 150;
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
