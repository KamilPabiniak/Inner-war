using System;
using UnityEngine;

public class PlayerCheatSheet : PlayerModule
{
    private PlayerInput _input;

    private void Start()
    {
        _input = GetComponent<PlayerInput>();
    }

    private void LateUpdate()
    {
        if (_input.IsEscapePressed)
        {
            GameEvents.OnTogglePanel.Invoke();
            _input.ResetEscapePressed();
        }
    }
}
