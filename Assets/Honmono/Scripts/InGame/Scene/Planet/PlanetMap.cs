using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetMap : MonoBehaviour ,NetworkManager.NetworkMessageEventListenrer{

    // -- 행성 스크롤 ------------------------------------------------ //
    public GameObject m_leftMap = null;
    public GameObject m_rightMap = null;
    public Camera m_camera = null;
    public GameObject m_leftLimitCheck = null;

    public GameObject m_leftMapLeftLimit = null;
    public GameObject m_leftMapRightLimit = null;

    public GameObject m_rightMapLeftLimit = null;
    public GameObject m_rightMapRightLimit = null;

    public string m_mapName = null;
    // --------------------------------------------------------------- //

    void Start()
    {
        // TEST CODE ::
        m_camera = Camera.main;
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
    }

    void Update()
    {
        if(NetworkOrderController.ORDER_NAME != null && 
            !NetworkOrderController.ORDER_NAME.Equals(GameManager.Instance().PLAYER.USER_NAME))
        {
            return;
        }
        float cameraHalfWdith = (Camera.main.orthographicSize * Screen.width / Screen.height);
        float cameraHalfHeight = (cameraHalfWdith * Screen.height / Screen.width);
        Vector3 pos = m_camera.transform.position;
        Vector3 checkPos = Vector3.zero;

        //Left Map  -- RightMap 일 경우
        if(m_leftMap.transform.position.x <= m_rightMap.transform.position.x)
        {
            // LEFT Check 
            checkPos = m_leftMapLeftLimit.transform.position;
            if (pos.x - cameraHalfWdith <= checkPos.x)
            {
                m_rightMap.transform.localPosition = new Vector3(
                    m_leftMap.transform.localPosition.x - 50 , 0 , 0);
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonPlanetInfoSend(
                    m_mapName , m_leftMap.transform.localPosition.x , m_leftMap.transform.localPosition.x - 50));
            }

            // Right Check 
            checkPos = m_rightMapRightLimit.transform.position;
            if (pos.x + cameraHalfWdith >= checkPos.x)
            {
                m_leftMap.transform.localPosition = new Vector3(
                    m_rightMap.transform.localPosition.x + 50 , 0 , 0);
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonPlanetInfoSend(
                    m_mapName , m_rightMap.transform.localPosition.x + 50 , m_rightMap.transform.localPosition.x));
            }
        }
        // Right Map -- Left Map 일 경우
        else if(m_leftMap.transform.position.x >= m_rightMap.transform.position.x)
        {
            // LEFT Check 
            checkPos = m_rightMapLeftLimit.transform.position;
            if (pos.x - cameraHalfWdith <= checkPos.x)
            {
                m_leftMap.transform.localPosition = new Vector3(
                    m_rightMap.transform.localPosition.x - 50 , 0 , 0);
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonPlanetInfoSend(
                    m_mapName , m_rightMap.transform.localPosition.x - 50 , m_rightMap.transform.localPosition.x));
            }
            // Right Check
            checkPos = m_leftMapRightLimit.transform.position;
            if (pos.x + cameraHalfWdith >= checkPos.x)
            {
                m_rightMap.transform.localPosition = new Vector3(
                    m_leftMap.transform.localPosition.x + 50 , 0 , 0);
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonPlanetInfoSend(
                    m_mapName ,m_leftMap.transform.localPosition.x,m_leftMap.transform.localPosition.x + 50 ));
            }
        }


        
    }

    public void ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        if (NetworkOrderController.ORDER_NAME != null &&
            NetworkOrderController.ORDER_NAME.Equals(GameManager.Instance().PLAYER.USER_NAME))
            return;

        if(e.msgType.Equals(NetworkManager.PLANET_INFO))
        {
            if(e.targetName.Equals(m_mapName))
            {
                float leftX = e.msg.GetField(NetworkManager.PLANET_INFO)[0].f;
                float rightX = e.msg.GetField(NetworkManager.PLANET_INFO)[1].f;

                Vector3 lPos = m_leftMap.transform.localPosition;
                Vector3 rPos = m_rightMap.transform.localPosition;
                m_leftMap.transform.localPosition = new Vector3(leftX , lPos.y , lPos.z);
                m_rightMap.transform.localPosition = new Vector3(rightX , rPos.y , rPos.z);
            }
        }
    }
}
