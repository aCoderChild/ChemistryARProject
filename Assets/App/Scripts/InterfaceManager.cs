using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    public void React()
    {
        // Access the objects from ObjectLibrary using Instance and Objects
        foreach (KeyValuePair<string, GameObject> element in ObjectLibrary.Instance.Objects)
        {
            element.Value.GetComponent<Element>().React();
        }
    }

    public void Reset()
    {
        // Access the objects from ObjectLibrary using Instance and Objects
        foreach (KeyValuePair<string, GameObject> element in ObjectLibrary.Instance.Objects)
        {
            element.Value.GetComponent<Element>().Reset();
        }
    }
}