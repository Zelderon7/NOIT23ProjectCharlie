using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VariableTypes
{
    Num,
    Bool,
}

public class ArgHolder : MonoBehaviour
{
    [SerializeField]
    VariableTypes type;

    [SerializeField]
    string _defaultValue;

    VariableArgument _variable;

    public string Value { get => _variable != null? _variable.Value : _defaultValue; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<VariableArgument>(out VariableArgument variable))
        {
            if (variable.Holder != null)
                return;
            if (variable.Type != type)
                return;
            
            variable.Holder = this;
            _variable = variable;
            variable.transform.parent = transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out VariableArgument variable))
        {
            if(variable == _variable)
            {
                _variable = null;
                variable.Holder = null;
            }
        }
    }
}
