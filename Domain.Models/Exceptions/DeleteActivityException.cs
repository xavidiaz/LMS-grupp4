namespace Domain.Models.Exceptions
{
    public sealed class DeleteActivityException : Exception
    {
        public string Title { get; } = "Cannot delete activity";

        public DeleteActivityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
