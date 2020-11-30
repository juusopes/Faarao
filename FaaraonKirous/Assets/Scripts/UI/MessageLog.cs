using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageLog : MonoBehaviour
{
    public static MessageLog Instance { get; private set; } = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
            return;
        }

        _texts = GetComponentsInChildren<Text>();
    }

    private Text[] _texts;

    private readonly List<Message> _messages = new List<Message>();

    private readonly List<float> _removeTimes = new List<float>();

    private struct Message
    {
        public string Text { get; set; }
        public Color Color { get; set; }

        public Message(string text, Color color)
        {
            Text = text;
            Color = color;
        }
    }

    public void AddMessage(string text, Color color)
    {
        Enqueue(text, color);
    }

    private void Update()
    {
        // Remove from queue if max size is reached
        while (_texts.Length < _messages.Count)
        {
            Dequeue();
        }

        // Remove from queue if timer has ran out
        while (0 < _messages.Count && _removeTimes[0] < Time.time)
        {
            Dequeue();
        }

        // Place messages
        for (int i = _messages.Count - 1; 0 <= i; --i)
        {
            string text = _messages[i].Text;
            Color color = _messages[i].Color;
            
            // Fadeout
            float startFadeoutTime = _removeTimes[i] - Constants.messageFadeoutTimespan;
            if (startFadeoutTime < Time.time) {
                float secondsSinceFadeoutStart = Time.time - startFadeoutTime;
                float fadeoutPercentage = secondsSinceFadeoutStart / Constants.messageFadeoutTimespan;

                // Set color
                color.a = Mathf.Clamp(1f - fadeoutPercentage, 0, 1);
            }

            int textFieldIndex = _messages.Count - 1 - i;
            _texts[textFieldIndex].text = text;
            _texts[textFieldIndex].color = color;
        }
    }

    private void Enqueue(string text, Color color)
    {
        _messages.Add(new Message(text, color));
        _removeTimes.Add(Time.time + Constants.messageLifetimeInSeconds);
    }

    private void Dequeue()
    {
        _messages.RemoveAt(0);
        _removeTimes.RemoveAt(0);
    }


}
