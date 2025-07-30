# Download Videos from Excel and Labeler

## UC

### 📤 Upload (up file)

- Xóa danh sách sheet cũ, xóa danh sách video cũ, xóa  file video và file txt trong thư mục
- Lưu tên file
- Lưu sheet
- Mặc định lấy sheet đầu tiên để làm việc

### 📝 Init download video (lưu thông tin, trạng thái video)

- Chọn sheet -> Init
- Nếu đã Init trước đó -> thì cần gán nhãn -> export excel với tích tùy chọn xóa hết dữ liệu sau khi export -> trạng thái trước đó xóa hết mới được Init

### 📥 Tải video

- Mỗi 1 video được tải sẽ cập nhật trạng thái đã tải.
- Có thể nhập số lượng video muốn tải, mặc định là 500.
- Trong khi tải có thể dừng, bấm tiếp tục tải những video còn lại.
- Video lưu tại thư mục **wwwroot/VideoFolder**.

### 📊 Export excel

- Cập nhật **Label**.
- Export có thêm tùy chọn là xóa hết dữ liệu sau khi export.
- Có thể export mà không cần tải tất cả video.

## ⚙️ Công nghệ sử dụng

### 🧠 Back-end
![.NET 8](https://img.shields.io/badge/.NET-8.0-purple?logo=dotnet&logoColor=white)

### 🎨 Front-end
![Angular](https://img.shields.io/badge/Angular-20-red?logo=angular&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/TailwindCSS-%5E4.1-38bdf8?logo=tailwindcss&logoColor=white)

### 🌐 Hosting
![Hosted on Vercel](https://img.shields.io/badge/Frontend-Vercel-black?logo=vercel&logoColor=white)
![API Local](https://img.shields.io/badge/API-Run%20locally-lightgrey?logo=windows-terminal)

### 🗃️ Database
![SQLite](https://img.shields.io/badge/SQLite-3-blue?logo=sqlite&logoColor=white)

### 🧰 IDE
![JetBrains Rider](https://img.shields.io/badge/IDE-Rider-d03b60?logo=jetbrains&logoColor=white)
![VS Code](https://img.shields.io/badge/Editor-VS%20Code-007ACC?logo=visual-studio-code&logoColor=white)

### 📦 Excel Library
![Aspose.Cells](https://img.shields.io/badge/Aspose.Cells-Library-green?logo=microsoft-excel&logoColor=white)


## 🔄 Luồng hoạt động

- Upload file excel.
- Chọn sheet làm việc.
- Init download để lưu trạng thái video, để Init được thì trạng thái làm việc trước đó phải trống.
- Download video, có thể tạm dừng, chọn số lượng video để tải
- Cập nhật Label cho video, xem video.
- Export file excel, nếu có tùy chọn remove all data thì xóa hết dữ liệu.
- Upload file excel mới thì file cũ sẽ bị xóa, đồng thời các trạng thái làm việc cũng xóa theo.


