using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QuanLyDanhMucThietBiCongNghe
{
    public partial class Form1 : Form
    {
        private List<Product> _productList = new List<Product>();
        private BindingSource _bindingSource = new BindingSource();
        private string _selectedImagePath = string.Empty;

        public Form1()
        {
            InitializeComponent();
            InitializeData();
            SetupBinding();
            UpdateStatus();
        }

        private void InitializeData()
        {
            cboCategory.Items.AddRange(new string[] { "Điện thoại", "Laptop", "Phụ kiện" });
            cboCategory.SelectedIndex = 0;
        }

        private void SetupBinding()
        {
            dgvProducts.AutoGenerateColumns = false;
            ApplyFilter();
        }

        // Hàm lọc dữ liệu dùng chung (Sửa lỗi 1: Cập nhật Grid chính xác khi đang Tìm kiếm)
        private void ApplyFilter()
        {
            string kw = txtSearch.Text.Trim();
            var data = string.IsNullOrEmpty(kw)
                ? _productList.ToList()
                : _productList.Where(p => p.ProductName.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            _bindingSource.DataSource = new BindingList<Product>(data);
            dgvProducts.DataSource = _bindingSource;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            lblStatus.Text = $"Tổng số sản phẩm: {_productList.Count}";
        }

        private bool ValidateInput()
        {
            bool isValid = true;
            errorProvider1.Clear();

            // TC02: Chỉ bắt lỗi Tên SP, Đơn giá, Số lượng để tránh bị 3 icon đỏ
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                errorProvider1.SetError(txtProductName, "Tên sản phẩm không được để trống!");
                isValid = false;
            }

            if (!decimal.TryParse(txtUnitPrice.Text.Trim(), out decimal price) || price <= 0)
            {
                errorProvider1.SetError(txtUnitPrice, "Đơn giá phải là số lớn hơn 0!");
                isValid = false;
            }

            if (!int.TryParse(txtQuantity.Text.Trim(), out int qty) || qty < 0)
            {
                errorProvider1.SetError(txtQuantity, "Số lượng phải là số nguyên không âm!");
                isValid = false;
            }

            return isValid;
        }

        private string AutoGenerateProductId()
        {
            int maxId = 0;
            foreach (var p in _productList)
            {
                if (p.ProductId.StartsWith("SP") && int.TryParse(p.ProductId.Substring(2), out int id))
                {
                    if (id > maxId) maxId = id;
                }
            }
            return $"SP{(maxId + 1):D2}";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            string pId = txtProductId.Text.Trim();
            if (string.IsNullOrEmpty(pId))
            {
                pId = AutoGenerateProductId(); // Tự sinh mã nếu để trống
            }
            else if (_productList.Any(p => p.ProductId.Equals(pId, StringComparison.OrdinalIgnoreCase)))
            {
                errorProvider1.SetError(txtProductId, "Mã sản phẩm đã tồn tại!");
                return;
            }

            Product pNew = new Product
            {
                ProductId = pId,
                ProductName = txtProductName.Text.Trim(),
                CategoryName = cboCategory.SelectedItem?.ToString() ?? "",
                UnitPrice = decimal.Parse(txtUnitPrice.Text.Trim()),
                Quantity = int.Parse(txtQuantity.Text.Trim()),
                ImagePath = _selectedImagePath
            };

            _productList.Add(pNew);
            ApplyFilter();
            ClearForm();
            MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;

            string pId = txtProductId.Text.Trim();
            var existing = _productList.FirstOrDefault(p => p.ProductId.Equals(pId, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                MessageBox.Show("Không tìm thấy sản phẩm cần cập nhật!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ValidateInput()) return;

            existing.ProductName = txtProductName.Text.Trim();
            existing.CategoryName = cboCategory.SelectedItem?.ToString() ?? "";
            existing.UnitPrice = decimal.Parse(txtUnitPrice.Text.Trim());
            existing.Quantity = int.Parse(txtQuantity.Text.Trim());
            if (!string.IsNullOrEmpty(_selectedImagePath))
            {
                existing.ImagePath = _selectedImagePath;
            }

            ApplyFilter();
            MessageBox.Show("Cập nhật sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;

            var currentProduct = dgvProducts.CurrentRow.DataBoundItem as Product;
            if (currentProduct == null) return;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa sản phẩm [{currentProduct.ProductName}]?",
                                        "Xác nhận xóa",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _productList.Remove(currentProduct);
                ApplyFilter();
                ClearForm();
                MessageBox.Show("Xóa sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Title = "Chọn ảnh sản phẩm";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedImagePath = ofd.FileName;
                    LoadImageToPictureBox(_selectedImagePath);
                }
            }
        }

        // Đã sửa: Sử dụng Bitmap để không khóa file ảnh trên đĩa
        private void LoadImageToPictureBox(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        picAvatar.Image?.Dispose();
                        picAvatar.Image = new Bitmap(Image.FromStream(stream));
                    }
                }
                catch
                {
                    picAvatar.Image = null;
                }
            }
            else
            {
                picAvatar.Image = null;
            }
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                var selectedProduct = dgvProducts.CurrentRow.DataBoundItem as Product;
                if (selectedProduct != null)
                {
                    txtProductId.Text = selectedProduct.ProductId;
                    txtProductName.Text = selectedProduct.ProductName;
                    cboCategory.SelectedItem = selectedProduct.CategoryName;
                    txtUnitPrice.Text = selectedProduct.UnitPrice.ToString("0");
                    txtQuantity.Text = selectedProduct.Quantity.ToString();
                    _selectedImagePath = selectedProduct.ImagePath;
                    LoadImageToPictureBox(_selectedImagePath);
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ExportCSV()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files (*.csv)|*.csv";
                sfd.FileName = "DanhMucSanPham.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Mã SP,Tên SP,Danh Mục,Đơn Giá,Số Lượng");

                        foreach (Product p in _productList)
                        {
                            // Xử lý Escape dấu ngoặc kép trong tên
                            string safeName = p.ProductName.Replace("\"", "\"\"");
                            sb.AppendLine($"\"{p.ProductId}\",\"{safeName}\",\"{p.CategoryName}\",{p.UnitPrice},{p.Quantity}");
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Có lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void menuExportCSV_Click(object sender, EventArgs e) => ExportCSV();
        private void btnExportCSV_Click(object sender, EventArgs e) => ExportCSV();

        private void menuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ClearForm()
        {
            txtProductId.Clear();
            txtProductName.Clear();
            txtUnitPrice.Clear();
            txtQuantity.Clear();
            if (cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;
            picAvatar.Image = null;
            _selectedImagePath = string.Empty;
            errorProvider1.Clear();
        }
    }

    public class Product
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
    }
}