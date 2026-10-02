Câu 1:
Sự khác nhau giữa Value Types và Reference Types (Stack vs Heap)
1. Vùng nhớ lưu trữ và dữ liệu:
Value Types: lưu giá trị thực sự trực tiếp trên Stack. Ngoại lệ: khi là trường/thuộc tính nằm trong một Class thì nằm trên Heap cùng Class đó (tương tự với phần tử của mảng, hoặc biến bị capture trong lambda/closure).
Reference Types: biến tham chiếu (chứa địa chỉ) nằm trên Stack, còn đối tượng thực sự nằm trên Heap.

2. Cơ chế gán biến:
Value Types: Copy by Value, tạo bản sao độc lập, đổi biến này không ảnh hưởng biến kia.
Reference Types: Copy by Reference, hai biến cùng trỏ một đối tượng trên Heap, đổi qua biến này thì biến kia đổi theo.

3. Quản lý bộ nhớ:
Stack: tự động giải phóng khi ra khỏi scope (pop stack frame), truy xuất rất nhanh.
Heap: do Garbage Collector dọn khi không còn tham chiếu nào trỏ tới.

4. Khả năng nhận null: Value Type mặc định không nhận null (dùng Nullable như int?); Reference Type có thể là null.

5. Khái niệm bổ sung:
Boxing: chuyển Value Type sang object (Reference Type), dữ liệu được cấp phát trên Heap. Unboxing: chuyển ngược lại. Cả hai đều tốn chi phí hiệu năng.
Tính immutable của string: string là Reference Type nhưng bất biến, mọi thao tác chỉnh sửa đều tạo một đối tượng string mới trên Heap nên khi gán/sửa trông giống Value Type.

Câu 2:
So sánh Init-only Properties (init) với set thông thường

1. Sự khác biệt:
set thông thường: gán và sửa giá trị ở bất kỳ thời điểm nào trong vòng đời đối tượng.
init (có từ C# 9): chỉ gán được trong quá trình khởi tạo, gồm Constructor, Object Initializer, biểu thức with (dùng với record) hoặc trong init accessor khác. Sau khi khởi tạo xong, thuộc tính trở thành chỉ đọc, sửa sẽ báo lỗi biên dịch.

2. Trường hợp sử dụng thực tế:
Thiết kế đối tượng bất biến (Immutable Objects): DTO, Value Object, các thông tin cố định như khóa chính (Id), mã số thuế, số CMND/CCCD, thông tin hóa đơn.
Cú pháp Object Initializer ngắn gọn, không phải viết Constructor nhiều tham số.
Dùng với record và with để tạo bản sao có chỉnh sửa mà không đổi bản gốc.
An toàn hơn khi chạy đa luồng vì dữ liệu không đổi sau khi tạo.

Câu 3:
Phân biệt virtual (lớp cha) và override (lớp con) trong Đa hình

1. Khái niệm và vai trò:
virtual (lớp cha): khai báo phương thức có hành vi mặc định và cho phép lớp con ghi đè.
override (lớp con): ghi đè, thay thế logic của phương thức virtual bằng logic phù hợp hơn. Phải cùng chữ ký (tên, tham số, kiểu trả về) với phương thức virtual. Lớp con không bắt buộc override, và có thể gọi lại bản cha bằng base.

2. Cơ chế hoạt động (vtable và Late Binding):
Khi nạp type lúc runtime, CLR dựng bảng hàm ảo vtable lưu địa chỉ các phương thức.
Khi chạy, cơ chế Late Binding (Dynamic Binding) tra vtable để gọi đúng phương thức override của lớp con theo kiểu thực tế của đối tượng trên Heap, dù biến được khai báo kiểu lớp cha.

3. Phân biệt thêm:
new (method hiding): che giấu phương thức cha, được quyết định lúc compile time theo kiểu của biến, không có đa hình.
abstract: không có thân phương thức, lớp con bắt buộc phải override.

câu 4:
Vì sao thành phần static không truy xuất qua instance tạo bằng new?

1. Bản chất cấp phát bộ nhớ:
Thành phần static: thuộc mức Lớp (Class level), chỉ có một bản duy nhất dùng chung, được khởi tạo khi type được nạp hoặc truy cập lần đầu, tồn tại suốt vòng đời ứng dụng.
Instance: tạo trên Heap bằng new, thuộc mức Đối tượng (Instance level), chứa dữ liệu riêng của từng đối tượng.

2. Không có con trỏ this: phương thức static không gắn với đối tượng cụ thể nào nên không có this, vì vậy không tự truy cập được thành viên instance.

3. Quy tắc thiết kế C#:
Tránh mơ hồ ngữ nghĩa: static là tài nguyên dùng chung, không thuộc riêng instance nào.
C# bắt buộc gọi qua tên lớp (ClassName.StaticMember) để thể hiện rõ điều đó. Truy cập qua instance sẽ báo lỗi biên dịch CS0176.

4. Chiều ngược lại: phương thức static muốn dùng thành viên instance thì phải có một đối tượng cụ thể được truyền vào hoặc tự tạo ra.
      
