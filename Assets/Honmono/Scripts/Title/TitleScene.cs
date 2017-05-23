using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScene : MonoBehaviour {

    void Start()
    {
        SoundManager.Instance().PlayBGM(0);
    }

    // 타이틀에서 처리할 것들을 이 클래스에서 처리한다
    public void TitleAnimationEnd()
    {
        this.GetComponent<Animator>().enabled = false;
        PopupManager.Instance().AddPopup("LoginPopup");
    }

    public void TitleEnd()
    {
        PopupManager.Instance().AddPopup("CharacterSelectPopup");
    }

}
