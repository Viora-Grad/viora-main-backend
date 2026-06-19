namespace Viora.Application.Abstractions.Mail;

internal static class EmailTemplateFactory
{
    public static EmailMessage ApplicationAccepted(string userName) =>
        new("Your Viora onboarding application has been accepted", $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
              <title>Application Accepted</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f4f6f9;font-family:'Segoe UI',Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f6f9;padding:40px 0;">
                <tr>
                  <td align="center">
                    <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);">

                      <!-- Header -->
                      <tr>
                        <td style="background:linear-gradient(135deg,#1a1a2e 0%,#16213e 60%,#0f3460 100%);padding:40px 48px;text-align:center;">
                          <h1 style="margin:0;color:#e94560;font-size:28px;font-weight:700;letter-spacing:2px;text-transform:uppercase;">VIORA</h1>
                          <p style="margin:6px 0 0;color:#a0aec0;font-size:13px;letter-spacing:1px;text-transform:uppercase;">Healthcare Management Platform</p>
                        </td>
                      </tr>

                      <!-- Status Badge -->
                      <tr>
                        <td align="center" style="padding:36px 48px 0;">
                          <span style="display:inline-block;background-color:#d1fae5;color:#065f46;font-size:13px;font-weight:600;padding:6px 20px;border-radius:999px;letter-spacing:0.5px;">
                            ✓ &nbsp; Application Approved
                          </span>
                        </td>
                      </tr>

                      <!-- Body -->
                      <tr>
                        <td style="padding:28px 48px 0;">
                          <h2 style="margin:0 0 12px;color:#1a1a2e;font-size:22px;font-weight:600;">Welcome aboard, {userName}!</h2>
                          <p style="margin:0 0 16px;color:#4a5568;font-size:15px;line-height:1.7;">
                            We're thrilled to let you know that your onboarding application for <strong>Viora</strong> has been reviewed and <strong>officially accepted</strong>.
                            Your organization is now part of the Viora network.
                          </p>
                          <p style="margin:0 0 24px;color:#4a5568;font-size:15px;line-height:1.7;">
                            To get started and unlock all platform features, you'll need to select a subscription plan that fits your organization's needs.
                          </p>
                        </td>
                      </tr>

                      <!-- Steps -->
                      <tr>
                        <td style="padding:0 48px;">
                          <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f8fafc;border-radius:10px;padding:24px;margin-bottom:8px;">
                            <tr>
                              <td style="width:36px;vertical-align:top;padding-top:2px;">
                                <span style="display:inline-block;width:28px;height:28px;background-color:#e94560;border-radius:50%;color:#fff;font-size:13px;font-weight:700;text-align:center;line-height:28px;">1</span>
                              </td>
                              <td style="padding-left:14px;">
                                <p style="margin:0 0 2px;color:#1a1a2e;font-size:14px;font-weight:600;">Log in to your Viora dashboard</p>
                                <p style="margin:0;color:#718096;font-size:13px;">Use the credentials you registered with during onboarding.</p>
                              </td>
                            </tr>
                            <tr><td colspan="2" style="height:16px;"></td></tr>
                            <tr>
                              <td style="width:36px;vertical-align:top;padding-top:2px;">
                                <span style="display:inline-block;width:28px;height:28px;background-color:#e94560;border-radius:50%;color:#fff;font-size:13px;font-weight:700;text-align:center;line-height:28px;">2</span>
                              </td>
                              <td style="padding-left:14px;">
                                <p style="margin:0 0 2px;color:#1a1a2e;font-size:14px;font-weight:600;">Navigate to Subscription Plans</p>
                                <p style="margin:0;color:#718096;font-size:13px;">Browse our available plans and compare features and limits.</p>
                              </td>
                            </tr>
                            <tr><td colspan="2" style="height:16px;"></td></tr>
                            <tr>
                              <td style="width:36px;vertical-align:top;padding-top:2px;">
                                <span style="display:inline-block;width:28px;height:28px;background-color:#e94560;border-radius:50%;color:#fff;font-size:13px;font-weight:700;text-align:center;line-height:28px;">3</span>
                              </td>
                              <td style="padding-left:14px;">
                                <p style="margin:0 0 2px;color:#1a1a2e;font-size:14px;font-weight:600;">Enroll in a plan</p>
                                <p style="margin:0;color:#718096;font-size:13px;">Select your plan to activate your branches, services, and staff quota.</p>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>

                      <!-- CTA -->
                      <tr>
                        <td align="center" style="padding:32px 48px;">
                          <a href="#" style="display:inline-block;background-color:#e94560;color:#ffffff;font-size:15px;font-weight:600;padding:14px 40px;border-radius:8px;text-decoration:none;letter-spacing:0.5px;">
                            View Subscription Plans
                          </a>
                        </td>
                      </tr>

                      <!-- Divider -->
                      <tr>
                        <td style="padding:0 48px;">
                          <hr style="border:none;border-top:1px solid #e2e8f0;margin:0;" />
                        </td>
                      </tr>

                      <!-- Footer -->
                      <tr>
                        <td style="padding:24px 48px 36px;text-align:center;">
                          <p style="margin:0 0 6px;color:#a0aec0;font-size:12px;">
                            If you have any questions, contact us at
                            <a href="mailto:teamcomplex.grad@gmail.com" style="color:#e94560;text-decoration:none;">support@viora.com</a>
                          </p>
                          <p style="margin:0;color:#cbd5e0;font-size:11px;">© 2026 Viora. All rights reserved.</p>
                        </td>
                      </tr>

                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """);

    public static EmailMessage LegalPaperExpiredAdmin(string organizationName, string ownerName, string paperName, DateTime expiredOnUtc) =>
        new($"[Action Required] Legal paper expired — {organizationName}", $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
              <title>Legal Paper Expired</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f4f6f9;font-family:'Segoe UI',Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f6f9;padding:40px 0;">
                <tr>
                  <td align="center">
                    <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);">

                      <!-- Header -->
                      <tr>
                        <td style="background:linear-gradient(135deg,#1a1a2e 0%,#16213e 60%,#0f3460 100%);padding:40px 48px;text-align:center;">
                          <h1 style="margin:0;color:#e94560;font-size:28px;font-weight:700;letter-spacing:2px;text-transform:uppercase;">VIORA</h1>
                          <p style="margin:6px 0 0;color:#a0aec0;font-size:13px;letter-spacing:1px;text-transform:uppercase;">Admin Notification</p>
                        </td>
                      </tr>

                      <!-- Alert Badge -->
                      <tr>
                        <td align="center" style="padding:36px 48px 0;">
                          <span style="display:inline-block;background-color:#fee2e2;color:#991b1b;font-size:13px;font-weight:600;padding:6px 20px;border-radius:999px;letter-spacing:0.5px;">
                            ⚠ &nbsp; Legal Paper Expired
                          </span>
                        </td>
                      </tr>

                      <!-- Body -->
                      <tr>
                        <td style="padding:28px 48px 0;">
                          <h2 style="margin:0 0 12px;color:#1a1a2e;font-size:22px;font-weight:600;">Document renewal required</h2>
                          <p style="margin:0 0 20px;color:#4a5568;font-size:15px;line-height:1.7;">
                            A legal paper submitted by an organization has expired and requires your attention.
                            Please review the details below and reach out to the organization owner to request an updated document.
                          </p>
                        </td>
                      </tr>

                      <!-- Details Card -->
                      <tr>
                        <td style="padding:0 48px;">
                          <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f8fafc;border-radius:10px;border-left:4px solid #e94560;padding:20px 24px;">
                            <tr>
                              <td style="padding:6px 0;">
                                <p style="margin:0;color:#718096;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">Organization</p>
                                <p style="margin:4px 0 0;color:#1a1a2e;font-size:15px;font-weight:600;">{organizationName}</p>
                              </td>
                            </tr>
                            <tr>
                              <td style="padding:6px 0;">
                                <p style="margin:0;color:#718096;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">Owner</p>
                                <p style="margin:4px 0 0;color:#1a1a2e;font-size:15px;font-weight:600;">{ownerName}</p>
                              </td>
                            </tr>
                            <tr>
                              <td style="padding:6px 0;">
                                <p style="margin:0;color:#718096;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">Document</p>
                                <p style="margin:4px 0 0;color:#1a1a2e;font-size:15px;font-weight:600;">{paperName}</p>
                              </td>
                            </tr>
                            <tr>
                              <td style="padding:6px 0;">
                                <p style="margin:0;color:#718096;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">Expired On</p>
                                <p style="margin:4px 0 0;color:#e94560;font-size:15px;font-weight:600;">{expiredOnUtc:dd MMM yyyy} UTC</p>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>

                      <!-- CTA -->
                      <tr>
                        <td align="center" style="padding:32px 48px;">
                          <a href="#" style="display:inline-block;background-color:#e94560;color:#ffffff;font-size:15px;font-weight:600;padding:14px 40px;border-radius:8px;text-decoration:none;letter-spacing:0.5px;">
                            Review Organization
                          </a>
                        </td>
                      </tr>

                      <!-- Divider -->
                      <tr>
                        <td style="padding:0 48px;">
                          <hr style="border:none;border-top:1px solid #e2e8f0;margin:0;" />
                        </td>
                      </tr>

                      <!-- Footer -->
                      <tr>
                        <td style="padding:24px 48px 36px;text-align:center;">
                          <p style="margin:0 0 6px;color:#a0aec0;font-size:12px;">This is an automated internal notification from the Viora platform.</p>
                          <p style="margin:0;color:#cbd5e0;font-size:11px;">© 2026 Viora. All rights reserved.</p>
                        </td>
                      </tr>

                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """);

    public static EmailMessage LegalPaperExpiredClient(string userName, string paperName, DateTime expiredOnUtc) =>
        new("Action required — your legal document has expired", $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
              <title>Legal Paper Expired</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f4f6f9;font-family:'Segoe UI',Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f6f9;padding:40px 0;">
                <tr>
                  <td align="center">
                    <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);">

                      <!-- Header -->
                      <tr>
                        <td style="background:linear-gradient(135deg,#1a1a2e 0%,#16213e 60%,#0f3460 100%);padding:40px 48px;text-align:center;">
                          <h1 style="margin:0;color:#e94560;font-size:28px;font-weight:700;letter-spacing:2px;text-transform:uppercase;">VIORA</h1>
                          <p style="margin:6px 0 0;color:#a0aec0;font-size:13px;letter-spacing:1px;text-transform:uppercase;">Healthcare Management Platform</p>
                        </td>
                      </tr>

                      <!-- Alert Badge -->
                      <tr>
                        <td align="center" style="padding:36px 48px 0;">
                          <span style="display:inline-block;background-color:#fee2e2;color:#991b1b;font-size:13px;font-weight:600;padding:6px 20px;border-radius:999px;letter-spacing:0.5px;">
                            ⚠ &nbsp; Document Expired
                          </span>
                        </td>
                      </tr>

                      <!-- Body -->
                      <tr>
                        <td style="padding:28px 48px 0;">
                          <h2 style="margin:0 0 12px;color:#1a1a2e;font-size:22px;font-weight:600;">Attention required, {userName}</h2>
                          <p style="margin:0 0 16px;color:#4a5568;font-size:15px;line-height:1.7;">
                            One of the legal documents registered under your organization has expired.
                            To keep your account in good standing, please contact the Viora support team to submit an updated copy.
                          </p>
                        </td>
                      </tr>

                      <!-- Details Card -->
                      <tr>
                        <td style="padding:0 48px 8px;">
                          <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f8fafc;border-radius:10px;border-left:4px solid #e94560;padding:20px 24px;">
                            <tr>
                              <td style="padding:6px 0;">
                                <p style="margin:0;color:#718096;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">Document</p>
                                <p style="margin:4px 0 0;color:#1a1a2e;font-size:15px;font-weight:600;">{paperName}</p>
                              </td>
                            </tr>
                            <tr>
                              <td style="padding:6px 0;">
                                <p style="margin:0;color:#718096;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">Expired On</p>
                                <p style="margin:4px 0 0;color:#e94560;font-size:15px;font-weight:600;">{expiredOnUtc:dd MMM yyyy} UTC</p>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>

                      <!-- What to do -->
                      <tr>
                        <td style="padding:24px 48px 0;">
                          <p style="margin:0 0 8px;color:#1a1a2e;font-size:14px;font-weight:600;">What should I do?</p>
                          <p style="margin:0;color:#4a5568;font-size:14px;line-height:1.7;">
                            Reach out to our support team with a renewed copy of the document. Our team will review and update your records promptly.
                            Failure to renew may affect your organization's standing on the platform.
                          </p>
                        </td>
                      </tr>

                      <!-- CTA -->
                      <tr>
                        <td align="center" style="padding:32px 48px;">
                          <a href="mailto:teamcomplex.grad@gmail.com" style="display:inline-block;background-color:#e94560;color:#ffffff;font-size:15px;font-weight:600;padding:14px 40px;border-radius:8px;text-decoration:none;letter-spacing:0.5px;">
                            Contact Support
                          </a>
                        </td>
                      </tr>

                      <!-- Divider -->
                      <tr>
                        <td style="padding:0 48px;">
                          <hr style="border:none;border-top:1px solid #e2e8f0;margin:0;" />
                        </td>
                      </tr>

                      <!-- Footer -->
                      <tr>
                        <td style="padding:24px 48px 36px;text-align:center;">
                          <p style="margin:0 0 6px;color:#a0aec0;font-size:12px;">
                            Questions? Contact us at
                            <a href="mailto:teamcomplex.grad@gmail.com" style="color:#e94560;text-decoration:none;">support@viora.com</a>
                          </p>
                          <p style="margin:0;color:#cbd5e0;font-size:11px;">© 2026 Viora. All rights reserved.</p>
                        </td>
                      </tr>

                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """);
}
