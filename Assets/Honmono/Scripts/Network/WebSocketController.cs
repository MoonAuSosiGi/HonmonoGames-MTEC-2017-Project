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
    private WebSocket m_socketOrder = null;

    //-----------------------------------------------------------------------------------------------//

    // ---------------------------------------------------------------------------------------------//

    public void SetupSocket(string url)
    {
       // url = "172.20.10.5:8090";
        m_socket = new WebSocket("ws://" + url + "/echo");
        m_socket.OnError += ErrorMessage;
        m_socket.OnMessage += RecieveMessage;
        m_socket.OnOpen += OnOpen;


        m_socketMove = new WebSocket("ws://" + url + "/user/move");
        m_socketMove.OnOpen += OnOpen;
        m_socketMove.OnError += ErrorMessage;
        m_socketMove.OnMessage += RecieveMoveMessage;


        m_socketEnemy = new WebSocket("ws://" + url + "/enemy/move");
        m_socketEnemy.OnError += ErrorMessage;
        m_socketEnemy.OnOpen += OnOpen;

        m_socketEnemy.OnMessage += RecieveMoveEnemyMessage;

        m_socketOrder = new WebSocket("ws://" + url + "/order");
        m_socketOrder.OnMessage += RecieveOrderMessage;
        m_socketOrder.OnError += ErrorMessage;

        SocketConnect(m_socket);
        SocketConnect(m_socketMove);
        SocketConnect(m_socketEnemy);
        SocketConnect(m_socketOrder);

        MDebug.Log("# WebSocket Server Connect ----------------------------------");
    }

    void SocketConnect(WebSocket socket)
    {
        socket.Connect();
      //  yield return null;
    }

    public void SendChatMessage(string json)
    {
        if(m_socket != null && m_socket.ReadyState == WebSocketState.Open)
            m_socket.Send(json);
    }

    public void SendMoveMessage(string json)
    {
        if (m_socketMove != null && m_socketMove.ReadyState == WebSocketState.Open)
            m_socketMove.Send(json);
    }

    public void SendEnemyMoveMessage(string json)
    {
        if (m_socketEnemy != null && m_socketEnemy.ReadyState == WebSocketState.Open)
        {
            m_socketEnemy.Send(json);
        }
    }
    // 오더를 쫙 내린다
    public void SendOrderMessage(string json)
    {
        if(m_socketOrder != null && m_socketOrder.ReadyState == WebSocketState.Open)
        {
            m_socketOrder.Send(json);
        }
    }


    //-- 서버로부터 받은 메시지를 넣는다 -----------------------------------------------------------//
    void RecieveMessage(object sender, MessageEventArgs e)
    {
        // 메시지 큐에 메시지 푸시
        NetworkManager.Instance().PushChatMessage(e.Data);
    }

    // 이동 메시지
    void RecieveMoveMessage(object sender,MessageEventArgs e)
    {
        NetworkManager.Instance().PushMoveMessage(e.Data);
    }

    //적의 이동 메시지
    void RecieveMoveEnemyMessage(object sender,MessageEventArgs e)
    {
        NetworkManager.Instance().PushEnemyMoveMessage(e.Data);
    }

    //오더받은거
    void RecieveOrderMessage(object sender, MessageEventArgs e)
    {
        NetworkManager.Instance().PushOrderMessage(e.Data);
    }



    // 기타 
    void ErrorMessage(object sender, ErrorEventArgs e)
    {
        MDebug.Log(e.Message);
    }

    void OnOpen(object sender, EventArgs e)
    {   
    }

    public bool IsSocketAlived()
    {
        if (m_socket == null || m_socketEnemy == null || m_socketMove == null || m_socketOrder == null)
            return false;

        return m_socket.ReadyState == WebSocketState.Open && m_socketMove.ReadyState == WebSocketState.Open && m_socketEnemy.ReadyState == WebSocketState.Open && m_socketOrder.ReadyState == WebSocketState.Open;
    }
    void test()
    {
       
    }
}