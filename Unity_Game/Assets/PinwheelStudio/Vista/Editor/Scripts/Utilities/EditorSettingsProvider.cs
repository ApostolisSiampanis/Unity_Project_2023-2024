#if VISTA
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.VistaEditor
{
    public class EditorSettingsProvider : SettingsProvider
    {
        public EditorSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        //[SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            if (IsSettingsAvailable())
            {
                var provider = new EditorSettingsProvider("Project/Vista/Editor", SettingsScope.Project);
                return provider;
            }

            return null;
        }

        public static bool IsSettingsAvailable()
        {
            return EditorSettings.Get() != null;
        }

        public override void OnGUI(string searchContext)
        {
            EditorSettings settings = EditorSettings.Get();
            Editor editor = Editor.CreateEditor(settings);
            editor.OnInspectorGUI();
        }
    }
}
#endif
