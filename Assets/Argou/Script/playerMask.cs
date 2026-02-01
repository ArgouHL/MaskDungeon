using UnityEngine;
using UnityEngine.UI;

public class playerMask : MonoBehaviour
{
   [SerializeField] GameObject[] masks;

    public void ChangeMask(int target)
    {
        target -= 1;
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

        StartCoroutine(FindObjectOfType<MaskManager>().MaskIconChange(target));
    }
}
