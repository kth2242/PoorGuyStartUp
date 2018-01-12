using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anonym.Isometric
{
    [System.Serializable]
    public struct Attachment{
        [SerializeField]
        public Iso2DObject Iso2DObj;        
        [SerializeField]
        public int indentLevel;

        public bool Init(GameObject _obj, int _indentLevel)
        {
            Clear();
            indentLevel = _indentLevel;
            Iso2DObj = _obj.GetComponentInChildren<Iso2DObject>();;
            return Iso2DObj != null;
        }
        public void Clear()
        {
            indentLevel = 0;
            Iso2DObj = null;
        }
    }

    [System.Serializable]
    public class AttachmentHierarchy
    {
        [SerializeField]
        public bool bFoldout = true;
        [SerializeField]
        public List<Attachment> childList = new List<Attachment>();

        public AttachmentHierarchy()
        {
            Clear();
        }

        public bool Init(GameObject _obj, int _indentLevel = 0)
        {
            Clear();
            
            bool bResult = false;
                    
            for(int i = 0 ; i < _obj.transform.childCount; ++i)
            {
                if (AddChild(_obj.transform.GetChild(i).gameObject, _indentLevel + 1))
                {
                    bResult = true;
                }
            }

            return bResult;
        }
        bool AddChild(GameObject _childObject, int _indentLevel)
        {
            if (childList.Exists(r => r.Iso2DObj.gameObject == _childObject))
                return false;
            Attachment _child = new Attachment();
            bool bResult = _child.Init(_childObject, _indentLevel + 1);
            if (bResult)
                childList.Add(_child);
            return bResult; 
        }
        public void Clear()
        {
            bFoldout = true; 
            childList.Clear();
        }
    }
}