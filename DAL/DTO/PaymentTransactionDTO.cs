using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class PaymentTransactionDTO
    {
        public int Id { get; set; }
        public int Total { get; set; }
        public int TransactionType { get; set; }
        public string? TransactionId { get; set; }
        public string? TransactionError { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? SuccessDate { get; set; }
        public string? PaymentUrl { get; set; }
        public int Status { get; set; }
        public int? MentorId { get; set; }
        public int EnrollmentId { get; set; }
    }
}
