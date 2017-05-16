using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI {

    public interface HPUpdateEvent
    {
        void HPUpdate(int curHP,int maxHP);
    }
}
