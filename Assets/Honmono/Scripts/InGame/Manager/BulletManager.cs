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
    [SerializeField]
    private List<Bullet> m_bulletList = new List<Bullet>();
    private int m_bulletIndex = 0;
    //----------------------------------//

    void Start()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            m_bulletList.Add(transform.GetChild(i).GetComponent<Bullet>());
            NetworkManager.Instance().AddNetworkEnemyMoveEventListener(m_bulletList[i]);
        }

       
    }

    public Bullet AddBullet(BULLET_TYPE type)
    {
        string path = null;

        switch(type)
        {
            case BULLET_TYPE.B_HERO_DEF: path = GamePath.WEAPON_BULLET_DEF; break;
            case BULLET_TYPE.B_BOSS1_P1: path = GamePath.WEAPON_BULLET_BOSS; break;
        }

        if (path == null)
            return null;


        //GameObject obj = Resources.Load(path) as GameObject;
        //GameObject bullet = GameObject.Instantiate(obj);
        //m_bulletList.Add(bullet.GetComponent<Bullet>());
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

    public void RemoveBullet(Bullet bullet)
    {
        bullet.ALIVE = false;
        bullet.SendMessage("MoveSend");
        bullet.gameObject.SetActive(false);
     //   NetworkManager.Instance().RemoveNetworkEnemyMoveEventListener(bullet);

        //GameObject.Destroy(bullet.gameObject);
    }

    public void RemoveBullet(string bulletName)
    {
        Bullet target = null;
        foreach(Bullet bullet in m_bulletList)
        {
            if(bullet != null)
            {
                if(bullet.BULLET_NAME == bulletName)
                {
                    target = bullet;
                    break;
                }
            }
        }

        if(target != null)
        {
            RemoveBullet(target);
        }
    }
    
}
