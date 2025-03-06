using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observable
{
    public string label;
    private Func<object> getValue; // Function to retrieve current value

    public Observable(string label, Func<object> getValue)
    {
        this.label = label;
        this.getValue = getValue;
    }

    public object GetValue() => getValue();
}

public class ObservableProperty : PropertyAttribute { }
