using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Olafs.OAuth2.Models;

namespace Olafs.OAuth2
{
    // ReSharper disable once UnusedMember.Global
    public class Authenticate<T>
	{

		private readonly string _oauthClientId;

		private readonly bool _oauthDisableSslValidationCheck;

		private readonly string _oauthSecret;

		private readonly string _oauthServerUrl;
	    private bool _debugEnabled = false;
	    private string _debugOutput;
	    private T AccessToken { get; set; }

		private string ErrorMessage { get; set; }

		private OauthRequestToken RequestToken { get; set; }

		public Authenticate(string oauthClientId, bool oauthDisableSslValidationCheck, string oauthSecret, string oauthServerUrl)
		{
			_oauthClientId = oauthClientId;
			_oauthDisableSslValidationCheck = oauthDisableSslValidationCheck;
			_oauthSecret = oauthSecret;
			_oauthServerUrl = oauthServerUrl;
		}

	    // ReSharper disable once UnusedMember.Global
	    public AuthenticateResult<T> Execute(string code)
		{
			DisableSslValidationCheck();
			if (!GetRequestToken(code))
			{
				return GetErrorResult();
			}

			if (!GetAccessToken())
			{
				return GetErrorResult();
			}


			return GetSuccessResult();
		}

	    // ReSharper disable once UnusedMember.Global
	    public bool Logout()
		{
			using (var client = GetHttpClient())
			{
				DisableSslValidationCheck();
				try
				{
					const string path = "/oauth/logout";
					var response = client.GetAsync(path).Result;
					var result = response.Content.ReadAsStringAsync().Result;
					response.EnsureSuccessStatusCode();
					return true;
				}
				catch (Exception ex)
				{
					ErrorMessage = ex.Message;
					return false;
				}
			}
		}

		private void DisableSslValidationCheck()
		{
			if (!_oauthDisableSslValidationCheck)
			{
				return;
			}

			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
		}

		private bool GetAccessToken()
		{
			var result = string.Empty;

			using (var client = GetHttpClient())
			{
				try
				{
				    if (_debugEnabled)
				    {
				        File.WriteAllText(_debugOutput, $"{RequestToken.access_token}\n");
				    }

				    var path = $"request_access?access_token={RequestToken.access_token}";
					var response = client.GetAsync(path).Result;
					result = response.Content.ReadAsStringAsync().Result;
					response.EnsureSuccessStatusCode();
					AccessToken = JsonConvert.DeserializeObject<T>(result);
					return true;
				}
				catch
				{
					ErrorMessage = result;
					return false;
				}
			}
		}

		private AuthenticateResult<T> GetErrorResult()
		{
			return new AuthenticateResult<T> { StatusMessage = ErrorMessage, Status = -1 };
		}

		private HttpClient GetHttpClient()
		{
			var client = new HttpClient { BaseAddress = new Uri(_oauthServerUrl) };
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			return client;
		}
		
		private bool GetRequestToken(string code)
		{
			var result = string.Empty;

			using (var client = GetHttpClient())
			{
				try
				{
					var path =
						$"request_token?code={code}&grant_type=authorization_code&client_id={_oauthClientId}&client_secret={_oauthSecret}";
					var response = client.GetAsync(path).Result;
					result = response.Content.ReadAsStringAsync().Result;
					response.EnsureSuccessStatusCode();
					RequestToken = JsonConvert.DeserializeObject<OauthRequestToken>(result);
					return true;
				}
				catch
				{
					ErrorMessage = result;
					return false;
				}
			}
		}

		private AuthenticateResult<T> GetSuccessResult()
		{
			var result = new AuthenticateResult<T>
			{
				StatusMessage = "Success",
				Status = 0,
				User = AccessToken
			};
			return result;
		}
	}
}
