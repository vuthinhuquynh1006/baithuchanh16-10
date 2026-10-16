using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace bai410
{
    public partial class Form1 : Form
    {
        private TableLayoutPanel tableLayoutPanelMain;
        private TableLayoutPanel tableLayoutInputFields; // Bảng bố trí form nhập liệu chuẩn tuyệt đối
        private Panel pnlInputContainer;

        private Label lblTicketId, lblStudentId, lblStudentName, lblEquipment, lblQuantity, lblBorrowDate, lblDueDate, lblReturnDate, lblStatus;
        private TextBox txtTicketId, txtStudentId, txtStudentName, txtQuantity;
        private ComboBox cbEquipment, cbStatus;
        private DateTimePicker dtpBorrowDate, dtpDueDate, dtpReturnDate;
        private CheckBox chkIsReturned;
        private Button btnAdd, btnUpdate, btnDelete, btnClear;
        private FlowLayoutPanel pnlButtons;

        private Panel pnlSearchFilter;
        private Label lblSearch, lblFilterStatus;
        private TextBox txtSearch;
        private ComboBox cbFilterStatus;

        private DataGridView dgvCheckouts;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuFile, menuExportCsv;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblTotalCount, lblOverdueCount;
        private ErrorProvider errorProvider1;

        private BindingList<CheckoutRecord> recordList;
        private BindingSource bindingSource;

        public Form1()
        {
            InitializeCustomComponent();
            InitializeData();
            LoadComboBoxData();
        }

        private void InitializeCustomComponent()
        {
            this.Text = "Lab Equipment Checkout - Quản lý mượn trả thiết bị phòng thực hành";
            this.Size = new Size(1200, 780);
            this.StartPosition = FormStartPosition.CenterScreen;

            errorProvider1 = new ErrorProvider();

            // 1. MenuStrip
            menuStrip1 = new MenuStrip();
            menuFile = new ToolStripMenuItem("File");
            menuExportCsv = new ToolStripMenuItem("Xuất danh sách đang lọc ra CSV...", null, menuExportCsv_Click);
            menuFile.DropDownItems.Add(menuExportCsv);
            menuStrip1.Items.Add(menuFile);
            this.MainMenuStrip = menuStrip1;
            this.Controls.Add(menuStrip1);

            // 2. StatusStrip
            statusStrip1 = new StatusStrip();
            lblTotalCount = new ToolStripStatusLabel();
            lblOverdueCount = new ToolStripStatusLabel();
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblTotalCount, new ToolStripStatusLabel(" | "), lblOverdueCount });
            this.Controls.Add(statusStrip1);

            // 3. TableLayoutPanel chính chia giao diện tổng thể
            tableLayoutPanelMain = new TableLayoutPanel();
            tableLayoutPanelMain.Dock = DockStyle.Fill;
            tableLayoutPanelMain.RowCount = 3;
            tableLayoutPanelMain.ColumnCount = 1;
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 250F)); // Khu vực nhập liệu
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // Khu vực tìm kiếm/lọc
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // DataGridView
            this.Controls.Add(tableLayoutPanelMain);

            tableLayoutPanelMain.Padding = new Padding(10, 30, 10, 25);

            // --- Khu vực nhập liệu sử dụng TableLayoutPanel con để KHÔNG BAO GIỜ bị đè chữ ---
            pnlInputContainer = new Panel() { Dock = DockStyle.Fill };

            tableLayoutInputFields = new TableLayoutPanel();
            tableLayoutInputFields.Dock = DockStyle.Fill;
            tableLayoutInputFields.ColumnCount = 4;
            tableLayoutInputFields.RowCount = 4;
            tableLayoutInputFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
            tableLayoutInputFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            tableLayoutInputFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
            tableLayoutInputFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));

            // Khởi tạo các Control
            lblTicketId = new Label() { Text = "Mã phiếu:", Anchor = AnchorStyles.Left };
            txtTicketId = new TextBox() { Dock = DockStyle.Fill };

            lblStudentId = new Label() { Text = "Mã SV:", Anchor = AnchorStyles.Left };
            txtStudentId = new TextBox() { Dock = DockStyle.Fill };

            lblStudentName = new Label() { Text = "Họ tên SV:", Anchor = AnchorStyles.Left };
            txtStudentName = new TextBox() { Dock = DockStyle.Fill }; // Sửa triệt để lỗi không gõ được

            lblEquipment = new Label() { Text = "Thiết bị:", Anchor = AnchorStyles.Left };
            cbEquipment = new ComboBox() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };

            lblQuantity = new Label() { Text = "Số lượng:", Anchor = AnchorStyles.Left };
            txtQuantity = new TextBox() { Dock = DockStyle.Fill, Text = "1" };

            lblBorrowDate = new Label() { Text = "Ngày mượn:", Anchor = AnchorStyles.Left };
            dtpBorrowDate = new DateTimePicker() { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };

            lblDueDate = new Label() { Text = "Hạn trả:", Anchor = AnchorStyles.Left };
            dtpDueDate = new DateTimePicker() { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };

            lblStatus = new Label() { Text = "Trạng thái:", Anchor = AnchorStyles.Left };
            cbStatus = new ComboBox() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cbStatus.Items.AddRange(new string[] { "Đang mượn", "Đã trả", "Quá hạn" });
            cbStatus.SelectedIndex = 0;

            lblReturnDate = new Label() { Text = "Ngày trả thực tế:", Anchor = AnchorStyles.Left };
            dtpReturnDate = new DateTimePicker() { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Enabled = false };

            chkIsReturned = new CheckBox() { Text = "Đã hoàn trả thiết bị", Anchor = AnchorStyles.Left };
            chkIsReturned.CheckedChanged += (s, e) => {
                dtpReturnDate.Enabled = chkIsReturned.Checked;
                if (chkIsReturned.Checked)
                {
                    dtpReturnDate.Value = DateTime.Now;
                    cbStatus.SelectedItem = "Đã trả";
                }
                else
                {
                    cbStatus.SelectedItem = "Đang mượn";
                }
            };

            // Đưa vào bảng lưới (Row, Col)
            tableLayoutInputFields.Controls.Add(lblTicketId, 0, 0);
            tableLayoutInputFields.Controls.Add(txtTicketId, 1, 0);
            tableLayoutInputFields.Controls.Add(lblStudentId, 2, 0);
            tableLayoutInputFields.Controls.Add(txtStudentId, 3, 0);

            tableLayoutInputFields.Controls.Add(lblStudentName, 0, 1);
            tableLayoutInputFields.Controls.Add(txtStudentName, 1, 1);
            tableLayoutInputFields.Controls.Add(lblEquipment, 2, 1);
            tableLayoutInputFields.Controls.Add(cbEquipment, 3, 1);

            tableLayoutInputFields.Controls.Add(lblQuantity, 0, 2);
            tableLayoutInputFields.Controls.Add(txtQuantity, 1, 2);
            tableLayoutInputFields.Controls.Add(lblBorrowDate, 2, 2);
            tableLayoutInputFields.Controls.Add(dtpBorrowDate, 3, 2);

            tableLayoutInputFields.Controls.Add(lblDueDate, 0, 3);
            tableLayoutInputFields.Controls.Add(dtpDueDate, 1, 3);
            tableLayoutInputFields.Controls.Add(lblStatus, 2, 3);
            tableLayoutInputFields.Controls.Add(cbStatus, 3, 3);

            pnlInputContainer.Controls.Add(tableLayoutInputFields);

            // Bố trí Panel chứa nút bấm bên dưới lưới nhập liệu
            Panel pnlBottomAction = new Panel() { Dock = DockStyle.Bottom, Height = 45 };

            pnlButtons = new FlowLayoutPanel() { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            btnAdd = new Button() { Text = "Thêm phiếu", Width = 100, Height = 32 };
            btnAdd.Click += btnAdd_Click;
            btnUpdate = new Button() { Text = "Cập nhật", Width = 95, Height = 32 };
            btnUpdate.Click += btnUpdate_Click;
            btnDelete = new Button() { Text = "Xóa", Width = 85, Height = 32 };
            btnDelete.Click += btnDelete_Click;
            btnClear = new Button() { Text = "Làm mới", Width = 95, Height = 32 };
            btnClear.Click += btnClear_Click;

            FlowLayoutPanel pnlLeftExtras = new FlowLayoutPanel() { Dock = DockStyle.Left, FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Padding = new Padding(0, 5, 0, 0) };
            pnlLeftExtras.Controls.Add(chkIsReturned);
            pnlLeftExtras.Controls.Add(lblReturnDate);
            pnlLeftExtras.Controls.Add(dtpReturnDate);

            pnlButtons.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });
            pnlBottomAction.Controls.Add(pnlLeftExtras);
            pnlBottomAction.Controls.Add(pnlButtons);

            // Gắn vào vùng container nhập liệu chung
            tableLayoutInputFields.Dock = DockStyle.Top;
            pnlInputContainer.Controls.Add(pnlBottomAction);
            tableLayoutPanelMain.Controls.Add(pnlInputContainer, 0, 0);

            // --- Khu vực Tìm kiếm và Lọc ---
            pnlSearchFilter = new Panel() { Dock = DockStyle.Fill };

            lblSearch = new Label() { Text = "Tìm kiếm (Mã SV / Tên thiết bị):", Location = new Point(5, 12), AutoSize = true };
            txtSearch = new TextBox() { Location = new Point(200, 9), Width = 250 };
            txtSearch.TextChanged += (s, e) => FilterData();

            lblFilterStatus = new Label() { Text = "Lọc trạng thái:", Location = new Point(480, 12), AutoSize = true };
            cbFilterStatus = new ComboBox() { Location = new Point(580, 9), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cbFilterStatus.Items.AddRange(new string[] { "Tất cả trạng thái", "Đang mượn", "Đã trả", "Quá hạn" });
            cbFilterStatus.SelectedIndex = 0;
            cbFilterStatus.SelectedIndexChanged += (s, e) => FilterData();

            pnlSearchFilter.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblFilterStatus, cbFilterStatus });
            tableLayoutPanelMain.Controls.Add(pnlSearchFilter, 0, 1);

            // --- Khu vực DataGridView ---
            dgvCheckouts = new DataGridView();
            dgvCheckouts.Dock = DockStyle.Fill;
            dgvCheckouts.AutoGenerateColumns = false;
            dgvCheckouts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCheckouts.ReadOnly = true;
            dgvCheckouts.AllowUserToAddRows = false;
            dgvCheckouts.CellClick += dgvCheckouts_CellClick;

            SetupDataGridViewColumns();
            tableLayoutPanelMain.Controls.Add(dgvCheckouts, 0, 2);
        }

        private void SetupDataGridViewColumns()
        {
            dgvCheckouts.Columns.Clear();
            dgvCheckouts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TicketId", HeaderText = "Mã phiếu", Width = 90 });
            dgvCheckouts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StudentId", HeaderText = "Mã SV", Width = 100 });
            dgvCheckouts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StudentName", HeaderText = "Họ tên sinh viên", Width = 180 });
            dgvCheckouts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EquipmentName", HeaderText = "Thiết bị", Width = 210 });
            dgvCheckouts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "SL", Width = 50 });
            dgvCheckouts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BorrowDate", HeaderText = "Ngày mượn", Width = 100, DefaultCellStyle = { Format = "dd/MM/yyyy" } });
            dgvCheckouts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DueDate", HeaderText = "Hạn trả", Width = 100, DefaultCellStyle = { Format = "dd/MM/yyyy" } });
            dgvCheckouts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReturnDate", HeaderText = "Ngày trả thực tế", Width = 120, DefaultCellStyle = { Format = "dd/MM/yyyy" } });
            dgvCheckouts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 110 });
        }

        private void InitializeData()
        {
            recordList = new BindingList<CheckoutRecord>()
            {
                new CheckoutRecord { TicketId = "PM001", StudentId = "SV01", StudentName = "Nguyễn Văn An", EquipmentName = "Máy hiện sóng (Oscilloscope)", Quantity = 1, BorrowDate = DateTime.Now.AddDays(-5), DueDate = DateTime.Now.AddDays(-1), ReturnDate = null, Status = "Quá hạn" },
                new CheckoutRecord { TicketId = "PM002", StudentId = "SV02", StudentName = "Trần Thị Bình", EquipmentName = "Bộ nguồn DC đa năng", Quantity = 2, BorrowDate = DateTime.Now.AddDays(-2), DueDate = DateTime.Now.AddDays(3), ReturnDate = null, Status = "Đang mượn" }
            };

            bindingSource = new BindingSource();
            bindingSource.DataSource = recordList;
            dgvCheckouts.DataSource = bindingSource;

            UpdateStatusStripInfo();
        }

        private void LoadComboBoxData()
        {
            cbEquipment.Items.AddRange(new string[] {
                "Máy hiện sóng (Oscilloscope)",
                "Bộ nguồn DC đa năng",
                "Đồng hồ vạn năng số (DMM)",
                "Mỏ hàn điện tử",
                "Kit vi điều khiển STM32"
            });
            if (cbEquipment.Items.Count > 0) cbEquipment.SelectedIndex = 0;
        }

        private void RefreshRecordStatuses()
        {
            foreach (var item in recordList)
            {
                if (item.Status != "Đã trả")
                {
                    if (item.DueDate.Date < DateTime.Now.Date)
                        item.Status = "Quá hạn";
                    else
                        item.Status = "Đang mượn";
                }
            }
        }

        private bool ValidateInput(bool isNew = true)
        {
            bool isValid = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtTicketId.Text))
            {
                errorProvider1.SetError(txtTicketId, "Mã phiếu không được để trống!");
                isValid = false;
            }
            else if (isNew && recordList.Any(x => x.TicketId == txtTicketId.Text.Trim()))
            {
                errorProvider1.SetError(txtTicketId, "Mã phiếu đã tồn tại!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtStudentId.Text))
            {
                errorProvider1.SetError(txtStudentId, "Mã sinh viên không được để trống!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtStudentName.Text))
            {
                errorProvider1.SetError(txtStudentName, "Tên sinh viên không được để trống!");
                isValid = false;
            }

            if (!int.TryParse(txtQuantity.Text.Trim(), out int qty) || qty <= 0)
            {
                errorProvider1.SetError(txtQuantity, "Số lượng phải lớn hơn 0!");
                isValid = false;
            }

            if (dtpDueDate.Value.Date < dtpBorrowDate.Value.Date)
            {
                errorProvider1.SetError(dtpDueDate, "Hạn trả không được trước ngày mượn!");
                isValid = false;
            }

            if (chkIsReturned.Checked && dtpReturnDate.Value.Date < dtpBorrowDate.Value.Date)
            {
                errorProvider1.SetError(dtpReturnDate, "Ngày trả thực tế không được trước ngày mượn!");
                isValid = false;
            }

            return isValid;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(true)) return;

            string status = cbStatus.SelectedItem?.ToString() ?? "Đang mượn";
            DateTime? retDate = null;

            if (chkIsReturned.Checked)
            {
                status = "Đã trả";
                retDate = dtpReturnDate.Value.Date;
            }

            var newRec = new CheckoutRecord
            {
                TicketId = txtTicketId.Text.Trim(),
                StudentId = txtStudentId.Text.Trim(),
                StudentName = txtStudentName.Text.Trim(),
                EquipmentName = cbEquipment.SelectedItem.ToString(),
                Quantity = int.Parse(txtQuantity.Text.Trim()),
                BorrowDate = dtpBorrowDate.Value.Date,
                DueDate = dtpDueDate.Value.Date,
                ReturnDate = retDate,
                Status = status
            };

            recordList.Add(newRec);
            ClearInputs();
            UpdateStatusStripInfo();
            MessageBox.Show("Thêm phiếu mượn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvCheckouts.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu mượn cần cập nhật trên bảng!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRec = dgvCheckouts.CurrentRow.DataBoundItem as CheckoutRecord;
            if (selectedRec != null)
            {
                if (!ValidateInput(false)) return;

                selectedRec.StudentId = txtStudentId.Text.Trim();
                selectedRec.StudentName = txtStudentName.Text.Trim();
                selectedRec.EquipmentName = cbEquipment.SelectedItem.ToString();
                selectedRec.Quantity = int.Parse(txtQuantity.Text.Trim());
                selectedRec.BorrowDate = dtpBorrowDate.Value.Date;
                selectedRec.DueDate = dtpDueDate.Value.Date;
                selectedRec.Status = cbStatus.SelectedItem?.ToString() ?? "Đang mượn";

                if (chkIsReturned.Checked)
                {
                    selectedRec.ReturnDate = dtpReturnDate.Value.Date;
                }
                else
                {
                    selectedRec.ReturnDate = null;
                }

                bindingSource.ResetBindings(false);
                UpdateStatusStripInfo();
                MessageBox.Show("Cập nhật phiếu mượn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCheckouts.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu mượn cần xóa trên bảng!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("Bạn có chắc chắn muốn xóa phiếu mượn này không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                var selectedRec = dgvCheckouts.CurrentRow.DataBoundItem as CheckoutRecord;
                if (selectedRec != null)
                {
                    recordList.Remove(selectedRec);
                    ClearInputs();
                    UpdateStatusStripInfo();
                    MessageBox.Show("Đã xóa phiếu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void dgvCheckouts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedRec = dgvCheckouts.Rows[e.RowIndex].DataBoundItem as CheckoutRecord;
                if (selectedRec != null)
                {
                    txtTicketId.Text = selectedRec.TicketId;
                    txtTicketId.Enabled = false;
                    txtStudentId.Text = selectedRec.StudentId;
                    txtStudentName.Text = selectedRec.StudentName;
                    cbEquipment.SelectedItem = selectedRec.EquipmentName;
                    txtQuantity.Text = selectedRec.Quantity.ToString();
                    dtpBorrowDate.Value = selectedRec.BorrowDate;
                    dtpDueDate.Value = selectedRec.DueDate;
                    cbStatus.SelectedItem = selectedRec.Status;

                    if (selectedRec.ReturnDate.HasValue)
                    {
                        chkIsReturned.Checked = true;
                        dtpReturnDate.Value = selectedRec.ReturnDate.Value;
                        dtpReturnDate.Enabled = true;
                    }
                    else
                    {
                        chkIsReturned.Checked = false;
                        dtpReturnDate.Enabled = false;
                    }
                }
            }
        }

        private void ClearInputs()
        {
            txtTicketId.Clear();
            txtTicketId.Enabled = true;
            txtStudentId.Clear();
            txtStudentName.Clear();
            txtQuantity.Text = "1";
            dtpBorrowDate.Value = DateTime.Now;
            dtpDueDate.Value = DateTime.Now.AddDays(7);
            chkIsReturned.Checked = false;
            dtpReturnDate.Enabled = false;
            cbStatus.SelectedIndex = 0;
            errorProvider1.Clear();
            dgvCheckouts.ClearSelection();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        private void FilterData()
        {
            RefreshRecordStatuses();
            string keyword = txtSearch.Text.Trim().ToLower();
            string selectedStatus = cbFilterStatus.SelectedItem?.ToString();

            var filtered = recordList.Where(x =>
                (string.IsNullOrEmpty(keyword) || x.StudentId.ToLower().Contains(keyword) || x.EquipmentName.ToLower().Contains(keyword)) &&
                (selectedStatus == "Tất cả trạng thái" || x.Status == selectedStatus)
            ).ToList();

            bindingSource.DataSource = new BindingList<CheckoutRecord>(filtered);
            UpdateStatusStripInfo();
        }

        private void UpdateStatusStripInfo()
        {
            int total = bindingSource.Count;
            int overdue = recordList.Count(x => x.Status == "Quá hạn");
            lblTotalCount.Text = $"Tổng số phiếu hiển thị: {total}";
            lblOverdueCount.Text = $"Số phiếu quá hạn (tổng): {overdue}";
        }

        private void menuExportCsv_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV file (*.csv)|*.csv";
                sfd.FileName = "DanhSachMuonThietBi.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("MaPhiếu,MaSV,HoTenSV,ThietBi,SoLuong,NgayMuon,HanTra,NgayTraThucTe,TrangThai");

                            foreach (var item in bindingSource.List)
                            {
                                if (item is CheckoutRecord rec)
                                {
                                    string retStr = rec.ReturnDate.HasValue ? rec.ReturnDate.Value.ToString("yyyy-MM-dd") : "";
                                    sw.WriteLine($"{rec.TicketId},{rec.StudentId},\"{rec.StudentName}\",\"{rec.EquipmentName}\",{rec.Quantity},{rec.BorrowDate:yyyy-MM-dd},{rec.DueDate:yyyy-MM-dd},{retStr},{rec.Status}");
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
    }
}