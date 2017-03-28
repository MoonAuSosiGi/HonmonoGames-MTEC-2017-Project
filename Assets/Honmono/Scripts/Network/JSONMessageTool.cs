using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JSONMessageTool  {


    public static string ToJsonMove(float x, float y, bool dir)
    {
        return "{" + ToJsonKeyValue(NetworkManager.USERNAME, GameManager.Instance().PLAYER.USER_NAME) + ","
                   + ToJsonKeyValue("x", x) + "," + ToJsonKeyValue("y", y) + ","+ ToJsonKeyValue("z", 0.0f) + ","
                   + ToJsonKeyValue(NetworkManager.DIR, dir) + "}";
    }

    public static string ToJsonChat(string chatMessage)
    {
        return "{" + ToJsonKeyValue(NetworkManager.MSGTYPE, NetworkManager.CHAT)                    +  ","
                   + ToJsonKeyValue(NetworkManager.USERNAME, GameManager.Instance().PLAYER.USER_NAME)   +  ","
                   + ToJsonKeyValue(NetworkManager.MSG, chatMessage) + "}";
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
