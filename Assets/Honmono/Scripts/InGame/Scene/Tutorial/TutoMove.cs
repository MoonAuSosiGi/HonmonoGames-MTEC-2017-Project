using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoMove : MonoBehaviour {

    public bool SUCCESS = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag.Equals("Player"))
        {
            SUCCESS = true;
            gameObject.SetActive(false);
        }
    }
}
