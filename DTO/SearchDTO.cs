namespace AjaxDemo.DTO
{
    public class SearchDTO
    {
        public int SpotId { get; set; }
        public int categoryId { get; set; } = 0; //= 0; 指定了屬性的初始值為 0。如果沒有特別指定，categoryId 的默認值為 0
        public string? keyword { get; set; } //沒有指定默認值，keyword 的初始值默認為 null。
        public int page { get; set; } = 1; //默認值設定為 1，表示如果沒有特別指定，則從第 1 頁開始。
        public int pageSize { get; set; } = 9; //默認值設定為 9，表示每頁顯示 9 個項目。
        public string? sortBy { get; set; } //用來指定排序的欄位。
        public string? sortType { get; set; } = "asc"; //用來指定排序的順序。默認值設定為 "asc"，表示默認為升序排序（asc 是 ascending 的簡寫）。

    }
}
