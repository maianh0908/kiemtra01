using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Code_winform
{
    public partial class Form1 : Form
    {
        // Lớp Model đại diện cho Sản phẩm
        public class Product
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string CategoryName { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public string ImagePath { get; set; }
        }

        private BindingList<Product> productList = new BindingList<Product>();
        private string selectedImagePath = "";

        public Form1()
        {
            InitializeComponent();
            InitData();
        }

        private void InitData()
        {
            // Khởi tạo danh mục mẫu
            cboCategory.Items.AddRange(new string[] { "Điện thoại", "Laptop", "Phụ kiện", "Linh kiện" });
            if (cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;

            // Gán dữ liệu cho DataGridView
            dgvProducts.AutoGenerateColumns = false;
            dgvProducts.DataSource = productList;

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            lblStatus.Text = $"Tổng số sản phẩm: {productList.Count}";
        }

        private void ClearInputs()
        {
            txtProductId.Clear();
            txtProductName.Clear();
            txtUnitPrice.Clear();
            txtQuantity.Clear();
            if (cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;
            picAvatar.Image = null;
            selectedImagePath = "";
            errorProvider1.Clear();
        }

        private bool ValidateInput()
        {
            errorProvider1.Clear();
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(txtProductId.Text))
            {
                errorProvider1.SetError(txtProductId, "Mã sản phẩm không được để trống!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                errorProvider1.SetError(txtProductName, "Tên sản phẩm không được để trống!");
                isValid = false;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal price) || price < 0)
            {
                errorProvider1.SetError(txtUnitPrice, "Đơn giá phải là số hợp lệ!");
                isValid = false;
            }

            if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 0)
            {
                errorProvider1.SetError(txtQuantity, "Số lượng phải là số nguyên dương!");
                isValid = false;
            }

            return isValid;
        }

        // --- CÁC THAO TÁC XỬ LÝ SỰ KIỆN ---

        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedImagePath = ofd.FileName;
                    picAvatar.Image = Image.FromFile(selectedImagePath);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            string id = txtProductId.Text.Trim();
            if (productList.Any(p => p.ProductId.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Mã sản phẩm đã tồn tại!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Product p = new Product
            {
                ProductId = id,
                ProductName = txtProductName.Text.Trim(),
                CategoryName = cboCategory.SelectedItem?.ToString() ?? "",
                UnitPrice = decimal.Parse(txtUnitPrice.Text.Trim()),
                Quantity = int.Parse(txtQuantity.Text.Trim()),
                ImagePath = selectedImagePath
            };

            productList.Add(p);
            ClearInputs();
            UpdateStatus();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;
            if (!ValidateInput()) return;

            string id = txtProductId.Text.Trim();
            Product p = productList.FirstOrDefault(item => item.ProductId.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (p != null)
            {
                p.ProductName = txtProductName.Text.Trim();
                p.CategoryName = cboCategory.SelectedItem?.ToString() ?? "";
                p.UnitPrice = decimal.Parse(txtUnitPrice.Text.Trim());
                p.Quantity = int.Parse(txtQuantity.Text.Trim());
                if (!string.IsNullOrEmpty(selectedImagePath))
                {
                    p.ImagePath = selectedImagePath;
                }

                dgvProducts.Refresh();
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Không tìm thấy mã sản phẩm để cập nhật!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;

            string id = txtProductId.Text.Trim();
            Product p = productList.FirstOrDefault(item => item.ProductId.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (p != null)
            {
                var dialogResult = MessageBox.Show($"Bạn có chắc chắn muốn xóa sản phẩm {p.ProductName}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    productList.Remove(p);
                    ClearInputs();
                    UpdateStatus();
                }
            }
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null && dgvProducts.CurrentRow.DataBoundItem is Product p)
            {
                txtProductId.Text = p.ProductId;
                txtProductName.Text = p.ProductName;
                cboCategory.SelectedItem = p.CategoryName;
                txtUnitPrice.Text = p.UnitPrice.ToString("0");
                txtQuantity.Text = p.Quantity.ToString();

                selectedImagePath = p.ImagePath;
                if (!string.IsNullOrEmpty(p.ImagePath) && File.Exists(p.ImagePath))
                {
                    picAvatar.Image = Image.FromFile(p.ImagePath);
                }
                else
                {
                    picAvatar.Image = null;
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(keyword))
            {
                dgvProducts.DataSource = productList;
            }
            else
            {
                var filteredList = productList.Where(p => p.ProductName.ToLower().Contains(keyword)).ToList();
                dgvProducts.DataSource = new BindingList<Product>(filteredList);
            }
        }

        private void btnExportCSV_Click(object sender, EventArgs e)
        {
            menuExportCSV_Click(sender, e);
        }

        private void menuExportCSV_Click(object sender, EventArgs e)
        {
            if (productList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files (*.csv)|*.csv";
                sfd.FileName = "DanhMucSanPham.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Mã SP,Tên SP,Danh Mục,Đơn Giá,Số Lượng");

                    foreach (var item in productList)
                    {
                        sb.AppendLine($"\"{item.ProductId}\",\"{item.ProductName}\",\"{item.CategoryName}\",{item.UnitPrice},{item.Quantity}");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất dữ liệu ra file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}