using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameNameInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _continueButton;

    private string _name;

    private void OnEnable()
    {
        SetGameName();
    }
    public string GetName() => _inputField.text;

    public void SetGameName()
    {
        _name = _inputField.text;
        _continueButton.interactable = !string.IsNullOrEmpty(_name);
    }
}
