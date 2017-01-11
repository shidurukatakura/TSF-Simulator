using UnityEngine;
using System.Collections;

public class Male : MonoBehaviour, Human.HumanDelegate
{

    public GameObject tsAnimation;

    public float tsRate = 0.06f;

    private Human human;
    private Animator animator;

    // インスタンス化した時点でないと困るものを初期化（手動呼び出し）
    public void Initialize()
    {
        human.Sex = Global.Sex.Male;
        human.ColoredFullName = "<color=#00FFFF>" + human.FullName + "</color>";
    }

    void Awake()
    {
        human = GetComponent<Human>();
        animator = GetComponent<Animator>();

        human.humanDelegate = this;
    }

    void Start()
    {
        StartCoroutine(JudgeTs());
    }

    // TS判定
    IEnumerator JudgeTs()
    {
        yield return new WaitForSeconds(1);

        while (true)
        {
            int humanCount = FindObjectsOfType<Human>().Length;
            //float adjustRate = -1.0f / (Mathf.Pow(1.3f, -(humanCount / 4.0f + 12)) + 1.05f); // -1/(1.3^(-(x/4+12)))+1.05
            float adjustRate = 0.9f / (1 + Mathf.Exp(humanCount / 6.0f - 5)) + 0.1f;
            float adjustedTsRate = tsRate * adjustRate;
            //Debug.Log("hc: " + humanCount + " rate: " + adjustRate);
            if (Random.value < adjustedTsRate)
            {
                TsStart();
            }

            yield return new WaitForSeconds(1);
        }
    }

    public void TsStart(string tsReason = null)
    {
        iTween.Stop(gameObject, "move");
        iTween.Stop(gameObject, "birth");
        gameObject.transform.localScale = new Vector3(3, 3, 1);

        GameObject tsObj = (GameObject)Instantiate(tsAnimation, gameObject.transform.position, gameObject.transform.rotation);
        TsAnimation ts = tsObj.GetComponent<TsAnimation>();
        ts.transform.parent = gameObject.transform;

        string maleColoredFullName = human.ColoredFullName;

        Female female = gameObject.AddComponent<Female>();
        female.TsReason = (tsReason == null) ? Util.RandomElment(Global.TsReasons) : tsReason;
        female.Initialize();

        animator.SetInteger("Sex", (int)Global.Sex.Female);
        
        Global.Log.High(string.Format("{0}は{1}ため女の子になった", maleColoredFullName, female.TsReason));

        Destroy(this);
    }

    public Human DecideLover()
    {
        Female lover = Util.RandomElment(FindObjectsOfType<Female>());
        if (lover == null) return null;
        return lover.GetComponent<Human>();
    }
}