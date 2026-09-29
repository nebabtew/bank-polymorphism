using System;
using System.Globalization;
using System.IO;

namespace Problem1_Payroll
{
    /// <summary>
    /// Thrown when a line from employees.txt can't be parsed into a valid employee record.
    /// Carries the line number and the raw text so the caller can report exactly what failed.
    /// </summary>
    public class InvalidEmployeeDataException : Exception
    {
        /// <summary>The 1-based line number in the source file where parsing failed.</summary>
        public int LineNumber { get; }

        /// <summary>The raw text of the line that failed to parse.</summary>
        public string LineText { get; }

        /// <summary>
        /// Creates a new InvalidEmployeeDataException carrying the line number and offending text.
        /// </summary>
        /// <param name="lineNumber">The line number where parsing failed.</param>
        /// <param name="lineText">The raw text of the offending line.</param>
        /// <param name="message">A description of the error.</param>
        public InvalidEmployeeDataException(int lineNumber, string lineText, string message)
            : base(message)
        {
            LineNumber = lineNumber;
            LineText = lineText;
        }
    }

    /// <summary>
    /// Reads employee records from a text file and produces a payroll report.
    /// </summary>
    public class PayrollFileProcessor
    {
        /// <summary>
        /// Calculates gross pay for one employee (rate * hours, no overtime) and adds it
        /// to the running total payroll.
        /// </summary>
        /// <param name="name">The employee's name.</param>
        /// <param name="rate">The employee's hourly rate.</param>
        /// <param name="hours">The number of hours worked.</param>
        /// <param name="grossPay">The gross pay calculated for this employee.</param>
        /// <param name="totalPayroll">The running total payroll, accumulated across calls.</param>
        public static void CalculatePay(string name, double rate, double hours, out double grossPay, ref double totalPayroll)
        {
            grossPay = rate * hours;
            totalPayroll += grossPay;
        }

        /// <summary>
        /// Entry point. Reads employees.txt line by line, prints a payroll report, and totals the payroll.
        /// Any line that fails to parse is reported and skipped rather than crashing the program.
        /// </summary>
        public static void Main()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            Console.WriteLine("=== Payroll Report ===");
            double totalPayroll = 0;

            using (StreamReader reader = new StreamReader("employees.txt"))
            {
                string? line;
                int lineNumber = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    try
                    {
                        string[] parts = line.Split(',');
                        string name = parts.Length > 0 ? parts[0] : line;

                        if (parts.Length != 3 ||
                            !double.TryParse(parts[1], out double rate) ||
                            !double.TryParse(parts[2], out double hours))
                        {
                            throw new InvalidEmployeeDataException(lineNumber, line, $"Could not parse rate/hours for '{name}'");
                        }

                        CalculatePay(name, rate, hours, out double grossPay, ref totalPayroll);
                        Console.WriteLine($"{name} | Rate: ${rate:N2} | Hours: {hours:N2} | Gross: ${grossPay:N2}");
                    }
                    catch (InvalidEmployeeDataException ex)
                    {
                        Console.WriteLine($"[ERROR] Line {ex.LineNumber}: {ex.Message}");
                    }
                }
            }

            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine($"Total Payroll: ${totalPayroll:N2}");
        }
    }
}
