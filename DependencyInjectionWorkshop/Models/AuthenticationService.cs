using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IOtpService _otpService;
        private readonly IProfile _profile;
        private readonly IHash _hash;

        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.ComputeHash(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            return hashedPassword == passwordFromDb && otp == currentOtp;
        }
    }
}