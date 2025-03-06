using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

public class ObservableObject : MonoBehaviour
{
    public List<ObservableField> observedFields = new List<ObservableField>();

    void Awake()
    {
        DiscoverObservableFields();
    }

    void DiscoverObservableFields()
    {
        observedFields.Clear();
        var fields = gameObject.GetComponentsInChildren<MonoBehaviour>();

        foreach (var component in fields)
        {
            Type type = component.GetType();
            foreach (var field in type.GetFields(BindingFlags.Instance))
            {
                if (Attribute.IsDefined(field, typeof(ObservableProperty)))
                {
                    observedFields.Add(new ObservableField(component, field));
                }
            }
        }
    }
}

[Serializable]
public class ObservableField
{
    public string fieldName;
    public Component targetComponent;

    private FieldInfo fieldInfo;

    public ObservableField(Component component, FieldInfo field)
    {
        fieldName = field.Name;
        targetComponent = component;
        fieldInfo = field;
    }

    public object GetValue()
    {
        return fieldInfo.GetValue(targetComponent);
    }
}