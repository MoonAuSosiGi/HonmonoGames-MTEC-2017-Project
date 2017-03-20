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

    Resolution m_resoluation = new Resolution(1920,1080);

    //---------------------------------------------------------------//

    void Start()
    {
        Screen.SetResolution(m_resoluation.width, m_resoluation.height,false);
    }
}
