using UnityEngine;
using System.Collections;

public class BearAnimation : MonoBehaviour {

    void OnComplete()
    {
        Destroy(gameObject);
    }

}
