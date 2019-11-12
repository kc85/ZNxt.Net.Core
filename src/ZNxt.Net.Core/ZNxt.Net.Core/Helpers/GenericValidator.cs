using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ZNxt.Net.Core.Helpers
{
    public static class GenericValidator
    {
        public static bool TryValidate(object obj, out ICollection<ValidationResult> results)
        {
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(
                obj, context, results,
                validateAllProperties: true
            );
        }

        public static bool IsValidModel(this object obj, out Dictionary<string,string> results)
        {
            results = new Dictionary<string, string>();
            ICollection<ValidationResult> valresults = null;
            var result = TryValidate(obj, out valresults);
            foreach (var error in valresults)
            {
                foreach (var item in error.MemberNames)
                {
                    results[item] = error.ErrorMessage;
                }
            }
            return result;
        }
        public static bool IsValidModel(this object obj)
        {
            ICollection<ValidationResult> valresults = null;
            return TryValidate(obj, out valresults);
        }

    }
}
