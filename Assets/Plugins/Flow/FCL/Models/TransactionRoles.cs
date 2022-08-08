namespace Flow.FCL.Models
{
    public class TransactionRoles
    {
        public bool Proposer { get; set; }

        public bool Authorizer { get; set; }

        public bool Payer { get; set; }
    }
}