namespace PaymentSystem.Dto
{
    public class IyzicoPayUserDto
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserSurname { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public string? UserAddress { get; set; }
        public string? UserCity { get; set; }
        public string? Price { get; set; }
        public string? Date { get; set; }
        public string? PaymentId { get; set; }
        public string? UserReceivingPaymentId{ get; set; }
        public string? UserReceivingPayment { get; set;} //Ödemeyi alan kullanıcı
    }
}