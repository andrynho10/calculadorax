# CalculadoraX – Arquitectura

## Capas

- **Presentación (WPF)**: `MainWindow` aloja tres pestañas (`OrderPurchaseView`, `HonorariumView`, `CurrencyConverterView`). Cada vista se vincula a su `ViewModel` correspondiente mediante `DataContext`.
- **ViewModels**: implementan `INotifyPropertyChanged`, encapsulan estado/formato amigable y orquestan servicios. `MainViewModel` mantiene instancias compartidas y expone comandos globales (actualizar divisas).
- **Servicios**:
  - `OrderCalculationService` (IVA 19 %) y `HonorariumCalculationService` (retención 14,5 %) realizan el cálculo matemático independiente de la vista.
  - `MindicadorCurrencyService` obtiene CLP/UF/USD/EUR desde la API pública que replica indicadores del Banco Central y cachea respuestas locales para modo offline.
- **Modelos**: registros inmutables que representan los resultados (`OrderCalculationResult`, `HonorariumCalculationResult`, `CurrencyQuote`, `CurrencyConversionResult`).
- **Infraestructura**: conversores WPF, helpers de formato, y `LocalCurrencyCache` que serializa el último set de indicadores en disco.

## Flujo

1. La vista produce un comando (por ejemplo `CalcularOrden`).  
2. El `ViewModel` valida/normaliza la entrada (solo números positivos).  
3. Invoca el servicio adecuado y traduce la respuesta a propiedades amigables (`MontoBrutoTexto`, etc.).  
4. Para divisas: `MindicadorCurrencyService` revisa cache → si está fresco (<1 h) usa cache; si no, llama al endpoint HTTPS, actualiza cache y notifica hora de actualización.

## Manejo de errores

- Validaciones básicas en ViewModels con mensajes neutros.  
- Servicios lanzan `InvalidOperationException` cuando la entrada no tiene sentido (ej.: tasa <= 0).  
- El tab de divisas expone estados `IsBusy`, `HasError`, `ErrorMessage` para mostrar banners discretos.

Este documento guía la implementación sin atarnos aún a detalles de estilo, y sirve como referencia rápida para futuras extensiones (ej. nuevas tasas o más monedas).
