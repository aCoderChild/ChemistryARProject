using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Element : MonoBehaviour
{
    public string symbol = ""; // Ensure this is set in the Inspector

    [Space(5)]
    public bool isReactive = false;
    public List<GameObject> molecules = new List<GameObject>();
    [Space(5)]

    [HideInInspector] public bool reacted = false;

    [HideInInspector] public GameObject atom = null;
    [HideInInspector] public Trigger[] triggers = null;
    [HideInInspector] public Dictionary<string, GameObject> reactions = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // Setup rigidbody properties
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        // Get atom gameobject (always first in tree, so index = 0)
        if (transform.childCount > 0)
            atom = transform.GetChild(0).gameObject;

        // Get all triggers in children objects
        triggers = GetComponentsInChildren<Trigger>();

        // Build reactions dictionary
        foreach (var molecule in molecules)
        {
            if (molecule != null)
            {
                reactions.Add(molecule.name, molecule);
            }
            else
            {
                Debug.LogWarning("Found null molecule in reactions list, skipping...");
            }
        }
    }

    public void React()
    {
        if (isReactive && triggers.Length > 1)
        {
            reacted = true;
            List<string> symbols = new List<string>();

            // Get symbols of the connected elements
            foreach (var trigger in triggers)
            {
                if (trigger.element != null)
                    symbols.Add(trigger.element.symbol);
            }

            // Reaction logic (Example: H2O reaction)
            // if (symbols.Count == 2 && symbols[0] == "H" && symbols[1] == "H")
            // {
            //     atom.SetActive(false);
            //     triggers[0].element.atom.SetActive(false);
            //     triggers[1].element.atom.SetActive(false);

            //     if (reactions.ContainsKey("H2O"))
            //         reactions["H2O"].SetActive(true);
            // }
            if (symbol == "O") {
                atom.SetActive(false);
                triggers[0].element.atom.SetActive(false);
                triggers[1].element.atom.SetActive(false);

                if (reactions.ContainsKey("H2O"))
                    reactions["H2O"].SetActive(true);
            } else if (symbol == "C") {
                atom.SetActive(false);
                triggers[0].element.atom.SetActive(false);
                triggers[1].element.atom.SetActive(false);

                if (reactions.ContainsKey("CO2"))
                    reactions["CO2"].SetActive(true);
            } else if (symbol == "H") {
                atom.SetActive(false);
                triggers[0].element.atom.SetActive(false);
                
                if (reactions.ContainsKey("HCl"))
                    reactions["HCl"].SetActive(true);
            }  else if (symbol == "Na") {
                atom.SetActive(false);
                triggers[0].element.atom.SetActive(false);
        

                if (reactions.ContainsKey("NaCl"))
                    reactions["NaCl"].SetActive(true);
            } 
            else
            {
                Debug.LogWarning("No valid reaction found for: " + symbol);
            }
        }
    }

    public void Reset()
    {
        if (reacted)
        {
            reacted = false;

            // Reactivate atoms and hide the reactions
            if (atom != null)
                atom.SetActive(true);

            if (triggers.Length > 0)
            {
                triggers[0].element.atom.SetActive(true);
                triggers[1].element.atom.SetActive(true);
            }

            if (reactions.ContainsKey("H2O"))
                reactions["H2O"].SetActive(false);
        }
    }
}