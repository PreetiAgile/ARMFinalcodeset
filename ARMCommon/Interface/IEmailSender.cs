using ARMCommon.Model;

namespace ARMCommon.Interface
{
    public interface IEmailSender
    {
        Task SendEmailAsync(Message message);
    }
}
