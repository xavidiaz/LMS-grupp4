# Lexicon LMS – grupp 4

> **Tillfällig README.** Den uppdateras när LMS-templaten läggs in i repot.

Slutprojekt på Lexicon: en lärplattform (LMS) byggd med Blazor Web App, remote API, YARP/BFF och EF Core.

## Länkar

| Vad | Länk |
|---|---|
| Projektboard | https://github.com/orgs/Lexicon-grupp4/projects/1 |
| Azure – webbapp (LMS.Blazor) | https://lms2-web.azurewebsites.net/ |
| Azure – API (LMS.API, Swagger) | https://lms2-api.azurewebsites.net/swagger |
| Deploy-workflow | [`.github/workflows/main_lms-grupp4.yml`](.github/workflows/main_lms-grupp4.yml) |

# Data Model
> Utkast. Uppdateras när entiteterna är bestämda och skapade i `Domain.Models`.

```mermaid
erDiagram
 
    Course ||--o{ Module : "has"

    Module ||--o{ Activity : "has"

    ActivityType ||--o{ Activity : "type of"
 
    Course ||--o{ ApplicationUser : "has users"
 
    Course ||--o{ Document : "has"

    Module ||--o{ Document : "has"

    Activity ||--o{ Document : "has"
 
    ApplicationUser ||--o{ Document : "uploads"

    ApplicationUser ||--o{ Notification : "receives"

    ApplicationUser ||--o{ Submission : "submits"

    ApplicationUser ||--o{ Feedback : "gives"
 
    Activity ||--o{ Submission : "has"

    Submission ||--o{ Feedback : "has"
 
    Course {

        int Id PK

        string Name

        string Code

        string Description

        datetime StartDate

        datetime EndDate

    }
 
    Module {

        int Id PK

        int CourseId FK

        string Name

        string Description

        datetime StartDate

        datetime EndDate

    }
 
    ActivityType {

        int Id PK

        string Name

    }
 
    Activity {

        int Id PK

        int ModuleId FK

        int ActivityTypeId FK

        string Title

        string Description

        datetime StartTime

        datetime EndTime

    }
 
    ApplicationUser {

        string Id PK

        string FirstName

        string LastName

        string Email

        int CourseId FK

    }
 
    Document {

        int Id PK

        int ActivityId FK 

        int CourseId FK 

        int ModuleId FK 

        string FileName

        string FilePath

        datetime UploadedAt

        string UploadedById FK

    }
 
    Submission {

        int Id PK

        int ActivityId FK

        string UserId FK

        string FileName

        string FilePath

        datetime SubmittedAt

        string Status

    }
 
    Feedback {

        int Id PK

        int SubmissionId FK

        string UserId FK

        string Comment

        string Grade

        datetime CreatedAt

    }
 
    Notification {

        int Id PK

        string UserId FK

        string Message

        bool IsRead

        datetime CreatedAt

    }
 ```


## Branches

| Branch | Syfte |
|---|---|
| `main` | Verifierade leveranser. Push hit deployar till Azure. Inga direkta commits. |
| `development` | Default-branch. Alla feature-PRs går hit. |
| `feature/usXX-kort-namn` | Ny funktionalitet, t.ex. `feature/us04-modules` |
| `bugfix/kort-namn` | Buggrättningar, t.ex. `bugfix/submission-deadline` |

**Sprint Goal 1**

Skapa en stabil grund för LMS:et genom att verifiera templaten och GitHub-flödet samt bygga den grundläggande strukturen för användare och kurser.

**Sprint Goal 2**

Skapa en säker och tydlig kursupplevelse för studenten.


## Arbetsflöde

1. Välj en sub-issue på boarden, tilldela dig själv och flytta den till **In progress**.
2. Skapa en branch från `development`, gärna direkt från issuet:
   ```bash
   gh issue develop <nr> --name feature/us04-modules --base development --checkout
   ```
3. Implementera och verifiera lokalt.
4. Öppna en PR mot `development`. Skriv `Closes #nr` för varje sub-issue som PR:n färdigställer.
5. En annan gruppmedlem granskar och godkänner.
6. Merge. Sub-issues stängs och flyttas till **Done** automatiskt.
7. `development` → `main` via PR när gruppen har en verifierad leveranspunkt.

Håll högst 2–3 PRs samtidigt i Ready for review. Prioritera review när kön växer.

## Arbetssätt under Sprint 1

- **15 min Daily Standup kl. 09:15**
  - Vad gjorde jag igår?
  - Vad ska jag göra idag?
  - Finns det något som blockerar mig?

- **Avstämning efter lunch kl. 13:15**
  - Kort avstämning om hur arbetet går.
  - Ta upp problem eller blockeringar.
  - Stäm av om någon behöver hjälp.

- **Mötesdisciplin**
  - En person pratar i taget.
  - Låt den som pratar tala till punkt innan nästa person börjar.
  - Håll diskussionerna korta och relevanta för sprinten.
  - Om en längre diskussion behövs tas den efter standup eller i ett separat kort möte.

- **Pull Request**
  - Skriv issue-numret, t.ex. `#123`, när du skickar PR.

- **PR får inte bli sittande**
  - Pull Requests ska inte ligga och blockera gruppens arbete.
  - Om en PR fastnar på grund av fel, konflikter eller annat problem stannar gruppen upp.
  - Gruppen tar ett kort gemensamt möte och hjälps åt att lösa problemet samt frågar läraren vid behov.
  - Review prioriteras när PR-kön växer.


## Statusar på boarden

| Status | Betydelse |
|---|---|
| Backlog | Kravet finns men är inte valt för aktivt arbete. |
| Sprintbacklog | Valt för den aktuella sprinten. |
| In progress | Någon arbetar aktivt med uppgiften. |
| In review | En PR är Ready for review. |
| Done | Granskat, mergat och verifierat. |

## Definition of Done 

- [ ] Uppfyller issuebeskrivningen och berörda acceptance criteria.
- [ ] Bygger utan fel och kan startas.
- [ ] Server-side validering och authorization är på plats.
- [ ] Databasändringar har en fungerande migration.
- [ ] Klientens API-anrop går via YARP/BFF-flödet.
- [ ] Testscenarier kontrollerade, automatiserade tester passerar.
- [ ] PR granskad av minst en annan gruppmedlem och kommentarer hanterade.
- [ ] Mergad till `development` och berörda sub-issues stängda.
- [ ] README uppdaterad vid ändrad installation eller konfiguration.


## Kom igång
Det behövs framför allt för US06 Dokument, US07 studentdelning och US09 inlämningar. US06 kräver validering av tillåtna filtyper och maximal filstorlek, och samma filregler ska återanvändas i US07. Lexicon LMS med UserStories US09 kräver också att filtyp och filstorlek valideras.

## Filstrategi

- Filer lagras på serversidan och metadata sparas i databasen.
- Tillåtna filtyper: PDF, DOCX, PPTX, XLSX, TXT, JPG/JPEG och PNG.
- Maximal filstorlek: 10 MB per fil.
- Filtyp och filstorlek valideras server-side.
- Vi kommer att ha server-side-validering för säkerhet och prestanda, samt client-side-validering för användarfeedback och för att undvika onödiga requests till servern.
- Nedladdning sker genom API/BFF/YARP med behörighetskontroll.
- Samma filregler återanvänds för dokumentdelning och inlämningar.

  ## Kom igång
_Fylls i när templaten finns: krav (.NET 10 SDK), secrets, databas och hur API och Blazor startas._

## Testa API:et

Varje user story med endpoints har en egen fil i `LMS.API/`, t.ex. `US09-Submissions.http`.

1. Starta API:et och se till att databasen är seedad.
2. Uppdatera `@SeedPassword` högst upp i filen så att det matchar din maskin: `dotnet user-secrets list --project LMS.API` (nyckeln `password`).
3. Kör filen uppifrån och ned. Varje rubrik anger förväntad status, t.ex. `(403)`. Läs alltid `Status:` i svaret.

Rubriker med `returns ... today` är kända buggar. Den första statusen är den korrekta, och den andra är vad API:et svarar i dag.

## Deploy

Push till `main` kör GitHub Actions som bygger och publicerar två appar till Azure App Service:

| Projekt | App Service | URL |
|---|---|---|
| `LMS.Blazor/LMS.Blazor` | `lms2-web` | https://lms2-web.azurewebsites.net/ |
| `LMS.API` | `lms2-api` | https://lms2-api.azurewebsites.net/swagger |

Azure-inloggningen gäller bara körningar från `main`.

**Varje deploy från `main` raderar all data i Azure-databasen.** Workflowen tar bort alla tabeller och kör EF Core-migreringarna, och API:et seedar databasen när det startar. Om ett steg misslyckas stoppas deployen. Anslutningssträngen ligger i GitHub-secreten `AZURE_SQL_CONNECTION_STRING`. Ta bort steget `Reset database` om databasen ska börja innehålla riktig data.

API:et har ingen sida på rot-URL:en (`/` ger 404), använd `/swagger`. Swagger visas bara när `ASPNETCORE_ENVIRONMENT` är `Development`, vilket just nu är satt i appinställningarna för `lms2-api`. Om den inställningen tas bort slutar Swagger-länken att fungera.
