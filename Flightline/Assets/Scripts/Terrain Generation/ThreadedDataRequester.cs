using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ThreadedDataRequester : MonoBehaviour
{
    static ThreadedDataRequester instance;

    Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

    private void Awake()
    {
        instance = FindObjectOfType<ThreadedDataRequester>();
    }
    public static void RequestData(Func<object> generateData, Action<object> callback)
    {

        ThreadStart threadStart = delegate {
            instance.DataThread(generateData, callback);
        };

        new Thread(threadStart).Start(); // Take in the thread I just created and start it
    }

    void DataThread(Func<object> generateData, Action<object> callback) // This function is taking place within the thread
    {
        object data = generateData(); // Generate the data from the given function
        lock (dataQueue) // Stops other queues executing their threads while the current queue is executing
        {
            dataQueue.Enqueue(new ThreadInfo(callback, data)); // Add this to the queue
        }
    }

    void Update()
    {
        if (dataQueue.Count > 0) // If there are items waiting in the queue
        {
            for (int i = 0; i < dataQueue.Count; i++) // For each item in the queue
            {
                ThreadInfo threadInfo = dataQueue.Dequeue(); // Set threadInfo to the next item currently waiting in the queue
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    struct ThreadInfo
    {
        public readonly Action<object> callback;
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }
}
