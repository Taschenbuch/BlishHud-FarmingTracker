using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class ApiToken
    {
        public ApiToken(IReadOnlyList<TokenPermission> requiredApiTokenPermissions, Gw2ApiManager gw2ApiManager)
        {
            var missingPermissions = GetMissingPermissions(requiredApiTokenPermissions, gw2ApiManager);
            MissingPermissions.AddRange(missingPermissions);
            ApiTokenState = GetApiTokenState(requiredApiTokenPermissions, gw2ApiManager);
            RequiredPermissions = requiredApiTokenPermissions.ToList();
        }

        public bool CanAccessApi => ApiTokenState == ApiTokenState.CanAccessApi;
        public ApiTokenState ApiTokenState { get; }
        public List<TokenPermission> MissingPermissions { get; } = new List<TokenPermission>();
        public List<TokenPermission> RequiredPermissions { get; }

        public string CreateApiTokenErrorTooltipText()
        {
            return ApiTokenState switch
            {
                ApiTokenState.hasNotLoggedIntoCharacterSinceStartingGw2 
                    => "Error: You have to log into a character once after starting Guild Wars 2. Otherwise the module gets no api acess from blish.",
                ApiTokenState.ApiTokenMissing 
                    => $"Error: Api key missing. Please add an api key with these permissions: {string.Join(", ", RequiredPermissions)}.\n" +
                       "If that does not fix the issue try disabling the module and then enabling it again.",
                ApiTokenState.RequiredPermissionsMissing 
                    => $"Error: Api key is missing these permissions: {string.Join(", ", MissingPermissions)}.\nPlease add a new api key with all required permissions.",
                _   => $"This should not happen. ApiTokenState: {ApiTokenState}",
            };
        }

        public string CreateApiTokenErrorLabelText()
        {
            return ApiTokenState switch
            {
                ApiTokenState.hasNotLoggedIntoCharacterSinceStartingGw2
                    => "Log into character!",
                ApiTokenState.ApiTokenMissing
                    => $"Add API key!",
                ApiTokenState.RequiredPermissionsMissing
                    => "Missing API key permissions!",
                _ => $"This should not happen. ApiTokenState: {ApiTokenState}",
            };
        }

        private ApiTokenState GetApiTokenState(IReadOnlyList<TokenPermission> requiredApiTokenPermissions, Gw2ApiManager gw2ApiManager)
        {
            if (string.IsNullOrWhiteSpace(GameService.Gw2Mumble.PlayerCharacter.Name))
                return ApiTokenState.hasNotLoggedIntoCharacterSinceStartingGw2;
            else if (!gw2ApiManager.HasPermissions(API_TOKEN_PERMISSIONS_EVERY_API_KEY_HAS_BY_DEFAULT))
                return ApiTokenState.ApiTokenMissing;
            else if (!gw2ApiManager.HasPermissions(requiredApiTokenPermissions))
                return ApiTokenState.RequiredPermissionsMissing;
            else
                return ApiTokenState.CanAccessApi;
        }

        private IEnumerable<TokenPermission> GetMissingPermissions(IReadOnlyList<TokenPermission> requiredPermissions, Gw2ApiManager gw2ApiManager)
        {
            foreach (var requiredPermission in requiredPermissions)
                if (!gw2ApiManager.HasPermissions(new List<TokenPermission> { requiredPermission }))
                    yield return requiredPermission;
        }

        private static IReadOnlyList<TokenPermission> API_TOKEN_PERMISSIONS_EVERY_API_KEY_HAS_BY_DEFAULT => new List<TokenPermission>
        {
            TokenPermission.Account
        };
    }
}
