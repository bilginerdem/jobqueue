# JobQueue

**JobQueue** is a lightweight, thread-based sequential task execution library for .NET.  
It lets you enqueue tasks that are guaranteed to be processed in order by a dedicated background thread — ideal for scenarios like message handling, background jobs, or serialized I/O operations.

---

## ✨ Features

- ✅ Thread-safe sequential job execution
- ✅ Dedicated background processing thread
- ✅ Simple and expressive API
- ✅ Built-in control methods (Start, Suspend, Resume, Cancel, etc.)
- ✅ Error event handling

---

## 📦 Installation

```bash
dotnet add package JobQueue
```

---

## 🚀 Quick Start

```csharp
// 1. Create a JobQueue that receives byte[] and processes it using OnReceiveMessage
var messages = new JobQueue<byte[]>("ReceiveMessage", OnReceiveMessage);

// 2. Subscribe to error events
messages.Error += (_, args) => logger.Error(args.Exception, "ReceiveMessage");

// 3. Push a job into the queue
messages.Push(e.Packet);
```

---

## ⏱ Scheduled Jobs

If you need a job to run periodically or with delay, use the JobBuilder:

```csharp
var connectionJob = JobBuilder
    .Create()
    .Schedule(TimeSpan.FromSeconds(3)) // Delay
    .OnAction(OnConnectionJob)         // Delegate to run
    .Build();
```

---

## 🧠 Lifecycle Control

You can start, stop or pause the queue anytime:

```csharp
messages.Start();              // Starts the worker
messages.Suspend();            // Pauses job processing
messages.Resume();             // Resumes after suspension
messages.Cancel();             // Gracefully cancels the queue
messages.Join();               // Waits until all jobs are done
messages.Wait(TimeSpan.FromSeconds(10));  // Waits with timeout
```

---

## 📌 Event Handling

Subscribe to error events to catch exceptions thrown by job handlers:

```csharp
messages.Error += (_, args) =>
{
    Console.WriteLine($"Error occurred: {args.Exception.Message}");
};
```

---

## 🧪 Example Use Cases

- Message processing (e.g., MQ, WebSocket)
- Sequential file/database writes
- Rate-limited API calls
- Retry or delayed job handling

---

## 📃 License

MIT — see [LICENSE](LICENSE) for details.
