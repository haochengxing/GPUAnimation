using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T:new ()
{
    private static T mInstance = default(T);
    private static object mLock = new object();

    public static T Instance
    {
        get
        {
            lock (mLock)
            {
                if (mInstance == null)
                {
                    mInstance = new T();
                }
                return mInstance;
            }
        }
    }
}
