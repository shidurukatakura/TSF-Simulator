using UnityEngine;
using System.IO;
using System.Collections;

public class Util {

    /// <summary>
    /// 引数のオブジェクトを配置可能なポジションをランダムで返す。
    /// オブジェクトは、ワールドの外縁からオブジェクトの幅・高さの半分を差し引いた内側に配置可能とする。
    /// </summary>
    /// <param name="gameObject">配置するオブジェクト</param>
    /// <returns>ランダムに選択された配置可能なポジション</returns>
    public static Vector3 RandomPosition(GameObject gameObject)
    {
        Bounds spriteBounds = gameObject.GetComponent<SpriteRenderer>().bounds;
        float spriteWidthHalf = spriteBounds.size.x / 2;
        float spriteHeightHalf = spriteBounds.size.y / 2;
        float x = Random.Range(spriteWidthHalf, Global.WorldSize.x - spriteWidthHalf);
        float y = Random.Range(spriteHeightHalf, Global.WorldSize.y - spriteHeightHalf);
        return Global.WorldMin + new Vector3(x, y);
    }

    public static T RandomElment<T>(T[] array)
    {
        if (array == null || array.Length == 0) return default(T);
        return array[Random.Range(0, array.Length)];
    }

    public static string ReadFileToString(string path)
    {
        string text = null;
        FileInfo fi = new FileInfo(path);
        try
        {
            using(StreamReader sr = new StreamReader(fi.OpenRead(), System.Text.Encoding.UTF8))
            {
                text = sr.ReadToEnd();
            }
        } catch (System.Exception)
        {
            Global.Log.High("ファイルの読み込みに失敗しました(" + path + ")");
            Debug.Log("ファイルの読み込みに失敗しました(" + path + ")");
        }

        return text;
    }

    public static string[] ReadFileToStringArray(string path)
    {
        string text = ReadFileToString(path);
        return text.Split('\n');
    }

    public static IEnumerator DelayMethod(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

}
