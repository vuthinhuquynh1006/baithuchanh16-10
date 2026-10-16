using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace bai49
{
    public partial class Form1 : Form
    {
        // Khai báo các Controls giao diện
        private TableLayoutPanel tableLayoutPanelMain;
        private Panel pnlInput;
        private Label lblId, lblName, lblPhone, lblDate, lblDoctor, lblReason, lblStatus;
        private TextBox txtAppointmentId, txtPatientName, txtPhone, txtReason;
        private DateTimePicker dtpAppointmentDate;
        private ComboBox cbDoctor, cbStatus;
        private Button btnAdd, btnUpdate, btnDelete, btnClear;

        private Panel pnlSearchFilter;
        private Label lblSearch, lblFilterStatus;
        private TextBox txtSearch;
        private ComboBox cbFilterStatus;

        private DataGridView dgvAppointments;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuFile, menuExportCsv;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatusCount;
        private ErrorProvider errorProvider1;

        // Dữ liệu
        private BindingList<Appointment> appointmentList;
        private BindingSource bindingSource;

        public Form1()
        {
            InitializeCustomComponent();
            InitializeData();
            LoadComboBoxData();
        }

        private void InitializeCustomComponent()
        {
            this.Text = "Clinic Appointment Manager - Quản lý lịch hẹn phòng khám";
            this.Size = new Size(1000, 680);
            this.StartPosition = FormStartPosition.CenterScreen;

            errorProvider1 = new ErrorProvider();

            // 1. MenuStrip
            menuStrip1 = new MenuStrip();
            menuFile = new ToolStripMenuItem("File");
            menuExportCsv = new ToolStripMenuItem("Xuất danh sách ra CSV...", null, menuExportCsv_Click);
            menuFile.DropDownItems.Add(menuExportCsv);
            menuStrip1.Items.Add(menuFile);
            this.MainMenuStrip = menuStrip1;
            this.Controls.Add(menuStrip1);

            // 2. StatusStrip
            statusStrip1 = new StatusStrip();
            lblStatusCount = new ToolStripStatusLabel();
            statusStrip1.Items.Add(lblStatusCount);
            this.Controls.Add(statusStrip1);

            // 3. TableLayoutPanel chính chia giao diện co giãn
            tableLayoutPanelMain = new TableLayoutPanel();
            tableLayoutPanelMain.Dock = DockStyle.Fill;
            tableLayoutPanelMain.RowCount = 3;
            tableLayoutPanelMain.ColumnCount = 1;
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F)); // Khu vực nhập liệu
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // Khu vực tìm kiếm/lọc
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // DataGridView co giãn
            this.Controls.Add(tableLayoutPanelMain);

            tableLayoutPanelMain.Padding = new Padding(10, 30, 10, 25);

            // --- Khu vực nhập liệu (pnlInput) ---
            pnlInput = new Panel();
            pnlInput.Dock = DockStyle.Fill;

            lblId = new Label() { Text = "Mã lịch hẹn:", Location = new Point(10, 15), AutoSize = true };
            txtAppointmentId = new TextBox() { Location = new Point(100, 12), Width = 150 };

            lblName = new Label() { Text = "Họ tên BN:", Location = new Point(280, 15), AutoSize = true };
            txtPatientName = new TextBox() { Location = new Point(360, 12), Width = 180 };

            lblPhone = new Label() { Text = "Số điện thoại:", Location = new Point(570, 15), AutoSize = true };
            txtPhone = new TextBox() { Location = new Point(660, 12), Width = 150 };

            lblDate = new Label() { Text = "Ngày giờ hẹn:", Location = new Point(10, 55), AutoSize = true };
            dtpAppointmentDate = new DateTimePicker() { Location = new Point(100, 52), Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Width = 150 };

            lblDoctor = new Label() { Text = "Bác sĩ:", Location = new Point(280, 55), AutoSize = true };
            cbDoctor = new ComboBox() { Location = new Point(360, 52), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };

            lblStatus = new Label() { Text = "Trạng thái:", Location = new Point(570, 55), AutoSize = true };
            cbStatus = new ComboBox() { Location = new Point(660, 52), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };

            lblReason = new Label() { Text = "Lý do khám:", Location = new Point(10, 95), AutoSize = true };
            txtReason = new TextBox() { Location = new Point(100, 92), Width = 440 };

            // Các Nút Chức Năng
            btnAdd = new Button() { Text = "Thêm", Location = new Point(560, 90), Width = 70 };
            btnAdd.Click += btnAdd_Click;

            btnUpdate = new Button() { Text = "Cập nhật", Location = new Point(640, 90), Width = 75 };
            btnUpdate.Click += btnUpdate_Click;

            btnDelete = new Button() { Text = "Xóa", Location = new Point(725, 90), Width = 65 };
            btnDelete.Click += btnDelete_Click;

            btnClear = new Button() { Text = "Làm mới", Location = new Point(800, 90), Width = 75 };
            btnClear.Click += btnClear_Click;

            pnlInput.Controls.AddRange(new Control[] { lblId, txtAppointmentId, lblName, txtPatientName, lblPhone, txtPhone,
                lblDate, dtpAppointmentDate, lblDoctor, cbDoctor, lblStatus, cbStatus, lblReason, txtReason,
                btnAdd, btnUpdate, btnDelete, btnClear });

            tableLayoutPanelMain.Controls.Add(pnlInput, 0, 0);

            // --- Khu vực Tìm kiếm và Lọc (pnlSearchFilter) ---
            pnlSearchFilter = new Panel();
            pnlSearchFilter.Dock = DockStyle.Fill;

            lblSearch = new Label() { Text = "Tìm kiếm (Tên/SĐT):", Location = new Point(10, 12), AutoSize = true };
            txtSearch = new TextBox() { Location = new Point(130, 9), Width = 250 };
            txtSearch.TextChanged += txtSearch_TextChanged;

            lblFilterStatus = new Label() { Text = "Lọc trạng thái:", Location = new Point(410, 12), AutoSize = true };
            cbFilterStatus = new ComboBox() { Location = new Point(510, 9), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cbFilterStatus.SelectedIndexChanged += cbFilterStatus_SelectedIndexChanged;

            pnlSearchFilter.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblFilterStatus, cbFilterStatus });
            tableLayoutPanelMain.Controls.Add(pnlSearchFilter, 0, 1);

            // --- Khu vực Bảng hiển thị (DataGridView) ---
            dgvAppointments = new DataGridView();
            dgvAppointments.Dock = DockStyle.Fill;
            dgvAppointments.AutoGenerateColumns = false;
            dgvAppointments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAppointments.ReadOnly = true;
            dgvAppointments.AllowUserToAddRows = false;
            dgvAppointments.CellClick += dgvAppointments_CellClick;

            SetupDataGridViewColumns();
            tableLayoutPanelMain.Controls.Add(dgvAppointments, 0, 2);
        }

        private void SetupDataGridViewColumns()
        {
            dgvAppointments.Columns.Clear();
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AppointmentId", HeaderText = "Mã lịch hẹn", Width = 100 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PatientName", HeaderText = "Họ tên bệnh nhân", Width = 160 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PhoneNumber", HeaderText = "Số điện thoại", Width = 120 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AppointmentDate", HeaderText = "Ngày giờ hẹn", Width = 140, DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm" } });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Doctor", HeaderText = "Bác sĩ", Width = 130 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Reason", HeaderText = "Lý do khám", Width = 160 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 110 });
        }

        private void InitializeData()
        {
            appointmentList = new BindingList<Appointment>()
            {
                new Appointment { AppointmentId = "LH001", PatientName = "Nguyễn Văn A", PhoneNumber = "0901234567", AppointmentDate = DateTime.Now.AddHours(2), Doctor = "BS. Trần Văn B", Reason = "Đau đầu", Status = "Chờ khám" },
                new Appointment { AppointmentId = "LH002", PatientName = "Trần Thị C", PhoneNumber = "0987654321", AppointmentDate = DateTime.Now.AddDays(1), Doctor = "BS. Lê Thị D", Reason = "Khám tổng quát", Status = "Chờ khám" }
            };

            bindingSource = new BindingSource();
            bindingSource.DataSource = appointmentList;
            dgvAppointments.DataSource = bindingSource;

            UpdateStatusCount();
        }

        private void LoadComboBoxData()
        {
            cbDoctor.Items.AddRange(new string[] { "BS. Trần Văn B", "BS. Lê Thị D", "BS. Hoàng Văn E" });
            if (cbDoctor.Items.Count > 0) cbDoctor.SelectedIndex = 0;

            string[] statuses = new string[] { "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" };
            cbStatus.Items.AddRange(statuses);
            if (cbStatus.Items.Count > 0) cbStatus.SelectedIndex = 0;

            cbFilterStatus.Items.Add("Tất cả trạng thái");
            cbFilterStatus.Items.AddRange(statuses);
            cbFilterStatus.SelectedIndex = 0;
        }

        private bool ValidateInput(bool isNew = true)
        {
            bool isValid = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtAppointmentId.Text))
            {
                errorProvider1.SetError(txtAppointmentId, "Mã lịch hẹn không được để trống!");
                isValid = false;
            }
            else if (isNew && appointmentList.Any(x => x.AppointmentId == txtAppointmentId.Text.Trim()))
            {
                errorProvider1.SetError(txtAppointmentId, "Mã lịch hẹn đã tồn tại!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtPatientName.Text))
            {
                errorProvider1.SetError(txtPatientName, "Tên bệnh nhân không được để trống!");
                isValid = false;
            }

            string phone = txtPhone.Text.Trim();
            if (string.IsNullOrWhiteSpace(phone) || !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9,10}$"))
            {
                errorProvider1.SetError(txtPhone, "Số điện thoại không hợp lệ (phải bắt đầu bằng 0 và có 10-11 chữ số)!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                errorProvider1.SetError(txtReason, "Lý do khám không được để trống!");
                isValid = false;
            }

            if (dtpAppointmentDate.Value < DateTime.Now)
            {
                errorProvider1.SetError(dtpAppointmentDate, "Ngày giờ hẹn không được nằm trong quá khứ!");
                isValid = false;
            }

            return isValid;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(true)) return;

            var newAppt = new Appointment
            {
                AppointmentId = txtAppointmentId.Text.Trim(),
                PatientName = txtPatientName.Text.Trim(),
                PhoneNumber = txtPhone.Text.Trim(),
                AppointmentDate = dtpAppointmentDate.Value,
                Doctor = cbDoctor.Text,
                Reason = txtReason.Text.Trim(),
                Status = cbStatus.Text
            };

            appointmentList.Add(newAppt);
            ClearInputs();
            UpdateStatusCount();
            MessageBox.Show("Thêm lịch hẹn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvAppointments.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn lịch hẹn cần cập nhật trên bảng!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedAppt = dgvAppointments.CurrentRow.DataBoundItem as Appointment;
            if (selectedAppt != null)
            {
                if (!ValidateInput(false)) return;

                selectedAppt.PatientName = txtPatientName.Text.Trim();
                selectedAppt.PhoneNumber = txtPhone.Text.Trim();
                selectedAppt.AppointmentDate = dtpAppointmentDate.Value;
                selectedAppt.Doctor = cbDoctor.Text;
                selectedAppt.Reason = txtReason.Text.Trim();
                selectedAppt.Status = cbStatus.Text;

                bindingSource.ResetBindings(false);
                UpdateStatusCount();
                MessageBox.Show("Cập nhật lịch hẹn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvAppointments.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn lịch hẹn cần xóa trên bảng!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("Bạn có chắc chắn muốn xóa lịch hẹn này không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                var selectedAppt = dgvAppointments.CurrentRow.DataBoundItem as Appointment;
                if (selectedAppt != null)
                {
                    appointmentList.Remove(selectedAppt);
                    ClearInputs();
                    UpdateStatusCount();
                    MessageBox.Show("Đã xóa lịch hẹn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void dgvAppointments_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedAppt = dgvAppointments.Rows[e.RowIndex].DataBoundItem as Appointment;
                if (selectedAppt != null)
                {
                    txtAppointmentId.Text = selectedAppt.AppointmentId;
                    txtAppointmentId.Enabled = false; // Khóa mã không cho sửa khi đang cập nhật
                    txtPatientName.Text = selectedAppt.PatientName;
                    txtPhone.Text = selectedAppt.PhoneNumber;
                    dtpAppointmentDate.Value = selectedAppt.AppointmentDate;
                    cbDoctor.Text = selectedAppt.Doctor;
                    txtReason.Text = selectedAppt.Reason;
                    cbStatus.Text = selectedAppt.Status;
                }
            }
        }

        private void ClearInputs()
        {
            txtAppointmentId.Clear();
            txtAppointmentId.Enabled = true;
            txtPatientName.Clear();
            txtPhone.Clear();
            txtReason.Clear();
            dtpAppointmentDate.Value = DateTime.Now;
            if (cbDoctor.Items.Count > 0) cbDoctor.SelectedIndex = 0;
            if (cbStatus.Items.Count > 0) cbStatus.SelectedIndex = 0;
            errorProvider1.Clear();
            dgvAppointments.ClearSelection();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        private void FilterData()
        {
            string keyword = txtSearch.Text.Trim().ToLower();
            string selectedStatus = cbFilterStatus.SelectedItem?.ToString();

            var filtered = appointmentList.Where(x =>
                (string.IsNullOrEmpty(keyword) || x.PatientName.ToLower().Contains(keyword) || x.PhoneNumber.Contains(keyword)) &&
                (selectedStatus == "Tất cả trạng thái" || x.Status == selectedStatus)
            ).ToList();

            bindingSource.DataSource = new BindingList<Appointment>(filtered);
            UpdateStatusCount();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void cbFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void UpdateStatusCount()
        {
            lblStatusCount.Text = $"Số lịch hẹn hiển thị: {bindingSource.Count}";
        }

        private void menuExportCsv_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV file (*.csv)|*.csv";
                sfd.FileName = "DanhSachLichHen.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("MaLichHen,HoTenBenhNhan,SoDienThoai,NgayGioHen,BacSi,LyDoKham,TrangThai");

                            foreach (var item in bindingSource.List)
                            {
                                if (item is Appointment appt)
                                {
                                    sw.WriteLine($"{appt.AppointmentId},{appt.PatientName},{appt.PhoneNumber},{appt.AppointmentDate:yyyy-MM-dd HH:mm},{appt.Doctor},\"{appt.Reason}\",{appt.Status}");
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