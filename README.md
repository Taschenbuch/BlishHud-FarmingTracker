# BlishHud-FarmingTracker
Guild Wars 2 Blish HUD module that allows tracking of farmed items and currencies

# Development
## SonarCloud
- This github repository is added to SonarCloud free plan. 
- SonarCloud analysis can be accessed by opening https://sonarcloud.io/ and logging in with the github account of this repo.

## Mock DRF messages
for testing purposes mock drf messages can be created. 

1. Open `FarmingTracker.FakeDrfWebsockeServer` folder in visual studio code and open the `index.mjs` file.
1. Put a mock drf token into a text file and update the path used for `drfToken` variable in `index.mjs`. Using a real instead of a mock drf token here makes switching between mock and real drf server easier.
1. Add custom drf messages or uncomment existing messages in the `messages` array to select which drf messages should be send.
1. Run `npm run` in visual studio code terminal to start the fake drf server.
1. Open the `FarmingTracker` solution in visual studio and start debugging the `FarmingTracker` project to start the blish module in debug mode.
1. Select the debug tab in the farming tracker window. Check the `fake drf server` checkbox. This debug tab is only visible when the `FarmingTracker` project runs in visual studio debug mode.

## Code 
- signed and unsigned variable prefix:
  - In the code everything coin, profit and stat count related can be unsigned (absolute of a signed value or never negative by default) or signed (already include a sign).
  - This is sometimes difficult to recognize at first glance and resulted in bugs in the past. Because of that the signed/unsigned prefixes are added to variables. 