﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;


public class NetworkMoving : MonoBehaviour, NetworkManager.NetworkMoveEventListener
{

    // -- 기본 정보 -------------------------------------------------------------------//

    private string m_name = null;
    private SpriteRenderer m_renderer = null;
    private SkeletonAnimation m_skeltonAni = null;
    private int m_networkSpace = 0;

    // true 일 경우 flip 을 받아와서 적용 아니면 비적용
    private bool m_autoDir = false;


    public bool AUTO_DIR { get { return m_autoDir; } set { m_autoDir = value; } }
    public string NAME { get { return m_name; } set { m_name = value; } }

    // -- 클라에서 보간용 --------------------------------------------------------------//
    private Vector3 m_prevPos = Vector3.zero;
    private Vector3 m_targetPos = Vector3.zero;
    private float m_lastSendTime = 0.0f;
    // --------------------------------------------------------------------------------//

    void Start()
    {
        m_prevPos = transform.position;
        this.m_renderer = this.GetComponent<SpriteRenderer>();
        this.m_skeltonAni = this.GetComponent<SkeletonAnimation>();
        NetworkManager.Instance().AddNetworkEnemyMoveEventListener(this);
    }

    public void SetupNetworkSpace(int space)
    {
        m_networkSpace = space;
    }

    void NetworkManager.NetworkMoveEventListener.ReceiveMoveEvent(JSONObject json)
    {
        JSONObject obj = json.GetField("Enemies");

        float x, y, z;
        x = y = z = 0.0f;
        bool flip = false;
        bool ck = false;

        Vector3 drPos = Vector3.zero;
        Vector3 targetPos = Vector3.zero;

        for (int i = 0; i < obj.Count; i++)
        {
            // 이름이 다르다면 패스
            if (m_name.Equals(obj[i].GetField("Name").str))
            {
                x = obj[i].GetField("X").f;
                y = obj[i].GetField("Y").f;
                z = obj[i].GetField("Z").f;
                flip = obj[i].GetField("Dir").b;
                ck = true;
                JSONObject v = obj[i].GetField(NetworkManager.DIRVECTOR);
                drPos = new Vector3(v.GetField("X").f , v.GetField("Y").f , v.GetField("Z").f);
                break;
            }
        }


        if (!ck)
            return;

        Vector3 newPos = new Vector3(x, y);
       

        targetPos = drPos;

        transform.eulerAngles = new Vector3(0,0 , z);

        if (m_skeltonAni != null)
        {
            if (m_skeltonAni.skeleton.flipX != flip)
            {
                if(AUTO_DIR)
                    m_skeltonAni.skeleton.flipX = flip;
                targetPos = newPos;
            }
        }
        else if (m_renderer != null)
        {
            if (m_renderer.flipX != flip)
            {
                m_renderer.flipX = flip;
                targetPos = newPos;
            }
        }
        transform.position = targetPos;        
    }

    //-- Network Message 에 따른 이동 보간 ( 네트워크 플레이어 ) ------------------------------------//
    void NetworkMoveLerp()
    {
       // transform.position = m_targetPos;
        //m_syncTime += Time.deltaTime;
        

        //// 네트워크 보간( 테스트 완료 - 로컬 )
        //if (m_delay > 0)
        //    transform.position = Vector3.Lerp(transform.position, m_targetPos, m_syncTime / m_delay);
        
        //P + V * D
        // 레이턴시 ->보낸다 ->받는다(시간)
        // target Pos 를 재계산
        // 클라 A  좌표 샌딩 ->  서버 -> 클라B 좌표 받음
        // 클라B가 알수 있는 레이턴시는 서버 <-> 클라B
        // 클라 A에 대한 시간을 알수 없음 
        // 클라 A가 시간을 보냄 -> 클라 B에서 계산방법 (딜레이는 현재시간 - A의 보낸 시간 )
        // (속도는 정해져있음)
        
    }
    //-----------------------------------------------------------------------------------------------//

    void Update()
    {
     //   if (NetworkOrderController.ORDER_NAME != GameManager.Instance().PLAYER.USER_NAME)
            NetworkMoveLerp();
     //   else
     //       MoveSend();
    }

    void MoveSend()
    {
        Vector3 pos = transform.position;
        float distance = Vector3.Distance(m_prevPos, pos);
        m_prevPos = transform.position;
        
        Vector3 velocity = (transform.position - m_prevPos) / Time.deltaTime;
        Vector3 sendPos = m_prevPos + (velocity * (Time.deltaTime - m_lastSendTime));

        NetworkManager.Instance().SendEnemyMoveMessage(
            JSONMessageTool.ToJsonEnemyMove(m_name, sendPos.x, sendPos.y, 0, true,Vector3.zero));

    }
}
