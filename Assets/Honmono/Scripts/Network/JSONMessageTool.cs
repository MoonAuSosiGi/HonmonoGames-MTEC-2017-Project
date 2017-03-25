using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JSONMessageTool  {


    public static string ToJsonMove(float x, float y,bool dir)
    {
        string d = (dir) ? "true" : "false";
        return "{\"" + NetworkManager.MSGTYPE + "\":\"" + NetworkManager.MOVE
            + "\",\"" + NetworkManager.USER + "\":\"" + GameManager.Instance().PLAYER.USER_NAME
            + "\",\"" + NetworkManager.MSG + "\":{\"x\":" + x + ",\"y\":" + y + ",\"dir\":"+d+"}}";
    }

    public static string ToJsonChat(string chatMessage)
    {
        return "{\"" + NetworkManager.MSGTYPE + "\":" + NetworkManager.CHAT
            + ",\"" + NetworkManager.USER + "\":" + GameManager.Instance().PLAYER.USER_NAME
            + ",\"" + NetworkManager.MSG + "\":\"" + chatMessage + "\"}";
    }

}
