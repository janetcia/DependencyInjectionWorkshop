namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);

            var isValid = base.Verify(accountId, password, otp);
            if (isValid)
            {
                Reset(accountId);
            }
            else
            {
                Add(accountId);
            }
            
            return isValid;
        }

        private void Reset(string accountId)
        {
            _failedCounter.ResetFailedCount(accountId);
        }

        private void Add(string accountId)
        {
            _failedCounter.AddFailedCount(accountId);
        }

        private void CheckAccountIsLocked(string accountId)
        {
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
        }
    }
}