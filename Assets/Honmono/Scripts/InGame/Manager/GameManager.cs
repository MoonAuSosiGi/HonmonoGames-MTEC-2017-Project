﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singletone<GameManager> {

    //-- GameManager ------------------------------------------------//
    // 게임의 전반적인 내용에 대한 관리를 하는 클래스
    private struct Resolution
    {
        public int width;
        public int height;
        public Resolution(int w,int h) { width = w;  height = h; }
    }

    Resolution m_resoluation = new Resolution(1920,1080);

    // 플레이어 정보
    private PlayerInfo m_player = new PlayerInfo();
    public PlayerInfo PLAYER { get { return m_player; } }

    // 로봇
    private HeroRobo m_robo = new HeroRobo();
    public HeroRobo ROBO {  get { return m_robo; } }

    // 조작자인가
    private bool m_isOriginOrder = false;
    public bool ORIGIN_ORDER  {get { return m_isOriginOrder; } set { m_isOriginOrder = value; } }

    //---------------------------------------------------------------//

    void Start()
    {
        Application.runInBackground = true;
        Screen.SetResolution(m_resoluation.width, m_resoluation.height,false);
        PopupManager.Instance().AddPopup("LoginPopup");
        //PopupManager.Instance().AddPopup("NetworkConnectPopup");
    }

    
    public void HeroSetup(Hero hero)
    {
        m_player.PLAYER_HERO = hero;
    }

    public void HeroRoboSetup(HeroRobo robo)
    {
        m_robo = robo;
    }
}
