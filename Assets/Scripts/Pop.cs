using UnityEngine;

public class Pop : MonoBehaviour
{
    public float popAmount = 1.2f;
    public float popSpeed = 10f;

    public void DoPop()
    {
        StopAllCoroutines();
        StartCoroutine(PopAnim());
    }

    System.Collections.IEnumerator PopAnim()
    {
        Vector3 orig = Vector3.one;
        Vector3 big = orig * popAmount;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * popSpeed;
            transform.localScale = Vector3.Lerp(orig, big, t);
            yield return null;
        }

        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * popSpeed;
            transform.localScale = Vector3.Lerp(big, orig, t);
            yield return null;
        }
    }
}
