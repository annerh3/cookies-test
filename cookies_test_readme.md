# 🍪 Cookies Test - Autenticación Segura

## 📋 Descripción del Proyecto

Este repositorio es una implementación de prueba que demuestra el uso seguro de cookies HTTP-Only para autenticación y autorización en aplicaciones web, como alternativa más segura al almacenamiento de JWT tokens en localStorage.

## 🔐 Problema de Seguridad Identificado

Tradicionalmente, muchas aplicaciones web almacenan JWT tokens en `localStorage` del navegador para mantener la sesión del usuario. Sin embargo, esta práctica presenta una **vulnerabilidad crítica** a ataques XSS (Cross-Site Scripting), ya que cualquier script malicioso puede acceder fácilmente a `localStorage` y robar los tokens de autenticación.

## ✅ Solución Implementada

### Persistant Cookies HTTP-Only
En lugar de usar `localStorage`, esta implementación utiliza **persistant cookies HTTP-Only** para almacenar:
- **Access Token (JWT)**: Para la autenticación del usuario
- **Refresh Token**: Para renovar automáticamente el access token

### Ventajas de esta Aproximación:
- 🛡️ **Protección contra XSS**: Las cookies HTTP-Only no son accesibles desde JavaScript
- 🔄 **Renovación automática**: Sistema de refresh tokens para mantener la sesión
- 🔒 **Mayor seguridad**: Los tokens se envían automáticamente en cada petición HTTP
- ⛔ **Expiración**: Las cookies tienen el mismo tiempo de expiracion que el JWT y Refresh Token, respectivamente.

## 🏗️ Arquitectura del Sistema

### Sistema de Autenticación y Autorización
- **Aplicación**: Lista de contactos con roles de usuario 
- **Roles disponibles**: 
  - `USER`: Usuarios normales
  - `ADMIN`: Administradores 
- **Funcionalidades**: CRUD de contactos con autorización basada en roles

**NOTA**: No se le dió permisos especiales a los `ADMIN`.
## 👥 Usuarios de Prueba

### Usuarios Normales (Rol: USER)
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

### Administradores (Rol: ADMIN)
```
annerh3@gmail.com
admin@gmail.com
```

**Contraseña para todos los usuarios**: `Temporal01*`

## 🎮 Cómo Probar la Aplicación

1. **Accede a la demo**: [https://golden-eclair-48296b.netlify.app](https://golden-eclair-48296b.netlify.app)

2. **Inicia sesión** con cualquiera de los usuarios de prueba:
   - Contraseña: `Temporal01*`

3. **Observa las cookies** en las herramientas de desarrollador:
   - Ve a DevTools → Application → Cookies
   - Notarás que los tokens están almacenados como cookies HTTP-Only
   - Intenta acceder a estas cookies desde la consola - verás que no es posible

4. **Prueba las funcionalidades**:
   - Visualiza contactos.

## 🌐 Demo en Vivo

**Puedes probar la aplicación directamente en**: [https://golden-eclair-48296b.netlify.app](https://golden-eclair-48296b.netlify.app)

No necesitas instalar nada localmente para probar las funcionalidades. Simplemente usa cualquiera de los usuarios de prueba listados más abajo con la contraseña `Temporal01*`.


## 🔧 Tecnologías Utilizadas

- **Backend**: (Especificar: Node.js, Express, etc.)
- **Base de datos**: (Especificar: MySQL, PostgreSQL, etc.)
- **Autenticación**: JWT con cookies HTTP-Only
- **Frontend**: (Especificar: React, Vue, etc.)

## 📡 Endpoints de la API

### Autenticación
- `POST api/auth/login` - Iniciar sesión
- `POST api/auth/refresh-token` - Renovar token
- `GET api/auth/validate` - Validar Autenticación del usuario.

### Contactos
- `GET api/contacts` - Listar contactos


## 🤝 Contribución

Este es un proyecto de prueba y aprendizaje. Las contribuciones son bienvenidas para mejorar la implementación de seguridad.



## ⚠️ Advertencia

Este repositorio es solo para fines educativos y de prueba. No utilizar en producción sin una revisión de seguridad completa.

