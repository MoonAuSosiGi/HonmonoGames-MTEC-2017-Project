using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkOrderController : MonoBehaviour,NetworkManager.NetworkMessageEventListenrer {

    private static string m_orderName;
    private static short m_orderSpace;


    public static short ORDER_SPACE
    {
        get { return m_orderSpace; }
        set { m_orderSpace = value; }
    }
    public static string ORDER_NAME
    {
        get { return m_orderName; }
        set { m_orderName = value; }
    }
    
    

   
    void Start () {
        NetworkManager.Instance().AddNetworkMessageEventListener(this);
        Invoke("test", 15.0f);
       
	}
	void test()
    {
        
        //NetworkManager.Instance().PushChatMessage(JsonUtility.ToJson(e));
        NetworkManager.Instance().SendNetworkMessage(JSONMessageTool.ToJsonCreateOrder("asdtz","boss1"));
    }
    // Update is called once per frame
    void Update () {
        int i = 0;
		if(i > 8)
        {

        }
	}

    //ORDER 타겟 변경
    // msg orderName 이름 / orderTarget 어떤 내용의 오더를 내리는지
    // 우주 / 로봇안 / 행성   - space - robot - planet   z값 0  / 1  / 2로 구별
    // TODO 
    // ORDER    
    void NetworkManager.NetworkMessageEventListenrer.ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {

        if (e.msgType == NetworkManager.CHAT)
            return;


        switch(e.msgType)
        {
            case NetworkManager.CREATE: // 생성해라

                switch(e.msg.GetField(NetworkManager.CREATE_TARGET).str)
                {
                    case "boss1_bullet":
                        break;
                    case "myTeam_bullet":
                        if (e.targetName.IndexOf(GameManager.Instance().PLAYER.USER_NAME) >= 0)
                            return;

                       GameObject obj =  BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_HERO_DEF);
                        Bullet bullet = obj.GetComponent<Bullet>();
                        MDebug.Log("Target " + e.targetName);
                        bullet.SetupBullet(e.targetName,true);

                        Vector3 pos = new Vector3(e.msg.GetField("X").f, e.msg.GetField("Y").f, e.msg.GetField("Z").f);
                        bullet.GetComponent<SpriteRenderer>().flipX = e.msg.GetField(NetworkManager.DIR).b;

                        bullet.transform.position = new Vector3(pos.x, pos.y);
                        //총알을 생성하고 좌표 및 로테이션 처리를 해주고 업데이트는 Recvieve 에서
                        break;
                    case "boss1":
                        GameObject boss = MapManager.Instance().AddObject(GamePath.BOSS1);

                        if(ORDER_NAME == GameManager.Instance().PLAYER.USER_NAME
                            && ORDER_SPACE == 0)
                        {

                        }
                        else
                        {
                            NetworkMoving moving = boss.AddComponent<NetworkMoving>();
                            moving.NAME = e.targetName;
                        }                        

                        
                        break;
                }

                break;
            case NetworkManager.CH_ORIGINUSER: // 조작자를 바꿔라
                ORDER_NAME = e.msg.GetField("order").str;
                ORDER_SPACE = (short)e.msg.GetField("order_space").i;
                MDebug.Log(GameManager.Instance().ROBO == null);
                if (ORDER_NAME == GameManager.Instance().PLAYER.USER_NAME && ORDER_SPACE == 0)
                {
                    HeroRobo robo = GameManager.Instance().ROBO;
                    robo.ROBO_ISPLAYER = true;
                    robo.GetComponent<NetworkMoving>().NAME = ORDER_NAME + "_robo";
                    MDebug.Log("여긴 오면안돼");
                }
                else
                {
                    HeroRobo robo = GameManager.Instance().ROBO;

                    
                    robo.ROBO_ISPLAYER = false;
                    robo.GetComponent<NetworkMoving>().NAME = ORDER_NAME + "_robo";
                }
                MDebug.Log("조작자 바꿔라 " + ORDER_NAME + " " +GameManager.Instance().PLAYER.USER_NAME);
                break;

            case NetworkManager.CH_ORIGINUSER_REQ:
                MDebug.Log("뭔가 왔네");
                if(ORDER_NAME == GameManager.Instance().PLAYER.USER_NAME)
                {
                    MDebug.Log("전~~~송~~!!!");
                    NetworkManager.Instance().SendNetworkMessage(JSONMessageTool.ToJsonOrderChange(ORDER_NAME, 0));
                }
                break;
            case NetworkManager.REMOVE: // 지워라
                string removeTarget = e.targetName;

                break;
            case NetworkManager.ANIMATION: // 애니메이션 재생해라
                string aniTarget = e.targetName;
                break;
        }
    }

}
