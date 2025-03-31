using AutoMapper;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.ViewModels;

namespace TheGioiDiaMVC.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<KhachHang, HoSoVM>().ReverseMap();
            CreateMap<DangKyVM, KhachHang>();
                //.ForMember(kh => kh.HoTen, option => option.MapFrom(DangKyVM => DangKyVM.HoTen))
                //.ReverseMap();
        }
    }
}
