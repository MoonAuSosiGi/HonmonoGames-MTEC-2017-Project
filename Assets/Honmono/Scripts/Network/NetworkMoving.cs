using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMoving : MonoBehaviour, NetworkManager.NetworkMoveEventListener
{

    // -- 기본 정보 -------------------------------------------------------------------//

    private string m_name = null;
    private SpriteRenderer m_renderer = null;
    private int m_networkSpace = 0;


    public string NAME { get { return m_name; } set { m_name = value; } }

    // -- 클라에서 보간용 --------------------------------------------------------------//
    private Vector3 m_targetPos = Vector3.zero;
    private float m_syncTime = 0.0f;
    private float m_delay = 0.0f;
    private float m_lastSyncTime = 0.0f;
    // --------------------------------------------------------------------------------//

    void Start()
    {
        this.m_renderer = this.GetComponent<SpriteRenderer>();
        NetworkManager.Instance().AddNetworkMoveEventListener(this);
    }

    public void SetupNetworkSpace(int space)
    {
        m_networkSpace = space;
    }

    void NetworkManager.NetworkMoveEventListener.ReceiveMoveEvent(JSONObject json)
    {
        if (NetworkOrderController.ORDER_NAME == GameManager.Instance().PLAYER.USER_NAME)
            return;
        JSONObject obj = json.GetField("Users");

        float x, y, z;
        x = y = z = 0.0f;
        bool flip = false;
        bool ck = false;
        for (int i = 0; i < obj.Count; i ++)
        {
            // 이름이 다르다면 패스
            if (m_name == obj[i].GetField(NetworkManager.USERNAME).str)
            {
                x = obj[i].GetField("X").f;
                y = obj[i].GetField("Y").f;
                z = obj[i].GetField("Z").f;
                flip = obj[i].GetField("Dir");
                ck = true;
                break;
            }
        }


        if (!ck)
            return;

        Vector3 newPos = new Vector3(x, y);

        float distance = Vector3.Distance(transform.position, newPos);
        this.m_renderer.flipX = flip;

        if(distance <= 0)
        {
            return;
        }

        m_targetPos = newPos;

        m_syncTime = 0.0f;
        m_delay = Time.time - m_lastSyncTime;
        m_lastSyncTime = Time.time;
    }

    //-- Network Message 에 따른 이동 보간 ( 네트워크 플레이어 ) ------------------------------------//
    void NetworkMoveLerp()
    {
        m_syncTime += Time.deltaTime;

        // 네트워크 보간( 테스트 완료 - 로컬 )
        if (m_delay > 0)
            transform.position = Vector3.Lerp(transform.position, m_targetPos, m_syncTime / m_delay);
    }
    //-----------------------------------------------------------------------------------------------//

    void Update()
    {
        if(NetworkOrderController.ORDER_NAME != GameManager.Instance().PLAYER.USER_NAME)
            NetworkMoveLerp();
    }
}
