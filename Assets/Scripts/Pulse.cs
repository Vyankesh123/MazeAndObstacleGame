using System.Collections;
using UnityEngine;

public class Pulse : MonoBehaviour
{
    public float popAmount = 1.12f;
    public float popSpeed = 8f;

    Vector3 baseScale;

    void Awake() { baseScale = transform.localScale; }

    public void DoPing()
    {
        StopAllCoroutines();
        StartCoroutine(PopAnim());
    }

    IEnumerator PopAnim()
    {
        var orig = baseScale;
        var big = orig * popAmount;

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
