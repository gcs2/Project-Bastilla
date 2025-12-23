---
description: Run all stability checks (compilation and tests)
---

# Stability Check Workflow

This workflow ensures the project is in a clean, stable state before any major commit or handoff.

// turbo-all
1. Run compilation check and automated tests
```powershell
powershell -ExecutionPolicy Bypass -File scripts/RunTests.ps1
```

2. Verify project is not locked (Unity Editor should be closed for batch mode to work reliably)

3. Review `unity_test_log.txt` for any hidden errors
