using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputService : ServiceBase
{
    [SerializeField] private Joystick _joystick;
    [SerializeField] private Button _bombButton;
    [SerializeField] private List<GameObject> _visuals = new List<GameObject>();
    [SerializeField] private PlayerBombVisualService _bombVisual;

    private bool _disableControll = true;
    private PlayerController _controller;
    private BombService _bombService;
    private Player _player;
    private Vector3 _movementInput;

    private void Start()
    {
        _bombButton.onClick.AddListener(PlaceBomb);
        ServiceLocator<IService>.Instance.Register(this);
    }

    private void Update()
    {
        if (_controller == null)
        {
            Debug.Log("Null controller");
            return;
        }
        if (_disableControll) return;

        _movementInput.x = _joystick.Horizontal;
        _movementInput.z = _joystick.Vertical;

        if(Input.GetKey(KeyCode.W))
        {
            _movementInput = new Vector3(0f, 0f, 1f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            _movementInput = new Vector3(0f, 0f, -1f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            _movementInput = new Vector3(-1f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            _movementInput = new Vector3(1f, 0f, 0f);
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            PlaceBomb();
        }

        MoveInput(_movementInput);
    }

    public void SetPlayerController(PlayerController controller)
    {
        _controller = controller;
    }

    public void SetPlayer(Player player)
    {
        _player = player;
        _bombService = _player.GetBombService();
    }

    public void DisableControll()
    {
        _disableControll = true;
        _movementInput = Vector3.zero;
        _joystick.SetToZero();
        foreach (var item in _visuals)
        {
            item.SetActive(false);
        }
    }

    public PlayerBombVisualService GetBombVisual() => _bombVisual;

    public void EnableControll()
    {
        _disableControll = false;

        foreach (var item in _visuals)
        {
            item.SetActive(true);
        }
    }

    private void PlaceBomb()
    {
        if (_player == null) return;
        if (_disableControll) return;

        _bombService.CmdPlaceBomb(_player);
    }

    private void MoveInput(Vector3 inputKeyboard)
    {
        _controller.CmdMovePlayer(inputKeyboard);
        _controller.SetMovementVector(inputKeyboard);
    }
}
