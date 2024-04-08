namespace TaskListProcessing;

public class TaskResult<T> : ITaskResult
{
    public TaskResult()
    {
        Name = "UNKNOWN";
        Data = default;
    }

    public TaskResult(string name, T data)
    {
        Name = name;
        Data = data;
    }

    public T? Data { get; set; }
    public string Name { get; set; }
}
