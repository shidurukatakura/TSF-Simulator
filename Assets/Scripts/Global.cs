using UnityEngine;
using System.Collections;

public class Global : MonoBehaviour {

    /* 定数 */
    public enum Sex { Female = 0, Male = 1 }
    public static int GENE_LENGTH = 8;
    public static int INITIAL_HUMAN_COUNT = 10;

    /* static変数 */
    public static int nextHumanId = 1;

    public GameObject human;

    public static GameObject World { get; private set; }
    public static Log Log { get; private set; } 
    public static Graph Graph { get; private set; }
    public static Earth Earth { get; private set; }

    public static Vector3 WorldSize { get; private set; }
    public static Vector3 WorldMin { get; private set; }
    public static string[] FamilyNames { get; private set; }
    public static string[] FirstNames { get; private set; }
    public static string[] TsReasons { get; private set; }

    public bool GameStarted { get; private set; }

    public bool GameOver { get; private set; }


    void Awake()
    {
        GameStarted = false;
        Time.timeScale = 0;

        World = GameObject.Find("World");
        Log = GameObject.Find("Log").GetComponent<Log>();
        Graph = FindObjectOfType<Graph>();
        Earth = FindObjectOfType<Earth>();

        ReadData();
        InspectWorld();
    }

    void StartGame()
    {
        GameStarted = true;
        Time.timeScale = 5;
        //Time.timeScale = 1;

        StartCoroutine(Populate(INITIAL_HUMAN_COUNT));
        StartCoroutine(CheckDecline());
    }

    public void ReadData()
    {
        FamilyNames = Util.ReadFileToStringArray(Application.dataPath + "/Data/FamilyName.txt");
        FirstNames = Util.ReadFileToStringArray(Application.dataPath + "/Data/FirstName.txt");
        TsReasons = Util.ReadFileToStringArray(Application.dataPath + "/Data/TsReason.txt");

    }

    public void InspectWorld()
    {
        Bounds bounds = World.GetComponent<MeshRenderer>().bounds;
        WorldSize = bounds.size;
        WorldMin = bounds.min;
    }

    public IEnumerator Populate(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 position = Util.RandomPosition(this.human);
            GameObject maleObj = (GameObject)Instantiate(this.human, position, gameObject.transform.rotation);
            Human human = maleObj.GetComponent<Human>();

            string familyName = Util.RandomElment(Global.FamilyNames);
            string firstName = Util.RandomElment(Global.FirstNames);
            human.FamilyName = familyName;
            human.FirstName = firstName;
            human.MotherId = -i;    // ダミーIDをセット
            human.FatherId = -i;
            human.Generation = 1;
            human.Gene = Random.Range(int.MinValue, int.MaxValue);
            maleObj.GetComponent<Male>().Initialize();

            Global.Log.Low(string.Format("{0}は生き残った", human.ColoredFullName));

            yield return new WaitForSeconds(0.2f);
        }
    }

    // 詰んだらライフを減らして終わらせる
    public IEnumerator CheckDecline()
    {
        yield return new WaitForSeconds(3);

        while (true)
        {

            Human[] humans = FindObjectsOfType<Human>();
            bool isDecline = true;
            if (humans.Length >= 2)
            {
                foreach (Human human in humans)
                {
                    if (human.Sex == Global.Sex.Male)
                    {
                        isDecline = false;
                        break;
                    }
                }
            }

            if (isDecline)
            {
                foreach (Human human in humans)
                {
                    human.life--;
                }
            }

            yield return new WaitForSeconds(2);
        }
    }

    void Update()
    {
        if (GameStarted)
        {
            Human[] humans = (Human[])FindObjectsOfType(typeof(Human));
            if (humans.Length == 0 && !GameOver)
            {
                Global.Log.High("そしてだれもいなくなった・・・");
                GameOver = true;
                GameStarted = false;
                //StartCoroutine( Util.DelayMethod(5, () => 
                //{
                //    GameOver = false;
                //    Graph.Reset();
                //    Earth.Reset();
                //    StartCoroutine(Populate(10));
                //}));
            }

        } else {
            if (Input.GetKey(KeyCode.Space)) {
                StartGame();
            }
        }
    }

}
