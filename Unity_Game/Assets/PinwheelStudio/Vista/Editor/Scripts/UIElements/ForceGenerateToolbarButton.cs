#if VISTA
#if UNITY_2021_2_OR_NEWER
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;
using UnityEditor.Toolbars;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;

namespace Pinwheel.VistaEditor.UIElements
{
    [EditorToolbarElement(ForceGenerateToolbarButton.ID, typeof(SceneView))]
    public class ForceGenerateToolbarButton : EditorToolbarButton
    {
        public const string ID = "vista-force-generate-button";

        private static Texture2D s_forceGenerateTexture;
        private static Texture2D forceGenerateTexture
        {
            get
            {
                if (s_forceGenerateTexture == null)
                {
                    s_forceGenerateTexture = Resources.Load<Texture2D>("Vista/Textures/ForceGenerate");
                }
                return s_forceGenerateTexture;
            }
        }

        public ForceGenerateToolbarButton()
        {
            text = "";
            icon = forceGenerateTexture;
            tooltip = "Force re-generate all Vista Manager instances in the scene";
            clicked += OnClicked;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (VistaManager.HasActiveTask())
            {
                icon = WaitIconProvider.GetTexture();
            }
            else
            {
                icon = forceGenerateTexture;
            }
        }

        private void OnClicked()
        {
            VistaManager[] vms = GameObject.FindObjectsOfType<VistaManager>();
            for (int i = 0; i < vms.Length; ++i)
            {
                vms[i].ForceGenerate();
            }
        }
    }
}
#endif
#endif