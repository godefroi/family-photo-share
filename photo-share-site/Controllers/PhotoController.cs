using System;
using System.Web;
using System.Web.Mvc;

using PhotoShare;

namespace photo_share_site.Controllers
{
    public class PhotoController : Controller
    {
        public ActionResult Upload()
        {
			foreach( HttpPostedFile f in Request.Files )
			{
			}

			return RedirectToAction("Index", "Home");
        }

		public ActionResult List(int? id)
		{
			var db        = new PhotoDb("photodb");
			var cur_page  = id ?? (int?)1;
			var page_size = 20;

			ViewData.Add("cur_page",  cur_page);
			ViewData.Add("page_size", page_size);

			return View(db.ListPhotos(1, cur_page.Value, page_size));
		}

		public ActionResult Thumb(int id)
		{
			var db = new PhotoDb("photodb");

			return new ImageResult(db.GetThumbnail(id));
		}

		public ActionResult Image(int id)
		{
			var db = new PhotoDb("photodb");

			return new ImageResult(db.GetPicture(id));
		}
    }
}
