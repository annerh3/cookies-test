# 🍪 Cookies Test - Secure Authentication

## 🌐 Available Languages
- [Español](README.es.md)
- [English](README.md) ← You are here

> [!IMPORTANT]  
> The servers for the application are currently unavailable. It'll be restored soon.

## 📋 Project Description

This repository is a test implementation demonstrating the secure use of HTTP-Only cookies for authentication and authorization in web applications, as a safer alternative to storing JWT tokens in localStorage.

## 🔐 Security Problem Identified

Traditionally, many web applications store JWT tokens in the browser's `localStorage` to maintain user sessions. However, this practice presents a **critical vulnerability** to XSS (Cross-Site Scripting) attacks, as any malicious script can easily access `localStorage` and steal authentication tokens.

## ✅ Implemented Solution

### Persistent HTTP-Only Cookies
Instead of using `localStorage`, this implementation uses **persistent HTTP-Only cookies** to store:
- **Access Token (JWT)**: For user authentication
- **Refresh Token**: To automatically renew the access token

### Advantages of this Approach:
- 🛡️ **XSS Protection**: HTTP-Only cookies are not accessible from JavaScript
- 🔄 **Automatic renewal**: Refresh token system to maintain sessions
- 🔒 **Enhanced security**: Tokens are sent automatically with each HTTP request
- ⛔ **Expiration**: Cookies have the same expiration time as the JWT and Refresh Token, respectively.

## 🏗️ System Architecture

### Authentication and Authorization System
- **Application**: Contact list with user roles
- **Available roles**:
  - `USER`: Regular users
  - `ADMIN`: Administrators
- **Features**: Contact CRUD with role-based authorization

**NOTE**: No special permissions were given to `ADMIN` users.

## 🌐 Live Demo

**You can test the application directly at**: [https://golden-eclair-48296b.netlify.app](https://golden-eclair-48296b.netlify.app)

No need to install anything locally to test the features. Simply use any of the test users listed below with the password `Temporal01*`.

## 👥 Test Users

### Regular Users (Role: USER)
```
naara.chavez@unah.hn
pilarh_hn@gmail.com
e.cat_src@gmail.com
menonita_src@gmail.com
vision_fund@gmail.com
src_muni@gmail.com
siscomp.hn@gmail.com
gerencia@aguasdesantarosa.org
aire.frio@gmail.com
m_lopez@gmail.com
ruthquintanilla3@icloud.com
s_hqz2@gmail.com
```

### Administrators (Role: ADMIN)
```
annerh3@gmail.com
admin@gmail.com
```

**Password for all users**: `Temporal01*`

## 🎮 How to Test the Application

1. **Access the demo**: [https://golden-eclair-48296b.netlify.app](https://golden-eclair-48296b.netlify.app)

2. **Log in** with any of the test users:
   - Password: `Temporal01*`

3. **Observe the cookies** in developer tools:
   - Go to DevTools → Application → Cookies
   - You'll notice tokens are stored as HTTP-Only cookies
   - Try accessing these cookies from the console - you'll see it's not possible

4. **Test the features**:
   - View contacts.

## 🔧 Technologies Used

- **Backend**: .NET
- **Database**: SQL Server
- **Authentication**: JWT with HTTP-Only cookies
- **Frontend**: React JS

## 📡 API Endpoints

### Authentication
- `POST api/auth/login` - Log in
- `POST api/auth/refresh-token` - Refresh token
- `GET api/auth/validate` - Validate user authentication.

### Contacts
- `GET api/contacts` - List contacts

## 🤝 Contributing

This is a test and learning project. Contributions are welcome to improve the security implementation.

## ⚠️ Warning

This repository is for educational and testing purposes only. Do not use in production without a complete security review.
