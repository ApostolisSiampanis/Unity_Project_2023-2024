using TMPro;
using UnityEngine;

namespace Farm.Scripts.Interaction_System
{
    public class InteractHint : MonoBehaviour
    {
        public string _hintMessage;
        private TextMeshProUGUI textMesh;

        public void Start()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
            if (textMesh == null) Debug.LogWarning(name + ": Text Mesh Pro not found.");
        }

        public void SetHintMessage(string hintMessage)
        {
            _hintMessage = hintMessage;
            if (textMesh != null) textMesh.text = _hintMessage;
        }
    
    }
}
