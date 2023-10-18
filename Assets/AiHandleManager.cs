using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Linq;

public class AiHandleManager : MonoBehaviour
{
    public static AiHandleManager Instance { get; private set; }

    public TextMeshProUGUI storyText;
    public TextMeshProUGUI answerText;
    public TMP_InputField questionText;
    public Button askButton;

    public List<Thread> threads = new List<Thread>(new Thread[5]);
    int Count = 0;// = 0 => yes = no | < 0 => yes < no | > 0 => yes > no
    bool isThreading;
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }
    void Start()
    {
        askButton.onClick.AddListener(ClickAsk);
        for (int i = 0; i < 5; i++)
            threads[i] = new Thread(SendAPI);
    }

    void ClickAsk()
    {
        isThreading = true;
        for (int i = 0; i < 5; i++)
            try
            {
                threads[i].Start();
            }
            catch (Exception) { }

        StartCoroutine(Threading());
        askButton.interactable = false;
    }

    void SendAPI()
    {
        string url = "https://generativelanguage.googleapis.com/v1beta2/models/text-bison-001:generateText?key=";
        string api = "AIzaSyCW8lX8bBySWe9-pig8RYoZx-IzO8K90eI";

        var httpRequest = (HttpWebRequest)WebRequest.Create(url + api);
        httpRequest.Method = "POST";

        httpRequest.Accept = "application/json";
        httpRequest.ContentType = "text/plain";

        string storyT = "I have a story like this: " + storyText.text + "'.";
        string questionT = "Now answer the question with only Yes or No: " + questionText.text;
        var prompt = "{\"prompt\" : {\"text\" : \"" + storyT + questionT + "\"}}";

        using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
        {
            streamWriter.Write(prompt);
        }

        var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            string result = streamReader.ReadToEnd().Split('"')[5];
            Count = (result == "No") ? Count - 1 : Count + 1;
        }
    }

    IEnumerator Threading()
    {
        while(threads.Any(thread => thread.IsAlive))
        {
            yield return new WaitForEndOfFrame();
        }

        print("All done");
        FinalResult();
        yield break;
    }

    void FinalResult()
    {
        askButton.interactable = true;
        if (Count > 0)
            answerText.SetText("Yes");
        else
            answerText.SetText("No");

        Count = 0;
        for (int i = 0; i < 5; i++)
            threads[i] = new Thread(SendAPI);
    }
}
