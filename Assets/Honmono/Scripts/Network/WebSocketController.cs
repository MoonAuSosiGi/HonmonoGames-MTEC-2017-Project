using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using WebSocketSharp;

public class WebSocketController : MonoBehaviour
{

    // 기본 세팅 ---------------------------------------------------//
    private string m_url = "localhost:8080";
    private WebSocket m_socket = null;
    private WebSocket m_socketM = null;
    //--------------------------------------------------------------//

    // 채팅용
    EventHandler<MessageEventArgs> m_chatRecv = null;
    // 이동용
    EventHandler<MessageEventArgs> m_moveRecv = null;

    // -------------------------------------------------------------//


    public void SetupChat(string url)
    {
       // m_target = targetObj;
        m_socket = new WebSocket("ws://" + m_url + "/echo");
        m_socket.OnMessage += RecieveMessage;
        m_socket.Connect();

        MDebug.Log("# WebSocket Server Connect ----------------------------------");
    }

    public void SetupMove(string url)
    {
        m_socketM = new WebSocket("ws://" + m_url + "/move");
        m_socketM.OnMessage += ReceieveMoveMessage;
        m_socketM.Connect();
    }

    public void SetChatReceiveMessage(EventHandler<MessageEventArgs> targetFunc)
    {
        m_chatRecv += targetFunc;
    }

    public void SetMoveReceiveMessage(EventHandler<MessageEventArgs> targetFunc)
    {
        m_moveRecv += targetFunc;
    }

    public void SendChatMessage(string message)
    { 
        if(m_socket != null && m_socket.IsAlive)
        {
            m_socket.Send(message);
            MDebug.Log("# WebSocket Client Send : " + message);
        }
    }

    public void SendMovePos(float x,float y)
    {
        if(m_socketM != null && m_socketM.IsAlive)
        {
            
        }
    }


    void RecieveMessage(object sender, MessageEventArgs e)
    {
        // TODO Message 분기
        m_chatRecv(sender, e);
        
    }

    void ReceieveMoveMessage(object sender, MessageEventArgs e)
    {
        m_moveRecv(sender, e);
    }
}