using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConventer.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public double Amount { get; set; }
        public double ConvertedAmount { get; set; }
        public DateTime ConversationDate { get; set; }
    }
}