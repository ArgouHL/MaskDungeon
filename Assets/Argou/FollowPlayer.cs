using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField]private Transform playerTrans;

   

    private void Update()
    {
        transform.position = playerTrans.position;

    }
}
