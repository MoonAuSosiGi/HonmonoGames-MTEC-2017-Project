using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JSONMessageTool  {

    // -- Json 변환용 클래스 --------------------------------------------------------------------------//

    class Msg
    {
        public string UserName = null;
        public string targetName = null;
        public string msgType = null;
        public object msg = null;

    }

    //메시지 정의 chat / actionTo / 

    class UserMove
    {
        public string UserName;
        public float x, y, z;
        public bool Dir;
        public UserMove(string n,float x,float y, float z,bool dir)
        {
            UserName = n;
            this.x = x; this.y = y; this.z = z;
            Dir = dir;
        }
    }

    class EnemyMove
    {
        public string name;
        public float x, y, z;
        public bool Dir;
        public string Status;
        public EnemyMove(string n,float x,float y,float z,bool dir,string status)
        {
            name = n;
            this.x = x; this.y = y; this.z = z;
            Dir = dir;
            this.Status = status;
        }
    }

    // -----------------------------------------------------------------------------------------------//

    public static string ToJsoinEnemyMove(
        string name,float x,float y,float z, bool dir,string status=null)
    {
        EnemyMove e = new EnemyMove(name,x,y,z,dir,status);
        return JsonUtility.ToJson(e);
    }

    // 이동
    public static string ToJsonInTheStar(bool enter)
    {
        JSONObject obj = GetDefJSON("" , NetworkManager.INTHE_STAR);
        obj.GetField(NetworkManager.ORDERS)[0].GetField(NetworkManager.MSG)
            .AddField(NetworkManager.INTHE_STAR , enter);
        return obj.ToString();
    }

    //boss scene
    public static string ToJSonChangeBossScene()
    {
        JSONObject obj = GetDefJSON("" , NetworkManager.BOSS_SCENE_MOVE);
        return obj.ToString();
    }


    public static string ToJsonMove(string name , float x , float y , float z , bool dir , Vector3 vec)
    {
        JSONObject obj = new JSONObject();
        obj.AddField(NetworkManager.USERNAME , name);
        obj.AddField("X" , x);
        obj.AddField("Y" , y);
        obj.AddField("Z" , z);
        obj.AddField(NetworkManager.DIR , dir);

        JSONObject dirObj = new JSONObject();
        dirObj.AddField("X" , vec.x);
        dirObj.AddField("Y" , vec.y);
        dirObj.AddField("Z" , vec.z);

        obj.AddField(NetworkManager.DIRVECTOR , dirObj);
        return obj.ToString();
    }

    // Animation 동기화용
    public static string ToJsonAIMessageAnimation(string patternName,string aniName,int index,bool loop)
    {
        JSONObject obj = GetDefJSON("" , NetworkManager.AI);
        JSONObject msg = obj.GetField(NetworkManager.ORDERS)[0].GetField(NetworkManager.MSG);
        msg.AddField(NetworkManager.AI_PATTERN_NAME , patternName);
        msg.AddField(NetworkManager.AI_ANI_NAME , aniName);
        msg.AddField(NetworkManager.AI_ANI_INDEX , index);
        msg.AddField(NetworkManager.AI_ANI_LOOP , loop);
        
        return obj.ToString();
    }

    public static string ToJsonAIMessageCStart()
    {
        JSONObject obj = GetDefJSON("" , NetworkManager.AI_C);
        JSONObject msg = obj.GetField(NetworkManager.ORDERS)[0].GetField(NetworkManager.MSG);
        msg.AddField(NetworkManager.AI_PATTERN_NAME , "C");

        return obj.ToString();
    }



    public static string ToJsonAIMessageD_Rotate(float rotate,Vector3 pos)
    {
        JSONObject obj = GetDefJSON("" , NetworkManager.AI_D_ROTATE);
        JSONObject msg = obj.GetField(NetworkManager.ORDERS)[0].GetField(NetworkManager.MSG);
        msg.AddField(NetworkManager.AI_PATTERN_NAME , "D");
        msg.AddField(NetworkManager.AI_D_ROTATE , rotate);
        msg.AddField("X" , pos.x);
        msg.AddField("Y" , pos.y);
        return obj.ToString();
    }

    // CLASER
    public static string ToJSonAIMessageC_LASER(bool laserEnable)
    {
        JSONObject obj = GetDefJSON("" , NetworkManager.AI_C_LASER);
        obj.GetField(NetworkManager.ORDERS)[0].GetField(NetworkManager.MSG).AddField(NetworkManager.AI_C_LASER , laserEnable);
        return obj.ToString();
    }

    // D effect
    public static string ToJsonAIMessageD_END()
    {
        JSONObject obj = GetDefJSON("" , NetworkManager.AI_D_END);
        return obj.ToString();
    }

    // Place Change
    public static string ToJsonOrderPlaceChange(string place)
    {
        JSONObject obj = GetDefJSON("" , NetworkManager.PLACE_CHANGE);
        obj.GetField(NetworkManager.ORDERS)[0].GetField(NetworkManager.MSG)
            .AddField(NetworkManager.PLACE_CHANGE , place);
        return obj.ToString();
    }

    // 첫 접속 후 캐릭터 생성해라!
    public static string ToJsonOrderUserCrateCharacter(string targetName)
    {
        JSONObject obj = GetDefJSON(targetName, NetworkManager.USER_CHARACTER_CREATE);
        return obj.ToString();
    }


    //게임 스타트
    public static string ToJsonOrderGameSatart()
    {
        JSONObject obj = GetDefJSON("", NetworkManager.GAME_START);
        return obj.ToString();
    }

    // 로봇 조종자
    public static string ToJsonOrderRobotSetting(bool enter = true)
    {
        JSONObject obj = GetDefJSON("", NetworkManager.ROBOT_DRIVER);
        obj.GetField(NetworkManager.ORDERS)[0]
            .GetField(NetworkManager.MSG).AddField(NetworkManager.ROBOT_DRIVER, enter);
        return obj.ToString();
    }

    // 로봇 총 조종자
    public static string ToJsonOrderRobotGunSetting(bool enter = true)
    {
        JSONObject obj = GetDefJSON("", NetworkManager.ROBOT_GUNNER);
        obj.GetField(NetworkManager.ORDERS)[0]
            .GetField(NetworkManager.MSG).AddField(NetworkManager.ROBOT_GUNNER, enter);
        return obj.ToString();
    }

    // 총 각도 전송
    public static string ToJsonOrderGunAngle(string targetName,float angle)
    {
        JSONObject obj = GetDefJSON(targetName, NetworkManager.GUN_ANGLE_CHANGE);
        JSONObject obj2 = obj.GetField(NetworkManager.ORDERS)[0]
            .GetField(NetworkManager.MSG);
        
        obj.GetField(NetworkManager.ORDERS)[0]
            .GetField(NetworkManager.MSG).AddField(NetworkManager.GUN_ANGLE_CHANGE, angle);
        return obj.ToString();
    }

    // 상태값 전송 - 주로 int 형 상태값
    public static string ToJsonOrderStateValueChange(string targetName, int val)
    {
        JSONObject obj = GetDefJSON(targetName, NetworkManager.STATE_CHANGE);
        obj.GetField(NetworkManager.ORDERS)[0]
            .GetField(NetworkManager.MSG).AddField(NetworkManager.STATE_CHANGE, val);
        return obj.ToString();
    }

    //접속
    public static string ToJsonOrderUserEnter(int userIndex,PlayerInfo.PLAYER_STATUS status,string name,bool ready)
    {
        JSONObject obj = GetDefJSON("", NetworkManager.USER_CONNECT);
        JSONObject obj2 = new JSONObject();
        obj2.AddField(NetworkManager.USER_CONNECT, userIndex);
        obj2.AddField(NetworkManager.STATUS_SPEED, status.STAT_SPEED);
        obj2.AddField(NetworkManager.STATUS_POWER, status.STAT_POWER);
        obj2.AddField(NetworkManager.STATUS_REPAIR, status.STAT_REPAIR);
        obj2.AddField(NetworkManager.CLIENT_ID, name);
        obj2.AddField(NetworkManager.READY_STATE, ready);

        obj.GetField(NetworkManager.ORDERS)[0]
       .GetField(NetworkManager.MSG)
       .AddField(NetworkManager.USER_CONNECT, obj2);

        return obj.ToString();
    }

    //정보 요청
    public static string ToJsonOrderUserInfoReq()
    {
        JSONObject obj = GetDefJSON("", NetworkManager.USER_INFO_REQ);
       
        return obj.ToString();
    }

    //레디
    public static string ToJsonOrderUserReady(bool r,int index)
    {
        JSONObject obj = GetDefJSON(GameManager.Instance().PLAYER.USER_NAME, NetworkManager.USER_READY);
        JSONObject obj2 = new JSONObject();
        obj2.AddField(NetworkManager.USER_READY, r);
        obj2.AddField(NetworkManager.USER_INDEX, index);
        obj.GetField(NetworkManager.ORDERS)[0]
      .GetField(NetworkManager.MSG)
      .AddField(NetworkManager.USER_READY, obj2);
        return obj.ToString();
    }

    //접속 끊기 TODO
    public static string ToJsonOrderUserLogOut(int userIndex, PlayerInfo.PLAYER_STATUS status, string name,bool ready = false)
    {
        JSONObject obj = GetDefJSON("", NetworkManager.USER_LOGOUT);
        JSONObject obj2 = new JSONObject();
        obj2.AddField(NetworkManager.USER_LOGOUT, userIndex);
        obj2.AddField(NetworkManager.STATUS_SPEED, status.STAT_SPEED);
        obj2.AddField(NetworkManager.STATUS_POWER, status.STAT_POWER);
        obj2.AddField(NetworkManager.STATUS_REPAIR, status.STAT_REPAIR);
        obj2.AddField(NetworkManager.CLIENT_ID, name);
        obj2.AddField(NetworkManager.READY_STATE, ready);
        obj2.AddField(NetworkManager.USER_INDEX, GameManager.Instance().PLAYER.NETWORK_INDEX);

        obj.GetField(NetworkManager.ORDERS)[0]
       .GetField(NetworkManager.MSG)
       .AddField(NetworkManager.USER_LOGOUT, obj2);

        return obj.ToString();
    }

    //충돌을 알려야할때
    public static string ToJsonOrderCrash(string name1,string name2)
    {
        JSONObject obj = GetDefJSON(GameManager.Instance().PLAYER.USER_NAME, NetworkManager.CRASH);
        JSONObject obj2 = new JSONObject();

        obj2.AddField(NetworkManager.CRASH_NAME1, name1);
        obj2.AddField(NetworkManager.CRASH_NAME2, name2);

        obj.GetField(NetworkManager.ORDERS)[0]
            .GetField(NetworkManager.MSG)
            .AddField(NetworkManager.CRASH, obj2);

        return obj.ToString();
    }

    public static string ToJsonOrderChange(string order, int order_space)
    {
        JSONObject obj = GetDefJSON("",NetworkManager.CH_ORIGINUSER);
       
        JSONObject obj2 = new JSONObject();
        obj2.AddField("order", order);
        obj2.AddField("order_space", order_space);

        obj.GetField(NetworkManager.ORDERS)[0]
       .GetField(NetworkManager.MSG)
       .AddField(NetworkManager.ORDER, obj2);
        

       // MDebug.Log(JsonUtility.ToJson(msg));
        return obj.ToString();
    }
    

    public static string ToJsonOrderRequest(string order,int order_space)
    {
        JSONObject obj = GetDefJSON("", NetworkManager.CH_ORIGINUSER_REQ);

        JSONObject obj2 = new JSONObject();
        obj2.AddField("order", order);
        obj2.AddField("order_space", order_space);

        obj.GetField(NetworkManager.ORDERS)[0]
       .GetField(NetworkManager.MSG)
       .AddField(NetworkManager.ORDER, obj2);
        // MDebug.Log(JsonUtility.ToJson(msg));
        return obj.ToString();
    }

    public static string ToJsonCreateOrder(string targetName,string createObj)
    {
        JSONObject obj = GetDefJSON(targetName,NetworkManager.CREATE);

        JSONObject obj2 = new JSONObject();
        obj2.AddField(NetworkManager.CREATE_TARGET, createObj);

        obj.GetField(NetworkManager.ORDERS)[0]
           .GetField(NetworkManager.MSG)
           .AddField(NetworkManager.CREATE, obj2);
        return obj.ToString();
    }

    public static string ToJsonCreateOrder(string targetName, string createObj,float x,float y,float z,bool dir)
    {
        JSONObject obj = GetDefJSON(targetName, NetworkManager.CREATE);

        JSONObject obj2 = new JSONObject();
        obj2.AddField(NetworkManager.CREATE_TARGET, createObj);
        obj2.AddField("X", x);
        obj2.AddField("Y", y);
        obj2.AddField("Z", z);
        obj2.AddField(NetworkManager.DIR, dir);

        obj.GetField(NetworkManager.ORDERS)[0]
            .GetField(NetworkManager.MSG)
            .AddField(NetworkManager.CREATE, obj2);
       return obj.ToString();
    }

    public static string ToJsonRemoveOrder(string targetName,string removeObjName)
    {
        JSONObject obj = GetDefJSON(targetName, NetworkManager.REMOVE);

        obj.GetField(NetworkManager.ORDERS)[0]
            .GetField(NetworkManager.MSG)
            .AddField(NetworkManager.REMOVE, removeObjName);

        return obj.ToString();
    }
        public static string ToJsonChat(string chatMessage)
    {
        JSONObject obj = GetDefJSON("", NetworkManager.CHAT);
        obj.GetField(NetworkManager.ORDERS)[0]
            .GetField(NetworkManager.MSG)
            .AddField(NetworkManager.USERNAME, GameManager.Instance().PLAYER.USER_NAME);

        obj.GetField(NetworkManager.ORDERS)[0]
            .GetField(NetworkManager.MSG)
            .AddField(NetworkManager.MSG, chatMessage);

        return obj.ToString();
    }

    /*
     * {
	        "clientid": "",
	        "timestamp": "",
	        "orders": [{
		        "order": "",
		        "msg": {}
	        }]
        }
     */
    private static JSONObject GetDefJSON(string targetName,string type)
    {
        JSONObject obj = new JSONObject();
        obj.AddField(NetworkManager.TIMESTAMP, "test");
        obj.AddField(NetworkManager.CLIENT_ID, GameManager.Instance().PLAYER.USER_NAME);

        JSONObject orders = new JSONObject();

        JSONObject temp = new JSONObject();
        temp.AddField(NetworkManager.ORDER, type);
        
        JSONObject msg = new JSONObject();
        msg.AddField(NetworkManager.TARGETNAME, targetName);
        temp.AddField(NetworkManager.MSG, msg);

        orders.Add(temp);
        obj.AddField(NetworkManager.ORDERS, orders);

        return obj;
    }

}
