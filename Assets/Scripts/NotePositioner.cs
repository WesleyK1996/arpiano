using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePositioner : MonoBehaviour
{
    public float ShouldBePosition;
    public bool IsFinished;

    // Update is called once per frame
    void Update()
    {
        if (ShouldBePosition > transform.localPosition.z)
        {
            Vector3 P = transform.localPosition;
            P.z += 0.05f;
            transform.localPosition = P;
        }
    }

    public void SetSharpKey(bool b)
    {
        transform.Find("Sharp").GetComponent<Renderer>().enabled = b;
    }

    public IEnumerator FinishKey()
    {
        // For now no fade out
        GetComponent<Renderer>().enabled = false;
        transform.Find("Sharp").GetComponent<Renderer>().enabled = false;

        yield return new WaitForSecondsRealtime(0.5f);

        IsFinished = true;
    }
}
