using System;
using UnityEngine;

public class GridObjectDataSheet : MonoBehaviour
{
    public bool CanWalkOver;
    [SerializeField] UnityEngine.Object _autoInteract;

    public bool IsAutoInteractable
    {
        get { return AutoInteract != null; }
    }

    public IInteractableGridObject AutoInteract { 
        get 
        { 
            if(_autoInteract == null)
                return null;
            if (_autoInteract is IInteractableGridObject) 
                return _autoInteract as IInteractableGridObject; 
            else throw new ArgumentException("Invalid Argument"); 
        }
    }
}
