using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Anonym.Isometric
{
    public class IsometricSortingOrderUtility
    {
        public static int IsometricSortingOrder(Transform _transform)
        {
            if (IsoMap.IsNull || !IsoMap.instance.bUseIsometricSorting)
                return 0;
            Vector3 v3Tmp = IsoMap.instance.fResolutionOfIsometric;
            return Mathf.RoundToInt(v3Tmp.x * _transform.position.x
                + v3Tmp.y * _transform.position.y - v3Tmp.z * _transform.position.z);
        }
    }
    
    [DisallowMultipleComponent]
    [ExecuteInEditMode][DefaultExecutionOrder(2)]
    public class IsometircSortingOrder : MonoBehaviour
    {
        const int Default_LastSortingOrder = int.MinValue;
        [SerializeField]
        int iLastSortingOrder = 0;
        public void Corrupt_LastSortingOrder()
        {
            iLastSortingOrder = Default_LastSortingOrder;
        }
        public bool Corrupted_LastSortingOrder()
        {
            return iLastSortingOrder == Default_LastSortingOrder;
        }

        [SerializeField]
        int iParticleSortingAdd = 0;

        List<SpriteRenderer> _dependentList = new List<SpriteRenderer>();
        List<SpriteRenderer> DependentList{get{
            update_Child();
            return _dependentList;
        }}

        List<Vector3> _particleLastPositionList = new List<Vector3>();
        List<ParticleSystemRenderer> _particleSystemRendererList = new List<ParticleSystemRenderer>();
        List<ParticleSystemRenderer> ParticleSystemRendererList{get{
            update_Child();
            return _particleSystemRendererList;
        }}
        

        bool bCorrupted = false;
        public void Corrupt_Child(bool bFlag)
        {
            bCorrupted = bFlag;
        }

        ParticleSystem.Particle[] particles = null;
        int setParticleArray(ParticleSystem _ps)
        {
            if (particles == null || particles.Length < _ps.main.maxParticles)
                particles = new ParticleSystem.Particle[_ps.main.maxParticles];
            return _ps.GetParticles(particles);
        }

        void update_particleSortingOrder(bool bJustDoIt = false)
        {
            Vector3 _rendererPosition;

            for (int i = 0 ; i < ParticleSystemRendererList.Count ; ++i)
            {
                _rendererPosition = ParticleSystemRendererList[i].transform.position;
                if (IsoMap.instance.bUseIsometricSorting)
                {
                    if (bJustDoIt || _particleLastPositionList[i] != _rendererPosition)
                    {
                        _particleLastPositionList[i] = _rendererPosition;
                        ParticleSystemRendererList[i].sortingOrder = _iExternAdd + iParticleSortingAdd +
                            IsometricSortingOrderUtility.IsometricSortingOrder(ParticleSystemRendererList[i].transform);
                    }
                }
            }
        }
        void update_Child(bool bJustDoIt = false)
        {
            if (bJustDoIt || bCorrupted)
            {
                _particleSystemRendererList.Clear();
                _particleLastPositionList.Clear();
                ParticleSystemRenderer[] _particlesSystemRenderers = transform.GetComponentsInChildren<ParticleSystemRenderer>();
                if (_particlesSystemRenderers != null)
                {
                    for (int i = 0 ; i < _particlesSystemRenderers.Length ; ++i)
                    {
                        _particleSystemRendererList.Add(_particlesSystemRenderers[i]);
                        _particleLastPositionList.Add(Vector3.zero);
                    }
                }

                _dependentList.Clear();

#if UNITY_EDITOR    
                _regularColliderList.Clear();
                
                Iso2DObject _iso2D;
                RegularCollider _rc;
#endif

                SpriteRenderer[] _sprrs = transform.GetComponentsInChildren<SpriteRenderer>();
                for(int i = 0 ; i < _sprrs.Length; ++i)
                {
#if UNITY_EDITOR                    
                    if ((_iso2D = _sprrs[i].GetComponent<Iso2DObject>()) != null)
                    {
                        if ((_rc = _iso2D.RC) != null)
                        {
                            if (!_regularColliderList.Exists(r => r == _rc))
                            {
                                _regularColliderList.Add(_rc);
                                continue;
                            }
                        }
                    }
#endif
                    _dependentList.Add(_sprrs[i]);
                }
            }
            bCorrupted = false;
        }

#if UNITY_EDITOR        
        List<RegularCollider> _regularColliderList = new List<RegularCollider>();
        List<RegularCollider> RegularColliderList{get{
            update_Child();
            return _regularColliderList;
        }}
#endif        

#region Dynamic_ISO
        void OnTransformChildrenChanged()
        {
            update_Child(true);
            Update_SortingOrder(true);
        }
        void OnTransformParentChanged()
        {
            Update_SortingOrder(true);
        }
        void OnEnable()
        {
            update_Child(true);
            Update_SortingOrder(true);
        }
        void Update()
        {
            if (transform.hasChanged)
            {
                update_SortingOrder();
                transform.hasChanged = false;
            }
            update_particleSortingOrder();
        }
#endregion

        [SerializeField]
        int _iExternAdd = 0;
        public int iExternAdd{set{_iExternAdd = value;}}
        public void Update_SortingOrder(bool bJustDoIt = false)
        {
            update_SortingOrder(bJustDoIt);
            update_particleSortingOrder(bJustDoIt);
        }
        void update_SortingOrder(bool bJustDoIt = false)
        {
            if (IsoMap.IsNull)
                return;

            bool isLastCalc = false;
            if (!IsoMap.instance.bUseIsometricSorting && !Corrupted_LastSortingOrder())
            {
                Corrupt_LastSortingOrder();
                isLastCalc = true;
            }

            if (IsoMap.instance.bUseIsometricSorting)
            {
                int _iNewSortingOrder = IsometricSortingOrderUtility.IsometricSortingOrder(transform) + _iExternAdd;
                if (bJustDoIt || _iNewSortingOrder != iLastSortingOrder)
                {
                    iLastSortingOrder = _iNewSortingOrder;
                }
            }

            if (IsoMap.instance.bUseIsometricSorting)
            {
                for (int i = 0; i < DependentList.Count; ++i)
                {                        
                    DependentList[i].sortingOrder = iLastSortingOrder + i;
                }
            }
            else if (isLastCalc || bJustDoIt)
            {
                for (int i = 0; i < DependentList.Count; ++i)
                {                        
                    DependentList[i].sortingOrder = 0;
                }
                
                for (int i = 0 ; i < ParticleSystemRendererList.Count ; ++i)
                {
                    ParticleSystemRendererList[i].sortingOrder = 0;
                }
            }
            
#if UNITY_EDITOR
            for(int i = 0 ; i < RegularColliderList.Count; ++i)
            {
                RegularColliderList[i].Update_SortingOrder();
            }
#endif
        }
    }
}