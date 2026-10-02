using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuanLyPhuongTien
{
    // ===================== A. LỚP CHA TRỪU TƯỢNG =====================
    public abstract class PhuongTien
    {
        private string _maPT = "PT000";
        private string _tenHang = "";
        private int _namSanXuat;
        private decimal _giaGoc;

        public string MaPT
        {
            get => _maPT;
            set => _maPT = string.IsNullOrWhiteSpace(value) ? "PT000" : value.Trim();
        }

        public string TenHang
        {
            get => _tenHang;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Tên hãng không được để trống!");
                _tenHang = value.Trim();
            }
        }

        public int NamSanXuat
        {
            get => _namSanXuat;
            set
            {
                if (value < 1900 || value > DateTime.Now.Year)
                    throw new ArgumentException("Năm sản xuất không hợp lệ!");
                _namSanXuat = value;
            }
        }

        public decimal GiaGoc
        {
            get => _giaGoc;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Giá gốc phải lớn hơn 0!");
                _giaGoc = value;
            }
        }

        protected PhuongTien(string maPT, string tenHang, int namSanXuat, decimal giaGoc)
        {
            MaPT = maPT;
            TenHang = tenHang;
            NamSanXuat = namSanXuat;
            GiaGoc = giaGoc;
        }

        public abstract decimal TinhGiaLanBanh();

        public virtual string GetInfo()
        {
            return $"Mã: {MaPT} | Hãng: {TenHang} | Năm SX: {NamSanXuat} | Giá gốc: {GiaGoc:N0} VNĐ";
        }
    }

    // ===================== B. Ô TÔ =====================================
    public class OTo : PhuongTien
    {
        private int _soChoNgoi;
        private double _dungTichDongCo;

        public int SoChoNgoi
        {
            get => _soChoNgoi;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Số chỗ ngồi phải lớn hơn 0!");
                _soChoNgoi = value;
            }
        }

        public double DungTichDongCo
        {
            get => _dungTichDongCo;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Dung tích động cơ phải lớn hơn 0!");
                _dungTichDongCo = value;
            }
        }

        public OTo(string maPT, string tenHang, int namSanXuat, decimal giaGoc,
                   int soChoNgoi, double dungTichDongCo)
            : base(maPT, tenHang, namSanXuat, giaGoc)
        {
            SoChoNgoi = soChoNgoi;
            DungTichDongCo = dungTichDongCo;
        }

        public override decimal TinhGiaLanBanh()
        {
            if (SoChoNgoi <= 9)
                return GiaGoc + GiaGoc * 0.12m + GiaGoc * 0.30m; // Thuế trước bạ 12% + TTĐB 30%
            return GiaGoc + GiaGoc * 0.10m;                      // Thuế trước bạ 10%
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $" | Số chỗ: {SoChoNgoi} | Dung tích ĐC: {DungTichDongCo}L";
        }
    }

    // ===================== C. XE MÁY ===================================
    public class XeMay : PhuongTien
    {
        private int _dungTichXylanh;

        public int DungTichXylanh
        {
            get => _dungTichXylanh;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Dung tích xy lanh phải lớn hơn 0!");
                _dungTichXylanh = value;
            }
        }

        public XeMay(string maPT, string tenHang, int namSanXuat, decimal giaGoc, int dungTichXylanh)
            : base(maPT, tenHang, namSanXuat, giaGoc)
        {
            DungTichXylanh = dungTichXylanh;
        }

        public override decimal TinhGiaLanBanh()
        {
            return DungTichXylanh < 175
                ? GiaGoc + GiaGoc * 0.02m
                : GiaGoc + GiaGoc * 0.05m;
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $" | Xy lanh: {DungTichXylanh}cc";
        }
    }

    // ===================== D. QUẢN LÝ ==================================
    public class QuanLyPhuongTien
    {
        private readonly List<PhuongTien> _danhSach = new();

        public void AddPhuongTien(PhuongTien pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            _danhSach.Add(pt);
        }

        public void DisplayAll()
        {
            if (_danhSach.Count == 0)
            {
                Console.WriteLine("Danh sách trống.");
                return;
            }
            foreach (var pt in _danhSach)
            {
                Console.WriteLine(pt.GetInfo());
                Console.WriteLine($"   => Giá lăn bánh: {pt.TinhGiaLanBanh():N0} VNĐ");
            }
        }

        public PhuongTien? FindMaxGiaLanBanh()
        {
            return _danhSach.OrderByDescending(p => p.TinhGiaLanBanh()).FirstOrDefault();
        }

        public List<PhuongTien> SearchByName(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new List<PhuongTien>();

            return _danhSach
                .Where(p => p.TenHang.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }
    }

    // ===================== KIỂM THỬ (TEST CASES) =====================
    public class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("  KỊCH BẢN KIỂM THỬ (TEST CASES) - AUTOSPEED");
            
            // TC01: Validation năm sản xuất = 1850
            Console.WriteLine("TC01: Validation năm sản xuất");
            try
            {
                var loi = new OTo("PT001", "Toyota", 1850, 1_000_000_000m, 5, 2.0);
                Console.WriteLine("FAIL: Đáng lẽ phải ném ngoại lệ!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"PASS: Ném ngoại lệ thành công! \"{ex.Message}\"");
            }

            // TC02: Ô tô 5 chỗ, giá gốc 1 tỷ -> 1.42 tỷ
            Console.WriteLine("\nTC02: Giá lăn bánh ô tô 5 chỗ");
            var oto = new OTo("PT001", "Toyota", 2023, 1_000_000_000m, 5, 2.0);
            decimal giaOTo = oto.TinhGiaLanBanh();
            Console.WriteLine($"Giá lăn bánh: {giaOTo:N0} VNĐ " +
                              (giaOTo == 1_420_000_000m ? "=> PASS" : "=> FAIL"));

            // TC03: Xe máy 150cc, giá gốc 50 triệu -> 51 triệu
            Console.WriteLine("\nTC03: Giá lăn bánh xe máy 150cc");
            var xe = new XeMay("PT002", "Honda", 2024, 50_000_000m, 150);
            decimal giaXe = xe.TinhGiaLanBanh();
            Console.WriteLine($"Giá lăn bánh: {giaXe:N0} VNĐ " +
                              (giaXe == 51_000_000m ? "=> PASS" : "=> FAIL"));

            // TC04: Đa hình trong List<PhuongTien>
            Console.WriteLine("\nTC04: Đa hình trong List<PhuongTien>");
            var ql = new QuanLyPhuongTien();
            ql.AddPhuongTien(oto);
            ql.AddPhuongTien(xe);

            // Bổ sung vòng lặp minh họa trực tiếp Đa hình theo yêu cầu Test Case 04
            var ds = new List<PhuongTien> { oto, xe };
            foreach (PhuongTien pt in ds)
            {
                Console.WriteLine($"[{pt.GetType().Name}] {pt.TinhGiaLanBanh():N0} VNĐ");
            }

            Console.WriteLine("\nIn toàn bộ bằng QuanLyPhuongTien.DisplayAll()");
            ql.DisplayAll();

            // TC05: Tìm phương tiện có giá lăn bánh Max
            Console.WriteLine("\nTC05: Tìm giá lăn bánh Max");
            var max = ql.FindMaxGiaLanBanh();
            if (max != null)
            {
                Console.WriteLine($"Đối tượng Max: {max.GetInfo()}");
                Console.WriteLine($"Giá lăn bánh: {max.TinhGiaLanBanh():N0} VNĐ " +
                                  (max is OTo && max.TinhGiaLanBanh() == 1_420_000_000m ? "=> PASS" : "=> FAIL"));
            }

            // Kiểm tra mở rộng SearchByName
            Console.WriteLine("\nKIỂM TRA MỞ RỘNG: SearchByName(\"honda\")");
            foreach (var pt in ql.SearchByName("honda"))
            {
                Console.WriteLine(pt.GetInfo());
            }

            Console.WriteLine("  HOÀN THÀNH TOÀN BỘ BÀI KIỂM THỬ!");
        }
    }
}