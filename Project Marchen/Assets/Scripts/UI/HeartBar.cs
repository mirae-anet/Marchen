using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// @brief HP를 표시하는 Bar.
public class HeartBar : MonoBehaviour
{

    public Slider slider;
    /// @brief HP 최댓값 설정.
    public void SetMaxHP(int HP)
    {
        slider.maxValue = HP;
    }

    /// @brief 변화한 HP값에 맞춰 Bar의 slider를 갱신
    public void SetSlider(int HP)
    {
        slider.value = HP;
    }
}
