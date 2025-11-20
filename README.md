# CalculadoraX

Aplicación WPF para calcular órdenes de compra, boletas de honorarios y conversiones de moneda.

## Requisitos

- Windows 10/11
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) con el componente **.NET Desktop Development** (incluye Windows Desktop Runtime/WPF)

## Ejecución local para desarrollo

1. Clonar el repositorio.
2. Abrir una terminal de **Developer PowerShell** o **Command Prompt** en Windows dentro de la carpeta del proyecto.
3. Restaurar dependencias y compilar:
   ```bash
   dotnet restore
   dotnet build
   ```
4. Ejecutar desde la CLI:
   ```bash
   dotnet run --project CalculadoraX/CalculadoraX.csproj
   ```
   o abrir `CalculadoraX.sln` en Visual Studio y presionar **F5**.

## Empaquetado

Cuando la app esté lista para distribución, se recomienda crear un instalador MSIX o ClickOnce desde Visual Studio para asegurar la instalación en PCs Windows.
