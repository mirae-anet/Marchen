using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type {Heart};

    [Header("설정")]
    public Type type;
    [Range(1f, 100f)]
    public int value = 20;

    void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);
    }

    public Type getType()
    {
        return type;
    }

    public int getValue()
    {
        return value;
    }
}