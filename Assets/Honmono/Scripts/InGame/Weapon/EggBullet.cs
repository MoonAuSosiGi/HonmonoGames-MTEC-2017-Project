using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggBullet : Bullet
{

    void OnTriggerEnter2D(Collider2D col)
    {
        if (m_isNetworkObject)
            return;

        string prefabPath = GamePath.MONSTER1;

        GameObject obj = MapManager.Instance().AddMonster(prefabPath , m_bulletName + "_" + Monster.m_index , transform.position);
        DeleteBullet();
    }
}