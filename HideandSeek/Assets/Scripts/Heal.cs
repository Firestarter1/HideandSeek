using UnityEngine;
using System.Collections;

public class HealthPack : MonoBehaviour
{
    enum HealType { Moving, Stationary, HOT}
    [SerializeField] HealType type;
    Rigidbody rb;

    [SerializeField] int healAmount;
    [SerializeField] float healRate;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

    bool isHealing;

    private void Start()
    {
        if (type == HealType.Moving)
        {
            Destroy(gameObject, destroyTime);
            rb.linearVelocity = transform.forward * speed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;
        IHeal heal = other.GetComponent<IHeal>();

        if (heal != null && type != HealType.HOT)
        {
            heal.Heal(healAmount);
            Destroy(gameObject);
        }
    }

    // Heal over time
    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
            return;

        IHeal heal = other.GetComponent<IHeal>();

        if (heal != null && type == HealType.HOT)
        {
            if (!isHealing)
            {
                StartCoroutine(healOther(heal));
            }
        }
    }

    IEnumerator healOther(IHeal h)
    {
        isHealing = true;
        h.Heal(healAmount);
        yield return new WaitForSeconds(healRate);
        isHealing = false;
    }
}
