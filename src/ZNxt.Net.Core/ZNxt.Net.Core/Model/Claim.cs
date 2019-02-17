namespace ZNxt.Net.Core.Model
{
    public class Claim
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public Claim(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
