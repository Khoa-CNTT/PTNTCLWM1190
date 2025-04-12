namespace TheGioiDiaMVC.ViewModels
{
    public class LienHeVM
    {
        public string? MaLH { get; set; } = Guid.NewGuid().ToString();
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string NoiDung { get; set; } = null!;
    }

}
