namespace Olafs.OAuth2.Models
{
	public class AuthenticateResult<T>
	{
		public int Status { get; set; }

		public string StatusMessage { get; set; }

		public T User { get; set; }
	}
}