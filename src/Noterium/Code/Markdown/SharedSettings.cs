namespace Noterium.Code.Markdown
{
    public static class SharedSettings
    {
        /// <summary>
        ///     Tabs are automatically converted to spaces as part of the transform
        ///     this constant determines how "wide" those tabs become in spaces
        /// </summary>
        public const int TabWidth = 4;

        public const string MarkerToDo = @"[*\-\s]\s\[(?:\s|x)\]";
        public const string MarkerUl = @"[*+-]";
        public const string MarkerOl = @"\d+[.]";
    }
}