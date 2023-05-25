using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartBar : MonoBehaviour
{

    public Slider slider;
    public void SetMaxHP(byte HP)
    {
        slider.maxValue = HP;
    }

    public void SetSlider(byte HP)
    {
        slider.value = HP;
    }
}
