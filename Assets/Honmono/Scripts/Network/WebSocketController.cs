using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using WebSocketSharp;

public class WebSocketController : MonoBehaviour
{

    // 기본 세팅 ------------------------------------------------------------------------------------//
    private WebSocket m_socket = null;
    private WebSocket m_socketMove = null;
    private WebSocket m_socketEnemy = null;
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

        m_socketMove = new WebSocket("ws://" + url + "/user/move");
        m_socketMove.OnError += ErrorMessage;
        m_socketMove.OnMessage += RecieveMoveMessage;
        m_socketMove.Connect();

        m_socketEnemy = new WebSocket("ws://" + url + "/enemy/move");
        m_socketEnemy.OnError += ErrorMessage;
        m_socketEnemy.OnMessage += RecieveMoveEnemyMessage;
        m_socketEnemy.Connect();

        MDebug.Log("# WebSocket Server Connect ----------------------------------");
    }

    public void SendChatMessage(string json)
    {
        if(m_socket != null && m_socket.IsAlive)
            m_socket.Send(json);
    }

    public void SendMoveMessage(string json)
    {
        if (m_socketMove != null && m_socketMove.IsAlive)
            m_socketMove.Send(json);
    }

    public void SendEnemyMoveMessage(string json)
    {
        if (m_socketEnemy != null && m_socketEnemy.IsAlive)
            m_socketEnemy.Send(json);
    }

    //-- 서버로부터 받은 메시지를 넣는다 -----------------------------------------------------------//
    void RecieveMessage(object sender, MessageEventArgs e)
    {
        // 메시지 큐에 메시지 푸시
        NetworkManager.Instance().PushChatMessage(e.Data);
    }

    void RecieveMoveMessage(object sender,MessageEventArgs e)
    {
        MDebug.Log(e.Data);
        NetworkManager.Instance().PushMoveMessage(e.Data);
    }

    void RecieveMoveEnemyMessage(object sender,MessageEventArgs e)
    {
        MDebug.Log(e.Data);
        NetworkManager.Instance().PushEnemyMoveMessage(e.Data);
    }

    void ErrorMessage(object sender, ErrorEventArgs e)
    {
        MDebug.Log(e.Message);
    }

}