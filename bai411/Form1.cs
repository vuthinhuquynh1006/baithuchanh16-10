using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace bai411
{
    public partial class Form1 : Form
    {
        private BindingList<ServiceOrder> orderList;
        private BindingSource bindingSource;

        private MenuStrip menuStrip1;
        private ToolStripMenuItem mnFile, mnExportCsv, mnExit;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblCount, lblTotalCost;

        private Label lbl1, lbl2, lbl3, lbl4, lbl5, lbl6, lbl7, lbl8, lblSearch, lblFilter;
        private TextBox txtOrderID, txtCustomerName, txtPhone, txtErrorDescription, txtEstimatedCost, txtSearch;
        private ComboBox cboDeviceType, cboStatus, cboFilterStatus;
        private DateTimePicker dtpReceiveDate;
        private PictureBox picDevice;
        private Button btnChooseImage, btnAdd, btnUpdate, btnDelete, btnClear;
        private DataGridView dgvOrders;
        private ErrorProvider errorProvider1;

        public Form1()
        {
            InitializeCustomLayout();
            InitializeData();
            SetupDataGridView();
            SetupComboBoxes();

            txtSearch.TextChanged += (s, e) => ApplyFilter();
            cboFilterStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
        }

        private void InitializeCustomLayout()
        {
            this.Text = "Repair Service Tracker - Quản lý đơn sửa chữa";
            this.Size = new Size(1160, 720);
            this.StartPosition = FormStartPosition.CenterScreen;

            errorProvider1 = new ErrorProvider();

            // MenuStrip
            menuStrip1 = new MenuStrip();
            mnFile = new ToolStripMenuItem("File");
            mnExportCsv = new ToolStripMenuItem("Xuất danh sách ra CSV", null, mnExportCsv_Click);
            mnExit = new ToolStripMenuItem("Thoát", null, mnExit_Click);
            mnFile.DropDownItems.Add(mnExportCsv);
            mnFile.DropDownItems.Add(mnExit);
            menuStrip1.Items.Add(mnFile);
            this.MainMenuStrip = menuStrip1;
            this.Controls.Add(menuStrip1);

            // StatusStrip
            statusStrip1 = new StatusStrip();
            lblCount = new ToolStripStatusLabel("Số đơn: 0   |   ");
            lblTotalCost = new ToolStripStatusLabel("Tổng chi phí (chưa xong): 0 VNĐ");
            statusStrip1.Items.Add(lblCount);
            statusStrip1.Items.Add(lblTotalCost);
            this.Controls.Add(statusStrip1);

            // Controls nhập liệu (Bên trái)
            int leftX = 20, topY = 45, gapY = 38;

            lbl1 = new Label { Text = "Mã đơn:", Left = leftX, Top = topY, Width = 100 };
            txtOrderID = new TextBox { Left = leftX + 110, Top = topY, Width = 180 };

            lbl2 = new Label { Text = "Tên khách hàng:", Left = leftX, Top = topY += gapY, Width = 100 };
            txtCustomerName = new TextBox { Left = leftX + 110, Top = topY, Width = 180 };

            lbl3 = new Label { Text = "Số điện thoại:", Left = leftX, Top = topY += gapY, Width = 100 };
            txtPhone = new TextBox { Left = leftX + 110, Top = topY, Width = 180 };

            lbl4 = new Label { Text = "Loại thiết bị:", Left = leftX, Top = topY += gapY, Width = 100 };
            cboDeviceType = new ComboBox { Left = leftX + 110, Top = topY, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };

            lbl5 = new Label { Text = "Mô tả lỗi:", Left = leftX, Top = topY += gapY, Width = 100 };
            txtErrorDescription = new TextBox { Left = leftX + 110, Top = topY, Width = 180, Height = 50, Multiline = true };
            topY += 15;

            lbl6 = new Label { Text = "Ngày tiếp nhận:", Left = leftX, Top = topY += gapY, Width = 100 };
            dtpReceiveDate = new DateTimePicker { Left = leftX + 110, Top = topY, Width = 180, Format = DateTimePickerFormat.Short };

            lbl7 = new Label { Text = "Chi phí dự kiến:", Left = leftX, Top = topY += gapY, Width = 100 };
            txtEstimatedCost = new TextBox { Left = leftX + 110, Top = topY, Width = 180 };

            lbl8 = new Label { Text = "Trạng thái:", Left = leftX, Top = topY += gapY, Width = 100 };
            cboStatus = new ComboBox { Left = leftX + 110, Top = topY, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };

            picDevice = new PictureBox { Left = leftX + 310, Top = 45, Width = 140, Height = 140, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            btnChooseImage = new Button { Text = "Chọn ảnh", Left = leftX + 310, Top = 195, Width = 140, Height = 30 };
            btnChooseImage.Click += btnChooseImage_Click;

            int btnY = 395;
            btnAdd = new Button { Text = "Thêm", Left = leftX, Top = btnY, Width = 80, Height = 35, BackColor = Color.LightGreen };
            btnUpdate = new Button { Text = "Cập nhật", Left = leftX + 90, Top = btnY, Width = 80, Height = 35, BackColor = Color.LightYellow };
            btnDelete = new Button { Text = "Xóa", Left = leftX + 180, Top = btnY, Width = 80, Height = 35, BackColor = Color.LightCoral };
            btnClear = new Button { Text = "Làm mới", Left = leftX + 270, Top = btnY, Width = 80, Height = 35 };

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += btnClear_Click;

            this.Controls.AddRange(new Control[] {
                lbl1, txtOrderID, lbl2, txtCustomerName, lbl3, txtPhone,
                lbl4, cboDeviceType, lbl5, txtErrorDescription, lbl6, dtpReceiveDate,
                lbl7, txtEstimatedCost, lbl8, cboStatus, picDevice, btnChooseImage,
                btnAdd, btnUpdate, btnDelete, btnClear
            });

            // Tìm kiếm & Lọc (Bên phải)
            int rightX = 490;
            lblSearch = new Label { Text = "Tìm kiếm:", Left = rightX, Top = 45, Width = 70 };
            txtSearch = new TextBox { Left = rightX + 75, Top = 43, Width = 170 };

            lblFilter = new Label { Text = "Lọc trạng thái:", Left = rightX + 260, Top = 45, Width = 90 };
            cboFilterStatus = new ComboBox { Left = rightX + 355, Top = 43, Width = 165, DropDownStyle = ComboBoxStyle.DropDownList };

            this.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblFilter, cboFilterStatus });

            // DataGridView
            dgvOrders = new DataGridView
            {
                Left = rightX,
                Top = 85,
                Width = 630,
                Height = 520,
                AllowUserToAddRows = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(dgvOrders);
        }

        private void InitializeData()
        {
            orderList = new BindingList<ServiceOrder>();
            bindingSource = new BindingSource();
            bindingSource.DataSource = orderList;

            orderList.Add(new ServiceOrder
            {
                OrderID = "DH001",
                CustomerName = "Nguyễn Văn A",
                Phone = "0901234567",
                DeviceType = "Laptop",
                ErrorDescription = "Hỏng màn hình, không lên nguồn",
                ReceiveDate = DateTime.Now.AddDays(-2),
                EstimatedCost = 1500000,
                Status = "Mới tiếp nhận",
                ImagePath = ""
            });

            bindingSource.PositionChanged += (s, e) => DisplaySelectedRow();
        }

        private void SetupDataGridView()
        {
            dgvOrders.AutoGenerateColumns = false;
            dgvOrders.DataSource = bindingSource;
            dgvOrders.Columns.Clear();

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OrderID", HeaderText = "Mã đơn", Width = 70 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerName", HeaderText = "Tên khách hàng", Width = 120 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "Số điện thoại", Width = 95 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DeviceType", HeaderText = "Thiết bị", Width = 85 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ErrorDescription", HeaderText = "Mô tả lỗi", Width = 130 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReceiveDate", HeaderText = "Ngày nhận", Width = 85, DefaultCellStyle = { Format = "dd/MM/yyyy" } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EstimatedCost", HeaderText = "Chi phí", Width = 90, DefaultCellStyle = { Format = "N0" } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 100 });

            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.ReadOnly = true;
        }

        private void SetupComboBoxes()
        {
            string[] deviceTypes = { "Laptop", "Điện thoại", "Máy tính bảng", "Quạt điện", "Tủ lạnh", "Thiết bị khác" };
            cboDeviceType.Items.AddRange(deviceTypes);
            cboDeviceType.SelectedIndex = 0;

            string[] statuses = { "Mới tiếp nhận", "Đang sửa", "Chờ linh kiện", "Hoàn tất", "Đã hủy" };
            cboStatus.Items.AddRange(statuses);
            cboStatus.SelectedIndex = 0;

            cboFilterStatus.Items.Add("Tất cả");
            cboFilterStatus.Items.AddRange(statuses);
            cboFilterStatus.SelectedIndex = 0;
        }

        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    picDevice.ImageLocation = ofd.FileName;
                    picDevice.Tag = ofd.FileName;
                }
            }
        }

        private void DisplaySelectedRow()
        {
            if (bindingSource.Current is ServiceOrder currentOrder)
            {
                txtOrderID.Text = currentOrder.OrderID;
                txtCustomerName.Text = currentOrder.CustomerName;
                txtPhone.Text = currentOrder.Phone;
                cboDeviceType.SelectedItem = currentOrder.DeviceType;
                txtErrorDescription.Text = currentOrder.ErrorDescription;
                dtpReceiveDate.Value = currentOrder.ReceiveDate;
                txtEstimatedCost.Text = currentOrder.EstimatedCost.ToString();
                cboStatus.SelectedItem = currentOrder.Status;

                if (!string.IsNullOrEmpty(currentOrder.ImagePath) && File.Exists(currentOrder.ImagePath))
                {
                    picDevice.ImageLocation = currentOrder.ImagePath;
                    picDevice.Tag = currentOrder.ImagePath;
                }
                else
                {
                    picDevice.Image = null;
                    picDevice.Tag = "";
                }
            }
        }

        private bool ValidateData(bool isNew)
        {
            errorProvider1.Clear();
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(txtOrderID.Text))
            {
                errorProvider1.SetError(txtOrderID, "Mã đơn không được để trống!");
                isValid = false;
            }
            else if (isNew && orderList.Any(o => o.OrderID.Equals(txtOrderID.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                errorProvider1.SetError(txtOrderID, "Mã đơn đã tồn tại!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                errorProvider1.SetError(txtCustomerName, "Tên khách hàng không được để trống!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                errorProvider1.SetError(txtPhone, "Số điện thoại không được để trống!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtErrorDescription.Text))
            {
                errorProvider1.SetError(txtErrorDescription, "Mô tả lỗi bắt buộc phải nhập!");
                isValid = false;
            }

            if (!decimal.TryParse(txtEstimatedCost.Text, out decimal cost) || cost < 0)
            {
                errorProvider1.SetError(txtEstimatedCost, "Chi phí dự kiến phải là số hợp lệ và không âm!");
                isValid = false;
            }

            return isValid;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateData(true)) return;

            var newOrder = new ServiceOrder
            {
                OrderID = txtOrderID.Text.Trim(),
                CustomerName = txtCustomerName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                DeviceType = cboDeviceType.SelectedItem.ToString(),
                ErrorDescription = txtErrorDescription.Text.Trim(),
                ReceiveDate = dtpReceiveDate.Value,
                EstimatedCost = decimal.Parse(txtEstimatedCost.Text),
                Status = cboStatus.SelectedItem.ToString(),
                ImagePath = picDevice.Tag?.ToString() ?? ""
            };

            orderList.Add(newOrder);
            ApplyFilter();
            MessageBox.Show("Thêm đơn hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (bindingSource.Current is ServiceOrder currentOrder)
            {
                if (!ValidateData(false)) return;

                currentOrder.CustomerName = txtCustomerName.Text.Trim();
                currentOrder.Phone = txtPhone.Text.Trim();
                currentOrder.DeviceType = cboDeviceType.SelectedItem.ToString();
                currentOrder.ErrorDescription = txtErrorDescription.Text.Trim();
                currentOrder.ReceiveDate = dtpReceiveDate.Value;
                currentOrder.EstimatedCost = decimal.Parse(txtEstimatedCost.Text);
                currentOrder.Status = cboStatus.SelectedItem.ToString();
                currentOrder.ImagePath = picDevice.Tag?.ToString() ?? "";

                bindingSource.ResetBindings(false);
                ApplyFilter();
                MessageBox.Show("Cập nhật đơn hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn đơn cần cập nhật!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (bindingSource.Current is ServiceOrder currentOrder)
            {
                var confirm = MessageBox.Show($"Bạn có chắc muốn xóa đơn {currentOrder.OrderID}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    orderList.Remove(currentOrder);
                    ApplyFilter();
                    MessageBox.Show("Đã xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dòng cần xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplyFilter()
        {
            string keyword = txtSearch.Text.Trim().ToLower();
            string selectedStatus = cboFilterStatus.SelectedItem?.ToString() ?? "Tất cả";

            var filtered = orderList.Where(o =>
                (string.IsNullOrEmpty(keyword) ||
                 o.OrderID.ToLower().Contains(keyword) ||
                 o.CustomerName.ToLower().Contains(keyword) ||
                 o.Phone.Contains(keyword)) &&
                (selectedStatus == "Tất cả" || o.Status == selectedStatus)
            ).ToList();

            dgvOrders.DataSource = filtered;
            UpdateStatusStrip(filtered);
        }

        private void UpdateStatusStrip(System.Collections.Generic.List<ServiceOrder> currentList)
        {
            int count = currentList.Count;
            decimal totalUncompletedCost = currentList
                .Where(o => o.Status != "Hoàn tất" && o.Status != "Đã hủy")
                .Sum(o => o.EstimatedCost);

            lblCount.Text = $"Số đơn: {count}   |   ";
            lblTotalCost.Text = $"Tổng chi phí (chưa xong): {totalUncompletedCost:N0} VNĐ";
        }

        private void mnExportCsv_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV file (*.csv)|*.csv";
                sfd.FileName = "RepairOrders.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("MaDon,TenKhachHang,SoDienThoai,LoaiThietBi,MoTaLoi,NgayTiepNhan,ChiPhiDuKien,TrangThai");
                            foreach (DataGridViewRow row in dgvOrders.Rows)
                            {
                                if (row.DataBoundItem is ServiceOrder o)
                                {
                                    sw.WriteLine($"{o.OrderID},{o.CustomerName},{o.Phone},{o.DeviceType},\"{o.ErrorDescription}\",{o.ReceiveDate:dd/MM/yyyy},{o.EstimatedCost},{o.Status}");
                                }
                            }
                        }
                        MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void mnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtOrderID.Clear();
            txtCustomerName.Clear();
            txtPhone.Clear();
            txtErrorDescription.Clear();
            txtEstimatedCost.Clear();
            picDevice.Image = null;
            picDevice.Tag = "";
            errorProvider1.Clear();
            txtOrderID.Focus();
        }
    }
}