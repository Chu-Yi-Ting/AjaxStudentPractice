using AjaxDemo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace AjaxDemo.Controllers
{
    public class ApiController : Controller
    {
        //[Route("")]
        private readonly MydbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment; 
        public ApiController(MydbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        //用api傳回content資料
        public IActionResult Index()
        {
            System.Threading.Thread.Sleep(5000);
            string content = "<h2>Hello 朱朱</h2>";
            return Content(content, "text/html", Encoding.UTF8);
        }

        //用api傳回json資料
        public IActionResult Cities()
        {
            var cities = _context.Addresses.Select(x => x.City).Distinct();
            return Json(cities);
        }

        //todo 根據 city 讀出鄉鎮區(site_id)的資料
        public IActionResult Districts(string city)
        {
            var districts = _context.Addresses
                .Where(x => x.City == city)
                .Select(x => x.SiteId).Distinct();
            return Json(districts);
        }
        //todo 根據 site_id 讀出路名(road) 
        public IActionResult Roads(string SiteId)
        {
            var Roads = _context.Addresses
                .Where(x => x.SiteId == SiteId)
                .Select(x => x.Road).Distinct();
            return Json(Roads);
        }

        //用api傳回圖檔
        public IActionResult Avatar(int id = 1)
        {
            var member = _context.Members.Find(id);
            if (member != null)
            {
                byte[] img = member.FileData;
                if (img != null)
                {
                    return File(img, "image/jpeg");
                }
            }
            return NotFound();

        }

        public IActionResult CallAPI()
        {

            return View();
        }

        //public IActionResult Register(string Name, string Email, int Age = 20)
        public IActionResult Register(UserDTO _user)
        {
            if (string.IsNullOrEmpty(_user.userName))
            {
                _user.userName = "Guest";
            }           

            //寫進資料庫
            Member member = new Member();
            member.Name = _user.userName;
            member.Email = _user.userEmail;
            member.Age = _user.userAge;
            member.FileName = _user.userPhoto.FileName;

            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", _user.userPhoto.FileName);
            using (var filestream = new FileStream(filePath, FileMode.Create))
            {
                _user.userPhoto.CopyTo(filestream);
            }

            //將上傳的檔案轉成二進位
            byte[] imgByte = null;
            using (var memoryStream = new MemoryStream())
            {
                _user.userPhoto.CopyTo(memoryStream);
                imgByte = memoryStream.ToArray();

            }
            member.FileData = imgByte;

            _context.Members.Add(member);
            _context.SaveChanges();

            //string info = $"Hello {_user.userName}, {_user.userAge}歲了, 電子郵件是{_user.userEmail}";
            string info = $"照片名稱: {_user.userPhoto.FileName}, 照片長度: {_user.userPhoto.Length}, 照片類型: {_user.userPhoto.ContentType}";
            //return Content(info, "text/plain", Encoding.UTF8);
            return Content(info);
        }

        public IActionResult CheckAccount(UserDTO _user)
        {
            
            var existingUser = _context.Members.FirstOrDefault(m => m.Name == _user.userName);

            if (existingUser != null)
            {
                
                return Content("帳號已存在", "text/plain", Encoding.UTF8);
            }

            
            return Content("帳號可用", "text/plain", Encoding.UTF8);
        }
    }
}
