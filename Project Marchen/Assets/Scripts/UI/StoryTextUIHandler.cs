using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// @brief 스토리 텍스트를 UI에 표시
public class StoryTextUIHandler : MonoBehaviour
{
    private bool isPrint = false;
    public TextMeshProUGUI storyText;
    private Queue<string> lineQueue = new Queue<string>();

    /// @brief 외부에서 스토리 텍스트 출력을 요청할 때 사용.
    /// @param textAsset txt파일
    /// @see StoryTextAction.PrintStory()
    public void StartStory(TextAsset textAsset)
    {
        if(isPrint)
            return;

        isPrint = true;
        ReadTextFile(textAsset);
    }
    /// @brief 파일을 한 줄씩 읽음.
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
    /// @brief 한 줄씩 큐에 저장함.
    private void MakeQueue(string[] lines)
    {
        lineQueue.Clear();

        for(int i = 0; i < lines.Length; i++)
        {
            lineQueue.Enqueue(lines[i]);
        }
        StartCoroutine(PrintStoryCO());
    }

    /// @brief 2초마다 한 줄씩 출력
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

    /// @brief 출력을 마치면 실행.
    /// @details UI 초기화, 비활성화
    private void EndStory()
    {
        storyText.text = string.Empty;
        this.gameObject.SetActive(false);
        isPrint = false;
    }

    /// @brief 현재 출력 중인지 확인
    /// @see StoryTextAction.OnTriggerStay()
    public bool getIsPrint(){
        return this.isPrint;
    }
}
