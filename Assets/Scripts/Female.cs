using UnityEngine;
using System.Collections;

public class Female : MonoBehaviour, Human.HumanDelegate {

    // 次の出産が可能になるまでのインターバル
    public float bearInterval = 1.0f;

    private GameObject humanPrefab;
    private GameObject bearAnimationPrefab;
    private Human human;
    private Animator animator;

    public string TsReason { get; set; }

    // 前の出産時刻（ゲーム開始からの経過秒数）
    float latestBaerTime;

    // インスタンス化した時点でないと困るものを初期化（手動呼び出し）
    public void Initialize()
    {
        human.Sex = Global.Sex.Female;
        human.ColoredFullName =  "<color=#FFAAFF>" + human.FullName + "</color>";
    }

    void Awake()
    {
        // AddComponentした場合インスペクタからのPrefab紐付けは無効？
        // ぬるりになるので動的にロードする
        humanPrefab = (GameObject)Resources.Load("Human");
        bearAnimationPrefab = (GameObject)Resources.Load("BearAnimation");

        human = GetComponent<Human>();
        animator = GetComponent<Animator>();

        human.humanDelegate = this;
    }

    void Start()
    {
        human.NextDestination();
    }

    // 出産判定
    void OnTriggerEnter2D(Collider2D c)
    {
        Human cHuman = c.GetComponent<Human>();

        // 女の子と衝突した場合は出産不可
        if (cHuman.Sex == Global.Sex.Female)
        {
            return;
        }

        //Debug.Log("id: " + human.Id + " father: " + human.FatherId + " mother: " + human.MotherId + " maleId: " + cHuman.Id + " maleMother: " + cHuman.MotherId + " maleFather: " + cHuman.FatherId);
        // 出産直後に産んだ子と接触してまた出産するのを防ぐ
        if (Time.time - latestBaerTime < bearInterval)
        {
            return;
        }

        bool isParentChild = human.IsParentChild(cHuman);
        bool isBrother = human.IsBrother(cHuman);

        // 血が濃いと交配不可
        if (!human.CanCross(cHuman))
        {
            string relation = "";
            if (isParentChild)
            {
                relation = "親子で";
            }
            else if (isBrother)
            {
                relation = "兄弟で";
            }

            Global.Log.Middle(string.Format("{0}と{1}は{2}交配したが子をなせなかった", human.ColoredFullName, cHuman.ColoredFullName, relation));

            return;
        }

        Bear(cHuman);

    }

    private void Bear(Human father)
    {
        GameObject childObj = (GameObject)Instantiate(humanPrefab, gameObject.transform.position, gameObject.transform.rotation);
        Human child = childObj.GetComponent<Human>();
        child.FamilyName = human.FamilyName;
        child.FirstName = Util.RandomElment(Global.FirstNames);
        child.MotherId = human.Id;
        child.FatherId = father.Id;
        child.Generation = Mathf.Max(human.Generation, father.Generation) + 1;
        child.Gene = IntermixGene(human.Gene, father.Gene);
        childObj.GetComponent<Male>().Initialize();

        GameObject bearObj = (GameObject)Instantiate(bearAnimationPrefab, childObj.transform.position, childObj.transform.rotation);
        bearObj.transform.parent = childObj.transform;

        animator.SetTrigger("Bear");

        latestBaerTime = Time.time;

        Global.Log.Low(string.Format("{0}と{1}の子{2}が誕生した", human.ColoredFullName, father.ColoredFullName, child.ColoredFullName));
    }

    private int IntermixGene(int gene1, int gene2)
    {
        int newGene = 0;

        for (int i = 0; i < Global.GENE_LENGTH; i++)
        {
            int adopted = 0;
            if (Random.Range(0, 2) == 0)
            {
                adopted = gene1 & 0x0F;
            }
            else
            {
                adopted = gene2 & 0x0F;
            }

            newGene |= adopted << 4 * i;

            gene1 >>= 4;
            gene2 >>= 4;
        }

        return newGene;
    }

    public Human DecideLover()
    {
        Male lover = Util.RandomElment(FindObjectsOfType<Male>());
        if (lover == null) return null;
        return lover.GetComponent<Human>();
    }

}
