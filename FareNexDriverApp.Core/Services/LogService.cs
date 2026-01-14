using FareNexDriverApp.Core.Interfaces.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;

namespace FareNextDriverApp.Core.Services
{
    public class LogService<TClass> : ILogService<TClass> where TClass : class
    {
        private readonly Lazy<ILogger<TClass>> _logger;

        public LogService(Lazy<ILogger<TClass>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Logs exceptions
        /// </summary>
        public void LogException(
            Exception exception,
            [CallerMemberName] string memberName = "")
        {
            _logger.Value.LogError(
                exception,
                "{Message} | Method: {MemberName} | Class: {ClassName}",
                exception.Message,
                memberName,
                typeof(TClass).Name);
        }

        /// <summary>
        /// Logs information
        /// </summary>
        public void LogInformation(
            string formattedString,
            [CallerMemberName] string memberName = "")
        {
            _logger.Value.LogInformation(
                "{Message} | Method: {MemberName} | Class: {ClassName}",
                formattedString,
                memberName,
                typeof(TClass).Name);
        }

        /// <summary>
        /// Logs method entry
        /// </summary>
        public void LogMethodEntry(
            TClass? className = default,
            [CallerMemberName] string memberName = "")
        {
            _logger.Value.LogInformation(
                "Method Entry | Method: {MemberName} | Class: {ClassName}",
                memberName,
                typeof(TClass).Name);
        }

        /// <summary>
        /// Logs method entry with additional info
        /// </summary>
        public void LogMethodEntry(
            string formattedString,
            TClass? className = default,
            [CallerMemberName] string memberName = "")
        {
            _logger.Value.LogInformation(
                "Method Entry | {Message} | Method: {MemberName} | Class: {ClassName}",
                formattedString,
                memberName,
                typeof(TClass).Name);
        }

        /// <summary>
        /// Logs method exit
        /// </summary>
        public void LogMethodExit(
            TClass? className = default,
            [CallerMemberName] string memberName = "")
        {
            _logger.Value.LogInformation(
                "Method Exit | Method: {MemberName} | Class: {ClassName}",
                memberName,
                typeof(TClass).Name);
        }

        /// <summary>
        /// Logs method exit with additional info
        /// </summary>
        public void LogMethodExit(
            string formattedString,
            TClass? className = default,
            [CallerMemberName] string memberName = "")
        {
            _logger.Value.LogInformation(
                "Method Exit | {Message} | Method: {MemberName} | Class: {ClassName}",
                formattedString,
                memberName,
                typeof(TClass).Name);
        }
    }
}
