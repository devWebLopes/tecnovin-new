using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Empresa.Util;

/// <summary>
/// Serviço de envio de e-mails
/// </summary>
public class Email
{
    private readonly IConfiguration _configuration;

    public Email(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string GetAppSettings(string nome)
    {
        return _configuration[$"Email:{nome}"] ?? _configuration.GetSection("Email")[nome] ?? string.Empty;
    }

    public bool EnviaEmail(string body, string assunto, string emailDestino, string emailCc, string anexo, string emailBcc)
    {
        using var oEmail = new MailMessage();

        try
        {
            var sDe = new MailAddress(GetAppSettings("smtpEmail"), GetAppSettings("smtpNome"));

            string[] emaliDest = emailDestino.Replace(",", ";").Split(';');
            foreach (var item in emaliDest)
            {
                if (!string.IsNullOrEmpty(item))
                    oEmail.To.Add(item);
            }

            string[] emaliCc = emailCc.Replace(",", ";").Split(';');
            foreach (var item in emaliCc)
            {
                if (!string.IsNullOrEmpty(item))
                    oEmail.CC.Add(item);
            }

            if (!string.IsNullOrEmpty(emailBcc))
                oEmail.Bcc.Add(emailBcc);

            oEmail.From = sDe;
            oEmail.Priority = MailPriority.Normal;
            oEmail.IsBodyHtml = true;

            if (!string.IsNullOrEmpty(anexo))
                oEmail.Attachments.Add(new Attachment(anexo));

            oEmail.Subject = assunto;
            oEmail.Body = body;

            using var oEnviar = new SmtpClient
            {
                Host = GetAppSettings("smtpServidor"),
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(GetAppSettings("smtpEmail"), GetAppSettings("smtpSenha"))
            };

            oEnviar.Send(oEmail);
            return true;
        }
        catch
        {
            throw;
        }
    }
}