using System;
using System.Runtime.CompilerServices;

namespace FareNexDriverApp.Core.Interfaces.Core
{
    public interface ILogService<TClass> where TClass : class
    {
        void LogMethodEntry(
            TClass? className = default,
            [CallerMemberName] string memberName = "");

        void LogMethodEntry(
            string formattedString,
            TClass? className = default,
            [CallerMemberName] string memberName = "");

        void LogMethodExit(
            TClass? className = default,
            [CallerMemberName] string memberName = "");

        void LogMethodExit(
            string formattedString,
            TClass? className = default,
            [CallerMemberName] string memberName = "");

        void LogInformation(
            string formattedString,
            [CallerMemberName] string memberName = "");

        void LogException(
            Exception exception,
            [CallerMemberName] string memberName = "");
    }
}
