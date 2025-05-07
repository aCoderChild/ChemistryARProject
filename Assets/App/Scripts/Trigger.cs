using UnityEngine;

public class Trigger : MonoBehaviour
{
    [HideInInspector] public Element element;

    private void OnTriggerEnter(Collider other)
    {
        // If element is already assigned, ignore this trigger event
        if (element)
            return;

        // Get the Element component in the parent or the object that entered the trigger
        element = other.GetComponentInParent<Element>();

        if (element)
        {
            // Optionally log or perform any other logic on entry
            Debug.Log("Element entered trigger: " + element.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Only clear element if it matches the one that triggered the enter
        if (!element)
            return;

        // If the element exiting is the one that was assigned, nullify it
        Element exitingElement = other.GetComponentInParent<Element>();
        if (exitingElement == element)
        {
            element = null;
            Debug.Log("Element exited trigger: " + exitingElement.name);
        }
    }
}