using Microsoft.Data.SqlClient;
using System;
using System.IO;

namespace StudentManagementSystem
{
    internal class Program
    {
        private static string ConnectionString = GetConnectionString();

        static void Main(string[] args)
        {
            while (true)
            {
                DisplayMenu();

                string choice = Console.ReadLine() ?? "";

                try
                {
                    switch (choice)
                    {
                        case "1":
                            DisplayAllStudents();
                            break;

                        case "2":
                            SearchStudent();
                            break;

                        case "3":
                            RegisterStudent();
                            break;

                        case "4":
                            EnrolStudent();
                            break;

                        case "5":
                            CaptureOrUpdateMark();
                            break;

                        case "6":
                            ViewStudentResults();
                            break;

                        case "7":
                            ViewStudentsWithoutEnrolments();
                            break;

                        case "8":
                            RecordPayment();
                            break;

                        case "9":
                            Console.WriteLine("\nThank you for using the Student Management System.");
                            return;

                        default:
                            Console.WriteLine("\nInvalid choose 1-9.");
                            break;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("\nDatabase error:");
                    Console.WriteLine(GetFriendlySqlError(ex));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nUnexpected error:");
                    Console.WriteLine(ex.Message);
                }

                Console.WriteLine("\nPress ENTER to continue...");
                Console.ReadLine();
            }
        }
        static void DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine("   STUDENT MANAGEMENT SYSTEM");
            Console.WriteLine("1. Display all students");
            Console.WriteLine("2. Search for a student");
            Console.WriteLine("3. Register a student");
            Console.WriteLine("4. Enrol a student");
            Console.WriteLine("5. Capture or update a mark");
            Console.WriteLine("6. View student results");
            Console.WriteLine("7. View students without enrolments");
            Console.WriteLine("8. Record a payment");
            Console.WriteLine("9. Exit");
            Console.Write("Select an option: ");
        }
        static void DisplayAllStudents()
        {
            string sql = @"
                SELECT 
                    StudentID,
                    StudentNumber,
                    FulllName,
                    Email,
                    Status
                FROM Students
                ORDER BY StudentID;";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine(" ALL STUDENTS");

                    bool found = false;

                    while (reader.Read())
                    {
                        found = true;

                        Console.WriteLine(
                            $"ID: {reader["StudentID"]} | " +
                            $"Number: {reader["StudentNumber"]} | " +
                            $"Name: {reader["FulllName"]} | " +
                            $"Email: {GetNullableString(reader["Email"])} | " +
                            $"Status: {GetNullableString(reader["Status"])}"
                        );
                    }

                    if (!found)
                    {
                        Console.WriteLine("No students found.");
                    }
                }
            }
        }
        static void SearchStudent()
        {
            string studentNumber = ReadRequiredString(
                "\nEnter student number: ");

            string sql = @"
                SELECT
                    StudentID,
                    StudentNumber,
                    FulllName,
                    Email,
                    Status
                FROM Students
                WHERE StudentNumber = @StudentNumber;";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@StudentNumber", System.Data.SqlDbType.VarChar, 20)
                    .Value = studentNumber;

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine("\nSTUDENT FOUND");

                        Console.WriteLine($"Student ID: {reader["StudentID"]}");
                        Console.WriteLine($"Student Number: {reader["StudentNumber"]}");
                        Console.WriteLine($"Name: {reader["FulllName"]}");
                        Console.WriteLine($"Email: {GetNullableString(reader["Email"])}");
                        Console.WriteLine($"Status: {GetNullableString(reader["Status"])}");
                    }
                    else
                    {
                        Console.WriteLine("\nNo student was found with that student number.");
                    }
                }
            }
        }
        static void RegisterStudent()
        {
            Console.WriteLine("\n REGISTER STUDENT ");

            string studentNumber = ReadRequiredString("Student number: ");
            string fullName = ReadRequiredString("Full name: ");
            string email = ReadRequiredString("Email: ");

            string sql = @"
                INSERT INTO Students
                (
                    StudentNumber,
                    FulllName,
                    Email,
                    Status
                )
                VALUES
                (
                    @StudentNumber,
                    @FullName,
                    @Email,
                    'Yes'
                );";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@StudentNumber", System.Data.SqlDbType.VarChar, 20)
                    .Value = studentNumber;

                command.Parameters.Add("@FullName", System.Data.SqlDbType.VarChar, 50)
                    .Value = fullName;

                command.Parameters.Add("@Email", System.Data.SqlDbType.VarChar, 100)
                    .Value = email;

                connection.Open();

                int rows = command.ExecuteNonQuery();

                if (rows > 0)
                {
                    Console.WriteLine("\nStudent registered successfully.");
                }
            }
        }
        static void EnrolStudent()
        {
            Console.WriteLine("\n ENROL STUDENT ");

            int studentId = ReadPositiveInt("Student ID: ");
            int courseId = ReadPositiveInt("Course ID: ");

            string sql = @"
                INSERT INTO Enrollment
                (
                    StudentID,
                    CourseID,
                    EnrollmentDate,
                    FinalMark
                )
                VALUES
                (
                    @StudentID,
                    @CourseID,
                    @EnrollmentDate,
                    0
                );

                UPDATE Students
                SET Status = 'Yes'
                WHERE StudentID = @StudentID;";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@StudentID", System.Data.SqlDbType.Int)
                    .Value = studentId;

                command.Parameters.Add("@CourseID", System.Data.SqlDbType.Int)
                    .Value = courseId;

                command.Parameters.Add("@EnrollmentDate", System.Data.SqlDbType.DateTime)
                    .Value = DateTime.Now;

                connection.Open();

                int rows = command.ExecuteNonQuery();

                if (rows > 0)
                {
                    Console.WriteLine("\nStudent enrolled successfully.");
                }
            }
        }
        static void CaptureOrUpdateMark()
        {
            Console.WriteLine("\n CAPTURE / UPDATE MARK ");

            int studentId = ReadPositiveInt("Student ID: ");
            int courseId = ReadPositiveInt("Course ID: ");
            int mark = ReadMark("Final mark (0-100): ");

            string sql = @"
                UPDATE Enrollment
                SET FinalMark = @FinalMark
                WHERE StudentID = @StudentID
                  AND CourseID = @CourseID;";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@FinalMark", System.Data.SqlDbType.Int)
                    .Value = mark;

                command.Parameters.Add("@StudentID", System.Data.SqlDbType.Int)
                    .Value = studentId;

                command.Parameters.Add("@CourseID", System.Data.SqlDbType.Int)
                    .Value = courseId;

                connection.Open();

                int rows = command.ExecuteNonQuery();

                if (rows > 0)
                {
                    Console.WriteLine("\nMark updated successfully.");
                    Console.WriteLine("The mark change has been recorded by the audit trigger.");
                }
                else
                {
                    Console.WriteLine("\nNo enrolment was found for that student and course.");
                }
            }
        }

        static void ViewStudentResults()
        {
            Console.WriteLine("\n STUDENT RESULTS ");

            int studentId = ReadPositiveInt("Student ID: ");

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(
                "dbo.usp_GetStudentResults", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("@StudentID", System.Data.SqlDbType.Int)
                    .Value = studentId;

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    bool found = false;

                    while (reader.Read())
                    {
                        found = true;

                        Console.WriteLine(
                            $"Student: {GetNullableString(reader["FullName"])} | " +
                            $"Course: {GetNullableString(reader["CourseCode"])} - " +
                            $"{GetNullableString(reader["CourseName"])} | " +
                            $"Mark: {reader["FinalMark"]} | " +
                            $"Result: {GetNullableString(reader["Result"])}"
                        );
                    }

                    if (!found)
                    {
                        Console.WriteLine(
                            "\nNo results found for this student."
                        );
                    }
                }
            }
        }
        static void ViewStudentsWithoutEnrolments()
        {
            string sql = @"
                SELECT
                    s.StudentID,
                    s.StudentNumber,
                    s.FulllName,
                    s.Email
                FROM Students AS s
                LEFT JOIN Enrollment AS e
                    ON s.StudentID = e.StudentID
                WHERE e.StudentID IS NULL
                ORDER BY s.StudentID;";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine(
                        "\n STUDENTS WITHOUT ENROLMENTS "
                    );

                    bool found = false;

                    while (reader.Read())
                    {
                        found = true;

                        Console.WriteLine(
                            $"ID: {reader["StudentID"]} | " +
                            $"Number: {reader["StudentNumber"]} | " +
                            $"Name: {reader["FulllName"]} | " +
                            $"Email: {GetNullableString(reader["Email"])}"
                        );
                    }

                    if (!found)
                    {
                        Console.WriteLine(
                            "All students currently have enrolments."
                        );
                    }
                }
            }
        }
        static void RecordPayment()
        {
            Console.WriteLine("\n RECORD PAYMENT ");

            int studentId = ReadPositiveInt("Student ID: ");

            decimal amount = ReadPositiveDecimal(
                "Payment amount: ");

            DateTime paymentDate = ReadDate(
                "Payment date (yyyy-MM-dd): ");

            int referenceNumber = ReadPositiveInt(
                "Reference number: ");

            string sql = @"
                INSERT INTO Payment
                (
                    StudentID,
                    PaymentDate,
                    RefrenceNumber,
                    Amount
                )
                VALUES
                (
                    @StudentID,
                    @PaymentDate,
                    @ReferenceNumber,
                    @Amount
                );";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@StudentID", System.Data.SqlDbType.Int)
                    .Value = studentId;

                command.Parameters.Add("@PaymentDate", System.Data.SqlDbType.DateTime)
                    .Value = paymentDate;

                command.Parameters.Add("@ReferenceNumber", System.Data.SqlDbType.Int)
                    .Value = referenceNumber;

                SqlParameter amountParameter =
                    command.Parameters.Add(
                        "@Amount",
                        System.Data.SqlDbType.Decimal);

                amountParameter.Precision = 10;
                amountParameter.Scale = 2;
                amountParameter.Value = amount;

                connection.Open();

                int rows = command.ExecuteNonQuery();

                if (rows > 0)
                {
                    Console.WriteLine(
                        "\nPayment recorded successfully."
                    );
                }
            }
        }

        static string ReadRequiredString(string message)
        {
            while (true)
            {
                Console.Write(message);

                string value = Console.ReadLine() ?? "";

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }

                Console.WriteLine(
                    "This field is required. Try again."
                );
            }
        }

        static int ReadPositiveInt(string message)
        {
            while (true)
            {
                Console.Write(message);

                string input = Console.ReadLine() ?? "";

                if (int.TryParse(input, out int value) && value > 0)
                {
                    return value;
                }

                Console.WriteLine(
                    "Enter a valid positive whole number."
                );
            }
        }

        static int ReadMark(string message)
        {
            while (true)
            {
                Console.Write(message);

                string input = Console.ReadLine() ?? "";

                if (int.TryParse(input, out int mark)
                    && mark >= 0
                    && mark <= 100)
                {
                    return mark;
                }

                Console.WriteLine(
                    "Mark must be a number between 0 and 100."
                );
            }
        }

        static decimal ReadPositiveDecimal(string message)
        {
            while (true)
            {
                Console.Write(message);

                string input = Console.ReadLine() ?? "";

                if (decimal.TryParse(input, out decimal value)
                    && value > 0)
                {
                    return value;
                }

                Console.WriteLine(
                    "Please enter a valid amount greater than zero."
                );
            }
        }

        static DateTime ReadDate(string message)
        {
            while (true)
            {
                Console.Write(message);

                string input = Console.ReadLine() ?? "";

                if (DateTime.TryParse(input, out DateTime date))
                {
                    return date;
                }

                Console.WriteLine(
                    "Please enter a valid date."
                );
            }
        }
   static string GetNullableString(object value)
        {
            if (value == DBNull.Value)
            {
                return "N/A";
            }

            return value.ToString() ?? "N/A";
        }
     static string GetConnectionString()
        {
            string? connectionString =
                Environment.GetEnvironmentVariable(
                    "STUDENT_DB_CONNECTION");

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            return "Server=localhost;" +
                   "Database=StudentManagementDB;" +
                   "Trusted_Connection=True;" +
                   "TrustServerCertificate=True;";
        }
        static string GetFriendlySqlError(SqlException ex)
        {
            switch (ex.Number)
            {
                case 2601:
                case 2627:
                    return "This record already exists.";

                case 547:
                    return "The operation could not be completed because " +
                           "the referenced student or course does not exist.";

                case 515:
                    return "A required field was not supplied.";

                case 245:
                    return "One of the supplied values has an invalid data type.";

                default:
                    return ex.Message;
            }
        }
    }
}