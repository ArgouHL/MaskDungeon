using UnityEngine;

public class MaskBehaviour : MonoBehaviour
{
    private int type = 0; // 1: energy ball, 2: sector, 3:bomb, 4:rush
    private void Start()
    {
        var ps=GetComponentInChildren<ParticleSystem>();
        ps.Stop();
        ps.Play();

    }

    public void SetType(int newtype)
    {
        type = newtype;
    }

    public int GetType()
    {
        return type;
    }

 
    void OnTriggerEnter(Collider other)
    {
        PlayerBehavior pb = other.GetComponent<PlayerBehavior>();
        if(pb != null)
        {
            pb.AddAttackType(type);
            Destroy(gameObject);
        }
    }
}
