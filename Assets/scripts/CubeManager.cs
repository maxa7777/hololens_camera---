using System;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    DateTime? createTime;

    // Use this for initialization
    void Start()
    {
        createTime = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        if (createTime != null)
        {
            if ((DateTime.Now - createTime.Value).Seconds > 1)
            {
                GetComponent<Rigidbody>().useGravity = true;
                createTime = null;
            }
            else
            {
                var qua = gameObject.transform.rotation;
                qua.z += 0.1f;
                gameObject.transform.rotation = qua;
            }
        }
    }
}