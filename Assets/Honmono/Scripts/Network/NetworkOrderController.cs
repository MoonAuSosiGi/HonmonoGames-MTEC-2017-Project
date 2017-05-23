using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkOrderController : MonoBehaviour,NetworkManager.NetworkMessageEventListenrer {

    private static string m_orderName;
    private static short m_orderSpace;

    public enum AreaInfo
    {
        AREA_SPACE = 0,
        AREA_ROBOT = 1
    }

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
    
    

   
    void Awake () {
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
        
       
	}


    //ORDER 타겟 변경
    // msg orderName 이름 / orderTarget 어떤 내용의 오더를 내리는지
    // 우주 / 로봇안 / 행성   - space - robot - planet   z값 0  / 1  / 2로 구별
    // TODO 
    // ORDER    
    void NetworkManager.NetworkMessageEventListenrer.ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        
        switch (e.msgType)
        {
            case NetworkManager.PLACE_CHANGE:
                GameManager.Instance().CUR_PLACE = 
                    (GameManager.ROBO_PLACE)((int)e.msg.GetField(NetworkManager.PLACE_CHANGE).i);
                break;
            case NetworkManager.CREATE: // 생성해라

                switch (e.msg
                    .GetField(NetworkManager.CREATE)
                    .GetField(NetworkManager.CREATE_TARGET).str)
                {
                    case "effect":
                        {
                            if (e.user.Equals(GameManager.Instance().PLAYER.USER_NAME))
                                return;
                            JSONObject json = e.msg;
                            GameObject effect = MapManager.Instance().AddObject(
                                GamePath.EFFECT , new Vector3(json.GetField("X").f , json.GetField("Y").f , -1.0f));
                        }

                        break;

                    case "DamagePoint":
                        if (e.targetName.IndexOf(GameManager.Instance().PLAYER.USER_NAME) >= 0)
                            return;
                        JSONObject dpJson = e.msg;
                        GameObject dp = MapManager.Instance().AddObject(
                            GamePath.DAMAGE_POINT, new Vector3(
                                dpJson.GetField("X").f , dpJson.GetField("Y").f , -1.0f));
                        
                        RoboDamagePoint dps =  dp.GetComponent<RoboDamagePoint>();
                        dps.name = e.targetName;
                        dps.NETWORK_OBJECT = true;
                        break;
                    case "boss1_bullet":
                        {
                            if (e.targetName.IndexOf(GameManager.Instance().PLAYER.USER_NAME) >= 0)
                                return;

                            Bullet bullet = BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_BOSS1_P1);
                            //   MDebug.Log("Target " + e.targetName);  
                            bullet.SetupBullet(e.targetName , true , Vector3.zero);

                            JSONObject json = e.msg;

                            Vector3 pos = new Vector3(json.GetField("X").f , json.GetField("Y").f , -1.0f);
                            //bullet.GetComponent<SpriteRenderer>().flipX = e.orders.GetField(NetworkManager.MSG).GetField(NetworkManager.DIR).b;

                            bullet.transform.position = new Vector3(pos.x , pos.y);
                        }
                        break;
                    case "myTeam_bullet":
                        {
                            if (e.targetName.IndexOf(GameManager.Instance().PLAYER.USER_NAME) >= 0)
                                return;
                            Bullet bullet = BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_HERO_DEF);
                           
                            //   MDebug.Log("Target " + e.targetName);
                            bullet.SetupBullet(e.targetName , true , Vector3.zero);

                            JSONObject json = e.msg ;

                            Vector3 pos = new Vector3(json.GetField("X").f , json.GetField("Y").f , -1.0f);
                            //bullet.GetComponent<SpriteRenderer>().flipX = e.orders.GetField(NetworkManager.MSG).GetField(NetworkManager.DIR).b;

                            bullet.transform.position = new Vector3(pos.x , pos.y);
                            //총알을 생성하고 좌표 및 로테이션 처리를 해주고 업데이트는 Recvieve 에서
                        }
                        break;

                    case "boss1":
                        {
                            if (e.user.Equals(GameManager.Instance().PLAYER.USER_NAME))
                                return;
                            Vector3 p = new Vector3(e.msg.GetField("X").f , e.msg.GetField("Y").f , -1.0f);
                            GameObject boss = MapManager.Instance().AddObject(GamePath.BOSS1, new Vector3(p.x , p.y , -1));
                            boss.transform.eulerAngles = Vector3.zero;
                            boss.gameObject.AddComponent<NetworkMoving>().NAME = e.targetName;
                            boss.GetComponent<Stage1BOSS>().enabled = false;
                            boss.GetComponent<Stage1BOSS>().MONSTER_NAME = e.targetName;
                            boss.gameObject.AddComponent<NetworkStage1BOSS>().BOSS_NAME = e.targetName;
                            
                        }
                        break;
                    case "monster1":
                        {
                            if (e.user.Equals(GameManager.Instance().PLAYER.USER_NAME))
                                return;
                            
                            Vector3 p = new Vector3(e.msg.GetField("X").f , e.msg.GetField("Y").f , -1.0f);
                            GameObject m = MapManager.Instance().AddObject(GamePath.MONSTER1, new Vector3(p.x , p.y , -1));
                            
                            NetworkMoving moving = m.AddComponent<NetworkMoving>();
                            m.AddComponent<NetworkMonster>().NAME = e.targetName;
                            m.GetComponent<Stage1Monster>().enabled = false;
                            m.GetComponent<Stage1Monster>().MONSTER_NAME = e.targetName;
                            m.GetComponent<Stage1Monster>().NETWORKING = true;
                            moving.NAME = e.targetName;
                        }
                        break;
                    case "monster2":
                        {
                            if (e.user.Equals(GameManager.Instance().PLAYER.USER_NAME))
                                return;
                            Vector3 p = new Vector3(e.msg.GetField("X").f , e.msg.GetField("Y").f , -1.0f);
                            GameObject m = MapManager.Instance().AddObject(GamePath.MONSTER2, new Vector3(p.x , p.y , -1));
                            NetworkMoving moving = m.AddComponent<NetworkMoving>();
                            m.AddComponent<NetworkMonster>().NAME = e.targetName;
                            m.GetComponent<Stage1Monster>().MONSTER_NAME = e.targetName;
                            m.GetComponent<Stage1Monster>().enabled = false;
                            m.GetComponent<Stage1Monster>().NETWORKING = true;
                            moving.NAME = e.targetName;
                        }
                        break;
                }

                break;
            case NetworkManager.CH_ORIGINUSER: // 조작자를 바꿔라
                ORDER_NAME = e.msg.GetField(NetworkManager.ORDER).GetField(NetworkManager.ORDER).str;
                ORDER_SPACE = (short)e.msg.GetField(NetworkManager.ORDER).GetField("order_space").i;

                MDebug.Log("ORDER " + ORDER_NAME);

                MDebug.Log(GameManager.Instance().ROBO == null);

                if (ORDER_NAME == GameManager.Instance().PLAYER.USER_NAME && ORDER_SPACE == 0)
                {


                }
                else
                {
                }
                break;

            case NetworkManager.CH_ORIGINUSER_REQ:

                //호스트 요청이 왔으므로 호스트를 쏜다
                if (ORDER_NAME == GameManager.Instance().PLAYER.USER_NAME)
                {
                    NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderChange(ORDER_NAME, 0));
                }
                break;
            case NetworkManager.REMOVE: // 지워라
                string removeTarget = e.targetName;

                if (e.user == GameManager.Instance().PLAYER.USER_NAME)
                {
                    return;
                }
                switch (e.msg.GetField(NetworkManager.REMOVE).str)
                {
                    case "myTeam_bullet":
                        BulletManager.Instance().RemoveBullet(removeTarget,Bullet.BULLET_TARGET.PLAYER);
                        break;
                    case "boss1_bullet":
                        BulletManager.Instance().RemoveBullet(removeTarget , Bullet.BULLET_TARGET.ENEMY);
                        break;
                    case "Monster":
                        MapManager.Instance().RemoveObjectName(removeTarget);
                        break;
                }
                break;
            case NetworkManager.USER_LOGOUT:
                NetworkManager.Instance().LogOutUser(e.user);
                break;
            case NetworkManager.ROBOT_DRIVER: // 로봇 조종자
                if (e.msg.GetField(NetworkManager.ROBOT_DRIVER).b)
                    GameManager.Instance().ROBO.MOVE_PLYAER = e.user;
                else
                {
                    //if (e.user == GameManager.Instance().PLAYER.USER_NAME)
                    //    NetworkManager.Instance().GototheRobo();
                    GameManager.Instance().ROBO.MOVE_PLYAER = null;
                }
                //GameManager.Instance().ROBO.name = e.user + "_robot";
                break;
            case NetworkManager.ROBOT_GUNNER: // 로봇 총 쏘는사람
                if (e.msg.GetField(NetworkManager.ROBOT_GUNNER).b)
                    GameManager.Instance().ROBO.GUN_PLAYER = e.user;
                else
                {
                    if(e.user == GameManager.Instance().PLAYER.USER_NAME)
                        NetworkManager.Instance().GototheRobo();
                    GameManager.Instance().ROBO.GUN_PLAYER = null;
                }
                break;
            case NetworkManager.GAME_START:
                break;

            case NetworkManager.USER_CHARACTER_CREATE:
                NetworkManager.Instance().CreateUserCharacter(e.targetName);
                break;


        }
    }

}
