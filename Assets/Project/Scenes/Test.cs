using UnityEngine;
using System.Collections;
using System.Net;
using WebSocketSharp;

public class Test : MonoBehaviour {

    WebRequest wr;
    // 기본 세팅 ---------------------------------------------------//
    private string m_url = "localhost:8080";
    private WebSocket m_socket = null;

    //--------------------------------------------------------------//

    // Use this for initialization
    void Start () {
        StartCoroutine(IntoServer());

    }
	
	// Update is called once per frame
	void Update () {
        Debug.Log(m_socket.IsAlive);
    }

    void TestOnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
    }

    IEnumerator IntoServer()
    {
        m_socket = new WebSocket("ws://" + m_url + "/echo");
        m_socket.Connect();
        Debug.Log(m_socket.IsAlive);
        m_socket.Send("TESTTEST");
        yield return null;
    }
}
