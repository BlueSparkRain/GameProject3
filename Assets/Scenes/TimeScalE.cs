using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeScalE : MonoBehaviour
{
    public TMP_Text CurrrentScaleText;
    public Button scaleButton;
    // Start is called before the first frame update
    void Start(){
        scaleButton.onClick.AddListener(Scale);
    }

    int currentLevel=1;
    void Scale()
    {
        if (currentLevel + 1 <= 3){
            currentLevel += 1;
        }
        else { 
            currentLevel = 1;
        }
        float scale = Mathf.Pow(currentLevel, 2);
        Time.timeScale=scale;
        CurrrentScaleText.text ="x"+ scale.ToString();
    }

}
