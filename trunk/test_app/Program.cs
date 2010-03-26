using System;

using PhotoShare;
using System.IO;

namespace test_app
{
	class Program
	{
		static void Main(string[] args)
		{
			var db = new PhotoDb("photodb");

			/*foreach (var fn in Directory.GetFiles(@"F:\DCIM\100MEDIA\Washington D.C"))
				UploadPhoto(db, fn);
			foreach (var fn in Directory.GetFiles(@"F:\DCIM\100MEDIA", "*.jpg"))
				UploadPhoto(db, fn);*/

			/*foreach( var pi in db.ListPhotos(1, 0, 20) )
				//Console.WriteLine(db.GetThumbnail(pi.PhotoId).ImageData.Length);
				Console.WriteLine(pi.Filename);*/

			BackupPhotos(db, @"\\SRV-PARKER\Pictures");
		}

		static void UploadPhoto(PhotoDb db, string fn)
		{
			byte[] photo;

			Console.WriteLine("Uploading photo: {0}", Path.GetFileName(fn));

			using( var fs = new FileStream(fn, FileMode.Open) )
			{
				photo = new byte[fs.Length];
				fs.Read(photo, 0, (int)fs.Length);
			}

			var photo_id = db.AddPhoto(photo, fn, File.GetLastWriteTimeUtc(fn), 1);

			using( var fs = new FileStream("d:\\out.jpg", FileMode.Create) )
			{
				var thumb = db.GetThumbnail(photo_id);
				fs.Write(thumb.ImageData, 0, thumb.ImageData.Length);
			}
		}

		static void BackupPhotos(PhotoDb db, string path)
		{
			foreach( var dir in Directory.GetDirectories(path) )
				BackupPhotos(db, dir);

			foreach( var file in Directory.GetFiles(path) )
			{
				//if( db.FindPhoto(PhotoDb.CalculateHash(file)) == null )
				//	UploadPhoto(db, file);
				if( file.EndsWith("db") )
					Console.WriteLine(file);
			}
		}
	}
}
