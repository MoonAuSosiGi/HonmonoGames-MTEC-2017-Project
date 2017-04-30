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
    private List<Bullet> m_bulletList = new List<Bullet>();
    //----------------------------------//

    public GameObject AddBullet(BULLET_TYPE type)
    {
        string path = null;

        switch(type)
        {
            case BULLET_TYPE.B_HERO_DEF: path = GamePath.WEAPON_BULLET_DEF; break;
            case BULLET_TYPE.B_BOSS1_P1: path = GamePath.WEAPON_BULLET_BOSS; break;
        }

        if (path == null)
            return null;

     
        GameObject obj = Resources.Load(path) as GameObject;
        GameObject bullet = GameObject.Instantiate(obj);
        bullet.transform.parent = transform;

        // TODO TempCode...!
        //bullet.GetComponent<SpriteRenderer>().flipX = right;
        
        m_bulletList.Add(bullet.GetComponent<Bullet>());

        return bullet;
    }

    public void RemoveBullet(Bullet bullet)
    {
        this.m_bulletList.Remove(bullet);
        NetworkManager.Instance().RemoveNetworkEnemyMoveEventListener(bullet);
        GameObject.Destroy(bullet.gameObject);
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
