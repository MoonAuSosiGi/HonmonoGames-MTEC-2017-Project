using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePopup : MonoBehaviour {
    
    // -- 메시지 팝업 ( 기본 형태 팝업 ) ------------------------------------------------------------//
    //타입 지정
    public enum MESSAGEPOPUP_TYPE
    {
        DEF_OK = 0, // 디폴트, OK 버튼 하나 있음
        OK_CANCEL   // OK , CANCEL 버튼이 있음
    }

    [SerializeField]
    private Text m_title = null;
    [SerializeField]
    private Text m_text = null;

    [SerializeField]
    private GameObject m_TwoButton = null;
    [SerializeField]
    private GameObject m_OneButton = null;

    //팝업 타입
    private MESSAGEPOPUP_TYPE m_type = MESSAGEPOPUP_TYPE.DEF_OK;

    // 메시지를 받을 객체 
    private GameObject m_target = null;
    // 받을 메소드
    private string m_okFunc = null;
    private string m_cancelFunc = null;

    //---------------------------------------------------------------------------------------------//

    //-- 정보 받기 -------------------------------------------------------------------------------//
    public void Setup(MESSAGEPOPUP_TYPE type, string title, string text,
        GameObject target = null,string okFunc = null,string cancelFunc = null)
    {
        m_type = type;
        m_target = target;
        m_okFunc = okFunc;
        m_cancelFunc = cancelFunc;

        m_title.text = title;
        m_text.text = text;

        switch (m_type)
        {
            case MESSAGEPOPUP_TYPE.DEF_OK:
                m_OneButton.SetActive(true);
                m_TwoButton.SetActive(false);
                break;
            case MESSAGEPOPUP_TYPE.OK_CANCEL:
                m_OneButton.SetActive(false);
                m_TwoButton.SetActive(true);
                break;
        }

    }
    
    //-- 기본으로 받는 버튼 메소드 -----------------------------------------------------------------//
    public void OK()
    {
        if (m_target != null && m_okFunc != null)
            m_target.SendMessage(m_okFunc);
        Close();       
    }
    public void Cancel()
    {
        if (m_target != null && m_cancelFunc != null)
            m_target.SendMessage(m_cancelFunc);
        Close();
    }
    //--------------------------------------------------------------//

    void Close()
    {
        PopupManager.Instance().ClosePopup(gameObject);
    }
}
