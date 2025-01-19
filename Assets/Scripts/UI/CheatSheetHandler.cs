using UnityEngine;

public class CheatSheetHandler : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    
        private void OnEnable()
        {
            GameEvents.OnTogglePanel += TogglePanel;
        }
    
        private void OnDisable()
        {
            GameEvents.OnTogglePanel -= TogglePanel;
        }

        private void TogglePanel()
        {
            if (panel != null)
            {
                panel.SetActive(!panel.activeSelf);
            }
        }

}
