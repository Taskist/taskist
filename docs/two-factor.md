# Two-factor sign-in

Two-factor sign-in asks for a code from an authenticator app after the password, so a stolen password alone isn't enough to get into Taskist. It works with any app that supports standard time-based codes (TOTP), such as Microsoft Authenticator, Google Authenticator, 1Password, Bitwarden, Authy or Aegis.

- [For users: turn it on](#for-users-turn-it-on)
- [Signing in](#signing-in)
- [Lost your phone?](#lost-your-phone)
- [For administrators](#for-administrators)

## For users: turn it on

1. Open **My profile → Security** and click **Turn on** under *Two-factor sign-in*.
2. Scan the QR code with your authenticator app. If you can't scan it, type the key shown under the code into the app instead.
3. Enter the 6-digit code the app shows and click **Turn on**.
4. Taskist shows ten **recovery codes**. Save them somewhere safe, for example in your password manager. Each one signs you in once if you lose your phone. They aren't shown again.

The option is only shown for accounts with a Taskist password. If you sign in with single sign-on, your organisation's identity provider handles extra verification instead.

## Signing in

After your password, Taskist asks for the current code from your app. If you tick **Don't ask again on this device**, that browser skips the code step for the next 30 days (your administrator can change or turn off this period).

If a code is rejected, check that your phone's clock is set automatically, and use the newest code. A code can only be used once.

## Lost your phone?

- On the code step, click **Can't use your app? Use a recovery code** and enter one of your saved recovery codes.
- Once you're in, go to **My profile → Security** and either turn two-factor sign-in off and on again with your new phone, or click **New recovery codes** if you've used several.
- No phone and no recovery codes? Ask an administrator to **reset** your two-factor sign-in. You can then sign in with your password and set it up again.

## For administrators

### Settings

| Setting | Default | Description |
| --- | --- | --- |
| `Authentication:TwoFactor:RequireForAdministrators` | `false` | Administrators who sign in with a password must set up two-factor sign-in. Until they do, they can only use their profile page. |
| `Authentication:TwoFactor:RememberDeviceDays` | `30` | How long "Don't ask again on this device" lasts. `0` removes the option, so a code is asked at every sign-in. |

In Docker these are `TWO_FACTOR_REQUIRE_FOR_ADMINS` and `TWO_FACTOR_REMEMBER_DEVICE_DAYS` in `.env`.

### Resetting a user

Open **Users & Access → Users**, edit the user, and click **Reset two-factor**. This removes their authenticator key, recovery codes and remembered devices; they can sign in with just their password until they set it up again. Both your account and theirs get an entry in the activity log.

Before resetting, make sure the request really comes from that person: a reset by mistake puts the account back to password-only.

### Good to know

- Wrong codes count towards the normal failed-attempt lock-out (**Configuration → Settings → Security**), just like wrong passwords. A locked account is unlocked from the user list as usual.
- Turning two-factor sign-in off, or creating new recovery codes, needs a current code, so someone who briefly gets hold of an open session can't quietly remove it.
- Single sign-on users are never asked for a Taskist code. If you require MFA, enforce it at your identity provider.
- Keep the Data Protection key folder backed up (see [SECURITY.md](../SECURITY.md)). The authenticator keys are encrypted with it; if it's lost, every user with two-factor sign-in needs a reset.
