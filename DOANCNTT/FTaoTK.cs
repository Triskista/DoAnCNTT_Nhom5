using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DOANCNTT
{
    public partial class FTaoTK : Form
    {
        KetNoi db = new KetNoi();
        public FTaoTK()
        {
            InitializeComponent();
        }

        private void buttExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Login F = new Login();
            this.Hide();
            F.ShowDialog();
            this.Show();
        }

        private void btnTaoTk_Click(object sender, EventArgs e)
        {
            try
            {
                // Mở kết nối cơ sở dữ liệu
                db.openConnection();

                // Kiểm tra thông tin nhập
                if (string.IsNullOrWhiteSpace(txtHoten.Text) ||
                    string.IsNullOrWhiteSpace(txtMK.Text) ||
                    string.IsNullOrWhiteSpace(txtSDT.Text) ||
                    string.IsNullOrWhiteSpace(txtDN.Text))
                {
                    MessageBox.Show("Vui lòng nhập đủ thông tin", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txtSDT.Text.Length != 10 || !txtSDT.Text.All(char.IsDigit))
                {
                    MessageBox.Show("Số điện thoại không hợp lệ. Số điện thoại phải gồm 10 chữ số.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra tên đăng nhập đã tồn tại
                SqlCommand checkUsernameCmd = new SqlCommand("SELECT 1 FROM TaiKhoan WHERE TenDangNhap = @TenDangNhap", db.getConnection);
                checkUsernameCmd.Parameters.AddWithValue("@TenDangNhap", txtDN.Text);
                object usernameExists = checkUsernameCmd.ExecuteScalar();

                if (usernameExists != null)
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại. Vui lòng chọn tên đăng nhập khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Thực hiện chèn thông tin tài khoản mới
                SqlCommand insertAccountCmd = new SqlCommand(
                    "INSERT INTO TaiKhoan (TenDangNhap, HoTen, MatKhau, SDT) VALUES (@TenDangNhap, @HoTen, @MatKhau, @SDT)",
                    db.getConnection
                );
                insertAccountCmd.Parameters.AddWithValue("@TenDangNhap", txtDN.Text.Trim());
                insertAccountCmd.Parameters.AddWithValue("@HoTen", txtHoten.Text.Trim());
                insertAccountCmd.Parameters.AddWithValue("@MatKhau", txtMK.Text.Trim());
                insertAccountCmd.Parameters.AddWithValue("@SDT", txtSDT.Text.Trim());

                int rowsAffected = insertAccountCmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Tạo tài khoản thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Xóa các trường nhập liệu sau khi tạo tài khoản
                    txtDN.Clear();
                    txtHoten.Clear();
                    txtMK.Clear();
                    txtSDT.Clear();
                }
                else
                {
                    MessageBox.Show("Tạo tài khoản thất bại. Vui lòng thử lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
