using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINumber : MonoBehaviour {

    //-- Number Font 용 ----------------------------------------//
    public List<Sprite> m_numberList = new List<Sprite>();
    public List<Image> m_uiNumberList = new List<Image>();

    private int m_data = 0;

    public int NUMBER
    {
        get { return m_data; }
        set { m_data = value; RefreshImage(); }
    }
    //----------------------------------------------------------//

    void RefreshImage()
    {
        char[] numbers = m_data.ToString().ToCharArray();

        if (m_uiNumberList.Count <= numbers.Length)
        {
            for(int i = 0; i < m_uiNumberList.Count; i++)
            {
                m_uiNumberList[i].sprite = GetImageNumber(numbers[i]);
            }
        }
    }

    Sprite GetImageNumber(int num)
    {
        if (num < 0 || num >= 10)
            return null;

        return m_numberList[num];
    }
}
