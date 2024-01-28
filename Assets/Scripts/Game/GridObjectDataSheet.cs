using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObjectDataSheet : MonoBehaviour
{
    [SerializeField]
    public bool CanWalkOver;
    [SerializeField]
    private UnityEngine.Object autoInteract;

    public bool IsAutoInteractable
    {
        get { return AutoInteract != null; }
    }

    public IInteractableGridObject AutoInteract { 
        get 
        { 
            if(autoInteract == null)
                return null;
            if (autoInteract is IInteractableGridObject) 
                return autoInteract as IInteractableGridObject; 
            else throw new ArgumentException("Invalid Argument"); 
        }
    }
}
