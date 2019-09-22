using System;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "joey";
        private const string DefaultHashedPassword = "my hashed password";
        private const string DefaultInputPassword = "abc";
        private const string DefaultOtp = "123456";
        private const int DefaultFailedCount = 88;
        private IProfile _profile;
        private IHash _hash;
        private IOtpService _otpService;
        private IFailedCounter _failedCounter;
        private ILogger _logger;
        private INotification _notification;
        private IAuthentication _authentication;

        [SetUp]
        public void Setup()
        {
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otpService = Substitute.For<IOtpService>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _authentication = new AuthenticationService(_profile, _hash, _otpService);
            _authentication = new NotificationDecorator(_authentication, _notification);
            _authentication = new FailedCounterDecorator(_authentication, _failedCounter);
            _authentication = new LogFailedCountDecorator(_authentication, _failedCounter, _logger);
        }

        [Test]
        public void Is_valid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);

            ShouldBeValid(isValid);
        }

        [Test]
        public void Is_Invalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, "wrong otp");

            ShouldBeInvalid(isValid);
        }
        
        [Test]
        public void Reset_FailedCount_WhenValid()
        {
            WhenValid();
            ShouldResetFailedCount();
        }

        [Test]
        public void Add_FailedCount_WhenInvalid()
        {
            WhenInvalid();
            ShouldAddFailedCount();
        }

        [Test]
        public void Log_FailedCount_WhenInvalid()
        {
            GivenFailedCount(DefaultAccountId, DefaultFailedCount);
            WhenInvalid();
            LogShouldContains(DefaultAccountId, DefaultFailedCount.ToString());

        }

        [Test]
        public void NotifyUser_WhenInvalid()
        {
            WhenInvalid();
            ShouldNotify(DefaultAccountId);
        }

        [Test]
        public void Account_IsLocked()
        {
            GivenAccountIsLocked(DefaultAccountId, true);
            ShouldThrow<FailedTooManyTimesException>();
        }

        private void ShouldThrow<TException>() where TException : Exception
        {
            TestDelegate action = () => WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);
            Assert.Throws<TException>(action);
        }

        private void GivenAccountIsLocked(string accountId, bool isLocked)
        {
            _failedCounter.GetAccountIsLocked(accountId).Returns(isLocked);
        }

        private void ShouldNotify(string accountId)
        {
            _notification.Received(1).Send(accountId);
        }

        private void LogShouldContains(string accountId, string failedCount)
        {
            _logger.Received(1).Info(Arg.Is<string>(m => m.Contains(accountId) && m.Contains(failedCount)));
        }

        private void GivenFailedCount(string accountId, int failedCount)
        {
            _failedCounter.GetFailedCount(accountId).Returns(failedCount);
        }

        private void ShouldAddFailedCount()
        {
            _failedCounter.Received(1).AddFailedCount(DefaultAccountId);
        }

        private void WhenInvalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            WhenVerify(DefaultAccountId, DefaultInputPassword, "wrong otp");
        }

        private void ShouldResetFailedCount()
        {
            _failedCounter.Received(1).ResetFailedCount(DefaultAccountId);
        }

        private void WhenValid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);
        }


        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private bool WhenVerify(string accountId, string password, string otp)
        {
            return _authentication.Verify(accountId, password, otp);
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otpService.GetCurrentOtp(accountId).Returns(otp);
        }

        private void GivenHash(string inputPassword, string hashedPassword)
        {
            _hash.ComputeHash(inputPassword).Returns(hashedPassword);
        }

        private void GivenPassword(string accountId, string password)
        {
            _profile.GetPassword(accountId).Returns(password);
        }
    }
}