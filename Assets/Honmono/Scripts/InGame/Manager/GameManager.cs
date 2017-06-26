using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class GameManager : Singletone<GameManager> , NetworkManager.NetworkMessageEventListenrer{

    public Text m_queueCountChecker = null;
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

    // 테스트
    public string m_curSceneState = "login";
    // -- 게임 UI ------------------------------------------------------------------------------//
    // HUD
    public RoboHUD m_gameHUD = null;

    private List<GameUI.RobotHPUpdateEvent> m_hpReceieveList = new List<GameUI.RobotHPUpdateEvent>();
    private List<GameUI.ENERGYUpdateEvent> m_energyReceieveList = new List<GameUI.ENERGYUpdateEvent>();

    public void AddHPUpdateEvent(GameUI.RobotHPUpdateEvent recv)
    {
        if (!m_hpReceieveList.Contains(recv))
            m_hpReceieveList.Add(recv);
    }

    public void AddEnergyUpdateEvent(GameUI.ENERGYUpdateEvent recv)
    {
        if (!m_energyReceieveList.Contains(recv))
            m_energyReceieveList.Add(recv);
    }

    void UpdateHp(int curHP)
    {
        foreach (GameUI.RobotHPUpdateEvent recv in m_hpReceieveList)
            recv.HPUpdate(curHP , GameSetting.HERO_ROBO_MAX_HP);
    }

    void UpdateEnergy(float curEnergy)
    {
        foreach (GameUI.ENERGYUpdateEvent recv in m_energyReceieveList)
            recv.EnergyUpdate(curEnergy);
    }

    public void ChangeEnergy(float curEnergy)
    {
        UpdateEnergy(curEnergy);
    }

    public void ChangeHP(int curHP)
    {
        UpdateHp(curHP);
    }

    public void ChangeCharacterHP(int curHP,int maxHP)
    {
        ((GameUI.CharacterHPUpdateEvent)m_gameHUD).HPUpdate(curHP , maxHP);
    }

    // 현재 때리고 있는 몬스터 
    public void SetCurrentEnemy(Monster monster)
    {
        if (monster != null)
            m_gameHUD.gameObject.SetActive(true);
        else
            m_gameHUD.gameObject.SetActive(false);
        m_gameHUD.SetMonster(monster);
    }
    // -- Scene Management ---------------------------------------------------------------------//
    TargetMoveCamera m_moveCamera = null;
    
    public enum PLACE
    {
        TUTORIAL_START = 0,
        TUTORIAL_ROBO,
        TUTORIAL_ROBO_IN,
        ROBO_IN,
        SPACE,
        PLANET,
        STAGE1_BOSS,
        ONLY_POPUP_SHOW,
        ROBO,
        ROBO_IN_DRIVE,
        ROBO_IN_GUN,
        PLANET1,
        PLANET2
    }

    [Serializable]
    public struct PLACE_INFO
    {
        public PLACE placeType;
        public GameObject place;
        public GameObject targetObject;
        public GameObject backgroundObj;
    }
    // 게임 내 이동할 씬 리스트 
    public List<PLACE_INFO> m_placeList = new List<PLACE_INFO>();
    public GameObject m_uiObj = null;
    public GameObject m_funcTarget = null;
    private string m_func = null;
    private bool m_faceOutEndFunc = true;
    private GameObject m_moveTarget = null;
    private GameObject m_targetPlace = null;
    private GameObject m_backgroundObj = null;
    private CameraFilterPack_Color_RGB m_fadeObj = null;
    private PLACE m_place;

    public PLACE SCENE_PLACE
    {
        get { return m_place; }
        set { m_place = value; }
    }
    //------------------------------------------------------------------------------------------//
    //--옵저버용--------------------------------------------------------------------------------//
    public class GameUserInfo
    {
        public PLACE place;
        public Hero player;
        public GameUserInfo(PLACE p, Hero player)
        {
            place = p;
            this.player = player;
        }
    }
    //접속중인 유저들
    private List<GameUserInfo> m_gameUserInfo = new List<GameUserInfo>();
    public List<GameUserInfo> GAME_USER_INFO
    {
        get { return m_gameUserInfo; }
    }

    public void HudSetup(List<string> users)
    {
        m_gameHUD.SetPlayerInfo(users);
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(m_gameHUD);
        
    }

    public void AddObserverInfo(Hero hero)
    {
        m_gameUserInfo.Add(new GameUserInfo(PLACE.ROBO_IN , hero));
    }
    // 현재 보고 있는 유저
    private int m_curIndex = 0;
    //------------------------------------------------------------------------------------------//
    void Start()
    {
        Application.runInBackground = true;
        Screen.SetResolution(m_resoluation.width, m_resoluation.height,false);
        m_fadeObj = Camera.main.gameObject.GetComponent<CameraFilterPack_Color_RGB>();
        m_moveCamera = Camera.main.GetComponent<TargetMoveCamera>();
        //  PopupManager.Instance().AddPopup("NetworkConnectPopup");

        // -- ui setup ------------------- //
        AddHPUpdateEvent(m_gameHUD);
        AddEnergyUpdateEvent(m_gameHUD);
        //-------------------------------- //
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
    }

    
    public void HeroSetup(Hero hero)
    {
        m_player.PLAYER_HERO = hero;
    }

    public void HeroRoboSetup(HeroRobo robo)
    {
        m_robo = robo;
    }

    public void UIShow(bool b)
    {
        m_gameHUD.gameObject.SetActive(b);
        UpdateHp(ROBO.HP);
        UpdateEnergy(ROBO.ENERGY);
    }
    // -- 카메라 이펙트
    public void CameraShake(float time)
    {
        iTween.MoveBy(gameObject , iTween.Hash("x" , -0.3f ,"time",0.3f, "oncompletetarget" ,
            gameObject , "oncomplete" , "CameraShakeEnd"));
    }
    void CameraShakeEnd()
    {
        iTween.MoveBy(gameObject , iTween.Hash("x" , 0.3f , "time" , 0.3f));
    }

    // -- 게임 내에서의 상황 관리용 ---------------------------------------------------------- //
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

    // -- Scene 전환 관련 정리 ----------------------------------------------------------------//
    public void ChangeScene(PLACE place,GameObject funcTarget = null, string func = null,bool fadeOutEndFunc = true)
    {
        if(m_place == PLACE.ONLY_POPUP_SHOW && place == PLACE.ROBO_IN)
            NetworkManager.Instance().GameStart();

        m_place = place;

        PLACE_INFO info = m_placeList[(int)place];
        m_moveTarget = info.targetObject;
        m_targetPlace = info.place;
        m_backgroundObj = info.backgroundObj;
        Camera.main.orthographic = true;

        m_moveCamera.CORRECTION = Vector2.zero;
        m_moveCamera.m_target = null;

        m_funcTarget = funcTarget;
        m_func = func;
        m_faceOutEndFunc = fadeOutEndFunc;

        m_moveCamera.TARGET_MOVEABLE = false;
        switch (place)
        {
            case PLACE.STAGE1_BOSS:
                SoundManager.Instance().PlayBGM(4);
                break;
            case PLACE.PLANET:
                if(!NetworkOrderController.OBSERVER_MODE)
                    m_moveTarget = GameManager.Instance().PLAYER.PLAYER_HERO.gameObject;
                m_moveCamera.CORRECTION = new Vector2(0.0f , 5.5f);
                break;
            case PLACE.PLANET2:
            case PLACE.PLANET1:
                SoundManager.Instance().PlayBGM(3);
                if (!NetworkOrderController.OBSERVER_MODE)
                    m_moveTarget = GameManager.Instance().PLAYER.PLAYER_HERO.gameObject;
                m_moveCamera.TARGET_MOVEABLE = true;
                m_moveCamera.CORRECTION = new Vector2(0.0f , 6.0f);
                break;
            case PLACE.ROBO_IN_DRIVE:
            case PLACE.ROBO_IN_GUN:
            case PLACE.ROBO_IN:
                NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonEnergyUdate("robo" , ROBO.ENERGY));
                SoundManager.Instance().PlayBGM(2);
                if (!NetworkOrderController.OBSERVER_MODE)
                    m_moveTarget = GameManager.Instance().PLAYER.PLAYER_HERO.gameObject;
                m_moveCamera.CORRECTION = new Vector2(0.0f , 5.76f);
                m_moveCamera.TARGET_MOVEABLE = true;
                break;
            case PLACE.ROBO:
                m_backgroundObj = 
                    (CUR_PLACE == ROBO_PLACE.SPACE) ? 
                        m_placeList[(int)PLACE.SPACE].backgroundObj : 
                        m_placeList[(int)PLACE.STAGE1_BOSS].backgroundObj;
                m_moveTarget = m_targetPlace;
                break;

            case PLACE.TUTORIAL_START:
                m_targetPlace.transform.parent.parent.SendMessage("SetupTutorial");
                m_moveCamera.TARGET_MOVEABLE = true;
                break;
        }

        CameraFadeOut();

        if (!NetworkOrderController.OBSERVER_MODE)
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonUserPlaceChange((int)m_place));
    }

    bool m_ob_interaction = false;
    private string m_ob_intreactionMode = "";
    // 옵저버가 플레이어를 쫒아다닌다 
    public void ChangeUser(int userIndex)
    {
        if (userIndex < 0 || m_gameUserInfo.Count <= 0 || userIndex >= m_gameUserInfo.Count)
            return;

        GameUserInfo info = m_gameUserInfo[userIndex];
        ChangeScene(info.place);

        switch(info.place)
        {
            case PLACE.PLANET:
            case PLACE.ROBO_IN_GUN:
            case PLACE.ROBO_IN_DRIVE:
            case PLACE.ROBO_IN:
            case PLACE.PLANET1:
            case PLACE.PLANET2:
                m_moveTarget = info.player.gameObject;
                break;
            default:
                m_moveTarget = ROBO.gameObject;
                break;
        }

        if (info.place == PLACE.ROBO)
        {
            string moveCheck = (string.IsNullOrEmpty(ROBO.MOVE_PLYAER)) ? "" : ROBO.MOVE_PLYAER;
            string gunCheck = (string.IsNullOrEmpty(ROBO.GUN_PLAYER)) ? "" : ROBO.GUN_PLAYER;
            m_ob_interaction = true;
            m_ob_intreactionMode = 
                (info.player.USERNAME.Equals(gunCheck + "_robo")) ? 
                "WEAPON SYSTEM ONLINE" : (info.player.USERNAME.Equals(moveCheck + "_robo")) ? "DRIVE SYSTEM ONLINE" : "";
        }

        switch (info.place)
        {
            case PLACE.PLANET:
            case PLACE.PLANET1:
            case PLACE.PLANET2:
                info.player.SetSkinSuit();
                break;
            default:
                info.player.SetSkinNormal();
                break;

        }

    }


    void InteractionPopup()
    {
        PopupManager.Instance().AddInteractionPopup(m_ob_intreactionMode);
    }
    void Update()
    {
        //string peek = "";

        //if (NetworkManager.Instance().m_socketOrders.Count > 0)
        //    peek = NetworkManager.Instance().m_socketOrders.Peek();
        //m_queueCountChecker.text = "QUEUE Count : "
        //    + NetworkManager.Instance().m_socketOrders.Count + "\n" + peek;

        if (!NetworkOrderController.OBSERVER_MODE)
            return;

        // 옵저버에 관련된 로직
        int index = -1;
        if (Input.GetKeyDown(KeyCode.Q)) index = 0;
        if (Input.GetKeyDown(KeyCode.W)) index = 1;
        if (Input.GetKeyDown(KeyCode.E)) index = 2;
        if (Input.GetKeyDown(KeyCode.R)) index = 3;
        if (index < 0)
        {
            return;
        }
        else
        {
            GameUserInfo info = m_gameUserInfo[index];
            if (string.IsNullOrEmpty(info.player.USERNAME))
                return;

            m_curIndex = index;
        }

       

        ChangeUser(m_curIndex);
        
    }

    public void ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        if(NetworkOrderController.OBSERVER_MODE)
        {
            if(e.msgType.Equals(NetworkManager.USER_PLACE_CHANGE))
            {
                if (m_gameUserInfo == null)
                    return;
                for(int i = 0; i < m_gameUserInfo.Count; i++)
                {
                    
                    if (m_gameUserInfo[i].player.USERNAME.Equals(e.user + "_robo"))
                    {
                        m_gameUserInfo[i].place = (PLACE)e.msg.GetField(NetworkManager.USER_PLACE_CHANGE).i;

                        if (i == m_curIndex)
                            ChangeUser(m_curIndex);
                        return;
                    }
                }
            }
        }
    }

    void CameraFadeIn()
    {
        ColorValueEffect(true);
    }
    
    void CameraFadeOut()
    {
        m_uiObj.SetActive(false);
        ColorValueEffect(false);
    }

    void ColorValueEffect(bool fadeIn)
    {
        iTween.ValueTo(Camera.main.gameObject ,
            iTween.Hash(
                "from" , (fadeIn) ? 0 : 1 ,
                "to" , (fadeIn) ? 1 : 0 ,
                "time" , 0.5f ,
                //"easetype","easeOutElastic",
                "onupdatetarget" , gameObject ,
                "onupdate" , "Effect" ,
                "oncompletetarget" , gameObject ,
                "oncompleteparams" , fadeIn ,
                "oncomplete" , "EffectTweenEnd"));
    }

    void Effect(float f)
    {
        Color c = m_fadeObj.ColorRGB;
        c.r = f;
        c.g = f;
        c.b = f;
        m_fadeObj.ColorRGB = c;
    }

    void EffectTweenEnd(bool b)
    {
        // - 페이드 아웃 -
        if (!b)
        {
            Vector2 p = m_targetPlace.transform.position;
            
            Camera.main.transform.position = new Vector3(
                p.x + m_moveCamera.CORRECTION.x,
                p.y + m_moveCamera.CORRECTION.y,
                -3);
            if (m_moveTarget != null)
                m_moveTarget.transform.position = p;

            if(m_funcTarget != null && m_faceOutEndFunc)
                m_funcTarget.SendMessage(m_func);

            CameraFadeIn();
        }

        // - 페이드 인 -
        else
        {
            m_moveCamera.SetBackgrounds(m_backgroundObj);
            if (m_moveTarget != null)
            {
            //    m_moveCamera.TARGET_MOVEABLE = false;
                m_moveCamera.m_target = m_moveTarget;
            }
            else
            {
                m_moveCamera.m_target = null;
            }

            m_moveCamera.enabled = true;

            if (m_funcTarget != null && !m_faceOutEndFunc)
                m_funcTarget.SendMessage(m_func);

            if(m_ob_interaction)
            {
                InteractionPopup();
                m_ob_interaction = false;
            }

            if (m_place != PLACE.ONLY_POPUP_SHOW)
                UIShow(true);
            if (m_place == PLACE.TUTORIAL_ROBO || m_place == PLACE.TUTORIAL_ROBO_IN || m_place == PLACE.TUTORIAL_START)
                UIShow(false);
            m_uiObj.SetActive(true);
        }
    }


    public string GetCharacterNormalSkin(string skeletonDataAsset)
    {
        switch(skeletonDataAsset)
        {
            case "char_01_SkeletonData": return "char_01";
            case "char_02_SkeletonData": return "normal";
            case "char_03_SkeletonData": return "char_01";
        }

        return null;
    }
    public string GetCharacterSuitSkin(string skeletonDataAsset)
    {
        switch (skeletonDataAsset)
        {
            case "char_01_SkeletonData": return "char_01_a";
            case "char_02_SkeletonData": return "suit";
            case "char_03_SkeletonData": return "suit";
        }
        return null;
    }
    public string GetCharacterNormalClimbSkin(string skeletonDataAsset)
    {
        switch (skeletonDataAsset)
        {
            case "char_01_SkeletonData": return "default";
            case "char_02_SkeletonData": return "default";
            case "char_03_SkeletonData": return "default";
        }
        return null;
    }
    public string GetCharacterSuitClimbSkin(string skeletonDataAsset)
    {
        
        return "default";
    }
}
