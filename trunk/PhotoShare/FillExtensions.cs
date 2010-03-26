using System;
using System.Data;

using fourldn.Data.Tools;

namespace PhotoShare
{
	internal static class FillExtensions
	{
		internal static void FillFrom(this PhotoInfo o, IDataRecord dr)
		{
			o.PhotoId     = dr.GetInt32("photoid").Value;
			o.Title       = dr.GetString("title");
			o.Filename    = dr.GetString("filename");
			o.Description = dr.GetString("description");
			o.UploadedBy  = dr.GetString("username");
			o.ContentType = dr.GetString("contenttype");
			o.FileDate    = dr.GetDateTime("filedate").Value;
			o.UploadDate  = dr.GetDateTime("uploaddate").Value;
		}

		internal static void FillFrom(this ImageInfo o, IDataRecord dr)
		{
			o.ContentType = dr.GetString("contenttype");
			o.ImageData   = dr.GetBytes("imagedata");
		}
	}
}
