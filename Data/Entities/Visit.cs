namespace ASP_421.Data.Entities
{
    public class Visit
    {
        public Guid Id { get; set; }
        public DateTime VisitTime { get; set; }
        public string RequestPath { get; set; } = null!; // /Home/Privacy
        public string? UserLogin { get; set; } // null если не авторизован
        public string ConfirmationCode { get; set; } = null!; // код подтверждения
        public bool IsConfirmed { get; set; } = false; // статус подтверждения
        public DateTime? ConfirmedAt { get; set; } // время подтверждения
        public string? UserAgent { get; set; } // браузер пользователя
        public string? IpAddress { get; set; } // IP адрес
    }
}
