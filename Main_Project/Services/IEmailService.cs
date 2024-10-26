using System.Threading.Tasks;

namespace Main_Project.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string message);
        Task<bool> SendOtpAsync(string toEmail, string otp);

    }
}
