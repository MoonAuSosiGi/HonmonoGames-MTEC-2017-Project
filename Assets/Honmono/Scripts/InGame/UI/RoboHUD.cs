using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoboHUD : MonoBehaviour, 
    GameUI.RobotHPUpdateEvent, GameUI.ENERGYUpdateEvent , 
    GameUI.CharacterHPUpdateEvent, NetworkManager.NetworkMessageEventListenrer
{
    //-- 정보 ----------------------------------------------------------//
    // 상단 적 hp 전용
    public Image m_topEnemyHPBar = null;
    public Image m_enemyHPLeftArrow = null;
    public Image m_enemyHPRightArrow = null;

    // 왼쪽 캐릭터 hp 전용
    public List<PlayerInfo> m_userList = new List<PlayerInfo>();
    public List<Sprite> m_charHPList = new List<Sprite>();
    // 오른쪽 로봇 hp 전용
    public Image m_roboHPGaugeBar = null;
    public Text m_roboHPText = null;
    // 아래쪽 로봇 energy 전용
    public List<Sprite> m_energyGaugeBar = new List<Sprite>();
    public List<Image> m_energyList = new List<Image>();
    public Text m_energyPercent = null;

    // 현재 때리고 있는 몬스터
    private Monster m_currentTarget = null;

    [Serializable]
    public struct PlayerInfo
    {
        public Text userName;
        public Image playerHP;
    }

    //-----------------------------------------------------------------//

    private float m_energy = 0.0f;
    public TextMesh m_roboUI = null;

    // 유저 이름 등록용 임시 변수

    // --------------------------------------------------------------- //

    void Start()
    {
        Transform energy = transform.GetChild(0).GetChild(0);

        for (int i = 0; i < energy.childCount; i++)
            m_energyList.Add(energy.GetChild(i).GetComponent<Image>());
        MonsterHPOutCheck();
    }

    private float GetPercent(int c,int max)
    {
        return ((float)c / (float)max) * 100.0f;
    }

    public void SetPlayerInfo(List<string> users)
    {
        MDebug.Log("t " + users.Count);
        for (int i = 0; i < m_userList.Count; i++)
        {
            if (users.Count <= i || string.IsNullOrEmpty(users[i]))
            {
                m_userList[i].playerHP.gameObject.SetActive(false);
            }
            else
                m_userList[i].userName.text = users[i];
        }
    }


    public void SetMonster(Monster mon)
    {
        if (m_currentTarget != null)
            m_currentTarget.SetHUD(null);

        CancelInvoke("MonsterHPOutCheck");

        m_currentTarget = mon;
        m_currentTarget.SetHUD(this);
        m_topEnemyHPBar.transform.parent.gameObject.SetActive(true);

        // 일정시간 동안 안때리면 숨김
        Invoke("MonsterHPOutCheck" , 3.0f);
    }

    void MonsterHPOutCheck()
    {
        m_topEnemyHPBar.transform.parent.gameObject.SetActive(false);
    }
    

    // 몬스터 HP Update
    public void MonsterHPUpdate(int curHP,int maxHP)
    {
        if (maxHP == 0)
            return;
        MDebug.Log("h " + curHP + " max " + maxHP);
        float percent = ((float)curHP / (float)maxHP);
        float width = m_topEnemyHPBar.sprite.rect.size.x;
        RectTransform t = m_topEnemyHPBar.GetComponent<RectTransform>();
        t.sizeDelta = new Vector2(width * percent , t.sizeDelta.y);

        float arrowWidth = m_enemyHPLeftArrow.sprite.rect.size.x * 0.5f;

        Vector3 left = new Vector3(
            m_topEnemyHPBar.transform.position.x - (t.sizeDelta.x / 2.0f) - arrowWidth ,
            m_enemyHPLeftArrow.transform.position.y);
        Vector3 right = new Vector3(
            m_topEnemyHPBar.transform.position.x + (t.sizeDelta.x / 2.0f) + arrowWidth ,
            m_enemyHPLeftArrow.transform.position.y);

        m_enemyHPLeftArrow.transform.position = left;
        m_enemyHPRightArrow.transform.position = right;
    }
    


    // 캐릭터 HP -- 이것은 자기것 갱신일 때 
    void GameUI.CharacterHPUpdateEvent.HPUpdate(int curHP , int maxHP)
    {
        CharacterHPUpdate(0 , curHP , maxHP);
        UpdateUI();
    }

    void CharacterHPUpdate(int index,int curHP,int maxHP)
    {

        if (maxHP == 0)
            return;
        float percent = GetPercent(curHP , maxHP);

        Sprite spr = m_userList[index].playerHP.sprite;
        // 오름차순 이미지로 가정
        for (int i = m_charHPList.Count - 1; i >= 0; i--)
        {
            if (percent >= (float)i * 10.0f)
            {
                spr = m_charHPList[i];
                break;
            }
        }
        m_userList[index].playerHP.sprite = spr;
    }


    //오른쪽 상단 로봇 HP 
    void GameUI.RobotHPUpdateEvent.HPUpdate(int curHP,int maxHP)
    {
        if (maxHP == 0)
            return;
        float percent = ((float)curHP / (float) maxHP);
        float width = m_roboHPGaugeBar.sprite.rect.size.x;
        RectTransform t = m_roboHPGaugeBar.GetComponent<RectTransform>();
        t.sizeDelta = new Vector2(width * percent , t.sizeDelta.y);
        m_roboHPText.text = string.Format("{0:F1}%" , (percent * 100.0f));

    }

    // 오른쪽 하단 로봇 에너지
    void GameUI.ENERGYUpdateEvent.EnergyUpdate(float curEnergy)
    {
        float bar_percent = 100.0f / m_energyList.Count;

        int barIndex = m_energyGaugeBar.Count - 1;
        for(int i = m_energyList.Count-1; i >= 0; i--)
        {
            if(curEnergy >= bar_percent * i)
            {
                m_energyList[i].gameObject.SetActive(true);
                m_energyList[i].sprite = m_energyGaugeBar[barIndex--];
                if (barIndex < 0)
                    barIndex = 0;
            }
            else
                m_energyList[i].gameObject.SetActive(false);
        }

        m_energy = curEnergy;
        m_energyPercent.text = string.Format("{0:F1}%" , Mathf.Round(curEnergy));
        UpdateUI();
    }

    // 로봇 내부와 연결된 UI 
    void UpdateUI()
    {
        if (GameManager.Instance().ROBO == null)
            return;
            
        m_roboUI.text = "HP : " + GameManager.Instance().ROBO.HP + "\nENERGY\n " + string.Format("{0:F1}%" , GameManager.Instance().ROBO.ENERGY);
    }

    // 다른 플레이어의 체력 갱신용 ( 옵저버일 경우 전부 갱신용 )
    public void ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        if (e.msgType.Equals(NetworkManager.CHARACTER_HPUPDATE))
        {
            // HP UPDATE
            int curHP = (int)e.msg.GetField(NetworkManager.CHARACTER_HPUPDATE).i;
            int maxHP = (int)e.msg.GetField(NetworkManager.CHARACTER_MAXHP).i;
            string name = e.user;

            for (int i = 0; i < m_userList.Count; i++)
            {
                PlayerInfo info = m_userList[i];
                if (info.userName.text.Equals(name))
                {
                    CharacterHPUpdate(i , curHP , maxHP);
                    return;
                }
            }
        }
        else if (e.msgType.Equals(NetworkManager.HP_UPDATE))
        {
            if (e.targetName.Equals("robo"))
            {
                ((GameUI.RobotHPUpdateEvent)this).HPUpdate(
                    (int)e.msg.GetField(NetworkManager.HP_UPDATE).i , GameSetting.HERO_ROBO_MAX_HP);
            }
        }
        else if (e.msgType.Equals(NetworkManager.ENERGY_UPDATE))
        {
            if (e.targetName.Equals("robo"))
            {
                ((GameUI.ENERGYUpdateEvent)this).EnergyUpdate(
                    e.msg.GetField(NetworkManager.ENERGY_UPDATE).f);
            }
        }
    }
}
