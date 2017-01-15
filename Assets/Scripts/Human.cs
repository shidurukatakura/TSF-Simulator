using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UniRx;

public class Human : MonoBehaviour {

    /* 定数 */
    public static int NO_PARENT = 0;

    /* コードで取得するコンポーネント */
    protected Animator animator;

    /* インスペクタで設定するコンポーネント */
    public GameObject deathAnimation;
    public Text lifeText;
    public Text statusText;

    /* インスペクタで設定する設定値 */
    public float speed = 1.5f;
    public int life = 15;

    /* プロパティ */
    public int Id { get; set; }
    public int MotherId { get; set; }
    public int FatherId { get; set; }
    public string FamilyName { get; set; }
    public string FirstName { get; set; }
    public int Generation { get; set; }
    public Global.Sex Sex { get; set; }
    public string ColoredFullName { get; set; }
    public HumanDelegate humanDelegate { get; set; }
    public Vector3 Destination { get; set; }
    public int Gene { get; set; }
    public float BirthTime { get; set; }

    public float Age { get { return Time.time - BirthTime; } }

    public Subject<string> statusSubject = new Subject<string>();

    void Awake()
    {
        Id = Global.nextHumanId++;
        BirthTime = Time.time;

        statusText.text = "";
    }

    void Start () {
        animator = GetComponent<Animator>();

        statusSubject.Do(status => { statusText.text = status; })
            .Throttle(System.TimeSpan.FromMilliseconds(750))
            .Subscribe(_ => { if (!statusText.IsDestroyed()) statusText.text = ""; });

        StartCoroutine(CheckMutateGene());
        StartCoroutine(CheckDeath());

        NextDestination();
	}

    void Update()
    {
        lifeText.text = life.ToString();
    }

    public void NextDestination()
    {
        // 移動先の決定
        // ランダムに移動するか、異性の方に移動するかを決める(ランダム移動率：0.43+e^(-0.13(x+10))、xは人口)
        int humanCount = FindObjectsOfType<Human>().Length;
        float randomRate = 0.43f + Mathf.Exp(-0.13f * (humanCount + 10));
        //float randomRate = 0.5f;
        bool randomMove = Random.value < randomRate;
        if (!randomMove)
        {
            Human lover = humanDelegate.DecideLover();
            if (lover == null)
            {
                // 異性がいない場合はランダム移動
                randomMove = true;
            } else
            {
                Destination = lover.Destination;
            }
        }
        if (randomMove)
        {
            Destination = Util.RandomPosition(gameObject);
        }

        // キャラの向きの決定
        Vector3 current = transform.position;
        animator.SetFloat("DirectionX", Destination.x - current.x);
        animator.SetFloat("DirectionY", Destination.y - current.y);

        // 移動
        iTween.MoveTo(gameObject, iTween.Hash("position", Destination, "speed", speed,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "OnMoveCompleted",
            "oncompletetarget", gameObject, "name", "move"));
    }

    void OnMoveCompleted()
    {
        life--;

        NextDestination();
    }

    IEnumerator CheckDeath()
    {
        while (true)
        {
            if (life < 0)
            {
                Die();
            }

            yield return new WaitForEndOfFrame();
        }
    }

    void Die()
    {
        Graph graph = FindObjectOfType<Graph>();
        graph.AddAgeOfDeath(Age);

        GameObject deathObj = (GameObject)Instantiate(deathAnimation, transform.position, transform.rotation);
        Destroy(gameObject);

        Global.Log.Middle(string.Format("{0}は死亡した", ColoredFullName));
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        life--;
    }

    public bool IsParentChild(Human human)
    {
        return MotherId == human.Id ||
            FatherId == human.Id ||
            Id == human.MotherId ||
            Id == human.FatherId;
    }

    public bool IsBrother(Human human)
    {
        return MotherId == human.MotherId ||
            MotherId == human.FatherId ||
            FatherId == human.MotherId ||
            FatherId == human.FatherId;
    }

    public bool CanCross(Human human)
    {
        int match = 0;

        // 片方を反転してXORして4ビットずつ調べて1111の部分は一致
        int compare = ~Gene ^ human.Gene;
        for (int i = 0; i < Global.GENE_LENGTH; i++)
        {
            if ((compare & 0x0F) == 0x0F) match++;

            compare >>= 4;
        }

        return match < 4;
    }

    IEnumerator CheckMutateGene()
    {
        int count = 0;

        while (true)
        {
            //float probabirity = 1 / (2 + 6 * Mathf.Exp(9 - count / 2)) - 0.003034f; // シグモイド関数（ロジスティック曲線）
            //float probabirity = 1 / (2 + 6 * Mathf.Exp(-(count - 25) / 3)) - 0.003034f; // シグモイド関数（ロジスティック曲線）
            //float probabirity = 1 / (2 + 6 * Mathf.Exp(-(count - 22) / 3)) - 0.003034f; // シグモイド関数（ロジスティック曲線）
            float probabirity = 1 / (1.5f + 6 * Mathf.Exp(-(count - 22) / 3)) - 0.003039f; // シグモイド関数（ロジスティック曲線）
            count++;
            if (Random.value < probabirity)
            {
                Gene = MutateGene(Gene);
                statusSubject.OnNext("突然変異");
            }

            yield return new WaitForSeconds(1);
        }
    }

    // 各桁6/8の確率で遺伝子を突然変異させる
    int MutateGene(int gene)
    {
        int newGene = 0;

        for (int i = 0; i < Global.GENE_LENGTH; i++)
        {
            newGene <<= 4;

            if (Random.value < 6 / 8.0f)
            {
                newGene |= Random.Range(0, 16);
            } else
            {
                newGene |= (gene >> (7 - i) * 4) & 0x0F;
            }
        }

        return newGene;
    }

    public string FullName
    {
        get{
            return FamilyName + " " + FirstName;
        }
    }

    public interface HumanDelegate
    {
        Human DecideLover();
    }


}
