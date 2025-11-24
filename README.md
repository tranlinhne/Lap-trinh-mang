# Chat LAN TCP

Dự án **Chat LAN TCP** là ứng dụng chat nội bộ trên mạng LAN, gồm **Server** và **Client**, sử dụng **TCP socket** và **AES encryption** để bảo mật tin nhắn.

---

## 🔹 Tính năng chính

* Kết nối nhiều client đồng thời đến server.
* Gửi và nhận tin nhắn chat realtime.
* Hiển thị thông báo khi có người tham gia hoặc rời phòng chat.
* Danh sách người dùng online được cập nhật liên tục.
* Tin nhắn và danh sách user được **mã hóa AES** trước khi truyền, đảm bảo bảo mật.

---

## 🔹 Cấu trúc dự án

```
ChatServer/           -> Server TCP
    Program.cs         -> Xử lý kết nối, broadcast, mã hóa AES

ChatClient/           -> Client Windows Form
    Form1.cs           -> Giao diện, kết nối server, gửi/nhận tin nhắn
    AesEncryption.cs   -> Mã hóa/giải mã AES
```

---

## 🔹 Hướng dẫn sử dụng

### 1. Chạy Server

1. Mở `ChatServer` trên Visual Studio.
2. Build và chạy.
3. Server lắng nghe trên **cổng 9001** (hoặc IP tuỳ chọn).
4. Console server sẽ hiển thị log khi có client kết nối, gửi tin nhắn, hoặc rời phòng.

### 2. Chạy Client

1. Mở `ChatClient` trên Visual Studio.
2. Nhập:

   * IP server (ví dụ `192.168.1.10`)
   * Username của bạn
3. Nhấn **Connect** → client sẽ kết nối và nhận danh sách người online.
4. Gõ tin nhắn và nhấn **Enter** hoặc **Send** để gửi.

---

## 🔹 Cơ chế bảo mật

* Tất cả **tin nhắn chat** và **danh sách người dùng** được **AES mã hóa** trước khi gửi.
* Server giải mã dữ liệu từ client, xử lý và broadcast tiếp.
* Client nhận dữ liệu → giải mã AES → hiển thị.

---

## 🔹 Cơ chế thông báo

* Khi có client mới tham gia:

  * Server gửi tin nhắn `"username đã tham gia phòng chat"` cho các client khác.
  * Server gửi danh sách user mới cập nhật để client hiển thị.
* Khi client rời phòng:

  * Server gửi `"username đã rời phòng chat"` cho các client còn lại.
  * Server cập nhật danh sách user.

---

## 🔹 Yêu cầu

* .NET Framework 4.7.2 trở lên (hoặc .NET 6+ nếu sử dụng project mới).
* Chạy trên mạng LAN (hoặc localhost).
* Windows OS (vì client là Windows Form, server console).

---

## 🔹 Ghi chú

* Server có thể chạy trên một máy, client có thể chạy trên nhiều máy trong cùng mạng LAN.
* Danh sách client và tin nhắn được quản lý bằng `Dictionary<TcpClient, string>` trên server.
* Mỗi client được xử lý trong **task riêng** để hỗ trợ đa kết nối.

---

## 🔹 License

Miễn phí sử dụng và chỉnh sửa cho mục đích học tập.

---

> Project này phù hợp để trình bày trong lớp, giải thích về **TCP socket**, **bảo mật AES**, và **cơ chế broadcast tin nhắn cho nhiều client**.
