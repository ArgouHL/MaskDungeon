using UnityEngine;

public class playerMask : MonoBehaviour
{
   [SerializeField] GameObject[] masks;
    public void ChangeMask(int target)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i != target)
            {
                masks[i].SetActive(false);
            }
            else
            {
                masks[i].SetActive(true);
            }
        }
    }
}
