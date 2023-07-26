using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Text;

public class Program
{
    const string baseUrl = "https://jsonmock.hackerrank.com/api/football_matches";
    public static async Task Main()
    {
        string teamName = "Paris Saint-Germain";
        int year = 2013;
        int totalGoals = await GetTotalScoredGoals(teamName, year);

        Console.WriteLine("Team "+ teamName +" scored "+ totalGoals.ToString() + " goals in "+ year);

        teamName = "Chelsea";
        year = 2014;
        totalGoals = await GetTotalScoredGoals(teamName, year);

        Console.WriteLine("Team " + teamName + " scored " + totalGoals.ToString() + " goals in " + year);

        // Output expected:
        // Team Paris Saint - Germain scored 109 goals in 2013
        // Team Chelsea scored 92 goals in 2014

    }

    public static async Task<int> GetTotalScoredGoals(string team, int year)
    {
        var totalScoredGoals = 0;

        //Total de paginas de jogos em casa
        var totalHomePage = await GetAllTotalPage(team, year, true);
        //Quantidade de gols em casa
        for (int pageHome = 1; pageHome <= totalHomePage; pageHome++)
            totalScoredGoals += await GetAllTotalGoal(team, year, pageHome, true);


        //Total de paginas de jogos fora
        var totalAwayPage = await GetAllTotalPage(team, year, false);
        //Quantidade de gols fora
        for (int pageAway = 1; pageAway <= totalAwayPage; pageAway++)
            totalScoredGoals += await GetAllTotalGoal(team, year, pageAway, false);

        return totalScoredGoals;
    }

    private static async Task<int> GetAllTotalPage(string team, int year, bool isHome)
    {
        string apiUrl =  isHome ? $"{baseUrl}?year={year}&team1={team}" : $"{baseUrl}?year={year}&team2={team}";

        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<FootballMatchesResponse>(content);

                    return result?.Total_Pages ?? 0;
                }
                else
                {
                    return 0;
                }

            }
            catch (HttpRequestException)
            {
                return 0;
            }
        }
        
    }

    private static async Task<int> GetAllTotalGoal(string team, int year, int page, bool isHome)
    {
        var totalGoal = 0;
        string apiUrl = isHome ? $"{baseUrl}?year={year}&team1={team}&page={page}" : $"{baseUrl}?year={year}&team2={team}&page={page}";

        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<FootballMatchesResponse>(content);

                    if (result != null && result.Data?.Count > 0)
                    {
                        foreach (var data in result.Data)
                            totalGoal += isHome ? data.Team1Goals : data.Team2Goals;
                    }
                }
                else
                {
                    Console.WriteLine($"Erro na requisição: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Não foi possivel carregar os scoreds da página: {page} do team: {team}. Erro na requisição: {ex.Message}");
            }

            return totalGoal;
        }
    }
}


class FootballMatchesResponse
{
    public int Page { get; set; }
    public int Per_Page { get; set; }
    public int Total { get; set; }
    public int Total_Pages { get; set; }
    public List<DataResponse>? Data { get; set; }

}

class DataResponse
{
    public string? Competition { get; set; }
    public string? Year { get; set; }
    public string? Round { get; set; }
    public string? Team1 { get; set; }
    public string? Team2 { get; set; }
    public int Team1Goals { get; set; }
    public int Team2Goals { get; set; }
}