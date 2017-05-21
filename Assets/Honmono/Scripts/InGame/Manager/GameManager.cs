using System.Collections;
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

    Resolution m_resoluation = new Resolution(1920 ,1080);

    // 플레이어 정보
    private PlayerInfo m_player = new PlayerInfo();
    public PlayerInfo PLAYER { get { return m_player; } }

    // 로봇
    private HeroRobo m_robo;
    public HeroRobo ROBO {  get { return m_robo; } set { m_robo = value; } }

    // 조작자인가
    private bool m_isOriginOrder = false;
    public bool ORIGIN_ORDER  {get { return m_isOriginOrder; } set { m_isOriginOrder = value; } }

    //---------------------------------------------------------------//

    void Start()
    {
        Application.runInBackground = true;
        Screen.SetResolution(m_resoluation.width, m_resoluation.height,false);
        
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

    // -- 게임 내에서의 상황 관리용 ------------------------------------------------ //
    // 로봇의 현재 위치를 추적한다.

    public enum ROBO_PLACE
    {
        SPACE,
        BOSS_AREA,
        PLANET
    }

    private ROBO_PLACE m_curPlace = ROBO_PLACE.SPACE;

    public ROBO_PLACE CUR_PLACE
    {
        get { return m_curPlace; }
        set {
            m_curPlace = value;
            if(NetworkOrderController.ORDER_NAME.Equals(PLAYER.USER_NAME))
                NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonPlaceChange((int)m_curPlace));
        }
    }
    
}
