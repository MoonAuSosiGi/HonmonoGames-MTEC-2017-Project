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
    public void SetupWebSocket()
    {
        m_socket.SetupChat(m_serverURL);
    }
    public void SetupWebSocketMove()
    {
        m_socket.SetupMove(m_serverURL);
    }

    public void SetChatRecv(EventHandler<MessageEventArgs> chatRecvFunc)
    {
        m_socket.SetChatReceiveMessage(chatRecvFunc);
    }

    public void SetMoveRecv(EventHandler<MessageEventArgs> moveRecvFunc)
    {
        m_socket.SetMoveReceiveMessage(moveRecvFunc);
    }

    public void SendChatMessage(string message)
    {
        m_socket.SendChatMessage(message);
    }

}
