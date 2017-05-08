using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {


    // TEST 용 코드 ( 추후 Hero Robo 에 합쳐짐 ) 


    int index = 0;

    void Update()
    {
        //MDebug.Log("BULLET");
        float angle = 0.0f;
        if (Input.GetKey(KeyCode.U))
        {
            angle = - 0.5f;
            transform.Rotate(new Vector3(0, 0, angle), Space.World);
        }

        if (Input.GetKey(KeyCode.J))
        {
            angle = 0.5f;
            transform.Rotate(new Vector3(0, 0, angle), Space.World);
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            Bullet b = BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_HERO_DEF);
            Vector3 pos = transform.position;
            //float width = (m_renderer.flipX) ? m_renderer.bounds.size.x / 2.0f : -m_renderer.bounds.size.x / 2.0f;
            b.transform.position = new Vector3(pos.x, pos.y, pos.z);

            b.transform.rotation = transform.rotation;

            string n = GameManager.Instance().PLAYER.USER_NAME + "_" + index;
            b.SetupBullet(n,false,Vector3.zero);
            //MDebug.Log("b " + n + "     " + JSONMessageTool.ToJsonCreateOrder(n, "myTeam_bullet", pos.x, pos.y, pos.z, bullet.GetComponent<SpriteRenderer>().flipX));
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonCreateOrder(n, "myTeam_bullet",pos.x,pos.y,pos.z,b.GetComponent<SpriteRenderer>().flipX));
            index++;
        }

    }

}
