using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : Singletone<BulletManager> {

    public enum BULLET_TYPE
    {
        B_HERO_DEF = 0  // 주인공의 디폴트 총알
    }

    //-----------------------------------//
    // 총알 리스트ㅡ
    private List<Bullet> m_bulletList = new List<Bullet>();
    //----------------------------------//

    public GameObject AddBullet(BULLET_TYPE type,bool right)
    {
        string path = null;

        switch(type)
        {
            case BULLET_TYPE.B_HERO_DEF: path = GamePath.WEAPON_BULLET_DEF; break;
        }

        if (path == null)
            return null;

     
        GameObject obj = Resources.Load(path) as GameObject;
        GameObject bullet = GameObject.Instantiate(obj);
        bullet.transform.parent = transform;

        // TODO TempCode...!
        bullet.GetComponent<SpriteRenderer>().flipX = right;
        
        m_bulletList.Add(bullet.GetComponent<Bullet>());

        return bullet;
    }

    public void RemoveBullet(Bullet bullet)
    {
        this.m_bulletList.Remove(bullet);
        GameObject.Destroy(bullet.gameObject);
    }
    
}
