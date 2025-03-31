using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TheGioiDiaMVC.Data;

public partial class HoiDap
{
    [Key]
    public int MaHd { get; set; }

    public string CauHoi { get; set; } = null!;

    public string TraLoi { get; set; } = null!;

    public DateOnly NgayDua { get; set; }

    public string MaNv { get; set; } = null!;

}
