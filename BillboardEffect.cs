using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardEffect : MonoBehaviour
{
    private Vector3 OriginalRotation;
    // Start is called before the first frame update
    private void Awake()
    {
        OriginalRotation = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }
}
