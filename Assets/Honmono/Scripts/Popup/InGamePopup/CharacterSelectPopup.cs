using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using UnityEngine.SceneManagement;
using System;

public class CharacterSelectPopup : MonoBehaviour,PopupManager.PopupHide {

    //-- 기본 정보 -------------------------------------------------------------------------//
    [SerializeField]
    private GameObject m_Left = null;

    [SerializeField]
    private GameObject m_Center = null;

    [SerializeField]
    private GameObject m_Right = null;

    public List<SkeletonGraphic> m_LeftList = new List<SkeletonGraphic>();
    public List<SkeletonGraphic> m_RightList = new List<SkeletonGraphic>();
    public List<SkeletonGraphic> m_CenterList = new List<SkeletonGraphic>();

    [SerializeField]
    private List<Image> m_speedList = new List<Image>();

    [SerializeField]
    private List<Image> m_powerList = new List<Image>();

    [SerializeField]
    private List<Image> m_repairList = new List<Image>();

    // Tween 전용 --
    private Vector3 m_leftStart = Vector3.zero;
    private Vector3 m_centerStart = Vector3.zero;
    private Vector3 m_rightStart = Vector3.zero;

    private SkeletonGraphic m_sprLeft = null;
    private SkeletonGraphic m_sprCenter = null;
    private SkeletonGraphic m_sprRight = null;

    public AudioClip m_swipe = null;
    public AudioClip m_selectFinish = null;
    private bool m_aniPlay = false;
    //-------------------------------------------------------------------------------------//

    public void CharacterSelectButton()
    {
        ReturnButton();
    }

    public void ReturnButton()
    {
        //씬 옮기기

        //SceneManager.LoadScene("TutorialScene");
        GameManager.Instance().PLAYER.SKELETON_DATA_ASSET = 
            m_Center.GetComponent<SkeletonGraphic>().SkeletonDataAsset.name;
        PopupManager.Instance().ClosePopup(gameObject);
    }

    public void HideEndEvent()
    {

        PopupManager.Instance().AddPopup("LobbyPopup");
    }

    //-------------------------------------------------------------------------------------//
    // Use this for initialization

    void Start () {
        m_leftStart = m_Left.transform.position;
        m_centerStart = m_Center.transform.position;
        m_rightStart = m_Right.transform.position;
        

        m_sprLeft = m_Left.GetComponent<SkeletonGraphic>();
        m_sprCenter = m_Center.GetComponent<SkeletonGraphic>();
        m_sprRight = m_Right.GetComponent<SkeletonGraphic>();

        m_Center.transform.SetAsFirstSibling();
        m_Left.transform.SetSiblingIndex(1);
        m_Right.transform.SetSiblingIndex(2);

        UIUpdate();

        Vector3 p = NetworkManager.Instance().m_playerStartPosition.transform.position;
        NetworkManager.Instance().SendMoveMessage(JSONMessageTool.ToJsonMove(
            GameManager.Instance().PLAYER.USER_NAME + "_robo" , 
            p.x, p.y , 0 , true , Vector3.zero));
      //  SoundManager.Instance().PlayBGM(1);
    }
	
	// Update is called once per frame
	void Update () {
        

        if (Input.GetKeyUp(KeyCode.LeftArrow) && iTween.Count(m_Left) == 0 && iTween.Count(m_Center) == 0 && iTween.Count(m_Right) == 0)
            LeftButton();
        else if (Input.GetKeyUp(KeyCode.RightArrow) && iTween.Count(m_Left) == 0 && iTween.Count(m_Center) == 0 && iTween.Count(m_Right) == 0)
            RightButton();

    }


    //-- 캐릭터 이동 ---------------------------------------------------------------------//

    public void LeftButton()
    {
        m_leftStart = m_Left.GetComponent<RectTransform>().position;
        m_centerStart = m_Center.GetComponent<RectTransform>().position;
        m_rightStart = m_Right.GetComponent<RectTransform>().position;
        SoundManager.Instance().PlaySound(m_swipe);
        if (!m_aniPlay)
            LeftMove();
    }

    public void RightButton()
    {
        m_leftStart = m_Left.transform.position;
        m_centerStart = m_Center.transform.position;
        m_rightStart = m_Right.transform.position;
        SoundManager.Instance().PlaySound(m_swipe);
        if (!m_aniPlay)
            RightMove();
    }

    private void LeftMove()
    {
        m_Right.transform.SetSiblingIndex(2);
        m_Left.transform.SetSiblingIndex(1);
        Tween(m_Left, m_Right.transform.position, "");
        Tween(m_Center, m_Left.transform.position, "");
        Tween(m_Right, m_Center.transform.position, "right_c");
    }

    private void RightMove()
    {
        m_Right.transform.SetSiblingIndex(1);
        m_Left.transform.SetSiblingIndex(2);
        Tween(m_Left, m_Center.GetComponent<RectTransform>().position, "");
        Tween(m_Center, m_Right.GetComponent<RectTransform>().position, "");
        Tween(m_Right, m_Left.GetComponent<RectTransform>().position, "right_l");
    }

    //-----------------------------------------------------------------------------------//

    private void Tween(GameObject obj , Vector3 target , string dir = null)
    {
        m_aniPlay = true;

        iTween.MoveTo(obj, iTween.Hash("x", target.x,"y",target.y,
            "easetype","easeOutQuart", 
            "oncompletetarget", gameObject,
            "oncomplete", "TweenEnd", 
            "oncompleteparams",dir));
    }

    private void TweenEnd(string info)
    {
        if (string.IsNullOrEmpty(info))
            return;
        string[] infos = info.Split('_');
        m_aniPlay = false;

        m_Left.transform.position = m_leftStart;
        m_Right.transform.position = m_rightStart;
        m_Center.transform.position = m_centerStart;

        if (infos[0] == "right")
        {

            if (infos[1] == "l")
            {
                // TODO 추후 캐릭터 추가시 여기서 바꿈 CENTER 로 바꿔야함
                // 바뀌기 전 이름
                SkeletonGraphic left = m_Left.GetComponent<SkeletonGraphic>();
                SkeletonGraphic right = m_Right.GetComponent<SkeletonGraphic>();
                SkeletonGraphic center = m_Center.GetComponent<SkeletonGraphic>();

                int l = ChangeCharacterInfo(m_LeftList , right.SkeletonDataAsset.name);
                int r = ChangeCharacterInfo(m_RightList , center.SkeletonDataAsset.name);
                int c = ChangeCharacterInfo(m_CenterList , left.SkeletonDataAsset.name);

                m_Left = m_LeftList[l].gameObject;
                m_Right = m_RightList[r].gameObject;
                m_Center = m_CenterList[c].gameObject;

            }
            else
            {
                // TODO 추후 캐릭터 추가시 여기서 바꿈 left 로 바꿔야함
                SkeletonGraphic left = m_Left.GetComponent<SkeletonGraphic>();
                SkeletonGraphic right = m_Right.GetComponent<SkeletonGraphic>();
                SkeletonGraphic center = m_Center.GetComponent<SkeletonGraphic>();

                int l = ChangeCharacterInfo(m_LeftList , center.SkeletonDataAsset.name);
                int r = ChangeCharacterInfo(m_RightList , left.SkeletonDataAsset.name);
                int c = ChangeCharacterInfo(m_CenterList , right.SkeletonDataAsset.name);

                m_Left = m_LeftList[l].gameObject;
                m_Right = m_RightList[r].gameObject;
                m_Center = m_CenterList[c].gameObject;
            }
        }
    }


    int ChangeCharacterInfo(List<SkeletonGraphic> list, string name)
    {
        int index = -1;
        for(int i = 0; i < list.Count; i++)
        {
            SkeletonGraphic s = list[i];
            if (s.skeletonDataAsset.name.Equals(name))
            {
                index = i;
                s.gameObject.SetActive(true);
            }
            else
                s.gameObject.SetActive(false);
        }
        return index;
    }

    //-- 능력치 조절 -------------------------------------------------------------------//

    public void SpeedControl(string objName)
    { 
        if (objName == "plus")
        {
            if (GameManager.Instance().PLAYER.STATUS.STAT_SPEED + 1 <= GameSetting.STAT.MAX_SPEED)
                GameManager.Instance().PLAYER.STATUS.STAT_SPEED++;
        }
        else if(objName == "minus")
        {
            if (GameManager.Instance().PLAYER.STATUS.STAT_SPEED - 1 >= 0)
                GameManager.Instance().PLAYER.STATUS.STAT_SPEED--;
        }
        UIUpdate();
    }

    public void PowerControl(string objName)
    {
        if (objName == "plus")
        {
            if (GameManager.Instance().PLAYER.STATUS.STAT_POWER + 1 <= GameSetting.STAT.MAX_POWER)
                GameManager.Instance().PLAYER.STATUS.STAT_POWER++;
        }
        else if (objName == "minus")
        {
            if (GameManager.Instance().PLAYER.STATUS.STAT_POWER - 1 >= 0)
                GameManager.Instance().PLAYER.STATUS.STAT_POWER--;
        }
        UIUpdate();
    }

    public void RefairControl(string objName)
    {
        if (objName == "plus")
        {
            if (GameManager.Instance().PLAYER.STATUS.STAT_REPAIR + 1 <= GameSetting.STAT.MAX_REPAIR)
                GameManager.Instance().PLAYER.STATUS.STAT_REPAIR++;
        }
        else if (objName == "minus")
        {
            if (GameManager.Instance().PLAYER.STATUS.STAT_REPAIR - 1 >= 0)
                GameManager.Instance().PLAYER.STATUS.STAT_REPAIR--;
        }
        
        UIUpdate();
    }

    //-- UI 갱신 -----------------------------------------------------------------------//

    private void UIUpdate()
    {
            //speed
        
        for(int i = 0; i < m_speedList.Count; i++)
        {
            if (i < GameManager.Instance().PLAYER.STATUS.STAT_SPEED)
                m_speedList[i].enabled = true;
            else
                m_speedList[i].enabled = false;
        }

        for (int i = 0; i < m_powerList.Count; i++)
        {
            if (i < GameManager.Instance().PLAYER.STATUS.STAT_POWER)
                m_powerList[i].enabled = true;
            else
                m_powerList[i].enabled = false;
        }

        for (int i = 0; i < m_repairList.Count; i++)
        {
            if (i < GameManager.Instance().PLAYER.STATUS.STAT_REPAIR)
                m_repairList[i].enabled = true;
            else
                m_repairList[i].enabled = false;
        }
    }

 
}
