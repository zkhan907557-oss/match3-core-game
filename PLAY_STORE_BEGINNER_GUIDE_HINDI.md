# Match-3 Game ko Google Play Store par kaise upload karein

Ye guide beginner ke liye hai. Abhi tumhare paas browser game file hai:

`deploy/index.html`

Google Play Store par HTML file direct upload nahi hoti. Play Store ke liye Android app bundle chahiye:

`.aab`

## Sabse important baat

Google Play Store par upload karne ke liye tumhe:

1. Android app banana hoga.
2. Uska signed `.aab` file export karna hoga.
3. Google Play Console account banana hoga.
4. App listing, screenshots, icon, privacy policy, testing, aur release setup karna hoga.

## Recommended beginner route

Tumhare game ke liye 2 practical options hain:

## Option A: Unity se proper Android game banana

Ye best hai agar tum real Unity game banana chahte ho.

Steps:

1. Unity Hub install/open karo.
2. New 2D Unity project banao.
3. Is folder ke scripts copy/import karo:
   `Assets/Scripts/Core`
4. Scene me empty GameObject banao: `GameManager`
5. Us par ye components add karo:
   - `GridManager`
   - `MatchDetector`
   - `PowerUpHandler`
   - `ScoreManager`
   - `TileSwap`
6. Tile prefab banao:
   - GameObject > 2D Object > Sprite
   - `Tile.cs` component add karo
   - `BoxCollider2D` add karo
   - Prefab bana kar `GridManager` me assign karo
7. Android Build Support install karo Unity Hub se.
8. Unity me jao:
   - File > Build Settings
   - Android select karo
   - Switch Platform
9. Player Settings me:
   - Package Name: `com.yourname.match3game`
   - Version: `1.0`
   - Bundle Version Code: `1`
10. Publishing Settings me keystore banao.
11. Build Settings me `Build App Bundle (Google Play)` ON karo.
12. Build karo. Output file `.aab` hogi.

## Option B: Is HTML game ko Android WebView app me wrap karna

Ye fast demo ke liye easy hai, lekin Play Store approval ke liye quality, privacy, icon, policy sab sahi hone chahiye.

Steps:

1. Android Studio install karo.
2. New Project > Empty Views Activity banao.
3. App name: `Match 3 Core`
4. Package name: `com.yourname.match3core`
5. `deploy/index.html` ko Android project ke `assets` folder me rakho.
6. MainActivity me WebView se local HTML open karo:
   `file:///android_asset/index.html`
7. Release ke liye signed `.aab` build karo.

## Play Console upload steps

1. Google Play Console account banao:
   https://play.google.com/console
2. One-time developer registration fee pay karni hoti hai.
3. Play Console me `Create app` karo.
4. App type me `Game` select karo.
5. App free/paid choose karo.
6. Store listing fill karo:
   - App name
   - Short description
   - Full description
   - Icon
   - Feature graphic
   - Screenshots
7. App content sections complete karo:
   - Privacy Policy
   - Data Safety
   - Ads declaration
   - Content rating
   - Target audience
8. Testing track me `.aab` upload karo.
9. Testing complete hone ke baad Production release submit karo.

## Tumhe kya banana padega

Minimum assets:

1. App icon: 512x512 PNG
2. Feature graphic: 1024x500 PNG
3. Phone screenshots: kam se kam 2
4. Privacy policy URL
5. Signed `.aab` file

## Abhi tumhare folder me kya ready hai

Browser playable file:

`deploy/index.html`

Unity scripts:

`Assets/Scripts/Core`

## Official docs

Google Play app setup:

https://support.google.com/googleplay/android-developer/answer/9859152

Android App Bundle:

https://developer.android.com/guide/app-bundle/app-bundle-format

Play Console start:

https://support.google.com/googleplay/android-developer/answer/6112435

