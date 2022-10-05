using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private GameObject head_1, head_2, shaft;

    public void Init(Vector3 arrow_scale)
    {
        transform.localScale = arrow_scale;
    }
}
