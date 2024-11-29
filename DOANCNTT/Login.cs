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
    public partial class Login : Form
    {
        KetNoi db = new KetNoi();

        public Login()
        {
            InitializeComponent();
        }

        private void buttExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void buttLogin_Click(object sender, EventArgs e)
        {
            try
            {
                // Mở kết nối
                db.openConnection();

                // Lấy thông tin từ TextBox
                string username = txbUser.Text.Trim();
                string password = txbPassWord.Text.Trim();

                // Kiểm tra tên đăng nhập và mật khẩu có bị bỏ trống không
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show(
                        "Tên đăng nhập và mật khẩu không được để trống.",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Câu lệnh SQL kiểm tra tài khoản
                string sql = "SELECT COUNT(*) FROM TaiKhoan WHERE TenDangNhap = @TenDangNhap AND MatKhau = @MatKhau";
                SqlCommand cmd = new SqlCommand(sql, db.getConnection);

                // Thêm tham số
                cmd.Parameters.AddWithValue("@TenDangNhap", username);
                cmd.Parameters.AddWithValue("@MatKhau", password);

                // Thực thi truy vấn và lấy kết quả
                int userExists = (int)cmd.ExecuteScalar();

                if (userExists > 0)
                {
                    // Đăng nhập thành công, chuyển thẳng vào Form1
                    Form1 mainForm = new Form1();

                    // Ẩn form hiện tại và mở form mới
                    this.Hide();
                    mainForm.ShowDialog();

                    // Đóng form hiện tại sau khi Form1 kết thúc
                    this.Close();
                }
                else
                {
                    // Thông báo khi sai tài khoản hoặc mật khẩu
                    MessageBox.Show(
                        "Tên đăng nhập hoặc mật khẩu chưa chính xác. Vui lòng thử lại!",
                        "Thông báo!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi kết nối hoặc lỗi khác
                MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTTK_Click(object sender, EventArgs e)
        {
            FTaoTK F = new FTaoTK();
            this.Hide();
            F.ShowDialog();
            this.Show();
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }
    }
}
