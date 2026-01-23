using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FareNexDriverApp.Core.Utilities
{
    public static class EnumerationExtensions
    {
        public static string? GetDisplayDescription(this Enum enumValue)
        {
            return enumValue
                .GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DisplayAttribute>()?
                .GetDescription();
        }
    }
}
