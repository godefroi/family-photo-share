using System;
using System.Data.Common;
using System.Data;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

using fourldn.Data.Tools;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;

namespace PhotoShare
{
	public class PhotoDb
	{
		static SHA1 m_hasher = SHA1.Create();

		string                           m_cname;
		Dictionary<Guid, ImageCodecInfo> m_codecs;
		Size                             m_thumb_size;

		public PhotoDb(string conn_name)
		{
			m_cname      = conn_name;
			m_codecs     = ImageCodecInfo.GetImageDecoders().ToDictionary(c => c.FormatID);
			m_thumb_size = new Size(200, 200);
		}

		public int AddPhoto(byte[] photo, string filename, DateTime file_date, int user)
		{
			byte[] thumb;
			string ctype = "image/unknown";
			int    ret   = -1;
			var    sql   = @"
	insert into photos
		( title, filename, uploadedby, description, picture, thumbnail, contenttype, filedate, uploaddate, hash )
	values
		( @fname, @fname, @user, @fname, @pic, @thumb, @ctype, @fdate, @udate, @hash )

	select convert(int, scope_identity())";

			using( var ms = new MemoryStream(photo) )
			using( var img = Image.FromStream(ms, true, true) )
			{
				if( m_codecs.ContainsKey(img.RawFormat.Guid) )
					ctype = m_codecs[img.RawFormat.Guid].MimeType;

				using( var t_img = CreateThumbnail(img, m_thumb_size) )
				using( var ms_t = new MemoryStream() )
				{
					t_img.Save(ms_t, img.RawFormat);

					thumb = ms_t.ToArray();
				}
			}

			using( var qh = new QueryHelper(m_cname) )
			{
				var parms = new DbParameter[]
					{
						qh.CreateParameter("@fname", DbType.String,   Path.GetFileName(filename)),
						qh.CreateParameter("@user",  DbType.Int32,    4, user),
						qh.CreateParameter("@pic",   DbType.Binary,   photo),
						qh.CreateParameter("@thumb", DbType.Binary,   thumb),
						qh.CreateParameter("@ctype", DbType.String,   ctype),
						qh.CreateParameter("@fdate", DbType.DateTime, file_date),
						qh.CreateParameter("@udate", DbType.DateTime, DateTime.Now.ToUniversalTime()),
						qh.CreateParameter("@hash",  DbType.String,   MakeHash(m_hasher.ComputeHash(photo))),
					};

				qh.ExecuteQuery(sql, parms, (p, dr) => ret = dr.Read() ? dr.GetInt32(0) : -1);
			}

			return ret;
		}

		public ImageInfo GetThumbnail(int photoid)
		{
			return GetImage(photoid, true);
		}

		public ImageInfo GetPicture(int photoid)
		{
			return GetImage(photoid, false);
		}

		public int? FindPhoto(byte[] hash)
		{
			var sql = "select photoid from photos where hash = @hash";

			using( var qh = new QueryHelper(m_cname) )
			{
				var parms = new DbParameter[]
					{
						qh.CreateParameter("@hash", DbType.String, MakeHash(hash)),
					};

				return qh.ExecuteQuery(sql, parms, (p, dr) => dr.Read() ? dr.GetInt32(0) : default(int?));
			}
		}

		public int? AuthenticateUser(string username, string password)
		{
			var sql = "select userid from users where username = @un and password = @pw";

			using( var qh = new QueryHelper(m_cname) )
			{
				var parms = new DbParameter[]
					{
						qh.CreateParameter("@un", DbType.String, username),
						qh.CreateParameter("@pw", DbType.String, MakeHash(System.Text.Encoding.UTF8.GetBytes(password))),
					};

				return qh.ExecuteQuery(sql, parms, (p, dr) => dr.Read() ? dr.GetInt32(0) : default(int?));
			}
		}

		public List<PhotoInfo> ListPhotos(int userid, int page_num, int page_size)
		{
			var sql = @"
select
	*
from
(
	select
		row_number() over ( order by uploaddate desc ) as [rownum],
		p.photoid,
		p.title,
		p.filename,
		p.description,
		u.username,
		p.contenttype,
		p.filedate,
		p.uploaddate
	from
		userphotos(@uid) p
			inner join users u on u.userid = p.uploadedby
) q1
where
	rownum > @start and rownum <= @end
order by
	uploaddate desc
";

			using( var qh = new QueryHelper(m_cname) )
			{
				var ret   = new List<PhotoInfo>();
				var parms = new DbParameter[]
					{
						qh.CreateParameter("@uid",   DbType.Int32, 4, userid),
						qh.CreateParameter("@start", DbType.Int32, 4, page_num * page_size),
						qh.CreateParameter("@end",   DbType.Int32, 4, (page_num * page_size) + page_size),
					};

				qh.ExecuteQuery(sql, parms, (p, r) =>
					{
						while( r.Read() )
						{
							var o = new PhotoInfo();
							o.FillFrom(r);
							ret.Add(o);
						}
					});

				return ret;
			}
		}

		public PhotoInfo GetPhotoInfo(int userid, int photoid)
		{
			var sql = @"
select
	p.photoid,
	p.title,
	p.filename,
	p.description,
	u.username,
	p.contenttype,
	p.filedate,
	p.uploaddate
from
	userphotos(@uid) p
		inner join users u on u.userid = p.uploadedby
where
	p.photoid = @pid
";

			using( var qh = new QueryHelper(m_cname) )
			{
				PhotoInfo ret   = null;
				var       parms = new DbParameter[]
					{
						qh.CreateParameter("@uid",   DbType.Int32, 4, userid),
						qh.CreateParameter("@pid",   DbType.Int32, 4, photoid),
					};

				qh.ExecuteQuery(sql, parms, (p, r) =>
					{
						if( r.Read() )
						{
							ret = new PhotoInfo();
							ret.FillFrom(r);
						}
					});

				return ret;
			}
		}

		public static byte[] CalculateHash(string filename)
		{
			using( var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read) )
				return m_hasher.ComputeHash(fs);
		}

		private ImageInfo GetImage(int photoid, bool thumb)
		{
			var sql = string.Format("select contenttype, {0} as imagedata from photos where photoid = @pid", thumb ? "thumbnail" : "picture");

			using( var qh = new QueryHelper(m_cname) )
			{
				var parms = new DbParameter[]
					{
						qh.CreateParameter("@pid", DbType.Int32, 4, photoid),
					};

				return qh.ExecuteQuery(sql, parms, (p, dr) =>
					{
						ImageInfo ret = null;

						if( dr.Read() )
						{
							ret = new ImageInfo();
							ret.FillFrom(dr);
						}

						return ret;
					});
			}
		}

		private static Image CreateThumbnail(Image orig_img, Size max_size)
		{
			var thumb_size = GetThumbSize(max_size, orig_img.Size);
			var thumb      = new Bitmap(thumb_size.Width, thumb_size.Height);

			using( var g = Graphics.FromImage(thumb) )
			{
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.DrawImage(orig_img, new Rectangle(Point.Empty, thumb.Size), new Rectangle(Point.Empty, orig_img.Size), GraphicsUnit.Pixel);
			}

			return thumb;
		}

		private static Size GetThumbSize(Size max_size, Size cur_size)
		{
			if( max_size.Width < 1 || max_size.Height < 1 || cur_size.Width < 1 || cur_size.Height < 1 )
				return Size.Empty;

			var max_ratio = (double)max_size.Width / (double)max_size.Height;
			var cur_ratio = (double)cur_size.Width / (double)cur_size.Height;
			int new_width;
			int new_height;

			if( max_ratio < cur_ratio )
			{
				new_width  = Math.Min(max_size.Width, cur_size.Width);
				new_height = (int)Math.Round(new_width / cur_ratio);
			}
			else
			{
				new_height = Math.Min(max_size.Height, cur_size.Height);
				new_width  = (int)Math.Round(new_height * cur_ratio);
			}

			return new Size(new_width, new_height);
		}

		private static string MakeHash(byte[] hash)
		{
			return BitConverter.ToString(hash).Replace("-", "");
		}
	}
}
