using Newtonsoft.Json;

public class BranchParameterDto
{
    #region Properties

    [JsonProperty("_id")]
    public string ID { get; set; }
    [JsonProperty("number")]
    public int Number { get; set; }
    [JsonProperty("adminCap")]
    public int AdminCap { get; set; }
    [JsonProperty("cost")]
    public double Cost { get; set; }
    [JsonProperty("time")]
    public double Time { get; set; }
    #endregion //Properties
}
