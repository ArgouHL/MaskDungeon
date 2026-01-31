using UnityEngine;

public class EnterTrigger : MonoBehaviour
{
    public LayerMask playerLayerMask;
    public RoomControl roomControl;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if ((playerLayerMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            roomControl.RoomStart();
           // Debug.Log("enter");
        }
        else
        {
           //  Debug.Log("enter");
        }
        
    }
}
