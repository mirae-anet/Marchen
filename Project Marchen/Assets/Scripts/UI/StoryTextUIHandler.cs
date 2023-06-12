using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoryTextUIHandler : MonoBehaviour
{
    private bool isPrint = false;
    public TextMeshProUGUI storyText;
    private Queue<string> lineQueue = new Queue<string>();

    public void StartStory(TextAsset textAsset)
    {
        if(isPrint)
            return;

        isPrint = true;
        ReadTextFile(textAsset);
    }
    private void ReadTextFile(TextAsset textAsset)
    {
        string[] lines;
        if(textAsset == null)
        {
            Debug.Log("file not found");
            lines = new string[]{"file not found"};
        }
        else
        {
            lines = textAsset.text.Split('\n');
        }
        MakeQueue(lines);
    }
    private void MakeQueue(string[] lines)
    {
        lineQueue.Clear();

        for(int i = 0; i < lines.Length; i++)
        {
            lineQueue.Enqueue(lines[i]);
        }
        StartCoroutine(PrintStoryCO());
    }

    IEnumerator PrintStoryCO()
    {   
        yield return new WaitForSeconds(1f);
        while(lineQueue.TryDequeue(out string result))
        {
            storyText.text = result;
            yield return new WaitForSeconds(2f);
        }
        EndStory();
    }
    private void EndStory()
    {
        storyText.text = string.Empty;
        this.gameObject.SetActive(false);
        isPrint = false;
    }
}
