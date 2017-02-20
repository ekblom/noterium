using System;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;

namespace Noterium.Code.Data
{
	public class DocumentEntitiy
    {
        public delegate void DocumentEntitiyDeleted(DocumentEntitiy ent);

	    public event DocumentEntitiyDeleted Deleted;

        public EntityType Type { get; set; }
		public TextAnchor StartIndex { get; }
	    public TextAnchor EndIndex { get; }

        public ISegment Segment { get; set; }

	    public DocumentEntitiy(TextAnchor start, TextAnchor end)
	    {
	        StartIndex = start;
	        EndIndex = end;

            StartIndex.Deleted += AnchorDeleted;
            EndIndex.Deleted += AnchorDeleted;
	    }

	    private void AnchorDeleted(object sender, EventArgs eventArgs)
	    {
            Deleted?.Invoke(this);
        }

	    public bool InRange(int carretIndex)
	    {
            if (StartIndex.IsDeleted)
                return false;

	        return StartIndex.Offset <= carretIndex && EndIndex.Offset >= carretIndex;
	    }
    }

	public enum EntityType
	{
		Image,
		List,
		Anchor,
		SimpleAnchor,
		Table
	}
}