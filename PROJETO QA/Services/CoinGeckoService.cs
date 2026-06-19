using System.Text.Json;
using System.Text.Json.Serialization;

namespace PROJETO_QA.Services
{
    internal class CoinGeckoService
    {
        private const string CoinGeckoUrl = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=brl";

        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(10),
        };

        public async Task<double> ObterPrecoBitcoinAsync()
        {
            try
            {
                if (!httpClient.DefaultRequestHeaders.UserAgent.Any())
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "AvaliacaoQA/1.0 (Windows Forms; contato: netoolivio34@gmail.com)"
                    ); // essa parte mostra para a api que eu sou um usuário e caso eu abuse ela tem um contato para chegar até mim
                }

                var respostaHttp = await httpClient.GetAsync(CoinGeckoUrl);

                if (!respostaHttp.IsSuccessStatusCode)
                {
                    throw new Exception(
                    $"Erro ao consultar API. Código HTTP: {(int)respostaHttp.StatusCode} - {respostaHttp.StatusCode}"
                    );
                }

                string json = await respostaHttp.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new Exception("A API retornou uma resposta vazia.");
                }

                var respostaBitcoin = JsonSerializer.Deserialize<RespostaBitcoin>(json);

                if (respostaBitcoin == null)
                {
                    throw new Exception("A API retornou uma resposta em formato inválido.");
                }

                if (respostaBitcoin.Bitcoin == null)
                {
                    throw new Exception("A resposta da API não contém a cotação do Bitcoin.");
                }

                return respostaBitcoin.Bitcoin.Brl;
            }

            catch (HttpRequestException)
            {
                throw new Exception("Falha na comunicação com a API. Verifique sua conexão com a internet ou tente novamente mais tarde.");
            }
            catch (TaskCanceledException)
            {
                throw new Exception("A API demorou mais do que o esperado para responder. Aguarde alguns instantes e tente novamente.");
            }
            catch (JsonException)
            {
                throw new Exception("A API retornou um JSON inválido.");
            }
        }

        private class RespostaBitcoin
        {
            [JsonPropertyName("bitcoin")]
            public Moeda? Bitcoin { get; set; }
        }

        private class Moeda
        {
            [JsonPropertyName("brl")]
            public double Brl { get; set; }
        }
    }
}
