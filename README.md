# CEP Customer Management System

Mini Customer Management System được xây dựng bằng .NET 8, ASP.NET Core Web API, Entity Framework Core và Blazor WebAssembly.

---

## Project Overview
Hệ thống Quản lý Khách hàng Mini (CEP Customer Management System) được thiết kế cho việc quản lý thông tin khách hàng đơn giản, tin cậy, chuẩn cấu trúc phân lớp giữa Backend Web API và Frontend Blazor WebAssembly, tuân thủ nguyên tắc Clean Code, không over-engineering và dễ dàng bảo trì.

---

## Features
- **Quản lý khách hàng**:
  - Xem danh sách khách hàng có phân trang (Pagination).
  - Tìm kiếm khách hàng theo Họ tên hoặc Số điện thoại.
  - Thêm mới khách hàng (với kiểm tra hợp lệ dữ liệu cả phía Client và Server).
  - Cập nhật thông tin khách hàng (không cho phép sửa Mã khách hàng sau khi tạo).
  - Xóa khách hàng với hộp thoại xác nhận (Confirmation Dialog).
  - Quản lý trạng thái khách hàng (Active / Inactive).
- **Xác thực & Phân quyền**:
  - JWT Authentication với token-based security.
  - Phân quyền API bằng `[Authorize]`.
  - Quản lý trạng thái đăng nhập qua `CustomAuthenticationStateProvider`.
- **Giao diện hiện đại**:
  - Blazor WebAssembly kết hợp thư viện MudBlazor chuyên nghiệp, trực quan.
  - Thông báo Snackbar, biểu tượng Loading khi thực hiện tác vụ bất đồng bộ.
- **Audit Logging**:
  - Ghi vết lịch sử các thao tác CREATE, UPDATE, DELETE trên hệ thống.

---

## Technology Stack

### Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8 (Code First)
- Microsoft SQL Server
- JWT Authentication (Bearer)
- Swagger / OpenAPI

### Frontend
- Blazor WebAssembly (.NET 8)
- MudBlazor Component Library
- HttpClient & DelegatingHandler
- JWT Authentication State

### Development Tools
- Visual Studio Code
- Integrated Terminal & .NET CLI
- Git & GitHub

---

## Architecture
Hệ thống tuân thủ kiến trúc phân tách rõ ràng:
```
Blazor WebAssembly (Frontend)
       │
       │  HTTP / REST API (JWT Bearer)
       ▼
ASP.NET Core Web API (Backend)
       │
       ▼
  Service Layer
       │
       ▼
Entity Framework Core
       │
       ▼
   SQL Server
```
*Frontend độc lập, chỉ giao tiếp với Backend thông qua REST API, không truy cập trực tiếp cơ sở dữ liệu.*

---

## Folder Structure
```
CEP
├── CEP.sln
├── README.md
├── .gitignore
│
├── Backend
│   ├── CEP.Backend.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   └── CustomersController.cs
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── DbInitializer.cs
│   ├── Models/
│   │   ├── Customer.cs
│   │   ├── User.cs
│   │   └── AuditLog.cs
│   ├── DTOs/
│   │   ├── Customers/
│   │   └── Auth/
│   ├── Interfaces/
│   ├── Services/
│   ├── Helpers/
│   ├── Middleware/
│   ├── Common/
│   └── Migrations/
│
└── FrontEnd
    ├── CEP.FrontEnd.csproj
    ├── Program.cs
    ├── App.razor
    ├── _Imports.razor
    ├── Pages/
    ├── Components/
    ├── Services/
    ├── Models/
    ├── Auth/
    ├── Layout/
    ├── Shared/
    └── wwwroot/
```

---

## Prerequisites
- .NET 8 SDK
- Visual Studio Code
- SQL Server
- Git
- dotnet-ef CLI Tool

---

## Hướng dẫn chạy dự án

### 1. Chạy Backend
Mở Terminal:
```bash
cd Backend
dotnet restore
dotnet ef database update
dotnet run --launch-profile http
```
- **Backend API:** `http://localhost:5106`
- **Swagger UI:** `http://localhost:5106/swagger`

---

### 2. Chạy Frontend
Mở một cửa sổ Terminal mới:
```bash
cd FrontEnd
dotnet run --launch-profile http
```
- **Giao diện Web:** `http://localhost:5158`

> **Lưu ý:** Lệnh trên dùng `--launch-profile http` để chạy cố định trên cặp port **5106** (Backend) và **5158** (Frontend) đã khớp sẵn cấu hình CORS và API, giúp copy-paste chạy ngay mà không gặp lỗi HTTPS hay mixed-content.

---

## Migration (Tùy chọn khi phát triển)
Tạo migration mới khi có sự thay đổi Entity:
```bash
cd Backend
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

---

## JWT Login
- Khi đăng nhập thành công, token JWT sẽ được lưu tại trình duyệt.
- Các request gửi tới Backend sẽ tự động đính kèm `Authorization: Bearer <token>`.
- Khi token hết hạn hoặc không hợp lệ, hệ thống sẽ tự động đăng xuất và chuyển hướng về trang `/login`.

---

## Demo Account
Hệ thống được seed sẵn tài khoản quản trị mặc định:
- **Username:** `admin`
- **Password:** `Admin@123`
*(Mật khẩu được lưu trữ an toàn dưới dạng Hash bằng PBKDF2-SHA256 (100.000 iterations & Salt ngẫu nhiên), không lưu Plain Text).*

---

## API Endpoints

### Authentication
- `POST /api/auth/login`: Đăng nhập lấy Bearer token.

### Customers
- `GET /api/customers?search=...&page=1&pageSize=10`: Lấy danh sách khách hàng có phân trang & tìm kiếm.
- `GET /api/customers/{id}`: Xem chi tiết khách hàng theo ID.
- `POST /api/customers`: Thêm mới khách hàng.
- `PUT /api/customers/{id}`: Cập nhật thông tin khách hàng.
- `DELETE /api/customers/{id}`: Xóa khách hàng.

---

## Git
Hệ thống sử dụng Git để quản lý phiên bản với một repository duy nhất tại thư mục gốc `CEP/`:
```bash
git status
git add .
git commit -m "<Commit Message>"
```

---

## GitHub Repository
Để kết nối với GitHub remote:
```bash
git remote add origin <GITHUB_REPOSITORY_URL>
git branch -M main
git push -u origin main
```

---

## Project Resources / Tài liệu đính kèm
- **Analysis & Planning:** https://drive.google.com/drive/folders/15eLty4_XojJxqrV30YWbrZFf8PrBYzdp?usp=sharing
- **Demo Video:** https://drive.google.com/drive/folders/11Gwsxh7Ha8SQTSjWLgGhKAObo_-0fSks?usp=sharing
- **Postman Collection:** https://drive.google.com/drive/folders/1sxfBzVDvaH3zTTKaLqyl3ZgeI8HoESHo?usp=sharing
