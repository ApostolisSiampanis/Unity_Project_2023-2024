#if VISTA
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.Graph
{
    public class TerrainGraphViewportContainer : VisualElement
    {
        private static readonly string KEY_HEIGHT = "height";
        public TerrainGraphViewportContainer()
        {
            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Graph/ViewportContainer");
            styleSheets.Add(uss);

            AddToClassList("viewport-container");
        }

        public void OnEnable()
        {
            string typeName = this.GetType().Name;

            float h = EditorPrefs.GetFloat(typeName + KEY_HEIGHT, -1);
            if (h > 0)
            {
                style.height = new StyleLength(h);
            }
            else
            {
                style.height = new StyleLength(new Length(50, LengthUnit.Percent));
            }
        }

        public void OnDisable()
        {
            string typeName = this.GetType().Name;
            if (resolvedStyle != null)
            {
                float h = resolvedStyle.height;
                EditorPrefs.SetFloat(typeName + KEY_HEIGHT, h);
            }
        }

    }
}
#endif
