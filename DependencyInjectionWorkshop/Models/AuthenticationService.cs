using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;
        private readonly IOtpService _otpService;
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly INotification _notification;

        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService,
            INotification notification, IFailedCounter failedCounter, ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _notification = notification;
            _failedCounter = failedCounter;
            _logger = logger;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _notification = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _logger = new Logger();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked) throw new FailedTooManyTimesException();

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.ComputeHash(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(accountId);

                return true;
            }

            _failedCounter.AddFailedCount(accountId);

            var failedCount = _failedCounter.GetFailedCount(accountId);

            _logger.Info($"accountId:{accountId} failed times:{failedCount}");

            _notification.Send(accountId);

            return false;
        }
    }
}