using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using WebSocketSharp;

public class NetworkManager : Singletone<NetworkManager> {


    // -- 기본 정보 -----------------------------------------//

    private string m_serverURL = "localhost:8080";

    [SerializeField]
    private RestController m_rest = null;
    [SerializeField]
    private WebSocketController m_socket = null;

    //-------------------------------------------------------//

    
    //-- REST ------------------------------------------------------------------------------------------------//
    public void Login(string id,string pwd, GameObject target, string targetFunc)
    {
        if (m_rest != null)
            m_rest.Login(id, pwd, target, targetFunc);
    }

    // WebSocket ---------------------------------------------------------------------------------------------//
    public void SetupWebSocket(GameObject chatRecvObj, EventHandler<MessageEventArgs> chatRecvFunc)
    {
        m_socket.Setup(m_serverURL, chatRecvObj, chatRecvFunc);
    }

    public void SendChatMessage(string message)
    {
        m_socket.SendChatMessage(message);
    }

}
