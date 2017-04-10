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
        public EnemyMove(string n,float x,float y,float z,bool dir)
        {
            name = n;
            this.x = x; this.y = y; this.z = z;
            Dir = dir;
        }
    }

    class Order
    {
        public string order;
        public int order_space;
    }
   

    // -----------------------------------------------------------------------------------------------//
    public static string ToJsoinEnemyMove(string name,float x,float y,float z, bool dir)
    {
        EnemyMove e = new EnemyMove(name,x,y,z,dir);
        return JsonUtility.ToJson(e);
    }


    public static string ToJsonMove(string name,float x, float y,float z, bool dir)
    {
        UserMove m = new UserMove(name, x, y, z, dir);
        return JsonUtility.ToJson(m);
        //return "{" + ToJsonKeyValue(NetworkManager.USERNAME, GameManager.Instance().PLAYER.USER_NAME) + ","
        //           + ToJsonKeyValue("x", x) + "," + ToJsonKeyValue("y", y) + ","+ ToJsonKeyValue("z", 0.0f) + ","
        //           + ToJsonKeyValue(NetworkManager.DIR, dir) + "}";
    }

    public static string ToJsonOrderChange(string order, int order_space)
    {
        JSONObject obj = GetDefJSON("",NetworkManager.CH_ORIGINUSER);
       
        JSONObject obj2 = new JSONObject();
        obj2.AddField("order", order);
        obj2.AddField("order_space", order_space);

        obj.AddField(NetworkManager.MSG, obj2);

       // MDebug.Log(JsonUtility.ToJson(msg));
        return obj.ToString();
    }

    public static string ToJsonOrderRequest(string order,int order_space)
    {
        JSONObject obj = GetDefJSON("", NetworkManager.CH_ORIGINUSER_REQ);

        JSONObject obj2 = new JSONObject();
        obj2.AddField("order", order);
        obj2.AddField("order_space", order_space);

        obj.AddField(NetworkManager.MSG, obj2);

        // MDebug.Log(JsonUtility.ToJson(msg));
        return obj.ToString();
    }

    public static string ToJsonCreateOrder(string targetName,string createObj)
    {
        JSONObject obj = GetDefJSON(targetName,NetworkManager.CREATE);

        JSONObject obj2 = new JSONObject();
        obj2.AddField(NetworkManager.CREATE_TARGET, createObj);
        obj.AddField(NetworkManager.MSG, obj2);
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
        obj.AddField(NetworkManager.MSG, obj2);
        
        return obj.ToString();
    }

    public static string ToJsonChat(string chatMessage)
    {
        return "{" + ToJsonKeyValue(NetworkManager.MSGTYPE, NetworkManager.CHAT)                    +  ","
                   + ToJsonKeyValue(NetworkManager.USERNAME, GameManager.Instance().PLAYER.USER_NAME)   +  ","
                   + ToJsonKeyValue(NetworkManager.MSG, chatMessage) + "}";
    }

    private static JSONObject GetDefJSON(string targetName,string type)
    {
        JSONObject obj = new JSONObject();
        obj.AddField(NetworkManager.MSGTYPE, type);
        obj.AddField(NetworkManager.USERNAME, GameManager.Instance().PLAYER.PLAYER_HERO.USERNAME);
        obj.AddField(NetworkManager.TARGETNAME, targetName);
        return obj;
    }

    // JSON Key-Value "key":"value"
    private static string ToJsonKeyValue(string key,string value)
    {
        return "\"" + key + "\":\"" + value + "\"";
    }

    private static string ToJsonKeyValue(string key, bool value)
    {
        string d = (value) ? "true" : "false";
        return "\"" + key + "\":" + d;
    }

    // JSON Key-Value "key":3.0
    private static string ToJsonKeyValue(string key, float value)
    {
        return "\"" + key + "\":" + value;
    }
}
