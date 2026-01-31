using System;
using UnityEngine;

public class DoorControl : MonoBehaviour
{
    [SerializeField]private GameObject door;
    [SerializeField] private Light wayLight;

    private void Awake()
    {
        wayLight.intensity = 0;
    }
    internal void Close()
    {
        door.SetActive(true);
    }

    internal void Open(bool showWay)
    {
        door.SetActive(false);
        if (showWay)
        {
            LeanTween.value(0, 40, 2).setOnUpdate((float val) => wayLight.intensity = val);
        }
    }
    internal void ShowWay()
    {
        door.SetActive(false);
    }
}
