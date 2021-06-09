using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;

namespace IronWeb.Data
{
    /// <summary>
    /// For Index EditDialog Validation
    /// </summary>
    public class DynamicFormValidation : ValidationAttribute
    {
        private readonly string _parentFiledName;
        private readonly string _filedType;
        private readonly string[] _validationTypes;

        public DynamicFormValidation(string parentFildName, string filedType, string[] validationTypes)
        {
            _parentFiledName = parentFildName;
            _filedType = filedType;
            _validationTypes = validationTypes;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance != null)
            {
                var parentFieldValueObject = validationContext.ObjectInstance.GetType()
                    .GetProperty(_parentFiledName).GetValue(validationContext.ObjectInstance, null);

                string currentFieldValue = value != null ? value as string : string.Empty;

                string parentFieldValue = parentFieldValueObject != null ? parentFieldValueObject as string : string.Empty;

                if(!string.IsNullOrEmpty(parentFieldValue) && parentFieldValue.ToLower() == _filedType.ToLower())
                {
                    if (string.IsNullOrEmpty(currentFieldValue) && _validationTypes.Any(_ => _.ToLower() == "required"))
                    {
                        return new ValidationResult($"{validationContext.DisplayName} is required", new[] { validationContext.MemberName });
                    }
                    else if (_validationTypes.Any(_ => _.ToLower() == "tempServerAddr"))
                    {
                        IPAddress iPAddress;

                        if (!IPAddress.TryParse(currentFieldValue.ToString(), out iPAddress))
                        {
                            return new ValidationResult($"Please Match IP Address Format.");
                        }
                    }
                    else if (_validationTypes.Any(_ => _.ToLower() == "tempClientAddr"))
                    {
                        IPAddress iPAddress;

                        if (!IPAddress.TryParse(currentFieldValue.ToString(), out iPAddress))
                        {
                            return new ValidationResult($"Please Match IP Address Format.");
                        }
                    }
                    else if (_validationTypes.Any(_ => _.ToLower() == "tempPortNum"))
                    {
                        if (Convert.ToInt32(currentFieldValue) < 0 || Convert.ToInt32(currentFieldValue) < 65536)
                        {
                            return new ValidationResult($"Please Match IP Address Format.");
                        }
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}
