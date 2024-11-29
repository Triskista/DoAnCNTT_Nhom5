using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Win32;
using System.Device.Location;
using Newtonsoft.Json.Linq;

namespace DOANCNTT
{
    public partial class Form1 : Form
    {
        private const string ApiKey = "AIzaSyA66KwUrjxcFG5u0exynlJ45CrbrNe3hEc";
        List<string> des = new List<string>();
        int i = 0;
        public Form1()
        {
            InitializeComponent();
        }

        // Tạo URL của Google Maps
        private string CreateGoogleMapsUrl(double latitude, double longitude)
        {
            string baseUrl = "https://www.google.com/maps/embed/v1/view";
            return $"{baseUrl}?key={ApiKey}&center={latitude},{longitude}&zoom=14";
        }

        private string CreateGoogleMapsUrl(List<string> orderedLocations)
        {
            string baseUrl = "https://www.google.com/maps/embed/v1/directions";
            if (orderedLocations.Count < 2)
            {
                MessageBox.Show("Cần ít nhất 2 điểm để tạo lộ trình.");
                return string.Empty;
            }

            if (orderedLocations.Count == 2)
            {
                return $"{baseUrl}?key={ApiKey}&origin={Uri.EscapeDataString(orderedLocations.First())}&destination={Uri.EscapeDataString(orderedLocations.Last())}";
            }
            else
            {
                string waypointsParam = string.Join("|", orderedLocations.Skip(1).Take(orderedLocations.Count - 2).Select(Uri.EscapeDataString));
                return $"{baseUrl}?key={ApiKey}&origin={Uri.EscapeDataString(orderedLocations.First())}&destination={Uri.EscapeDataString(orderedLocations.Last())}&waypoints={waypointsParam}";
            }
        }


        private async Task<int> GetDistanceAsync(string origin, string destination)
        {
            string apiKey = "jLx7Vu0hnvHV8PnxYNcoA6BvLKqZKsoRqegzBSUk5KpNNK0BbmrTbt6amQPSbBcX";
            string url = $"https://api.distancematrix.ai/maps/api/distancematrix/json?origins={Uri.EscapeDataString(origin)}&destinations={Uri.EscapeDataString(destination)}&key={apiKey}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Phân tích dữ liệu JSON
                    JObject data = JObject.Parse(responseBody);

                    if ((string)data["status"] == "OK")
                    {
                        var rows = data["rows"] as JArray;
                        if (rows != null && rows.Count > 0)
                        {
                            var elements = rows[0]["elements"] as JArray;
                            if (elements != null && elements.Count > 0)
                            {
                                var distanceField = elements[0]["distance"]?["value"];
                                if (distanceField != null)
                                {
                                    return int.Parse((string)distanceField); // Khoảng cách tính bằng mét
                                }
                            }
                        }
                        MessageBox.Show("Không thể lấy khoảng cách từ dữ liệu JSON.");
                    }
                    else
                    {
                        MessageBox.Show("Có lỗi xảy ra: " + (string)data["status"]);
                    }
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show("Yêu cầu thất bại: " + e.Message);
                }
                catch (JsonException e)
                {
                    MessageBox.Show("Lỗi phân tích dữ liệu JSON: " + e.Message);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Lỗi không xác định: " + e.Message);
                }
            }

            return int.MaxValue; // Trả về một số lớn nếu không lấy được khoảng cách
        }


        private void GetLocation()
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
            watcher.PositionChanged += (sender, e) =>
            {
                if (e.Position.Location.IsUnknown != true) // Kiểm tra xem vị trí có hợp lệ không
                {

                    var latitude = e.Position.Location.Latitude;
                    var longitude = e.Position.Location.Longitude;

                    string url = CreateGoogleMapsUrl(latitude, longitude);
                    string htmlContent = GenerateHtmlContent(url);
                    webViewMap.NavigateToString(htmlContent);
                    watcher.Stop(); // Dừng theo dõi sau khi lấy vị trí
                }
            };

            // Thêm sự kiện khi không thể lấy vị trí
            watcher.StatusChanged += (sender, e) =>
            {
                if (e.Status == GeoPositionStatus.Disabled)
                {
                    MessageBox.Show("Không thể lấy vị trí. Hãy kiểm tra quyền truy cập vị trí.");
                    watcher.Stop();
                }
            };

            watcher.Start();
        }


        // Tạo nội dung HTML chứa thẻ iframe để nhúng bản đồ
        private string GenerateHtmlContent(string mapUrl)
        {
            return $@"
                <html>
                    <head>
                        <meta charset='UTF-8'>
                        <style>body {{ margin: 0; padding: 0; }}</style>
                    </head>
                    <body>
                        <iframe width='100%' height='100%' frameborder='0' style='border:0'
                                src='{mapUrl}' allowfullscreen>
                        </iframe>
                    </body>
                </html>";
        }


        private async Task<int[,]> BuildDistanceMatrixAsync(List<string> locations)
        {
            int n = locations.Count;
            int[,] distanceMatrix = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        distanceMatrix[i, j] = 0;
                    }
                    else
                    {
                        distanceMatrix[i, j] = await GetDistanceAsync(locations[i], locations[j]);
                    }
                }
            }
            return distanceMatrix;
        }

        private List<int> FindShortestPath(int[,] distanceMatrix)
        {
            int n = distanceMatrix.GetLength(0);
            var path = new List<int>();
            bool[] visited = new bool[n];
            int current = 0;
            path.Add(current);
            visited[current] = true;

            for (int i = 1; i < n; i++)
            {
                int next = -1;
                int shortestDistance = int.MaxValue;

                for (int j = 0; j < n; j++)
                {
                    if (!visited[j] && distanceMatrix[current, j] < shortestDistance)
                    {
                        shortestDistance = distanceMatrix[current, j];
                        next = j;
                    }
                }

                if (next != -1)
                {
                    path.Add(next);
                    visited[next] = true;
                    current = next;
                }
            }

            return path;
        }

        private async void btnSubmit_Click(object sender, EventArgs e)
        {
            des.Clear();
            foreach (Control control in panel1.Controls)
            {
                if (control is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    des.Add(textBox.Text.Trim());
                }
            }

            if (des.Count < 2)
            {
                MessageBox.Show("Vui lòng nhập ít nhất hai điểm để tạo lộ trình.");
                return;
            }

            int[,] distanceMatrix = await BuildDistanceMatrixAsync(des);
            List<int> path = FindShortestPath(distanceMatrix);

            var orderedLocations = path.Select(index => des[index]).ToList();

            // Cập nhật lại nội dung của các TextBox theo thứ tự đã sắp xếp
            UpdateTextBoxesContent(orderedLocations);

            string url = CreateGoogleMapsUrl(orderedLocations);

            if (!string.IsNullOrEmpty(url))
            {
                string htmlContent = GenerateHtmlContent(url);
                webViewMap.NavigateToString(htmlContent);
            }
        }

        // Phương thức cập nhật nội dung của các TextBox theo thứ tự đã sắp xếp
        private void UpdateTextBoxesContent(List<string> orderedLocations)
        {
            int i = 0;
            foreach (Control control in panel1.Controls)
            {
                if (control is TextBox textBox)
                {
                    if (i < orderedLocations.Count)
                    {
                        textBox.Text = orderedLocations[i];
                        i++;
                    }
                }
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await webViewMap.EnsureCoreWebView2Async();  // Khởi tạo WebView2
            GetLocation();
        }



        private void btnthem_Click(object sender, EventArgs e)
        {
            if (i == 0)
            {
                TextBox newTextBox = new TextBox();
                newTextBox.Width = txtDestination.Width;
                newTextBox.Height = txtDestination.Height;

                // Tính toán vị trí Y cho TextBox mới
                int newYPosition = txtDestination.Bottom + (newTextBox.Height + 5) * i + 3; // Đặt vị trí Y cách nhau 5 pixel giữa các TextBox
                newTextBox.Location = new Point(txtDestination.Left, newYPosition);

                // Thêm tên cho TextBox mới
                newTextBox.Name = "txtDestination" + i; // Gán tên mới cho TextBox
                // Thêm TextBox mới vào Panel
                panel1.Controls.Add(newTextBox);
            }
            if (i < 9 && i > 0)
            {
                TextBox newTextBox = new TextBox();
                newTextBox.Width = txtDestination.Width;
                newTextBox.Height = txtDestination.Height;

                // Tính toán vị trí Y cho TextBox mới
                int newYPosition = txtDestination.Bottom + (newTextBox.Height + 5) * i; // Đặt vị trí Y cách nhau 5 pixel giữa các TextBox
                newTextBox.Location = new Point(txtDestination.Left, newYPosition);

                // Thêm tên cho TextBox mới
                newTextBox.Name = "txtDestination" + i; // Gán tên mới cho TextBox
                // Thêm TextBox mới vào Panel
                panel1.Controls.Add(newTextBox);


            }
            i++; // Tăng i lên 1 để tạo TextBox tiếp theo

        }

        private void buttExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Tìm TextBox mới nhất được thêm vào panel1
            TextBox lastAddedTextBox = null;

            foreach (Control control in panel1.Controls)
            {
                if (control is TextBox textBox && textBox.Name != "txtDestination" && textBox.Name != "txtOrigin")
                {
                    lastAddedTextBox = textBox; // Lấy TextBox cuối cùng
                }
            }

            // Nếu tìm thấy TextBox, tiến hành xóa
            if (lastAddedTextBox != null)
            {
                panel1.Controls.Remove(lastAddedTextBox); // Loại bỏ khỏi giao diện
                lastAddedTextBox.Dispose(); // Giải phóng tài nguyên
                i--; // Giảm chỉ số i để thêm TextBox tiếp theo đúng vị trí
            }
            else
            {
                MessageBox.Show("Không có TextBox nào để xóa!");
            }
        }
    }
}
