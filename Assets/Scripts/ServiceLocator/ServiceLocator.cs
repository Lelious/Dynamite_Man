using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator<T> : IServiceLocator<T>
{
    public static ServiceLocator<T> Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ServiceLocator<T>();
            }
            return _instance;
        }
    }

    private static ServiceLocator<T> _instance;
    protected Dictionary<Type, T> _serviceMap { get; }
    
    public ServiceLocator()
    {
        _serviceMap = new Dictionary<Type, T>();
        _instance = this;
    }

    public TP Get<TP>() where TP : T
    {
        if (_serviceMap.TryGetValue(typeof(TP), out T service))
        {
            return (TP)service;
        }
        else
        {
            throw new NotImplementedException($"ServiceLocator not contains {(TP)service}");
        }
    }

    public TP Register<TP>(TP newService) where TP : T
    {
        var type = newService.GetType();

        if (_serviceMap.TryAdd(type, newService))
        {
            Debug.Log($"Registered {typeof(TP)}");
            return newService;
        }
        else
        {
            throw new NotImplementedException($"ServiceLocator already contains {newService}");
        }
    }

    public void Unregister<TP>(TP service) where TP : T
    {
        var type = service.GetType();

        if (_serviceMap.ContainsKey(type))
        {
            _serviceMap.Remove(type);
        }
    }
}
