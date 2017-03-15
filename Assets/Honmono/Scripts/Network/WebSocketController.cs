using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class WebSocketController : MonoBehaviour
{

    // 기본 세팅 ---------------------------------------------------//
    private string m_url = "localhost:8080";
    private WebSocket m_socket = null;
    private GameObject m_target = null;


    public OnReceive m_receive = null;
    //--------------------------------------------------------------//

    // Delegate ----------------------------------------------------//
    //메시지를 받을 델리게이트
    public delegate void OnReceive(object o, MessageEventArgs e);
    // -------------------------------------------------------------//


    public void Setup(string url, OnReceive recv)
    {
        m_socket = new WebSocket("ws://" + m_url + "/echo");
        m_socket.OnMessage += RecieveMessage;
        m_receive = recv;
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
        if (m_receive != null)
            m_receive(sender, e);

    }
}