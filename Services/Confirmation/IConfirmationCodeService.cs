namespace ASP_421.Services.Confirmation
{
    public interface IConfirmationCodeService
    {
        /// <summary>
        /// Генерирует уникальный код подтверждения
        /// </summary>
        /// <returns>Код подтверждения (6-8 символов)</returns>
        string GenerateCode();
        
        /// <summary>
        /// Проверяет валидность кода подтверждения
        /// </summary>
        /// <param name="code">Код для проверки</param>
        /// <returns>true если код валидный</returns>
        bool IsValidCode(string code);
    }
}
