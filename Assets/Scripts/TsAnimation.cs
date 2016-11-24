using UnityEngine;
using System.Collections;

public class TsAnimation : MonoBehaviour {

    public void OnTsMidpoint()
    {
    }

    public void OnTsComplete()
    {
        Destroy(gameObject);
    }
}
