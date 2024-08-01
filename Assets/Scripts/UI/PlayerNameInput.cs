using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerNameInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _continueButton;

    private const string _playerName = "PlayerName";
    private string _name;

    public static string DisplayName { get; private set; }

    private void OnEnable()
    {
        SetInputField();
        SetPlayerName();
    }

    public void SetPlayerName()
    {
        _name = _inputField.text;
        _continueButton.interactable = !string.IsNullOrEmpty(_name);
    }

    public void SavePlayerName()
    {
        _name = _inputField.text;
        PlayerPrefs.SetString(_playerName, _name);
    }

    private void SetInputField()
    {
        if (PlayerPrefs.HasKey(_playerName))
        {
            _inputField.text = PlayerPrefs.GetString(_playerName);
        }
        else
        {
            _inputField.text = $"NewUser_{Random.Range(1000, 9999)}";
        }

        _name = _inputField.text;
    }
}
