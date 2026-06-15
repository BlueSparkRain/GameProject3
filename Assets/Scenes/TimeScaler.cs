using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeScaler : MonoBehaviour
{
    public Text CurrrentScaleText;
    public Button scaleButton;
    void Start(){
        scaleButton.onClick.AddListener(Scale);
    }

    int currentLevel=1;
    void Scale()
    {
        if (currentLevel + 1 <= 6){
            currentLevel += 1;
        }
        else { 
            currentLevel = 1;
        }
        float scale = Mathf.Pow(2, currentLevel - 1);
        Time.timeScale = scale;
        CurrrentScaleText.text = "x" + scale.ToString();
    }

}
