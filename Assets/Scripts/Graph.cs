using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Graph : MonoBehaviour {

    public static int graphScale = 500;

    public Material material;

    private Text graphValueText;
    private Text generationText;
    private Earth earth;

    private SimpleGraphSource humanCountGraphSource;
    private SimpleGraphSource bloodStrengthGraphSource;
    private TermAverageGraphSource ageOfDeathGraphSource;
    private SimpleGraphSource divercityGraphSource;
    private SimpleGraphSource earthGraphSource;

    private int generation;

    void Start()
    {
        material = new Material(Shader.Find("Hidden/Internal-Colored"));
        graphValueText = GameObject.Find("GraphValue").GetComponent<Text>();
        generationText = GameObject.Find("Generation").GetComponent<Text>();
        earth = FindObjectOfType<Earth>();

        Reset();
        StartCoroutine(RecordHistory());
    }

    private IEnumerator RecordHistory()
    {
        while (true)
        {
            Human[] humans = (Human[])FindObjectsOfType(typeof(Human));
            humanCountGraphSource.Add(humans.Length);

            float bloodStrength = CalcBloodStrength(humans);
            bloodStrengthGraphSource.Add(bloodStrength);

            ageOfDeathGraphSource.UpdateValues();

            float divercity = CalcDivercity(humans);
            divercityGraphSource.Add(divercity);

            earthGraphSource.Add(earth.Hp);

            if (humans.Length > 0)
            {
                generation = Mathf.Max(generation, humans.Max(human => human.Generation));
            }

            yield return new WaitForSeconds(1);
        }
    }

    private float CalcBloodStrength(Human[] humans)
    {
        int length = humans.Length;
        if (length <= 1) return 0.0f;

        int combination = 0;
        int cantCross = 0;
        for (int i = 0; i < length - 1; i++)
        {
            Human human1 = humans[i];
            for (int j = i + 1; j < length; j++)
            {
                Human human2 = humans[j];

                // 女同士の場合はもう子をなす可能性がないので含めない
                if (human1.Sex == Global.Sex.Female && human2.Sex == Global.Sex.Female) continue;

                combination++;
                if (!human1.CanCross(human2)) cantCross++;
            }
        }

        if (combination <= 0) return 0.0f;

        return ((float)cantCross) / combination;
    }

    // Shannon-Wiener多様度指数を求める
    // H' = -ΣPi log2 Pi （Piは種類iの優占度）
    // 遺伝子の各桁についてそれぞれ求めて平均する
    private float CalcDivercity(Human[] humans)
    {
        if (humans.Length == 0)
        {
            return 0.0f;
        }

        int[,] geneCounts = new int[Global.GENE_LENGTH, 16];

        foreach (Human human in humans)
        {
            int gene = human.Gene;
            for (int i = 0; i < Global.GENE_LENGTH; i++)
            {
                int kind = gene >> i * 4 & 0x0F;
                geneCounts[i, kind]++;
            }
        }

        float[] divercities = new float[Global.GENE_LENGTH];
        int humanCount = humans.Length;

        for (int i = 0; i < Global.GENE_LENGTH; i++)
        {
            float divercitySummary = 0;

            for (int j = 0; j < 16; j++)
            {
                int geneCount = geneCounts[i, j];
                if (geneCount == 0) continue;

                float dominance = ((float)geneCount) / humanCount;
                divercitySummary += dominance * Mathf.Log(dominance, 2);
            }

            divercities[i] = -divercitySummary;
        }

        return divercities.Average();
    }

    void OnPostRender()
    {
        graphValueText.text = "";

        GraphSource[] graphSources = {
            humanCountGraphSource,
            bloodStrengthGraphSource,
            ageOfDeathGraphSource,
            earthGraphSource,
        };

        material.SetPass(0);
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Begin(GL.LINES);

        foreach (GraphSource source in graphSources) DrawGraph(source);

        GL.End();
        GL.PopMatrix();

        DrawGraphValue();

        generationText.text = generation + "世代";
    }

    private void DrawGraph(GraphSource source)
    {
        GL.Color(source.Color);

        float prev = 0;

        int i = 0;
        foreach (float value in source.Values)
        {
            if (i > 0)
            {
                GL.Vertex3(1.0f / graphScale * i, prev / source.Max / 4 + 0.7f, 0.0f);
                GL.Vertex3(1.0f / graphScale * (i + 1), value / source.Max / 4 + 0.7f, 1.0f);
            }
            i++;
            prev = value;
        }
    }

    private void DrawGraphValue()
    {
        GraphSource[] graphSources = {
            humanCountGraphSource,
            bloodStrengthGraphSource,
            ageOfDeathGraphSource,
            earthGraphSource,
        };

        string graphValueText = "";
        foreach(GraphSource source in graphSources) graphValueText += source.GetGraphValue();
        this.graphValueText.text = graphValueText;
    }

    public void Reset()
    {
        humanCountGraphSource = new SimpleGraphSource()
        {
            Max = 40.0f,
            Color = Color.white,
            Format = "人口：{0:0}人",
        };
        divercityGraphSource = new SimpleGraphSource()
        {
            Max = 1.0f,
            Color = Color.yellow,
            Format = "{0:0.00}",
            GraphValueDelegate = () => { return ""; }
        };
        bloodStrengthGraphSource = new SimpleGraphSource()
        {
            Max = 0.5f,
            Color = new Color(0xFF / 255.0f, 0xAA / 255.0f, 0xFF / 255.0f),
            Format = "血の濃さ：{0:0.00%}",
            GraphValueDelegate = () =>
            {
                string valueStr = string.Format(bloodStrengthGraphSource.Format, bloodStrengthGraphSource.CurrentValue);
                string divercityValueStr = string.Format(divercityGraphSource.Format, divercityGraphSource.CurrentValue);
                return string.Format("<color={0}>{1}({2})</color>  ", bloodStrengthGraphSource.ColorStr, valueStr, divercityValueStr);
            }
        };
        ageOfDeathGraphSource = new TermAverageGraphSource(5)
        {
            Max = 20.0f,
            Color = Color.cyan,
            Format = "平均死亡年齢：{0:0.00}秒",
        };
        earthGraphSource = new SimpleGraphSource()
        {
            Max = earth.maxHp * 1.1f,
            Color = new Color(180.0f / 255, 1.0f, 30.0f / 255),
            Format = "地球環境：{0:0}",
        };
    }

    public void AddAgeOfDeath(float age)
    {
        ageOfDeathGraphSource.AddBuffer(age);
    }

    /// <summary>
    /// グラフ描画に必要な情報を保持し供給するインターフェース 
    /// </summary>
    public interface GraphSource
    {
        float Max { get; set; }
        string ColorStr { get; set; }
        Color Color { get; set; }
        string Format { get; set; }

        IEnumerable<float> Values { get; }
        float CurrentValue { get; }

        string GetGraphValue();
    }

    public class SimpleGraphSource : GraphSource
    {
        private Queue<float> m_Values = new Queue<float>();

        public float Max { get; set; }
        public string ColorStr { 
            get {
                int rint = (int)(Color.r * 255);
                int gint = (int)(Color.g * 255);
                int bint = (int)(Color.b * 255);
                return string.Format("#{0:X2}{1:X2}{2:X2}", rint, gint, bint);
            }
            set { }
        }
        public Color Color { get; set; }
        public string Format { get; set; }
        public float CurrentValue { get; set; }

        public Func<string> GraphValueDelegate { private get; set; }

        public SimpleGraphSource() {
            GraphValueDelegate = () =>
            {
                string valueStr = string.Format(Format, CurrentValue);
                return string.Format("<color={0}>{1}</color>  ", ColorStr, valueStr);
            };
        }

        public IEnumerable<float> Values
        {
            get { return m_Values; }
        }

        public void Add(float value)
        {
            Max = Mathf.Max(Max, value);
            CurrentValue = value;
            m_Values.Enqueue(value);

            if (m_Values.Count > Graph.graphScale)
            {
                m_Values.Dequeue();
            }
        }

        public string GetGraphValue()
        {
            return GraphValueDelegate();
        }
    }

    /// <summary>
    /// 直近N秒間の平均を保持するGraphSource
    /// </summary>
    public class TermAverageGraphSource : SimpleGraphSource
    {
        private Queue<List<float>> termValuesQueue = new Queue<List<float>>();
        private List<float> buffer = new List<float>();

        public TermAverageGraphSource(int term)
        {
            for (int i = 0; i < term; i++)
            {
                termValuesQueue.Enqueue(new List<float>());
            }
        }

        public void AddBuffer(float value)
        {
            buffer.Add(value);
        }

        public void UpdateValues()
        {

            termValuesQueue.Dequeue();
            termValuesQueue.Enqueue(buffer);
            buffer = new List<float>();

            int count = 0;
            float sum = 0;

            foreach (List<float> values in termValuesQueue)
            {
                foreach (float value in values)
                {
                    count++;
                    sum += value;
                }
            }

            float currentValue = (count == 0) ? CurrentValue : sum / count;
            Add(currentValue);
        }
    }

    public class BloodStrengthGraphSource : TermAverageGraphSource
    {
        private SimpleGraphSource divercityGraphSource;

        public BloodStrengthGraphSource(int term, SimpleGraphSource divercityGraphSource) : base(term) {
            this.divercityGraphSource = divercityGraphSource;
        }

        public string GraphValue
        {
            get{
                string valueStr = string.Format(Format, CurrentValue);
                string divercityValueStr = string.Format(divercityGraphSource.Format, divercityGraphSource.CurrentValue);
                return string.Format("<color={0}>{1}({2})</color>  ", ColorStr, valueStr, divercityValueStr);
            }
        }

    }

}
