namespace Noterium.Code.Messages
{
    internal struct ConfigureControlsForParnetType
    {
        public ParentType Type { get; }

        public ConfigureControlsForParnetType(ParentType type)
        {
            Type = type;
        }

        public enum ParentType
        {
            Notebook,
            Tag,
            Library
        }
    }
}