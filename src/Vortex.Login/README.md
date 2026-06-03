# Vortex.Login

Multi-screen Godot UI flow for the Habbo Hotel login process. Manages environment selection, credential input, avatar choice, and SSO token authentication.

## SDK

`Microsoft.NET.Sdk` — References `GodotSharp`/`Godot.SourceGenerators` 4.6.1.
Depends on: `Vortex.Core`, `Vortex.Habbo`, `Vortex.Bootstrap`, `Vortex.IID`, `Vortex.OnBoardingHcUI`

## Login Flow (4 Screens)

| Screen | Class | Purpose |
|--------|-------|---------|
| SCREEN_ENVIRONMENT (1) | `EnvironmentView` | Server region selection (flag grid) |
| SCREEN_LOGIN (2) | `LoginView` | Email + password input |
| SCREEN_AVATARS (3) | `AvatarView` | Character selection (up to 7 avatars) |
| SCREEN_SSO_TOKEN (4) | `SsoTokenView` | Direct SSO token input |

**Default entry:** SCREEN_SSO_TOKEN
**Exit:** `LoginFlowFinished` event with SSO token

## Key Classes

- **`LoginFlow`** — Root `Control`, implements `ILoginContext` + `ILoginViewer`. Owns a lightweight `CoreComponentContext` for config/communication/localization bootstrap. Manages screen transitions via `ShowScreen(id)`.
- **`EnvironmentView`** — Grid of flag buttons (en, pt, de, es, fi, fr, it, nl, tr, dev). Parses `live.environment.list` config.
- **`LoginView`** — Email/password form with SOL property persistence.
- **`AvatarView`** — Async image loading for avatar previews. `ImageLoader` fetches from Habbo imaging API.
- **`SsoTokenView`** — Parses `hhXX.part1.part2` token format. Maps `br`→`pt`, `us`→`en`.
- **`WebCaptchaView`** — Captcha placeholder (stub: press Enter).
- **`Background`** — Gradient + tiled texture backdrop.
- **`ImageLoader`** — Static async HTTP/res:// image loader with 10s timeout.

## Patterns

- **ILoginContext** (outgoing): `InitLogin()`, `InitLoginWithSsoToken()`, `LoginWithAvatar()`, `ShowScreen()`
- **ILoginViewer** (incoming): `ShowRegistrationError()`, `PopulateCharacterList()`, `ShowCaptchaError()`
- **Property interpolation:** `GetProperty(key)` checks configuration → fallback dict → localization, supports `${key}` and `%{param}%`
- **SOL persistence:** Saves/restores environment, username, password locally

## Directory Structure

```
src/
  ILoginContext.cs
  LoginFlow.cs          Main orchestrator (~500 lines)
  EnvironmentView.cs    Screen 1
  LoginView.cs          Screen 2
  AvatarView.cs         Screen 3
  SsoTokenView.cs       Screen 4
  WebCaptchaView.cs     Captcha stub
  Background.cs         Visual backdrop
  ImageLoader.cs        Async image fetching
  ImageLoaderEvent.cs   Event wrapper
```