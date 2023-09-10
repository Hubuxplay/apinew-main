using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

public class PokemonInfo
{
    public string Name { get; set; }
    public float Weight { get; set; }
    public float Height { get; set; }
    public int PokedexIndex { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://pokeapi.co/api/v2/pokemon?limit=100000&offset=0")
        };

        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JsonConvert.DeserializeObject<JObject>(body);
            JArray resultsArray = (JArray)jsonObject["results"];
            Random random = new Random();
            string selectedPokemonUrl = ((string)resultsArray[random.Next(resultsArray.Count)]["url"]).ToLower();

            var selectedPokemonInfo = await GetPokemonInfo(selectedPokemonUrl);

            string[] hangmanArt = {
                "  _______  \n |       | ",
                "  _______  \n |       | \n |       O ",
                "  _______  \n |       | \n |       O \n |      /|\\ ",
                "  _______  \n |       | \n |       O \n |      /|\\ \n |      /  ",
                "  _______  \n |       | \n |       O \n |      /|\\ \n |      / \\ ",
            };

            int lives = 5;
            char[] guessedLetters = new char[selectedPokemonInfo.Name.Length];

            Console.Clear();
            Console.WriteLine("Ahorcado");
            Console.WriteLine($"Vidas restantes: {lives}");

            while (lives > 0)
            {
                Console.WriteLine();
                Console.WriteLine(hangmanArt[5 - lives]);

                for (int i = 0; i < selectedPokemonInfo.Name.Length; i++)
                {
                    if (guessedLetters[i] != '\0')
                    {
                        Console.Write(guessedLetters[i]);
                    }
                    else
                    {
                        Console.Write('_');
                    }

                    Console.Write(" ");
                }

                Console.WriteLine();
                Console.WriteLine("Ingresa una letra: ");
                char guess = Char.ToLower(Console.ReadKey().KeyChar);

                if (selectedPokemonInfo.Name.Contains(guess))
                {
                    for (int i = 0; i < selectedPokemonInfo.Name.Length; i++)
                    {
                        if (selectedPokemonInfo.Name[i] == guess)
                        {
                            guessedLetters[i] = guess;
                        }
                    }
                }
                else
                {
                    lives--;
                }

                Console.Clear();
                Console.WriteLine("Ahorcado");
                Console.WriteLine($"Vidas restantes: {lives}");

                if (lives > 0)
                {
                    Console.WriteLine(hangmanArt[5 - lives]);
                }

                if (string.Join("", guessedLetters) == selectedPokemonInfo.Name)
                {
                    Console.Clear();
                    Console.WriteLine($"¡Felicidades! Has adivinado la palabra: {selectedPokemonInfo.Name}");
                    Console.WriteLine($"Peso: {selectedPokemonInfo.Weight} kg");
                    Console.WriteLine($"Altura: {selectedPokemonInfo.Height} m");
                    Console.WriteLine($"Índice del Pokédex: {selectedPokemonInfo.PokedexIndex}");
                    break;
                }
            }

            if (lives == 0)
            {
                Console.Clear();
                Console.WriteLine("¡Perdiste! La palabra correcta era: " + selectedPokemonInfo.Name);
                Console.WriteLine($"Peso: {selectedPokemonInfo.Weight} kg");
                Console.WriteLine($"Altura: {selectedPokemonInfo.Height} m");
                Console.WriteLine($"Índice del Pokédex: {selectedPokemonInfo.PokedexIndex}");
            }
        }
    }

    static async Task<PokemonInfo> GetPokemonInfo(string url)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();
            var pokemonData = JsonConvert.DeserializeObject<JObject>(body);

            var pokemonInfo = new PokemonInfo
            {
                Name = (string)pokemonData["name"],
                Weight = pokemonData["weight"]?.Value<float>() ?? 0.0f,
                Height = pokemonData["height"]?.Value<float>() ?? 0.0f,
                PokedexIndex = pokemonData["id"]?.Value<int>() ?? 0
            };

            return pokemonInfo;
        }
    }
}
