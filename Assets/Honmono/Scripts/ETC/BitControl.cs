using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitControl {

    public static bool Get(int num,int idx)
    {
        return ((1 << idx) & num) != 0; // 0010  1    10 10   = 1   true.
    }

    public static int Set(int num,int idx)
    {
        return num | (1 << idx);
    }

    public static int Clear(int num,int idx)
    {
        int mask = ~(1 << idx);
        return num & mask;
    }

}
