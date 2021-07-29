using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveNote : MonoBehaviour
{
    public float speed;
    public bool isLeft;

    void Update()
    {
        transform.Translate(transform.right * -speed);
        if (transform.localPosition.z >= 0)
        {
            if(isLeft)
            SheetManager.Instance.notesOnLeftSheet.Remove(gameObject);
            else
                SheetManager.Instance.notesOnRightSheet.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}
