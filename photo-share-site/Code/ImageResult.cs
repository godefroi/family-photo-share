using System;
using System.Web.Mvc;

using PhotoShare;

namespace photo_share_site
{
	public class ImageResult : ActionResult
	{
		ImageInfo m_img;

		public ImageResult(ImageInfo img)
		{
			m_img = img;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			context.HttpContext.Response.Clear();

			context.HttpContext.Response.ContentType = m_img.ContentType;

			context.HttpContext.Response.OutputStream.Write(m_img.ImageData, 0, m_img.ImageData.Length);
		}
	}
}
