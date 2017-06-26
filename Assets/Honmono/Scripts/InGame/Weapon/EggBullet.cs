using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggBullet : Bullet
{
    bool ck = false;

    void Start()
    {
        if(GameManager.Instance().PLAYER.USER_NAME.Equals(NetworkOrderController.ORDER_NAME))
            Invoke("EggStart" , 6.0f);
    }
    void EggStart()
    {

        string prefabPath = GamePath.MONSTER1;

        GameObject obj = MapManager.Instance().AddMonster(prefabPath , m_bulletName + "_" + Monster.m_index , transform.position);

        ck = true;
        this.GetComponent<AudioSource>().Play();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if(ck && !this.GetComponent<AudioSource>().isPlaying)
        {
            DeleteBullet();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (m_isNetworkObject)
            return;

        m_moveSpeed = 0.0f;
    }
}