using Masticore.Models;
using System;

namespace Masticore.Sms
{
    /// <summary>
    /// Phone number format
    /// </summary>
    public class PhoneNumber
    {
        /// <summary>
        /// Returns true if the given text is most likely a valid phone number
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsValidNumber(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            PhoneNumbers.PhoneNumberUtil phoneUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
            PhoneNumbers.PhoneNumber number = phoneUtil.Parse(text, "US");
            bool isPossiblePhoneNumber = !phoneUtil.IsPossibleNumber(number);
            return isPossiblePhoneNumber;
        }

        /// <summary>
        /// Examines the given text to determine the e164 format for the given number
        /// https://www.twilio.com/docs/glossary/what-e164#:~:text=Billy%20Chia-,E.,individual%20phones%20in%20different%20countries.
        /// Limited to being reliable in the US only
        /// </summary>
        /// <remarks>May throw exceptions on bad numbers, but never on null; Drops any extensions</remarks>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToE164Format(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    return null;
                }

                // Use https://github.com/twcclegg/libphonenumber-csharp
                // To format the number
                PhoneNumbers.PhoneNumberUtil phoneUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                PhoneNumbers.PhoneNumber number = phoneUtil.Parse(text, "US");
                bool isPossiblePhoneNumber = !phoneUtil.IsPossibleNumber(number);
                if (isPossiblePhoneNumber)
                {
                    throw new PhoneNumberParseException($"Given number is not valid: {text.LastChars(4)}");
                }

                return phoneUtil.Format(number, PhoneNumbers.PhoneNumberFormat.E164);
            }
            catch (PhoneNumberParseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PhoneNumberParseException(ex.Message);
            }
        }

        public PhoneNumber(string countryCode, string phoneNumber)
        {
            Number = $"+{countryCode}{phoneNumber}";
        }

        /// <summary>
        /// Read the given <see cref="User"/> and capture its phone number
        /// </summary>
        /// <param name="user"></param>
        public PhoneNumber(IUserFields user) : this(user.Phone) { }

        /// <summary>
        /// Read the given text to treat like a phone number
        /// </summary>
        /// <param name="text"></param>
        public PhoneNumber(string text)
        {
            Number = text;
        }

        public string Number { get; set; }

        /// <summary>
        /// Get true if <see cref="Number"/> is most likely a valid number
        /// </summary>
        public bool IsValid => IsValidNumber(Number);

        /// <summary>
        /// Gets the last 4 characters in the current phone number
        /// </summary>
        /// <remarks>Returns the text EMPTY if the number is non-existent</remarks>
        public string Last4Numbers
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Number))
                {
                    return "EMPTY";
                }

                return Number[^4..];
            }
        }

        /// <summary>
        /// Returns the current phone number in E164 format
        /// </summary>
        /// <returns></returns>
        public string ToE164Format()
        {
            return ToE164Format(Number);
        }
    }
}
