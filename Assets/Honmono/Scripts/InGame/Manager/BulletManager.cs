using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : Singletone<BulletManager> {

    public enum BULLET_TYPE
    {
        B_HERO_DEF = 0,  // 주인공의 디폴트 총알
        B_BOSS1_P1,      // 보스 1 패턴 1 총알
    }

    //-----------------------------------//
    // 총알 리스트ㅡ
    
    List<Bullet> m_bulletList = new List<Bullet>();
    List<Bullet> m_BossbulletList = new List<Bullet>();
    private int m_bulletIndex = 0;
    private int m_bossBulletIndex = 0;

    public GameObject m_heroBulletPool = null;
    public GameObject m_BossBulletPool = null;
    //----------------------------------//

    void Start()
    {
        for(int i = 0; i < m_heroBulletPool.transform.childCount; i++)
        {
            m_bulletList.Add(m_heroBulletPool.transform.GetChild(i).GetComponent<Bullet>());
            NetworkManager.Instance().AddNetworkEnemyMoveEventListener(m_bulletList[i]);
        }

       
    }

    public Bullet AddBullet(BULLET_TYPE type)
    {
        string path = null;

        switch(type)
        {
            case BULLET_TYPE.B_HERO_DEF: //path = GamePath.WEAPON_BULLET_DEF; break;
                {
                  return AddBulletHero();
                }
            case BULLET_TYPE.B_BOSS1_P1:
                {
                    path = GamePath.WEAPON_BULLET_BOSS;
                    return AddBulletBoss1(path);
                }
        }

        
       
        return null;
    }

    public void RemoveBullet(Bullet bullet)
    {
        bullet.ALIVE = false;
        bullet.SendMessage("MoveSend");
        bullet.gameObject.SetActive(false);
        
     //   NetworkManager.Instance().RemoveNetworkEnemyMoveEventListener(bullet);

        //GameObject.Destroy(bullet.gameObject);
    }

    public void RemoveBullet(string bulletName,Bullet.BULLET_TARGET b)
    {
        Bullet target = null;
        List<Bullet> list = null;
        if( b == Bullet.BULLET_TARGET.PLAYER)
        {
            list = m_bulletList;
        }
        else if( b == Bullet.BULLET_TARGET.ENEMY)
        {
            list = m_BossbulletList;
        }

        foreach (Bullet bullet in list)
        {
            if (bullet != null)
            {
                if (bullet.BULLET_NAME == bulletName)
                {
                    target = bullet;
                    break;
                }
            }
        }


        if (target != null)
        {
            RemoveBullet(target);
        }
    }

    Bullet AddBulletHero()
    {
        Bullet bullet = m_bulletList[m_bulletIndex++];
        bullet.transform.parent = transform;
        bullet.gameObject.SetActive(true);
        bullet.ALIVE = true;

        if (m_bulletIndex >= m_bulletList.Count)
        {
            m_bulletIndex = 0;
        }
        return bullet;
    }

    Bullet AddBulletBoss1(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        Bullet ck = null;

        //시나리오
        //가용한 총알이 있는지 체크
        for(int i = 0; i < m_BossbulletList.Count; i++)
        {
            if (!m_BossbulletList[i].ALIVE)
            {
                ck = m_BossbulletList[i];
                ck.gameObject.SetActive(true);
                ck.ALIVE = true;
                break;
            }
        } 

        if(ck == null)
        {
            GameObject obj = Resources.Load(path) as GameObject;
            GameObject bullet = GameObject.Instantiate(obj);
            Bullet b = bullet.GetComponent<Bullet>();
            bullet.SetActive(true);
            b.ALIVE = true;
            b.transform.parent = m_BossBulletPool.transform;
            m_BossbulletList.Add(b);
            return b;
        }
        else
        {
            return ck;
        }
    }

}
