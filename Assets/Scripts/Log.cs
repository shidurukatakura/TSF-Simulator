using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Log : MonoBehaviour {

    public int logCapacity = 10;

    public Text log;

    Queue<string> messages = new Queue<string>();

    void Awake()
    {
        log = GetComponent<Text>();
    }

    public void High(string message)
    {
        AddLog(string.Format("<color=#FFFF00>{0}</Color>", message));
    }

    public void Middle(string message)
    {
        AddLog(message);
    }

    public void Low(string message)
    {
        AddLog(string.Format("<color=#EEEEEE>{0}</Color>", message));
    }

    private void AddLog(string message)
    {
        if (messages.Count == logCapacity)
        {
            messages.Dequeue();
        }

        int time = (int)Time.time;
        messages.Enqueue(string.Format("{0:00}:{1:00} {2}", time / 60, time % 60, message));

        log.text = string.Join("\n", messages.ToArray());
    }
}
