# 🛒 E-Commerce App - ASP.NET Core

Este proyecto es una aplicación web de comercio electrónico desarrollada con **ASP.NET Core** usando **Razor Pages** y **Entity Framework Core**. Está orientado a cubrir funcionalidades esenciales de una tienda en línea, incluyendo manejo de productos, autenticación de usuarios, compras y administración.

## 🚀 Características principales

- Registro e inicio de sesión de usuarios con roles
- Gestión de productos (CRUD) con imágenes
- Filtrado por categorías
- Carrito de compras con validación de stock
- Proceso de checkout y creación de órdenes
- Historial de compras por usuario
- Panel de administración con métricas y visualización (Chart.js)
- Seguridad con control de acceso basado en roles

## 🧱 Tecnologías usadas

- ASP.NET Core 7.0
- Razor Pages + Identity
- Entity Framework Core + SQL Server
- Bootstrap 5 para el diseño responsivo
- Chart.js para gráficas en el dashboard

## 🛠️ Instalación y configuración

1. Clonar el repositorio:
git clone https://github.com/lucasncjs/MiniEcommerce
3. Crear la base de datos en SQL Server y actualizar el `appsettings.json` con tu cadena de conexión.
4. Ejecutar las migraciones:
dotnet ef database update
5. Ejecutar la aplicación:
dotnet run
6. Acceder desde tu navegador a `https://localhost:5001`

## 📁 Estructura del proyecto

- `Pages/` - Vistas Razor (Inicio, Productos, Categorías, Carrito, Checkout)
- `Data/` - Contexto de base de datos y migraciones
- `Models/` - Modelos de entidad
- `Services/` - Lógica de negocio (stock, carrito, órdenes)
- `wwwroot/` - Archivos estáticos (CSS, JS, imágenes)

## 📝 Estado del proyecto

Actualmente en desarrollo activo. Próximas mejoras:
- Confirmación de pago simulada
- Mejora en la experiencia de usuario móvil
- Validaciones dinámicas con JavaScript

---

Desarrollado por **Lucas Nahuel Fernández**  
Contacto: [lucasnfer1@gmail.com]
