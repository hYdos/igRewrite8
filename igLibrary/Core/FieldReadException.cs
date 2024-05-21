namespace igLibrary.Core
{
	public class FieldReadException : Exception
	{
		public new Exception InnerException { get; private set; }
		public string FilePath { get; private set; }
		public uint Offset { get; private set; }
		public igMetaObject MetaObject { get; private set; }
		public igMetaField Field { get; private set; }
		public override string Message
		{
			get
			{
				return $"Failed to read {MetaObject._name}::{Field._fieldName} from file {FilePath} at {Offset.ToString("X08")}.\n{InnerException.Message}";
			}
		}
		public override string? StackTrace => InnerException.StackTrace;

		public FieldReadException(Exception innerException, string filePath, uint offset, igMetaObject metaObject, igMetaField field)
		{
			InnerException = innerException;
			FilePath = filePath;
			Offset = offset;
			MetaObject = metaObject;
			Field = field;
		}
	}
}