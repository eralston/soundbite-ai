using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Masticore.Security
{
    /// <summary>
    /// Base class for exposing claims information.
    /// </summary>
    public abstract class ClaimsBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the claims information associated with this instance.
        /// </summary>
        protected IEnumerable<Claim> Claims { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="ClaimsBase"/> instance.
        /// </summary>
        /// <param name="claims">Claims information.</param>
        public ClaimsBase(IEnumerable<Claim> claims)
        {
            Claims = claims;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves a claim value from the available claims in the <see cref="Claims"/> property.
        /// </summary>
        /// <param name="key">Claim key to locate.</param>
        /// <param name="isRequired">Flag indicating that if the claim is not found an exception should be thrown.</param>
        /// <param name="validator">Validator used to validate the value.  Validation is only performed when a claim is found.</param>
        /// <returns>a string containing the claim value</returns>
        protected string GetClaim(string key, bool isRequired, Func<string, bool> validator)
        {
            return GetClaim(new[] { key }, isRequired, validator);
        }

        /// <summary>
        /// Retrieves a claim value from the available claims in the <see cref="Claims"/> property.
        /// </summary>
        /// <param name="keys">Claim keys to locate in order of preference.</param>
        /// <param name="isRequired">Flag indicating that if the claim is not found an exception should be thrown.</param>
        /// <param name="validator">Validator used to validate the value.  Validation is only performed when a claim is found.</param>
        /// <returns>a string containing the claim value</returns>
        protected string GetClaim(string[] keys, bool isRequired, Func<string, bool> validator)
        {
            // Locate any claims that have a matching key
            IList<Claim> claims = Claims?.Where(i => keys.Contains(i.Type)).ToList();

            // Determine whether any claims were found
            if (claims == null || claims.Count == 0)
            {
                // None were found
                if (isRequired)
                {
                    // Claims are required but none were found so throw an exception
                    string keyNames = keys.Length == 1 ? keys[0] : $"[{string.Join(", ", keys)}]";
                    throw new Exception($"Required claim [{keyNames}] was not found.");
                }
                else
                {
                    // No claims found but they are not required so just return null.
                    return null;
                }
            }
            else
            {
                // Stores validation errors accross multiple claims
                string validationErrors = null;

                // One or more claims were found so validate them
                claims = claims.Where(i => (!isRequired || !string.IsNullOrEmpty(i.Value))
                    && IsValid(i.Type, i.Value, validator, ref validationErrors)).ToList();

                if (claims.Count == 0)
                {
                    if (string.IsNullOrEmpty(validationErrors))
                    {
                        // No valid claims but no validation errors - indicates required claims were found
                        // but did not have a value (e.g. null/empty)
                        string keyNames = keys.Length == 1 ? keys[0] : $"[{string.Join(", ", keys)}]";
                        throw new Exception($"One or more required claim [{keyNames}] were found but did not contain a value.");
                    }
                    else
                    {
                        // All claims failed validation
                        throw new Exception(validationErrors);
                    }
                }
                else
                {
                    // There was at least one valid claim so return the first one that was valid
                    return claims[0].Value;
                }
            }
        }

        /// <summary>
        /// Helper method used to determine if a claim value is valid. 
        /// </summary>
        /// <param name="key">Name of the claim.</param>
        /// <param name="value">Value of the claim.</param>
        /// <param name="validator">Validator used to validate the claim value (if needed).</param>
        /// <param name="validationErrors">Provides a way to </param>
        /// <returns>Flag indicating whether value passed validation.  Validation passes when no validator is present.</returns>
        private bool IsValid(string key, string value, Func<string, bool> validator, ref string validationErrors)
        {
            try
            {
                if (validator != null)
                {
                    bool result = validator(value);
                    if (!result)
                    {
                        if (!string.IsNullOrEmpty(validationErrors))
                        {
                            validationErrors += "\r\n";
                        }
                        validationErrors += $"Validation failed for claim '{key}' with value '{value}'";
                    }
                    return result;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }


        #endregion
    }
}