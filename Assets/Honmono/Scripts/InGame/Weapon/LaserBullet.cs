using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBullet : MonoBehaviour {


    void OnTriggerEnter2D(Collider2D col)
    {
        // 맞았다 아프다 간다
        if(col.name == "ROBO")
            MDebug.Log("데미지를 입었다.");
    }
}
