using Masticore.Models;
using Masticore.Resources;
using System.Threading.Tasks;

namespace Masticore
{
    /// <summary>
    /// Abstraction of an email address
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Address() { }

        /// <summary>
        /// Constructor that takes a user and uses their email and display name
        /// </summary>
        /// <param name="user"></param>
        public Address(User user)
        {
            Email = user.Email;
            Name = user.DisplayName();
        }

        /// <summary>
        /// Constructor that takes an email and name
        /// </summary>
        /// <param name="email"></param>
        /// <param name="name"></param>
        public Address(string email, string name)
        {
            Email = email;
            Name = name;
        }

        /// <summary>
        /// Name of the destination person
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Email address of the destination person
        /// </summary>
        public string Email { get; set; }
    }

    /// <summary>
    /// One instance of a rendered view in both HTML and optional plain-text format
    /// </summary>
    public class Body
    {
        /// <summary>
        /// Plain text version of the email
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// HTML version of the email
        /// </summary>
        public string Html { get; set; }
    }


    /// <summary>
    /// An abstraction of a mail message
    /// </summary>
    public class MailMessage
    {
        /// <summary>
        /// From Address of the message
        /// </summary>
        public Address From { get; set; }

        /// <summary>
        /// Destination address for the message
        /// </summary>
        public Address To { get; set; }

        /// <summary>
        /// Optional CC addresses for the message
        /// </summary>
        public Address CC_1 { get; set; }

        /// <summary>
        /// Optional CC addresses for the message
        /// </summary>
        public Address CC_2 { get; set; }

        /// <summary>
        /// Subject for the message
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Body of the message
        /// </summary>
        public Body Body { get; set; }
    }

    /// <summary>
    /// Interface for a mailbox that can send mail
    /// </summary>
    public interface IMailbox
    {
        /// <summary>
        /// Send a mail message
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        Task SendAsync(MailMessage mail);

        /// <summary>
        /// Sends a mail message for the given org; this enables alternative mappings
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task SendForOrgAsync(MailMessage mail, string orgRoute);
    }
}
