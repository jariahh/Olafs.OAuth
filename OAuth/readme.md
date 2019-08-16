### Olaf's OAuth2.0 plugin

This is a very simple implimentation of OAuth2 that works with v2 of other versions are untested [https://github.com/justingreerbbi/wordpress-oauth-server](https://github.com/justingreerbbi/wordpress-oauth-server)

#### Install the nuget package   
`Install-Package Olafs.OAuth -Version 1.0.0`

#### Example
````C# 
public ActionResult Index(string code = "")
{
    var oAuthUri = ConfigurationManager.AppSettings["OauthServerUrl"];
    var oAuthKey = ConfigurationManager.AppSettings["OauthKey"];
    var oAuthSecret ConfigurationManager.AppSettings["OauthSecret"];
    var disableSSLValidation = false;
    if (string.IsNullOrEmpty(code) && !CookieHelper.IsAuthenticated())
    {
        return Redirect($"{oAuthUri}/authorize?client_id={oAuthKey}&state=none&response_type=code");
    }

    if (!string.IsNullOrEmpty(code))
    {
        var results = new Olafs.OAuth2.Authenticate<OauthAccessToken>(oAuthKey, disableSSLValidation, oAuthSecret, oAuthUri).Execute(code);
        Session["results"] = results;
    }
    return Redirect("~/");
}
````