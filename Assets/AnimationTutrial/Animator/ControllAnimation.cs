using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllAnimation : MonoBehaviour
{
    Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            if(!anim.GetBool("run"))
            {
                anim.SetBool("run", true);
            }
            else
            {
                anim.SetBool("run", false);
            }
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            anim.SetBool("move", true);
        }
    }
}
