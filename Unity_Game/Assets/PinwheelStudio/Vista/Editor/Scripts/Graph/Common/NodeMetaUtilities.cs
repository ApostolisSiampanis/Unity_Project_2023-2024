#if VISTA
using System;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    public static class NodeMetaUtilities
    {
        public static string ParseDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                return string.Empty;
            }

            try
            {
                string ss0 = "<ss>";
                string ss1 = "</ss>";

                if (SearcherUtils.IsSmartSearchSupported())
                {
                    string s = description.Replace(ss0, "").Replace(ss1, "");
                    return s;
                }
                else
                {
                    int startIndex = description.IndexOf(ss0);
                    int length = description.IndexOf(ss1) + ss1.Length - startIndex;
                    if (startIndex >= 0 && startIndex < description.Length && length > 0)
                    {
                        string s = description.Remove(startIndex, length);
                        return s;
                    }
                    else
                    {
                        return description;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to parse node description: {description}. Error: {e.ToString()}");
                return description;
            }
        }
    }
}
#endif
