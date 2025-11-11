using System.Text;
using System.Text.Json;

namespace NutriAIServicio;
public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaUrl = "http://localhost:11434";

    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_ollamaUrl);
    }

    // --- FUNCIÓN REVERTIDA a 5 parámetros ---
    public async Task<string> GetNutritionResponseAsync(int edad, double peso, double altura, string preferencias, string mensaje)
    {
        var prompt = $"""
            Eres NUTRI-AI, un nutricionista profesional certificado. Responde siempre en español.

            **INSTRUCCIONES DE DESVIACIÓN DE TEMA:**
            - Si la consulta del paciente en el `mensaje` **NO** está relacionada con nutrición, salud, patologías, fitness o sus datos corporales (ej: preguntas sobre política, historia, o ciencia no nutricional), debes responder de forma cortés, reafirmando tu especialidad.
            - **Respuesta estándar de desviación:** "Como NUTRI-AI, mi especialidad es ofrecerte guías nutricionales personalizadas. Por favor, realiza tu consulta enfocada en tu alimentación o salud para que pueda ayudarte."

            ---

            DATOS DEL PACIENTE:
            - Edad: {edad} años
            - Peso: {peso} kg
            - Altura: {altura} cm
            - Preferencias alimentarias: {preferencias}
            - IMC: {CalcularIMC(peso, altura):F1} ({ClasificarIMC(peso, altura)})

            CONSULTA: {mensaje}

            Respuesta:
            """;

        var requestData = new
        {
            model = "llama3-lora",
            prompt = prompt,
            stream = false,
            options = new
            {
                temperature = 0.7,
                num_predict = 600
            }
        };

        try
        {
            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/generate", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseContent);
                return document.RootElement.GetProperty("response").GetString() ?? "No se recibió respuesta";
            }
            else
            {
                return $"Error de Ollama: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            return $"Error de conexión con Ollama: {ex.Message}";
        }
    }

    // --- Se elimina la función CalcularTMB ---

    private double CalcularIMC(double peso, double altura)
    {
        var alturaMetros = altura / 100;
        return peso / (alturaMetros * alturaMetros);
    }

    private string ClasificarIMC(double peso, double altura)
    {
        var imc = CalcularIMC(peso, altura);
        return imc switch
        {
            < 18.5 => "Bajo peso",
            >= 18.5 and < 25 => "Peso normal",
            >= 25 and < 30 => "Sobrepeso",
            _ => "Obesidad"
        };
    }
}