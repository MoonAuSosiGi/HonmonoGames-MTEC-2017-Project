using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using WebSocketSharp;

public class WebSocketController : MonoBehaviour
{

    // 기본 세팅 ------------------------------------------------------------------------------------//
    private WebSocket m_socket = null;
    private WebSocket m_socketM = null;
    //-----------------------------------------------------------------------------------------------//

    // 채팅용
    EventHandler<MessageEventArgs> m_chatRecv = null;
    // 이동용
    EventHandler<MessageEventArgs> m_moveRecv = null;

    // ---------------------------------------------------------------------------------------------//

    public void SetupSocket(string url)
    {
        Application.runInBackground = true;
        MDebug.Log(Network.player.ipAddress);
        
        m_socket = new WebSocket("ws://" + url + "/echo");
        m_socket.OnError += ErrorMessage;
        m_socket.OnMessage += RecieveMessage;
        m_socket.Connect();
        MDebug.Log("# WebSocket Server Connect ----------------------------------");
    }

    public void SendNetworkMessage(string json)
    {
        m_socket.Send(json);
    }

    //-- 서버로부터 받은 메시지를 넣는다 -----------------------------------------------------------//
    void RecieveMessage(object sender, MessageEventArgs e)
    {
        // 메시지 큐에 메시지 푸시
        NetworkManager.Instance().PushSocketMessage(e.Data);
    }

    void ErrorMessage(object sender, ErrorEventArgs e)
    {
        MDebug.Log(e.Message);
    }
}