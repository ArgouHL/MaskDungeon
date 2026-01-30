using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class TestCode : MonoBehaviour
{
  static  TestInput testInput;
    public SpriteRenderer sr;
    private void Awake()
    {
        testInput = new TestInput();
        testInput.TestInputs.Enable();
    }

    private void OnEnable()
    {
        testInput.TestInputs.A.performed += TestA;
    }
    private void OnDisable()
    {
        testInput.TestInputs.A.performed -= TestA;
    }

    private void TestA(InputAction.CallbackContext context)
    {
        Debug.Log("AAA");
        sr.color = UnityEngine.Random.ColorHSV();
    }
}
