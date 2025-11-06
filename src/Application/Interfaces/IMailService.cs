using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IMailService
{
    Task<BaseResponse<bool>> SendAccountVerificationEmail(string emailAddress, string firstName, string subject, string confirmationLink);
    string GenerateEmailConfirmationLinkAsync(string token, string email);
    Task<BaseResponse<bool>> SendMail(List<string> destination, string[] replacements, string subject, string emailTemplatePath);
    string GeneratePasswordResetLinkAsync(string token, string email);
    Task<BaseResponse<bool>> SendPasswordResetEmail(string emailAddress, string subject, string passwordResetLink, string userName);
    Task<BaseResponse<bool>> SendAccountConfirmationEmail(string email, string name);
    Task SendAccountBlockedEmail(string email, string name);
    Task SendAccountUnBlockedEmail(string email, string name);
    Task<BaseResponse<bool>> SendAdminAccountVerificationEmail(string emailAddress, string firstName, string subject, string confirmationLink, string password);

}
