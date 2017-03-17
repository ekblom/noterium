namespace Noterium.Code.Messages
{
	internal struct QuickMessage
	{
		public string Message { get; private set; }

		public QuickMessage(string message)
		{
			Message = message;
		}
	}
}
