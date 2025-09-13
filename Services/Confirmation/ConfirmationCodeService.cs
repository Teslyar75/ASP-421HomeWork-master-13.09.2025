using ASP_421.Services.Random;

namespace ASP_421.Services.Confirmation
{
    public class ConfirmationCodeService : IConfirmationCodeService
    {
        private readonly IRandomService _randomService;
        private const int CodeLength = 6;
        private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public ConfirmationCodeService(IRandomService randomService)
        {
            _randomService = randomService;
        }

        public string GenerateCode()
        {
            var code = new char[CodeLength];
            for (int i = 0; i < CodeLength; i++)
            {
                code[i] = AllowedChars[_randomService.Next(AllowedChars.Length)];
            }
            return new string(code);
        }

        public bool IsValidCode(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length != CodeLength)
                return false;

            return code.All(c => AllowedChars.Contains(c));
        }
    }
}
