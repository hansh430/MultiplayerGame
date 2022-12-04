using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScript : MonoBehaviour
{
    public Transform target;
    public Transform throwPoint;
    public float timeTillHit=1f;
    private Rigidbody _rigidbody;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
       // _rigidbody.useGravity = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
           
        }
    }
    void Throw()
    {
       // _rigidbody.useGravity = 1;
        var xDistance = target.position.x - throwPoint.position.x;
        var yDistance = target.position.y - throwPoint.position.y;
        var angle = Mathf.Atan(yDistance + 4.98f * (timeTillHit * Time.deltaTime));
    }
}
