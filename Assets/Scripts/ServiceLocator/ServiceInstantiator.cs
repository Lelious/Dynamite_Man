using UnityEngine;

public class ServiceInstantiator : MonoBehaviour
{
    [SerializeField] private InputService _inputService;


    private void Awake()
    {
        InitializeInputService();
    }

    private void InitializeInputService() => ServiceLocator<IService>.Instance.Register(_inputService);
}
