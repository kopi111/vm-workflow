# ICTD Workflow - Installation Guide

## Prerequisites

| Component        | Version         | Notes                              |
|------------------|-----------------|------------------------------------|
| .NET Runtime     | 8.0+            | ASP.NET Core Runtime required      |
| MySQL / MariaDB  | 8.0+ / 10.5+    | Database server                    |
| IIS (Windows)    | 10+             | Or run as standalone Kestrel       |
| Active Directory | Windows Server  | Optional - for AD login            |

---

## 1. Database Setup

### Option A: Run the SQL script (recommended for production)

```bash
mysql -u root -p < docs/install-database.sql
```

This script is **safe to re-run** - it uses `CREATE TABLE IF NOT EXISTS` and `INSERT IGNORE` so existing data is never overwritten.

### Option B: Let the application create the database

The app calls `EnsureCreated()` on startup and seeds default data automatically. Just make sure the MySQL user has `CREATE` privileges.

### Update an existing database

Run the same script again:

```bash
mysql -u root -p < docs/install-database.sql
```

New tables will be created. Existing tables are untouched.

---

## 2. Application Configuration

Edit `appsettings.json` (located next to `VMWorkflow.API.dll`):

### Connection String

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_DB_HOST;Port=3306;Database=VMWorkflow;Uid=vmworkflow;Pwd=YOUR_PASSWORD;"
}
```

### JWT Settings

Generate a strong secret key (minimum 32 characters):

```json
"Jwt": {
    "Key": "CHANGE-THIS-TO-A-STRONG-SECRET-KEY-AT-LEAST-32-CHARS",
    "Issuer": "VMWorkflow.API",
    "Audience": "VMWorkflow.Web",
    "ExpireMinutes": 480
}
```

Or set the `JWT_SECRET_KEY` environment variable (takes priority over config file).

### Logging

```json
"Logging": {
    "LogPath": "C:\\Logs\\VMWorkflow"
}
```

Create this folder and ensure the app pool user has write access.

---

## 3. Active Directory Configuration

Set `Enabled` to `true` to authenticate users against AD instead of local passwords.

```json
"ActiveDirectory": {
    "Enabled": true,
    "Domain": "yourdomain.local",
    "LdapServer": "ldap://dc01.yourdomain.local",
    "LdapPort": 389,
    "SearchBase": "DC=yourdomain,DC=local",
    "UseSsl": false,
    "RoleMapping": {
        "VM-PlatformAdmins": "PlatformAdmin",
        "VM-SysAdmins": "SysAdmin",
        "VM-Requesters": "Requester",
        "VM-DataCenter": "DataCenter",
        "VM-NOC": "NOC",
        "VM-SOC": "SOC",
        "VM-IOCManagers": "IOCManager",
        "VM-CISO": "CISO",
        "VM-Ops": "Ops"
    },
    "DefaultRole": "Requester"
}
```

### What to fill in

| Setting        | Description                                     | Example                              |
|----------------|-------------------------------------------------|--------------------------------------|
| `Domain`       | Your AD domain name                             | `ictd.local`                         |
| `LdapServer`   | Domain controller hostname or IP                | `ldap://10.0.0.5`                    |
| `LdapPort`     | 389 (LDAP) or 636 (LDAPS)                      | `389`                                |
| `SearchBase`   | Base DN to search for users                     | `DC=ictd,DC=local`                   |
| `UseSsl`       | Use LDAPS (port 636) - recommended for prod     | `false`                              |
| `RoleMapping`  | Map AD group names (CN) to app roles            | See table below                      |
| `DefaultRole`  | Role assigned if user matches no AD group       | `Requester`                          |

### AD Group to App Role Mapping

Change the left side (AD group names) to match your actual AD groups:

| AD Group (CN)      | App Role        | Access                                   |
|--------------------|-----------------|------------------------------------------|
| VM-PlatformAdmins  | PlatformAdmin   | Full admin, user management              |
| VM-SysAdmins       | SysAdmin        | System admin details submission          |
| VM-Requesters      | Requester       | Create and track VM requests             |
| VM-DataCenter      | DataCenter      | Data center details submission           |
| VM-NOC             | NOC             | Network operations details               |
| VM-SOC             | SOC             | Security/firewall policy details         |
| VM-IOCManagers     | IOCManager      | IOC approval workflow                    |
| VM-CISO            | CISO            | CISO approval workflow                   |
| VM-Ops             | Ops             | Operations manager approval              |

### How AD Login Works

1. User enters their AD username and password on the login page
2. App binds to LDAP with those credentials to verify the password
3. On success, it queries AD for display name, email, and group memberships
4. First login: user is auto-created in the local database with the mapped role
5. Subsequent logins: display name and email are synced from AD
6. A JWT token is issued - the Blazor frontend works the same as before

### Disable AD (use local passwords)

```json
"ActiveDirectory": {
    "Enabled": false
}
```

When disabled, users authenticate with locally stored BCrypt-hashed passwords.

---

## 4. IIS Deployment (Windows)

### Publish the application

```bash
dotnet publish src/VMWorkflow.API/VMWorkflow.API.csproj -c Release -o C:\inetpub\VMWorkflow
```

### IIS Setup

1. Install the **ASP.NET Core Hosting Bundle** (download from Microsoft)
2. Create a new IIS site pointing to `C:\inetpub\VMWorkflow`
3. Set the Application Pool to **No Managed Code**
4. Ensure the App Pool identity has:
   - Read access to the application folder
   - Write access to the log folder (`C:\Logs\VMWorkflow`)
5. Configure the site binding (HTTP/HTTPS, hostname, port)

### CORS

If the Web UI is hosted on a different URL than the API, add the origins:

```json
"Cors": {
    "AllowedOrigins": [
        "https://vmworkflow.yourdomain.local"
    ]
}
```

---

## 5. Standalone Deployment (Kestrel)

```bash
dotnet VMWorkflow.API.dll --urls "http://0.0.0.0:5000"
```

Or run as a Windows Service / Linux systemd service.

---

## 6. Verification Checklist

- [ ] MySQL is running and accessible
- [ ] Database `VMWorkflow` exists with all tables
- [ ] `appsettings.json` has correct connection string
- [ ] JWT key is changed from the default
- [ ] Log folder exists and is writable
- [ ] (If AD) LDAP server is reachable from app server
- [ ] (If AD) AD group names in RoleMapping match actual groups
- [ ] Application starts without errors
- [ ] Login page loads at the configured URL
- [ ] A user can log in successfully

---

## 7. Default Accounts (Local Auth Only)

When `ActiveDirectory.Enabled` is `false`, the app seeds these accounts on first run:

| Username    | Password      | Role          |
|-------------|---------------|---------------|
| admin       | Password@123  | PlatformAdmin |
| sysadmin    | Password@123  | SysAdmin      |
| requester   | Password@123  | Requester     |
| datacenter  | Password@123  | DataCenter    |
| noc         | Password@123  | NOC           |
| soc         | Password@123  | SOC           |
| iocmanager  | Password@123  | IOCManager    |
| ciso        | Password@123  | CISO          |
| ops         | Password@123  | Ops           |
| developer   | Password@123  | Requester     |

**Change these passwords after first login.**

---

## 8. Troubleshooting

| Issue                          | Solution                                                    |
|--------------------------------|-------------------------------------------------------------|
| LDAP connection refused        | Check `LdapServer` hostname/IP and `LdapPort`, verify firewall allows traffic |
| "Invalid username or password" | Verify the user exists in AD and the `Domain` is correct    |
| User logs in but has wrong role| Check AD group membership and `RoleMapping` in config       |
| DB connection error            | Verify MySQL is running, credentials are correct, and the database exists |
| 500 error on startup           | Check logs in the configured `LogPath` folder               |
| Tables missing                 | Re-run `install-database.sql` - it only adds what's missing |
