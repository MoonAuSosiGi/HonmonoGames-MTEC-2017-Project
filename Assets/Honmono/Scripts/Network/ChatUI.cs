﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;
using UnityEngine.UI;
using System;

public class ChatUI : MonoBehaviour, NetworkManager.NetworkMessageEventListenrer
{
    // -- Chat UI -----------------------------------------------------------------//
    // 실제로 채팅이 보여질 부분
    [SerializeField]
    private List<Text> m_messages = new List<Text>();
    // 채팅이 입력되는 부분
    [SerializeField]
    private InputField m_inputField = null;

    // 실제 메시지들이 저장될 리스트
    private List<string> m_messageStrs = new List<string>();

    // ---------------------------------------------------------------------------//

    void Start()
    {
        NetworkManager.Instance().AddNetworkMessageEventListener(this);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            m_inputField.Select();
        }

        if (m_messageStrs.Count > 0)
        {
            for (int i = 1; i < m_messages.Count; i++)
            {
                m_messages[m_messages.Count - i].text = m_messages[m_messages.Count - i - 1].text;
            }
            m_messages[0].text = m_messageStrs[0];
            m_messageStrs.RemoveAt(0);
        }

    }

    public void SendMessageToServer()
    {
        NetworkManager.Instance().SendNetworkMessage(JSONMessageTool.ToJsonChat(m_inputField.text));
    //    m_messageStrs.Add(m_inputField.text);
        m_inputField.Select();
        m_inputField.text = "";
    }

    void ReceiveMessage(object sender, MessageEventArgs e)
    {
        m_messageStrs.Add(e.Data);
    }

    void NetworkManager.NetworkMessageEventListenrer.ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        
        if ( e.msgType != NetworkManager.CHAT)
            return;

        m_messageStrs.Add(e.user + " : " + e.msg.GetField(NetworkManager.MSG));
    }
}
