using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI {
    
    public interface RobotHPUpdateEvent
    {
        void HPUpdate(int curHP,int maxHP);
    }

    public interface CharacterHPUpdateEvent
    {
        void HPUpdate(int curHP , int maxHP);
    }

    public interface MonsterHPUpdateEvent
    {
        void HPUpdate(int curHP , int maxHP);
    }

    public interface ENERGYUpdateEvent
    {
        void EnergyUpdate(float curEnergy);
    }
}
