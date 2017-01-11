using UnityEngine;
using System.Collections;

public class Earth : MonoBehaviour {

    public int maxHp = 3000;
    public int regeneration = 20;

    public int Hp { get; private set; }

    private void Awake()
    {
        Reset();
    }

    public void Reset()
    {
        Hp = maxHp;
        StartCoroutine(CheckDead());
    }

    IEnumerator CheckDead()
    {
        while (true)
        {
            Human[] humans = FindObjectsOfType<Human>();
            Hp += regeneration - humans.Length;

            if (Hp > maxHp) Hp = maxHp;

            if (Hp < 0)
            {
                //StartCoroutine(WrathOfTheEarth());
                WrathOfTheEarth();
                yield break;
            }

            yield return new WaitForSeconds(1);
        }
    }

    //IEnumerator WrathOfTheEarth()
    //{
    //    while (true)
    //    {
    //        Male[] males = FindObjectsOfType<Male>();

    //        if (males.Length == 0) yield break;

    //        Male male = males[0];
    //        Human human = male.GetComponent<Human>();
    //        male.TsStart(tsReason: "地球環境を汚染しすぎた");

    //        yield return new WaitForSeconds(0.1f / males.Length);
    //    }
    //}

    void WrathOfTheEarth()
    {
        Male[] males = FindObjectsOfType<Male>();
        foreach (Male male in males) {
            Human human = male.GetComponent<Human>();
            male.TsStart(tsReason: "地球環境を汚染しすぎた");
        }
    }
}
