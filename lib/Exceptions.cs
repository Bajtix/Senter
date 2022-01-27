namespace Senter.Communication;

public class APIError : Exception {
    public string accessUrl, response;

    public APIError(string message, string accessUrl, string response = "[null response]") : base(message) {
        this.accessUrl = accessUrl;
        this.response = response;
    }
}