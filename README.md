# MedBook Associate Architect Exercise

Version 1.0

This exercise evaluates hands-on engineering judgment: reading an unfamiliar codebase, making a focused production-quality change, validating it, and explaining the architectural consequences.

## Choose one implementation

- **C# / .NET 8 (primary):** `csharp/`
- **Python 3.10+ (environment fallback):** `python/`

Use the language agreed with the interviewer. The exercises are functionally equivalent.

## Before the timed exercise

Run the baseline checks only. Do not begin changing code until the interviewer reads the exercise prompt.

### C#

```bash
dotnet run --project csharp/MedBook.Booking.Tests
```

### Python

```bash
python3 -m unittest discover -s python/tests -v
```

The baseline intentionally contains failing checks. A failing baseline means the exercise is ready; it is not an environment failure.

## During the exercise

You may use an approved AI coding assistant, documentation, and internet search. You remain responsible for every retained change. The interviewer may ask:

- What context and constraints did you give the tool?
- Which suggestion did you reject or correct?
- How did you verify the result?
- What sensitive information would you avoid sharing?

Use `AI-NOTES.md` only if the interviewer asks you to record a short note.

## Scope

Read `docs/domain-rules.md`. The interviewer will tell you which behavior to prioritize. You are not expected to complete every possible improvement in 35 minutes.

Do not add external packages. Keep the exercise runnable with the installed language runtime.
