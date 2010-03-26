using System;

namespace PhotoShare
{
	public class PhotoInfo
	{
		public int PhotoId { get; set; }

		public string Title { get; set; }

		public string Filename { get; set; }

		public string Description { get; set; }

		public string UploadedBy { get; set; }

		public string ContentType { get; set; }

		public DateTime FileDate { get; set; }

		public DateTime UploadDate { get; set; }
	}
}
