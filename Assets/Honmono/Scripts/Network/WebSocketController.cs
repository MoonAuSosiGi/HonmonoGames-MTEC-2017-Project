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
    private GameObject m_target = null;
    private string m_targetFunc = null;
    //--------------------------------------------------------------//


    // -------------------------------------------------------------//


    public void Setup(string url,GameObject targetObj, EventHandler<MessageEventArgs> targetFunc)
    {
        m_target = targetObj;
        m_socket = new WebSocket("ws://" + m_url + "/echo");
        m_socket.OnMessage += targetFunc;
        m_socket.Connect();

        MDebug.Log("# WebSocket Server Connect ----------------------------------");
    }

    public void SendChatMessage(string message)
    {
        if(m_socket != null && m_socket.IsAlive)
        {
            m_socket.Send(message);
            MDebug.Log("# WebSocket Client Send : " + message);
        }
    }


    void RecieveMessage(object sender, MessageEventArgs e)
    {
        if (m_target != null)
            m_target.SendMessage(m_targetFunc, e.Data);

    }
}