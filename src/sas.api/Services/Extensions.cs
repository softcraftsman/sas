namespace sas.api.Services
{
	internal class Extensions
	{
		public static bool AnyNull(params object[] args)
		{
			// TODO: Use lambda?
			foreach (var arg in args)
				if (arg is null) return true;
			return false;
		}
	}

	public class Result
	{
		public bool Success { get; set; }
		public string Message { get; set; }
	}
}
