# Single sign-on (OpenID Connect)

Taskist can let people sign in with the account they already have at your identity provider: Keycloak, Authentik, Microsoft Entra ID, Google, Okta, or any other OpenID Connect provider. It's off by default and set up entirely in configuration (`appsettings.json` or environment variables). There's no admin screen for it.

- [How it works](#how-it-works)
- [Before you start](#before-you-start)
- [Settings](#settings)
- [Keycloak: step by step](#keycloak-step-by-step)
- [Microsoft Entra ID](#microsoft-entra-id)
- [Google](#google)
- [Other providers](#other-providers)
- [Mapping provider groups to roles](#mapping-provider-groups-to-roles)
- [SSO-only mode](#sso-only-mode)
- [Troubleshooting](#troubleshooting)
- [Limitations](#limitations)

## How it works

The login page shows a **Sign in with …** button. After the person signs in at the provider, Taskist decides who they are:

| Situation | What happens |
| --- | --- |
| They have signed in with SSO before | Signed in to the same Taskist account, even if their e-mail has changed since. |
| First SSO sign-in, and a Taskist user has the same e-mail | The two are linked and they're signed in, **if the provider says the e-mail is verified**. |
| First SSO sign-in, no Taskist user with that e-mail | Refused with "ask your administrator", unless `AutoProvision` is on: then an account is created with the *Registered* role only. |
| The Taskist user is locked, inactive or deleted | Refused, as with a password sign-in. |

Everything after that is unchanged: roles, permissions, project access, session length and lock-out all work exactly as for password sign-in. People can see their linked sign-ins under **My profile → Security**, and administrators see them on the user's edit page.

## Before you start

1. **Set `App:BaseUrl`** to the address people use to reach Taskist, for example `https://taskist.example.com/`.
2. **The redirect URI** to register at your provider is that address followed by `signin-oidc`:
   `https://taskist.example.com/signin-oidc`
3. **Behind a reverse proxy** (nginx, Traefik, IIS, a load balancer), set `Security:ForwardedHeaders:Enabled` and list the proxy, as described in the [README](../README.md#configuration). Otherwise Taskist sees plain `http` and sends the wrong redirect URI, and the provider rejects it.
4. **Create the users first** (or turn on `AutoProvision`), using the same e-mail addresses they have at the provider.

## Settings

All settings live under `Authentication:Oidc`. In environment variables, use a double underscore: `Authentication__Oidc__ClientId`. The Docker sample maps the `SSO_*` variables in `.env` to the most common ones.

| Setting | Default | Description |
| --- | --- | --- |
| `Enabled` | `false` | Turns single sign-on on. Needs `Authority` and `ClientId`; if either is missing or invalid, SSO stays off and a warning is logged at startup. |
| `DisplayName` | `Single sign-on` | Button text: "Sign in with *DisplayName*". |
| `Authority` | — | The provider's issuer address (the part before `/.well-known/openid-configuration`). Must be `https`. |
| `ClientId` / `ClientSecret` | — | The client registered at the provider. The secret is read from configuration only, never stored in the database. |
| `Scopes` | `openid profile email` | Scopes to request. `openid` is always added. Add a groups scope here if your provider needs one. |
| `RequireVerifiedEmail` | `true` | Link to an existing account or create one only when the provider marks the e-mail as verified. See [Entra ID](#microsoft-entra-id) before changing it. |
| `AutoProvision` | `false` | Create a Taskist account on first sign-in, with the *Registered* role only. |
| `RoleClaim` / `RoleMappings` | — | Optional group → role mapping; see [below](#mapping-provider-groups-to-roles). |
| `DisableLocalLogin` | `false` | Hide the password form; see [SSO-only mode](#sso-only-mode). |
| `RequireHttpsMetadata` | `true` | Only set to `false` to test against a provider on plain `http://localhost`. Never in production. |

Example `appsettings.Production.json`:

```json
{
  "App": { "BaseUrl": "https://taskist.example.com/" },
  "Authentication": {
    "Oidc": {
      "Enabled": true,
      "DisplayName": "Acme SSO",
      "Authority": "https://id.example.com/realms/acme",
      "ClientId": "taskist",
      "ClientSecret": "<from your provider>"
    }
  }
}
```

Keep the client secret out of files you commit: use an environment variable (`Authentication__Oidc__ClientSecret`), user-secrets or your hosting platform's secret store.

## Keycloak: step by step

This sets up a throwaway Keycloak on your machine to try SSO in a few minutes. For production, follow the same steps in your real realm over HTTPS.

**1. Start Keycloak (test only):**

```bash
docker run --name keycloak -p 8081:8080 \
  -e KC_BOOTSTRAP_ADMIN_USERNAME=admin -e KC_BOOTSTRAP_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:26.0 start-dev
```

Open http://localhost:8081 and sign in as `admin` / `admin`.

**2. Create a realm** (top-left menu → *Create realm*), for example `acme`.

**3. Create the client:** *Clients → Create client*

- Client type: **OpenID Connect**, Client ID: `taskist` → *Next*
- **Client authentication: On**, Authentication flow: **Standard flow** only → *Next*
- **Valid redirect URIs:** `http://localhost:8080/signin-oidc` (your Taskist address + `signin-oidc`)
- **Web origins:** `http://localhost:8080` → *Save*
- Copy the secret from the **Credentials** tab.

**4. Create a user:** *Users → Create new user* with the same e-mail as a Taskist user, tick **Email verified**, then set a password on the *Credentials* tab.

**5. Configure Taskist.** With the Docker sample, in `.env`:

```bash
SSO_ENABLED=true
SSO_DISPLAY_NAME=Keycloak
SSO_AUTHORITY=http://host.docker.internal:8081/realms/acme
SSO_CLIENT_ID=taskist
SSO_CLIENT_SECRET=<secret from step 3>
```

Because this test Keycloak runs on plain HTTP, also add `Authentication__Oidc__RequireHttpsMetadata: "false"` to the `web` service's `environment` in `docker-compose.yml`. Don't do this in production. Then run `docker compose up -d`.

> The Taskist container and your browser must reach Keycloak at the **same** address, because the provider's address is part of every token. `host.docker.internal` works with Docker Desktop; on Linux, add `extra_hosts: ["host.docker.internal:host-gateway"]` to the `web` service and open Keycloak in your browser at `http://host.docker.internal:8081` too.

**6. Try it:** open Taskist, click **Sign in with Keycloak**, sign in as the user from step 4. You're signed in to the matching Taskist account, and **My profile → Security** shows the linked sign-in.

**Groups (optional):** in the client, *Client scopes → taskist-dedicated → Configure a new mapper → Group Membership*, token claim name `groups`, **Full group path off**. Then set `RoleClaim` to `groups` and add `RoleMappings` (see below).

## Microsoft Entra ID

1. *Entra admin center → App registrations → New registration.* Supported account types: **this organizational directory only** (single tenant). Redirect URI: platform **Web**, `https://taskist.example.com/signin-oidc`.
2. *Certificates & secrets → New client secret*; copy the value.
3. *Token configuration → Add optional claim → ID → `email`*.
4. Configure Taskist:
   - `Authority`: `https://login.microsoftonline.com/<tenant-id>/v2.0`
   - `ClientId`: the application (client) ID; `ClientSecret`: the secret from step 2.

**E-mail verification:** Entra ID does not send the `email_verified` claim, so with the default `RequireVerifiedEmail: true` first-time sign-ins can't be linked to existing accounts. For a **single-tenant** app whose e-mail addresses your organisation controls, set `RequireVerifiedEmail` to `false`. Don't do this for multi-tenant apps or tenants with guest users whose e-mail addresses you don't control: someone could then sign in as a Taskist user whose address they put on their own account.

**Groups:** *Token configuration → Add groups claim*. Entra sends group **object IDs** (GUIDs), so use those as keys in `RoleMappings`, with `RoleClaim` set to `groups`.

## Google

1. *Google Cloud console → APIs & Services → Credentials → Create credentials → OAuth client ID*, type **Web application**, authorised redirect URI `https://taskist.example.com/signin-oidc`.
2. Configure Taskist: `Authority` `https://accounts.google.com`, plus the client ID and secret.

Google sends verified e-mail addresses, so the defaults work. It does not send groups. To limit sign-ins to your company domain, keep `AutoProvision` off and only create Taskist users for your own addresses.

## Other providers

Any provider that supports OpenID Connect discovery works. Use the issuer address as `Authority`:

| Provider | `Authority` |
| --- | --- |
| Authentik | `https://auth.example.com/application/o/<application-slug>/` |
| Okta | `https://<your-org>.okta.com` (or a custom authorisation server) |
| GitLab | `https://gitlab.com` or your self-managed address |
| Auth0 | `https://<tenant>.auth0.com/` |

Register `https://taskist.example.com/signin-oidc` as the redirect URI, and make sure the `email` scope is allowed.

## Mapping provider groups to roles

Taskist can **add** roles based on the groups the provider sends. Set `RoleClaim` to the claim that carries the groups (often `groups`) and list the mappings, group → Taskist role *system name*:

```json
"RoleClaim": "groups",
"RoleMappings": {
  "taskist-leads": "TeamLead",
  "taskist-qa": "QualityAssurance",
  "taskist-pm": "ProjectManager"
}
```

As environment variables: `Authentication__Oidc__RoleClaim=groups`, `Authentication__Oidc__RoleMappings__taskist-leads=TeamLead`, and so on. These work in `docker-compose.yml` and on Windows, but a Linux shell can't `export` a variable whose name contains `-`. If your group names have dashes, put the mappings in `appsettings.Production.json`, or start the process with `env "Authentication__Oidc__RoleMappings__taskist-leads=TeamLead" …`.

- Roles are added at each SSO sign-in and **never removed**. Removing someone from a group at the provider doesn't take the role away in Taskist; an administrator does that on the user's page. If the group still maps to the role, it comes back at the next sign-in, so remove the group membership too.
- Only groups listed in `RoleMappings` have any effect, and group names are matched case-insensitively.
- Mapping a group to `SystemAdministrator` makes membership of that group equivalent to full admin rights. Do it deliberately.
- Roles are global. Project access (which projects someone sees) is still granted per project in Taskist.

## SSO-only mode

With `DisableLocalLogin: true` the login page shows only the SSO button, and password sign-in is refused for everyone **except system administrators**. Administrators can still use their password at:

```
https://taskist.example.com/Login?local=1
```

That's your way back in if the identity provider is down or misconfigured. Keep at least one administrator account with a strong password, and don't turn this mode on until you've tested SSO.

## Troubleshooting

| What you see | Likely cause |
| --- | --- |
| No SSO button | `Enabled` is false, or `Authority`/`ClientId` is missing or invalid; the startup log says which. |
| The provider shows "invalid redirect URI" | The registered redirect URI isn't exactly `<App:BaseUrl>signin-oidc`, or Taskist runs behind a proxy without forwarded headers (it sends `http://…`). |
| "Single sign-on didn't work" | The sign-in failed after the provider: check the Taskist log for "Single sign-on failed". Common causes: wrong client secret, the server clock is off, or the container reaches the provider at a different address than the browser. |
| "You don't have a Taskist account yet" | No Taskist user has that e-mail and `AutoProvision` is off. Create the user, or turn on auto-provisioning. |
| "Your e-mail address isn't verified" | The provider didn't mark the e-mail as verified. Verify it at the provider (Keycloak: *Email verified*), or see [Entra ID](#microsoft-entra-id). |
| "Your identity provider didn't send an e-mail address" | The `email` scope is missing or not allowed for the client. |
| "Already linked to a different account at your identity provider" | The Taskist user is linked to another identity at the same provider (for example, the account was re-created there). An administrator removes the old link on the user's page. |
| "Password sign-in is turned off" | SSO-only mode is on and this user isn't a system administrator. |

Successful SSO sign-ins are recorded in the activity log (**Configuration → Activity log**), including when an account was linked or created and which roles were added. Refused sign-ins are written to the application log as "Single sign-on refused (*reason*)", with the reason codes from the table above (`NotProvisioned`, `UnverifiedEmail`, `MissingEmail`, `LinkConflict`, `Unavailable`).

## Limitations

- One identity provider at a time.
- Signing out of Taskist doesn't sign you out of the provider; the next SSO sign-in may go straight through.
- Accounts created by `AutoProvision` have no mobile number or address. The user's edit page and **My profile** ask for them on the next save.
- SSO users don't need a Taskist password. Without one, they can't unlink their last SSO sign-in, and if SSO is turned off they need a password reset from an administrator.
