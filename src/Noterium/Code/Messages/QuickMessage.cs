namespace Noterium.Code.Messages
{
    internal struct QuickMessage
    {
        public string Message { get; }

        public QuickMessage(string message)
        {
            Message = message;
        }
    }
}