# Axiom Engine: Developer Cheatsheet

**Version:** 1.0 (Sun Eater Phase)
**Purpose:** Quick reference for critical workflows and commands.

## 1. The CIDER-V Loop (Verification)
*Continuous Integration, Debugging, Error-Resolution, Verification*

This is the standard loop for all development. **Do not commit** without running this.

### -> Step 1: Run The Suite
```powershell
.\scripts\RunTests.ps1
```
*   **Success**: "All tests passed!" (Green)
*   **Failure**: "Unity process exited with code 1" (Red) -> Check `unity_test_log.txt`

### -> Step 2: Analyze Logs (If Failed)
Open `unity_test_log.txt` and search for:
*   `error CS`: Compilation errors.
*   `Exception`: Runtime crashes.
*   `[FAIL]`: Specific assertion failures.

### -> Step 3: Fix & Repeat
Edit code, then run Step 1 again immediately.

---

## 2. Essential Commands

| Action | Command | Notes |
| :--- | :--- | :--- |
| **Run All Tests** | `.\scripts\RunTests.ps1` | The "Golden Standard". Runs in batch mode. |
| **Run EditMode Only** | `.\scripts\RunTests.ps1 -TestMode EditMode` | Faster, for logic/data tests. |
| **Check Git Status** | `git status` | See changed files. |
| **Stage All Changes** | `git add .` | prepares for commit. |
| **Commit Work** | `git commit -m "Message"` | Save to history. |

---

## 3. Demo Generation (Unity Editor)
*How to build the Sun Eater demo content.*

1.  Open Unity Project.
2.  Top Menu: **Axiom > Sun Eater > Build Demo (Full)**.
3.  Result: Generates `Assets/AxiomEngine/GameSpecific/SunEater/Demo/` assets.
4.  **Verification**: Press `Play` in the `SunEater_Demo_Scene`.

---

## 4. Terminal Safety Rules (CRITICAL)

> [!CAUTION]
> **NEVER** run recursive delete commands on the root directory.

*   **SAFE**: `Remove-Item unity_test_log.txt`
*   **UNSAFE**: `rm -rf *` (Never do this)
*   **UNSAFE**: `dotnet clean` (Unless you know exactly why)

---

## 5. Troubleshooting "Exit Code 1"
If the runner exits silently:
1.  Check `unity_test_log.txt`.
2.  If log ends with "Exiting without bug reporter", it is likely a **Domain Reload Crash** or **serialization error**.
3.  **Action**: Open Unity Editor manually (GUI). The error will often appear in the Console window immediately upon load.
