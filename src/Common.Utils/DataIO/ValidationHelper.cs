using System.ComponentModel.DataAnnotations;

namespace Common.Utils
{
	public static class ValidationHelper
	{
		public static bool Validate<T>(T obj, out ICollection<ValidationResult> results)
		{
			if (obj == null)
			{
				results = new List<ValidationResult>();
				return false;
			}
			results = new List<ValidationResult>();
			return Validator.TryValidateObject(obj, new ValidationContext(obj), results, true);
		}
	}
}
