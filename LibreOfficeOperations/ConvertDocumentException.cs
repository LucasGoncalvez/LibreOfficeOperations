using System;

namespace LibreOfficeOperations
{
	public class ConvertDocumentException : Exception
	{
		public ConvertDocumentException()
		{
		}

		public ConvertDocumentException(string message): base(message)
		{
		}

		public ConvertDocumentException(string message, Exception inner): base(message, inner)
		{
		}
	}
}
