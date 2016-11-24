using UnityEngine;
using System.Collections;

public class DeathAnimation : MonoBehaviour {

    void OnComplete()
    {
        Destroy(gameObject);
    }

}
