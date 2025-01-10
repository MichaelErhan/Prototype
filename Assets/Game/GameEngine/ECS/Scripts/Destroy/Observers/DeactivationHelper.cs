using UnityEngine;
using System.Collections;

public class DeactivationHelper : MonoBehaviour
{
    public void DeactivateAfterDelay(GameObject gameObject, float delay)
    {
        StartCoroutine(DeactivateCoroutine(gameObject, delay));
    }

    private IEnumerator DeactivateCoroutine(GameObject gameObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
