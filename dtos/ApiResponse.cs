public sealed class ApiResponse<T>
{
    public string Message { get; init; }
    public T? Data { get; init; }
    public object? Meta { get; init; }

    public ApiResponse(string message, T? data = default, object? meta = null)
    {
        Message = message;
        Data = data;
        Meta = meta;
    }
}
