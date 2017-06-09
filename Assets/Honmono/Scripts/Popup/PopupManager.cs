using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singletone<PopupManager> {


    // 기본 정보 --------------------------------------------------------//


    // popup이 담길 큐
    private Queue<GameObject> m_popupQueue = new Queue<GameObject>();
    private string m_tween = "easeInExpo";

    // -----------------------------------------------------------------//


    public int GetPopupCount()
    {
        return m_popupQueue.Count;
    }


    public interface PopupHide
    {
        void HideEndEvent();
    }
    

    // -- 팝업 메소드 ---------------------------------------------------------------------------------------------------------//

    // 팝업 추가
    public void AddPopup(string popupName)
    {
        GameObject popup = GameObject.Instantiate(Resources.Load(GamePath.POPUP + popupName), transform) as GameObject;
        popup.transform.localScale = new Vector3(0, 0, 0);

        m_popupQueue.Enqueue(popup);
        if (m_popupQueue.Count == 1)
            iTween.ScaleTo(popup, iTween.Hash("x", 1.0f, "y", 1.0f, "easetype", m_tween, "time", 0.3f));
    }

    // 기본 팝업 - OK
    public void MessagePopupOK(string title,string text,GameObject target = null,string okFunc = null)
    {
        GameObject popup = GameObject.Instantiate(Resources.Load(GamePath.POPUP + "MessagePopup"), transform) as GameObject;
        popup.transform.localScale = new Vector3(0, 0, 0);
        MessagePopup msgp = popup.GetComponent<MessagePopup>();
        msgp.Setup(MessagePopup.MESSAGEPOPUP_TYPE.DEF_OK, title, text, target, okFunc);

        m_popupQueue.Enqueue(popup);
        if (GetPopupCount() == 1)
            iTween.ScaleTo(popup, iTween.Hash("x", 1.0f, "y", 1.0f, "easetype", m_tween, "time", 0.3f));
    }

    // 기본 팝업 - ok cancel
    public void MessagePopupOKCancel(string title, string text, GameObject target = null, string okFunc = null,string cancelFunc=null)
    {
        GameObject popup = GameObject.Instantiate(Resources.Load(GamePath.POPUP + "MessagePopup"), transform) as GameObject;
        popup.transform.localScale = new Vector3(0, 0, 0);
        MessagePopup msgp = popup.GetComponent<MessagePopup>();
        msgp.Setup(MessagePopup.MESSAGEPOPUP_TYPE.OK_CANCEL, title, text, target, okFunc,cancelFunc);

        m_popupQueue.Enqueue(popup);
        if (GetPopupCount() == 1)
            iTween.ScaleTo(popup, iTween.Hash("x", 1.0f, "y", 1.0f, "easetype", m_tween, "time", 0.3f));
    }

    // 모든 팝업은 이것으로 닫아야 함.
    public void ClosePopup(GameObject popup)
    {
        GameObject obj = m_popupQueue.Peek();

        if (obj == popup)
        {
            iTween.ScaleTo(popup, iTween.Hash("x", 0.0f, "y", 0.0f, "easetype", m_tween, "time", 0.3f, 
                "oncompletetarget", gameObject, "oncomplete", "PopupCloseEnd"));
        }
    }

    void PopupCloseEnd()
    {
        GameObject obj = m_popupQueue.Dequeue();
        PopupHide hide = obj.GetComponent<PopupHide>();

        if (hide != null)
            hide.HideEndEvent();
        GameObject.Destroy(obj);

        if (m_popupQueue.Count > 0)
        {
            obj = m_popupQueue.Peek();
            iTween.ScaleTo(obj, iTween.Hash("x", 1.0f, "y", 1.0f, "easetype", m_tween, "time", 0.5f));
        }
    }
}
