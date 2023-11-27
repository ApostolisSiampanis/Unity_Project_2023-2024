#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;
using System;

namespace Pinwheel.Vista
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(IBiome))]
    public class BiomeVMConnector : MonoBehaviour
    {
        [SerializeField]
        protected string m_managerId;
        public string managerId
        {
            get
            {
                return m_managerId;
            }
            set
            {
                m_managerId = value;
            }
        }

        public IBiome GetBiome()
        {
            return GetComponent<IBiome>();
        }

        private void OnEnable()
        {
            VistaManager.collectFreeBiomes += OnCollectFreeBiome;
        }

        private void OnDisable()
        {
            VistaManager.collectFreeBiomes -= OnCollectFreeBiome;
        }

        private void OnCollectFreeBiome(VistaManager vm, Collector<IBiome> biomes)
        {
            if (string.Equals(vm.id, m_managerId))
            {
                biomes.Add(GetBiome());
            }
        }
    }
}
#endif
