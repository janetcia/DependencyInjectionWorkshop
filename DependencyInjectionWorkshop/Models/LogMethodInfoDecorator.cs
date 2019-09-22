namespace DependencyInjectionWorkshop.Models
{
    public class LogMethodInfoDecorator: AuthenticationBaseDecorator
    {
        private readonly ILogger _logger;

        public LogMethodInfoDecorator(IAuthentication authentication, ILogger logger) : base(authentication)
        {
            _logger = logger;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            Log($"accountId: {accountId}, password: {password}, otp: {otp}");
            var isValid = base.Verify(accountId, password, otp);
            Log($"isValid: {isValid}");
            return isValid;
        }

        private void Log(string message)
        {
            _logger.Info(message);
        }
    }
}