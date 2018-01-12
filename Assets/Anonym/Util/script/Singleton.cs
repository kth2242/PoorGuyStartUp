using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anonym.Util
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T instance
        {
            get
            {
                return IsNull ? null : _instance;
            }
        }
        public static bool IsNull{  
            get{
                if (_instance == null)
                    _instance = FindObjectOfType<T>();

                return _instance == null;
            }
        }

        protected virtual void Awake()
        {
            if (this != instance)
            {
                GameObject obj = this.gameObject;
                Destroy(this);
                Destroy(obj);
                return;
            }
        }
    }
}

