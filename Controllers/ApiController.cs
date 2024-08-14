using AjaxDemo.DTO;
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

        //旅遊景點搜尋、分頁、排序
        [HttpPost]  //標註 Spots 方法將會處理 HTTP POST 請求
        public IActionResult Spots([FromBody] SearchDTO _searchDTO) //SearchDTO 是一個資料傳輸物件（DTO），用於封裝從客戶端傳遞過來的資料。
                                                                    //[FromBody] 是一個屬性，表示 ASP.NET Core MVC 會從 HTTP 請求的主體中提取參數值並綁定到方法參數 _searchDTO。也就是說，客戶端發送的 JSON 資料會被反序列化為 SearchDTO 物件。
        {
            //根據分類編號讀取相關景點
            var spots = _searchDTO.categoryId == 0 ? _context.SpotImagesSpots : _context.SpotImagesSpots.Where(s => s.CategoryId == _searchDTO.categoryId);

            //關鍵字搜尋
            if (!string.IsNullOrEmpty(_searchDTO.keyword))
            {
                spots = spots.Where(s => s.SpotTitle.Contains(_searchDTO.keyword) || s.SpotDescription.Contains(_searchDTO.keyword));
            }

            //根據價錢搜尋
            //根據日期區間搜尋

            switch (_searchDTO.sortBy) //這段程式碼根據 _searchDTO.sortBy 的值決定如何排序資料。
            {
                case "SpotId": //如果 sortBy 的值是 "SpotId"，則根據 SpotId 進行排序。
                    spots = _searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotId) : spots.OrderByDescending(s => s.SpotId);
                    break;
                case "spotTitle": //如果 sortBy 的值是 "spotTitle"，則根據 SpotTitle 進行排序。
                    spots = _searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotTitle) : spots.OrderByDescending(s => s.SpotTitle);
                    break;
                case "categoryId": //如果 sortBy 的值是 "categoryId"，則根據 CategoryId 進行排序。
                    spots = _searchDTO.sortType == "asc" ? spots.OrderBy(s => s.CategoryId) : spots.OrderByDescending(s => s.CategoryId);
                    break;
                default: //如果 sortBy 的值不是 spotTitle 或 categoryId，則預設根據 SpotId 進行排序。
                    spots = _searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotId) : spots.OrderByDescending(s => s.SpotId);
                    break;
            }

            //總共有多少筆資料
            //計算篩選後的資料總數，並將其存儲在 totalCount 變數中。
            int totalCount = spots.Count(); 
            //取得每頁顯示的資料筆數，這個值來自 _searchDTO.pageSize。
            int pageSize = _searchDTO.pageSize;
            //取得當前的頁數，這個值來自 _searchDTO.page。
            int page = _searchDTO.page;
            //計算總共有幾頁，公式是將總資料數除以每頁的資料筆數，並使用 Math.Ceiling 函數將結果進行無條件進位。
            int totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);
            //這段程式碼進行分頁操作，使用 Skip((page - 1) * pageSize) 跳過前面的資料。使用 Take(pageSize) 取得當前頁的資料。
            spots = spots.Skip((page - 1) * pageSize).Take(pageSize);


            //設定回傳的資料
            //1.建立一個新的 SpotsPagingDTO 物件，這個物件將用來存放分頁的結果。
            SpotsPagingDTO pagingDTO = new SpotsPagingDTO();
            //將計算出的總頁數存放在 pagingDTO.TotalPages 中。
            pagingDTO.TotalPages = totalPages;
            //將篩選、排序、分頁後的資料轉換成列表，並存放在 pagingDTO.SpotsResult 中。
            pagingDTO.SpotsResult = spots.ToList();

            //return Json(spots);
            //最後，將 pagingDTO 物件轉換成 JSON 格式，並回傳給前端。
            return Json(pagingDTO);
        }

        public IActionResult Categories()
        {
            var categories = _context.Categories;
            SpotsCategoriesDTO CategoriesDTO = new SpotsCategoriesDTO();
            CategoriesDTO.CategoriesResult = categories.ToList();
            return Json(CategoriesDTO);
        }

    }
}
