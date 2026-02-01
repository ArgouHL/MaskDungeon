using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    public LayerMask playerLayerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if ((playerLayerMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            Menu.instance.GameWin();
           // Debug.Log("enter");
        }
       
    }
}
