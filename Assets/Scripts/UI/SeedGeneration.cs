using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SeedGeneration : MonoBehaviour
{
    public Island Generator;
    public TMP_InputField InputField;

    public void changeSeed()
    {

        string newSeed = InputField.text;

        if (string.IsNullOrEmpty(newSeed) || newSeed.Contains("-"))
        {
            newSeed = "0";
        }
        Generator.seed = int.Parse(newSeed);
        Generator.OnValidate();
    }

    public void randomSeed()
    {
        int newSeed = Random.Range(0, 999999);
        InputField.text = newSeed.ToString();

    }

}
