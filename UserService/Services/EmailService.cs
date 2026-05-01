using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace UserService.Services;

public record EmailOptions
{
    public string SmtpHost { get; init; } = "";
    public int SmtpPort { get; init; } = 587;
    public string From { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
}

public interface IEmailService
{
    Task SendWelcomeAsync(string toEmail, string role);
    Task SendAccountDeletedAsync(string toEmail);
}

public class EmailService(IOptions<EmailOptions> options) : IEmailService
{
    private readonly EmailOptions _opt = options.Value;

    public async Task SendWelcomeAsync(string toEmail, string role)
        => await SendAsync(toEmail, "Ласкаво просимо до LogopedApp! 🎉", BuildWelcomeBody(toEmail, role));

    public async Task SendAccountDeletedAsync(string toEmail)
        => await SendAsync(toEmail, "Ваш акаунт видалено", BuildDeletedBody(toEmail));

    private async Task SendAsync(string toEmail, string subject, string body)
    {
        using var client = new SmtpClient(_opt.SmtpHost, _opt.SmtpPort)
        {
            Credentials = new NetworkCredential(_opt.Username, _opt.Password),
            EnableSsl = true,
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_opt.From, _opt.DisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };

        message.To.Add(toEmail);
        await client.SendMailAsync(message);
    }

    private static string BuildWelcomeBody(string email, string role)
    {
        var roleText = role == "Logoped" ? "логопеда" : "батьків";

        return $"""
            <!DOCTYPE html>
            <html lang="uk">
            <head>
              <meta charset="UTF-8"/>
              <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
            </head>
            <body style="margin:0;padding:0;background:#f4f6fb;font-family:Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f6fb;padding:40px 0;">
                <tr>
                  <td align="center">
                    <table width="480" cellpadding="0" cellspacing="0"
                           style="background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
                      <tr>
                        <td style="background:#6C63FF;padding:32px;text-align:center;">
                          <h1 style="margin:0;color:#ffffff;font-size:26px;">LogopedApp 🗣️</h1>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:36px 40px;">
                          <h2 style="margin:0 0 12px;color:#1a1a2e;font-size:22px;">Вітаємо! 🎉</h2>
                          <p style="margin:0 0 16px;color:#555;font-size:15px;line-height:1.6;">
                            Ваш акаунт <strong>{email}</strong> успішно створено як акаунт {roleText}.
                          </p>
                          <p style="margin:0 0 24px;color:#555;font-size:15px;line-height:1.6;">
                            Тепер ви можете користуватися всіма можливостями додатку:
                          </p>
                          <table cellpadding="0" cellspacing="0" style="margin-bottom:28px;">
                            <tr><td style="padding:6px 0;color:#444;font-size:14px;">✅&nbsp; Логопедичні вправи</td></tr>
                            <tr><td style="padding:6px 0;color:#444;font-size:14px;">✅&nbsp; Ігри для автоматизації звуків</td></tr>
                            <tr><td style="padding:6px 0;color:#444;font-size:14px;">✅&nbsp; Відстеження прогресу</td></tr>
                          </table>
                          <p style="margin:0;color:#aaa;font-size:12px;">
                            Якщо ви не реєструвалися — просто проігноруйте цей лист.
                          </p>
                        </td>
                      </tr>
                      <tr>
                        <td style="background:#f4f6fb;padding:20px;text-align:center;">
                          <p style="margin:0;color:#bbb;font-size:12px;">© 2025 LogopedApp</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string BuildDeletedBody(string email)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="uk">
            <head>
              <meta charset="UTF-8"/>
              <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
            </head>
            <body style="margin:0;padding:0;background:#f4f6fb;font-family:Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f6fb;padding:40px 0;">
                <tr>
                  <td align="center">
                    <table width="480" cellpadding="0" cellspacing="0"
                           style="background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
                      <tr>
                        <td style="background:#EF4444;padding:32px;text-align:center;">
                          <h1 style="margin:0;color:#ffffff;font-size:26px;">LogopedApp 🗣️</h1>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:36px 40px;">
                          <h2 style="margin:0 0 12px;color:#1a1a2e;font-size:22px;">Акаунт видалено</h2>
                          <p style="margin:0 0 16px;color:#555;font-size:15px;line-height:1.6;">
                            Акаунт <strong>{email}</strong> було успішно видалено з системи.
                          </p>
                          <p style="margin:0 0 24px;color:#555;font-size:15px;line-height:1.6;">
                            Всі ваші дані були видалені. Якщо ви захочете повернутися — просто зареєструйтеся знову.
                          </p>
                          <p style="margin:0;color:#aaa;font-size:12px;">
                            Якщо ви не видаляли акаунт — негайно зверніться до підтримки.
                          </p>
                        </td>
                      </tr>
                      <tr>
                        <td style="background:#f4f6fb;padding:20px;text-align:center;">
                          <p style="margin:0;color:#bbb;font-size:12px;">© 2025 LogopedApp</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }
}